using Dapper;
using FruitSysWeb.Models.Sledljivost;
using FruitSysWeb.Services.Interfaces;
using FruitSysWeb.Constants;
using System.Text;
using Microsoft.Extensions.Logging;

namespace FruitSysWeb.Services.Implementations.IzvestajService
{
    /// <summary>
    /// Implementacija servisa za sledljivost robe (upstream i downstream)
    /// </summary>
    public class SledljivostService : ISledljivostService
    {
        private readonly DatabaseService _databaseService;
        private readonly SledljivostExcelService _excelService;
        private readonly SledljivostHtmlService _htmlService;
        private readonly ILogger<SledljivostService> _logger;

        public SledljivostService(
            DatabaseService databaseService,
            SledljivostExcelService excelService,
            SledljivostHtmlService htmlService,
            ILogger<SledljivostService> logger)
        {
            _databaseService = databaseService;
            _excelService = excelService;
            _htmlService = htmlService;
            _logger = logger;
        }

        #region Glavne metode za učitavanje sledljivosti

        /// <summary>
        /// Univerzalna pretraga - detektuje tip dokumenta
        /// </summary>
        public async Task<SledljivostModel?> UcitajSledljivost(string sifra)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sifra))
                    return null;

                // Detektuj tip dokumenta po prefiksu
                var normalized = sifra.Trim().ToUpper();
                
                if (normalized.StartsWith("RN-") || normalized.StartsWith("RN"))
                {
                    return await UcitajSledljivostPoRadnomNalogu(sifra);
                }
                else if (normalized.StartsWith("P-") || normalized.StartsWith("PL-"))
                {
                    return await UcitajSledljivostPoPaletnomListu(sifra);
                }
                
                // Pokušaj oba ako nije jasno
                var rezultatRN = await UcitajSledljivostPoRadnomNalogu(sifra);
                if (rezultatRN != null) return rezultatRN;
                
                return await UcitajSledljivostPoPaletnomListu(sifra);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajSledljivost");
                return null;
            }
        }

        /// <summary>
        /// Učitava kompletnu sledljivost za Radni Nalog
        /// </summary>
        public async Task<SledljivostModel?> UcitajSledljivostPoRadnomNalogu(string sifra)
        {
            try
            {
                var rezultat = new SledljivostModel
                {
                    TipDokumenta = "RadniNalog",
                    Sifra = sifra
                };

                // 1. Učitaj osnovne podatke Radnog Naloga
                rezultat.RadniNalog = await UcitajRadniNalogDetalje(sifra);
                if (rezultat.RadniNalog == null)
                    return null;

                rezultat.ID = rezultat.RadniNalog.ID;

                // 2. UPSTREAM - Evidencije Rada
                rezultat.EvidencijeRada = await UcitajEvidencijeRadaZaRadniNalog(rezultat.RadniNalog.ID);

                // 3. UPSTREAM - Paletni Listovi Ulaz (utrošeni u proizvodnji)
                rezultat.PaletniListoviUlaz = await UcitajUtrosenePaletneListove(rezultat.EvidencijeRada);

                // 4. UPSTREAM - Prijemnice za utrošene paletne listove
                rezultat.Prijemnice = await UcitajPrijemniceZaPaletneListove(rezultat.PaletniListoviUlaz);

                // 5. DOWNSTREAM - Paletni Listovi Izlaz (gotovi proizvodi)
                rezultat.PaletniListoviIzlaz = await UcitajGotovePaletneListove(rezultat.RadniNalog.ID);

                // 6. DOWNSTREAM - Otpremnice
                rezultat.Otpremnice = await UcitajOtpremniceZaRadniNalog(rezultat.RadniNalog.ID);

                // 7. Popuni "KoriscenUEvidencijama" SAMO za trenutni radni nalog
                if (rezultat.PaletniListoviUlaz.Any())
                {
                    var paletniListoviIDs = rezultat.PaletniListoviUlaz.Select(pl => pl.ID).ToList();
                    var koriscenjeUProizvodnji = await UcitajGdeJePaletniListKoriscen(paletniListoviIDs, rezultat.RadniNalog.ID);

                    foreach (var pl in rezultat.PaletniListoviUlaz)
                    {
                        if (koriscenjeUProizvodnji.ContainsKey(pl.ID))
                        {
                            pl.KoriscenUEvidencijama = koriscenjeUProizvodnji[pl.ID].Item1;
                            pl.KoriscenUSmenama = koriscenjeUProizvodnji[pl.ID].Item2;
                            pl.KoriscenURadnimNalozima = koriscenjeUProizvodnji[pl.ID].Item3;
                        }
                    }
                }

                return rezultat;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajSledljivostPoRadnomNalogu");
                return null;
            }
        }

        /// <summary>
        /// Učitava kompletnu sledljivost za Paletni List (DOWNSTREAM - obrnuta sledljivost)
        /// </summary>
        public async Task<SledljivostModel?> UcitajSledljivostPoPaletnomListu(string sifra)
        {
            try
            {
                var rezultat = new SledljivostModel
                {
                    TipDokumenta = "PaletniList",
                    Sifra = sifra
                };

                // 1. Učitaj osnovne podatke Paletnog Lista
                var paletniList = await UcitajPaletniListDetalje(sifra);
                if (paletniList == null)
                    return null;

                rezultat.ID = paletniList.ID;

                // 2. LEVA KOLONA - Proizvodnja (Radni Nalog i Evidencija za TAJ paletni list)
                _logger.LogInformation($"[DOWNSTREAM DEBUG] Paletni list - RadniNalogID: {paletniList.RadniNalogID}, EvidencijaRadaID: {paletniList.EvidencijaRadaID}");

                if (paletniList.RadniNalogID.HasValue)
                {
                    // Učitaj Radni Nalog koji je proizveo taj paletni list
                    var rn = await UcitajRadniNalogDetaljePoId(paletniList.RadniNalogID.Value);
                    if (rn != null)
                    {
                        rezultat.RadniNalog = rn;
                        _logger.LogInformation($"[DOWNSTREAM DEBUG] Radni Nalog učitan: {rn.Sifra}");
                    }
                    else
                    {
                        _logger.LogInformation($"[DOWNSTREAM DEBUG] Radni Nalog nije pronađen za ID: {paletniList.RadniNalogID.Value}");
                    }
                }

                if (paletniList.EvidencijaRadaID.HasValue)
                {
                    // Učitaj SAMO evidenciju rada za koju je TAJ paletni list vezan
                    var evidencija = await UcitajEvidencijuRada(paletniList.EvidencijaRadaID.Value);
                    if (evidencija != null)
                    {
                        rezultat.EvidencijeRada.Add(evidencija);
                        _logger.LogInformation($"[DOWNSTREAM DEBUG] Evidencija rada učitana: {evidencija.Sifra}, UtroseniPL IDs: {evidencija.UtroseniPaletniListoviIDs?.Count ?? 0}");
                    }
                    else
                    {
                        _logger.LogInformation($"[DOWNSTREAM DEBUG] Evidencija rada nije pronađena za ID: {paletniList.EvidencijaRadaID.Value}");
                    }
                }
                else
                {
                    _logger.LogInformation($"[DOWNSTREAM DEBUG] Paletni list nema EvidencijaRadaID");
                }

                // 3. DESNA KOLONA - Nabavka sirovine (Ručno povezani paletni listovi i njihove prijemnice)
                // Paletni list već ima učitane PovezaniPaletniListoviIDs u UcitajPaletniListDetalje()
                _logger.LogInformation($"[DOWNSTREAM DEBUG] Paletni list ID: {paletniList.ID}, PovezaniPaletniListoviIDs: {paletniList.PovezaniPaletniListoviIDs?.Count ?? 0}");

                if (paletniList.PovezaniPaletniListoviIDs?.Any() == true)
                {
                    // Koristi DOWNSTREAM servis - učitaj samo osnovne podatke paletnih listova (BEZ JOIN-ova na prijemnice)
                    var povezaniPLDetalji = await UcitajPaletneListoveOsnovniPodaciDown(paletniList.PovezaniPaletniListoviIDs);
                    _logger.LogInformation($"[DOWNSTREAM DEBUG] Učitano {povezaniPLDetalji?.Count ?? 0} povezanih PL detalja");

                    if (povezaniPLDetalji?.Any() == true)
                    {
                        rezultat.PaletniListoviUlaz = povezaniPLDetalji;

                        // Izvuci PrijemnicaStavkaID iz povezanih paletnih listova
                        var prijemnicaStavkaIDs = povezaniPLDetalji
                            .Where(pl => pl.PrijemnicaStavkaID.HasValue)
                            .Select(pl => pl.PrijemnicaStavkaID!.Value)
                            .Distinct()
                            .ToList();

                        _logger.LogInformation($"[DOWNSTREAM DEBUG] Izvučeno {prijemnicaStavkaIDs.Count} PrijemnicaStavkaID iz povezanih PL");

                        // Koristi NOVI servis da direktno učita prijemnice po PrijemnicaStavkaID
                        if (prijemnicaStavkaIDs.Any())
                        {
                            var prijemnice = await UcitajPrijemnicePoStavkama(prijemnicaStavkaIDs);
                            _logger.LogInformation($"[DOWNSTREAM DEBUG] Učitano {prijemnice?.Count ?? 0} prijemnica");

                            if (prijemnice?.Any() == true)
                            {
                                rezultat.Prijemnice = prijemnice;

                                foreach (var pr in prijemnice)
                                {
                                    _logger.LogInformation($"[DOWNSTREAM DEBUG]   - Prijemnica: {pr.Sifra}, Komitent: {pr.KomitentNaziv}");
                                }
                            }
                        }
                        else
                        {
                            _logger.LogInformation($"[DOWNSTREAM DEBUG] Povezani PL nemaju PrijemnicaStavkaID");
                        }
                    }
                }
                else
                {
                    _logger.LogInformation($"[DOWNSTREAM DEBUG] Nema povezanih paletnih listova (PovezaniPaletniListoviIDs prazno)");
                }

                // 4. Dodaj GLAVNI paletni list u Izlaz (za prikaz u Otpremnici)
                rezultat.PaletniListoviIzlaz.Add(paletniList);
                _logger.LogInformation($"[DOWNSTREAM DEBUG] Dodao glavni paletni list {paletniList.Sifra} u PaletniListoviIzlaz");

                // 5. Učitaj Otpremnice za Radni Nalog
                _logger.LogInformation($"[DOWNSTREAM DEBUG] Paletni list RadniNalogID: {paletniList.RadniNalogID?.ToString() ?? "NULL"}");
                if (paletniList.RadniNalogID.HasValue)
                {
                    var otpremnice = await UcitajOtpremniceZaRadniNalog(paletniList.RadniNalogID.Value);
                    if (otpremnice?.Any() == true)
                    {
                        rezultat.Otpremnice = otpremnice;
                        _logger.LogInformation($"[DOWNSTREAM DEBUG] Učitano {otpremnice.Count} otpremnica za Radni Nalog");
                        foreach (var otp in otpremnice)
                        {
                            _logger.LogInformation($"[DOWNSTREAM DEBUG]   - Otpremnica: {otp.Sifra}");
                        }
                    }
                    else
                    {
                        _logger.LogInformation($"[DOWNSTREAM DEBUG] Nema otpremnica za RadniNalogID: {paletniList.RadniNalogID.Value}");
                    }
                }
                else
                {
                    _logger.LogInformation($"[DOWNSTREAM DEBUG] Paletni list nema RadniNalogID");
                }

                return rezultat;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajSledljivostPoPaletnomListu");
                return null;
            }
        }

        // /// <summary>
        // /// Učitava ručno povezane paletne listove iz PaletniListoviPracenje tabele
        // /// NAPOMENA: Ova metoda je duplikat - aktuelna verzija je u #region PaletniListoviPracenje metode
        // /// </summary>
        // private async Task<List<long>> UcitajPovezanePaletneListoveIDs(long paletniListId)
        // {
        //     try
        //     {
        //         var sql = @"
        //             SELECT TPaletniListID
        //             FROM PaletniListoviPracenje
        //             WHERE PaletniListID = @PaletniListID
        //         ";

        //         var rezultat = await _databaseService.QueryAsync<long>(sql, new { PaletniListID = paletniListId });
        //         return rezultat.ToList();
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Greška u UcitajPovezanePaletneListoveIDs");
        //         return new List<long>();
        //     }
        // }

        #endregion

        #region Helper metode za učitavanje pojedinačnih entiteta

        /// <summary>
        /// Učitava osnovne podatke Radnog Naloga
        /// </summary>
        private async Task<RadniNalogDetalji?> UcitajRadniNalogDetalje(string sifra)
        {
            try
            {
                var sql = @"
                    SELECT 
                        rn.ID,
                        rn.Sifra,
                        rn.KomitentID,
                        k.Naziv as KomitentNaziv,
                        rn.LotNaloga,
                        rn.BrojPakovanja,
                        rn.Kolicina,
                        rn.DatumIsporuke as Datum,
                        rn.DokumentStatus
                    FROM RadniNalog rn
                    LEFT JOIN Komitent k ON rn.KomitentID = k.ID
                    WHERE rn.Sifra = @Sifra
                    LIMIT 1
                ";

                var rezultat = await _databaseService.QueryFirstOrDefaultAsync<RadniNalogDetalji>(sql, new { Sifra = sifra });
                
                if (rezultat != null)
                {
                    rezultat.StatusNaziv = DocumentStatus.GetDisplayName(rezultat.DokumentStatus ?? 0);
                }
                
                return rezultat;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajRadniNalogDetalje");
                return null;
            }
        }

        private async Task<RadniNalogDetalji?> UcitajRadniNalogDetaljePoId(long id)
        {
            try
            {
                var sql = @"
                    SELECT 
                        rn.ID,
                        rn.Sifra,
                        rn.KomitentID,
                        k.Naziv as KomitentNaziv,
                        rn.LotNaloga,
                        rn.BrojPakovanja,
                        rn.Kolicina,
                        rn.DatumIsporuke as Datum,
                        rn.DokumentStatus
                    FROM RadniNalog rn
                    LEFT JOIN Komitent k ON rn.KomitentID = k.ID
                    WHERE rn.ID = @ID
                    LIMIT 1
                ";

                var rezultat = await _databaseService.QueryFirstOrDefaultAsync<RadniNalogDetalji>(sql, new { ID = id });
                
                if (rezultat != null)
                {
                    rezultat.StatusNaziv = DocumentStatus.GetDisplayName(rezultat.DokumentStatus ?? 0);
                }
                
                return rezultat;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajRadniNalogDetaljePoId");
                return null;
            }
        }

        /// <summary>
        /// Učitava Evidencije Rada za Radni Nalog
        /// </summary>
        private async Task<List<EvidencijaRadaDetalji>> UcitajEvidencijeRadaZaRadniNalog(long radniNalogId)
        {
            try
            {
                var sql = @"
                    SELECT 
                        er.ID,
                        er.Sifra,
                        er.RadniNalogID,
                        er.SmenskiIzvestajID,
                        si.Broj as SmenskiIzvestajSifra,
                        er.Datum,
                        er.SmenskiIzvestajID,
                        er.BrojRadnihSati
                    FROM EvidencijaRada er
                    LEFT JOIN SmenskiIzvestaj si ON er.SmenskiIzvestajID = si.ID
                    WHERE er.RadniNalogID = @RadniNalogID
                      AND er.Obrisan = 0
                    ORDER BY er.Datum, er.Sifra
                ";

                var rezultat = await _databaseService.QueryAsync<EvidencijaRadaDetalji>(sql, new { RadniNalogID = radniNalogId });
                
                // Za svaku evidenciju, učitaj utrošene paletne listove
                foreach (var evidencija in rezultat)
                {
                    evidencija.UtroseniPaletniListoviIDs = await UcitajUtrosenePaletneListoveIDs(evidencija.ID);
                }
                
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajEvidencijeRadaZaRadniNalog");
                return new List<EvidencijaRadaDetalji>();
            }
        }

        private async Task<EvidencijaRadaDetalji?> UcitajEvidencijuRada(long evidencijaRadaId)
        {
            try
            {
                var sql = @"
                    SELECT 
                        er.ID,
                        er.Sifra,
                        er.RadniNalogID,
                        er.SmenskiIzvestajID,
                        si.Broj as SmenskiIzvestajSifra,
                        er.Datum,
                        er.SmenskiIzvestajID,
                        er.BrojRadnihSati
                    FROM EvidencijaRada er
                    LEFT JOIN SmenskiIzvestaj si ON er.SmenskiIzvestajID = si.ID
                    WHERE er.ID = @ID
                      AND er.Obrisan = 0
                    LIMIT 1
                ";

                var rezultat = await _databaseService.QueryFirstOrDefaultAsync<EvidencijaRadaDetalji>(sql, new { ID = evidencijaRadaId });
                
                if (rezultat != null)
                {
                    rezultat.UtroseniPaletniListoviIDs = await UcitajUtrosenePaletneListoveIDs(rezultat.ID);
                }
                
                return rezultat;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajEvidencijuRada");
                return null;
            }
        }

        /// <summary>
        /// Učitava ID-jeve utrošenih paletnih listova za evidenciju rada
        /// </summary>
        private async Task<List<long>> UcitajUtrosenePaletneListoveIDs(long evidencijaRadaId)
        {
            try
            {
                var sql = @"
                    SELECT PaletniListID
                    FROM EvidencijaRada_UtroseniPaletniListovi
                    WHERE EvidencijaRadaID = @EvidencijaRadaID
                ";

                var rezultat = await _databaseService.QueryAsync<long>(sql, new { EvidencijaRadaID = evidencijaRadaId });
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajUtrosenePaletneListoveIDs");
                return new List<long>();
            }
        }

        /// <summary>
        /// Učitava utrošene paletne listove iz evidencija rada
        /// </summary>
        private async Task<List<PaletniListDetalji>> UcitajUtrosenePaletneListove(List<EvidencijaRadaDetalji> evidencije)
        {
            try
            {
                var sviPaletniListoviIDs = evidencije
                    .SelectMany(e => e.UtroseniPaletniListoviIDs)
                    .Distinct()
                    .ToList();

                if (!sviPaletniListoviIDs.Any())
                    return new List<PaletniListDetalji>();

                var sql = @"
                    SELECT
                        pl.ID,
                        pl.Sifra,
                        pl.Tezina,
                        pl.Aktivan,
                        pl.DatumKreiranja,
                        pl.DokumentStatus,
                        pl.ArtikalID,
                        a.Naziv as ArtikalNaziv,
                        pl.KomitentID,
                        k.Naziv as KomitentNaziv,
                        pl.PakovanjeID,
                        pak.Naziv as PakovanjeNaziv,
                        pl.EvidencijaRadaID,
                        pl.OtpremnicaStavkaID,
                        pl.PrijemnicaStavkaID,
                        pl.RadniNalogID,
                        pl.OtpremnicaDobavljaca,
                        pl.LotDobavljaca,
                        er.Sifra as EvidencijaRadaSifra,
                        pr.Sifra as PrijemnicaSifra,
                        otp.Sifra as OtpremnicaSifra,
                        rn.Sifra as RadniNalogSifra,
                        si.Broj as SmenskiIzvestajSifra
                    FROM PaletniList pl
                    LEFT JOIN Artikal a ON pl.ArtikalID = a.ID
                    LEFT JOIN Komitent k ON pl.KomitentID = k.ID
                    LEFT JOIN Pakovanje pak ON pl.PakovanjeID = pak.ID
                    LEFT JOIN EvidencijaRada er ON pl.EvidencijaRadaID = er.ID
                    LEFT JOIN PrijemnicaStavka ps ON pl.PrijemnicaStavkaID = ps.ID
                    LEFT JOIN Prijemnica pr ON ps.PrijemnicaID = pr.ID
                    LEFT JOIN OtpremnicaStavka os ON pl.OtpremnicaStavkaID = os.ID
                    LEFT JOIN Otpremnica otp ON os.OtpremnicaID = otp.ID
                    LEFT JOIN RadniNalog rn ON pl.RadniNalogID = rn.ID
                    LEFT JOIN SmenskiIzvestaj si ON er.SmenskiIzvestajID = si.ID
                    WHERE pl.ID IN @IDs
                ";

                var rezultat = await _databaseService.QueryAsync<PaletniListDetalji>(sql, new { IDs = sviPaletniListoviIDs });
                var paletniListoviList = rezultat.ToList();

                // Učitaj SVE povezane paletne listove u JEDNOM upitu (optimizacija)
                if (paletniListoviList.Any())
                {
                    var povezaniPodaci = await UcitajSvePovezanePaletneListove(paletniListoviList.Select(pl => pl.ID).ToList());

                    foreach (var pl in paletniListoviList)
                    {
                        pl.StatusNaziv = DocumentStatus.GetDisplayName(pl.DokumentStatus ?? 0);

                        if (povezaniPodaci.ContainsKey(pl.ID))
                        {
                            pl.PovezaniPaletniListoviIDs = povezaniPodaci[pl.ID].Item1;
                            pl.PovezaniPaletniListoviSifre = povezaniPodaci[pl.ID].Item2;
                        }

                        // NE popunjavamo KoriscenUEvidencijama ovde - to će se raditi u UcitajSledljivostPoRadnomNalogu
                        // sa filterom za trenutni radni nalog
                    }
                }

                return paletniListoviList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajUtrosenePaletneListove");
                return new List<PaletniListDetalji>();
            }
        }

        /// <summary>
        /// Učitava paletne listove gotovih proizvoda za radni nalog
        /// </summary>
        private async Task<List<PaletniListDetalji>> UcitajGotovePaletneListove(long radniNalogId)
        {
            try
            {
                var sql = @"
                    SELECT
                        pl.ID,
                        pl.Sifra,
                        pl.Tezina,
                        pl.Aktivan,
                        pl.DatumKreiranja,
                        pl.DokumentStatus,
                        pl.ArtikalID,
                        a.Naziv as ArtikalNaziv,
                        a.MagacinID as ArtikalMagacinID,
                        pl.KomitentID,
                        k.Naziv as KomitentNaziv,
                        pl.PakovanjeID,
                        pak.Naziv as PakovanjeNaziv,
                        pl.EvidencijaRadaID,
                        pl.OtpremnicaStavkaID,
                        pl.PrijemnicaStavkaID,
                        pl.RadniNalogID,
                        pl.OtpremnicaDobavljaca,
                        pl.LotDobavljaca,
                        er.Sifra as EvidencijaRadaSifra,
                        pr.Sifra as PrijemnicaSifra,
                        otp.Sifra as OtpremnicaSifra,
                        rn.Sifra as RadniNalogSifra,
                        si.Broj as SmenskiIzvestajSifra
                    FROM PaletniList pl
                    LEFT JOIN Artikal a ON pl.ArtikalID = a.ID
                    LEFT JOIN Komitent k ON pl.KomitentID = k.ID
                    LEFT JOIN Pakovanje pak ON pl.PakovanjeID = pak.ID
                    LEFT JOIN EvidencijaRada er ON pl.EvidencijaRadaID = er.ID
                    LEFT JOIN PrijemnicaStavka ps ON pl.PrijemnicaStavkaID = ps.ID
                    LEFT JOIN Prijemnica pr ON ps.PrijemnicaID = pr.ID
                    LEFT JOIN OtpremnicaStavka os ON pl.OtpremnicaStavkaID = os.ID
                    LEFT JOIN Otpremnica otp ON os.OtpremnicaID = otp.ID
                    LEFT JOIN RadniNalog rn ON pl.RadniNalogID = rn.ID
                    LEFT JOIN SmenskiIzvestaj si ON er.SmenskiIzvestajID = si.ID
                    WHERE pl.RadniNalogID = @RadniNalogID
                    ORDER BY pl.DatumKreiranja
                ";

                var rezultat = await _databaseService.QueryAsync<PaletniListDetalji>(sql, new { RadniNalogID = radniNalogId });
                var paletniListoviList = rezultat.ToList();

                // Učitaj SVE povezane paletne listove u JEDNOM upitu (optimizacija)
                if (paletniListoviList.Any())
                {
                    var povezaniPodaci = await UcitajSvePovezanePaletneListove(paletniListoviList.Select(pl => pl.ID).ToList());

                    foreach (var pl in paletniListoviList)
                    {
                        pl.StatusNaziv = DocumentStatus.GetDisplayName(pl.DokumentStatus ?? 0);

                        if (povezaniPodaci.ContainsKey(pl.ID))
                        {
                            pl.PovezaniPaletniListoviIDs = povezaniPodaci[pl.ID].Item1;
                            pl.PovezaniPaletniListoviSifre = povezaniPodaci[pl.ID].Item2;
                        }
                    }
                }

                return paletniListoviList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajGotovePaletneListove");
                return new List<PaletniListDetalji>();
            }
        }

        private async Task<PaletniListDetalji?> UcitajPaletniListDetalje(string sifra)
        {
            try
            {
                var sql = @"
                    SELECT
                        pl.ID,
                        pl.Sifra,
                        pl.Tezina,
                        pl.Aktivan,
                        pl.DatumKreiranja,
                        pl.DokumentStatus,
                        pl.ArtikalID,
                        a.Naziv as ArtikalNaziv,
                        pl.KomitentID,
                        k.Naziv as KomitentNaziv,
                        pl.PakovanjeID,
                        pak.Naziv as PakovanjeNaziv,
                        pl.EvidencijaRadaID,
                        pl.OtpremnicaStavkaID,
                        pl.PrijemnicaStavkaID,
                        pl.RadniNalogID,
                        pl.OtpremnicaDobavljaca,
                        pl.LotDobavljaca,
                        er.Sifra as EvidencijaRadaSifra,
                        pr.Sifra as PrijemnicaSifra,
                        otp.Sifra as OtpremnicaSifra,
                        rn.Sifra as RadniNalogSifra,
                        si.Broj as SmenskiIzvestajSifra
                    FROM PaletniList pl
                    LEFT JOIN Artikal a ON pl.ArtikalID = a.ID
                    LEFT JOIN Komitent k ON pl.KomitentID = k.ID
                    LEFT JOIN Pakovanje pak ON pl.PakovanjeID = pak.ID
                    LEFT JOIN EvidencijaRada er ON pl.EvidencijaRadaID = er.ID
                    LEFT JOIN PrijemnicaStavka ps ON pl.PrijemnicaStavkaID = ps.ID
                    LEFT JOIN Prijemnica pr ON ps.PrijemnicaID = pr.ID
                    LEFT JOIN OtpremnicaStavka os ON pl.OtpremnicaStavkaID = os.ID
                    LEFT JOIN Otpremnica otp ON os.OtpremnicaID = otp.ID
                    LEFT JOIN RadniNalog rn ON pl.RadniNalogID = rn.ID
                    LEFT JOIN SmenskiIzvestaj si ON er.SmenskiIzvestajID = si.ID
                    WHERE pl.Sifra = @Sifra
                    LIMIT 1
                ";

                var rezultat = await _databaseService.QueryFirstOrDefaultAsync<PaletniListDetalji>(sql, new { Sifra = sifra });

                if (rezultat != null)
                {
                    rezultat.StatusNaziv = DocumentStatus.GetDisplayName(rezultat.DokumentStatus ?? 0);

                    // Učitaj povezane paletne listove (ID-jevi i šifre)
                    rezultat.PovezaniPaletniListoviIDs = await UcitajPovezanePaletneListoveIDs(rezultat.ID);
                    rezultat.PovezaniPaletniListoviSifre = await UcitajPovezanePaletneListoveSifre(rezultat.ID);

                    _logger.LogInformation($"[DEBUG] UcitajPaletniListDetalje '{sifra}': Povezanih PL: {rezultat.PovezaniPaletniListoviIDs?.Count ?? 0}");
                }

                return rezultat;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajPaletniListDetalje");
                return null;
            }
        }

        /// <summary>
        /// Učitava prijemnice za paletne listove
        /// </summary>
        /// <summary>
        /// UPSTREAM - Učitava osnovne podatke paletnih listova sa JOIN-ovima na dokumente (za RN pretragu)
        /// </summary>
        private async Task<List<PaletniListDetalji>> UcitajPaletneListoveOsnovniPodaci(List<long> paletniListoviIDs)
        {
            try
            {
                _logger.LogInformation($"[UPSTREAM] UcitajPaletneListoveOsnovniPodaci: Tražim {paletniListoviIDs.Count} PL: [{string.Join(", ", paletniListoviIDs)}]");

                if (!paletniListoviIDs.Any())
                    return new List<PaletniListDetalji>();

                var sql = @"
                    SELECT
                        pl.ID,
                        pl.Sifra,
                        pl.Tezina,
                        pl.Aktivan,
                        pl.DatumKreiranja,
                        pl.DokumentStatus,
                        pl.ArtikalID,
                        a.Naziv as ArtikalNaziv,
                        a.MagacinID as ArtikalMagacinID,
                        pl.KomitentID,
                        k.Naziv as KomitentNaziv,
                        pl.PakovanjeID,
                        pak.Naziv as PakovanjeNaziv,
                        pl.EvidencijaRadaID,
                        pl.OtpremnicaStavkaID,
                        pl.PrijemnicaStavkaID,
                        pl.RadniNalogID,
                        pl.OtpremnicaDobavljaca,
                        pl.LotDobavljaca,
                        er.Sifra as EvidencijaRadaSifra,
                        pr.Sifra as PrijemnicaSifra,
                        otp.Sifra as OtpremnicaSifra,
                        rn.Sifra as RadniNalogSifra,
                        si.Broj as SmenskiIzvestajSifra
                    FROM PaletniList pl
                    LEFT JOIN Artikal a ON pl.ArtikalID = a.ID
                    LEFT JOIN Komitent k ON pl.KomitentID = k.ID
                    LEFT JOIN Pakovanje pak ON pl.PakovanjeID = pak.ID
                    LEFT JOIN EvidencijaRada er ON pl.EvidencijaRadaID = er.ID
                    LEFT JOIN PrijemnicaStavka ps ON pl.PrijemnicaStavkaID = ps.ID
                    LEFT JOIN Prijemnica pr ON ps.PrijemnicaID = pr.ID
                    LEFT JOIN OtpremnicaStavka os ON pl.OtpremnicaStavkaID = os.ID
                    LEFT JOIN Otpremnica otp ON os.OtpremnicaID = otp.ID
                    LEFT JOIN RadniNalog rn ON pl.RadniNalogID = rn.ID
                    LEFT JOIN SmenskiIzvestaj si ON er.SmenskiIzvestajID = si.ID
                    WHERE pl.ID IN @IDs
                ";

                var rezultat = await _databaseService.QueryAsync<PaletniListDetalji>(sql, new { IDs = paletniListoviIDs });
                var lista = rezultat.ToList();

                _logger.LogInformation($"[UPSTREAM] UcitajPaletneListoveOsnovniPodaci: Učitano {lista.Count} PL");

                foreach (var pl in lista)
                {
                    pl.StatusNaziv = DocumentStatus.GetDisplayName(pl.DokumentStatus ?? 0);
                    _logger.LogInformation($"[UPSTREAM]   - PL ID={pl.ID}, Sifra={pl.Sifra}, PrijemnicaSifra={pl.PrijemnicaSifra ?? "NULL"}");
                }

                return lista;
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"[ERROR] Greška u UcitajPaletneListoveOsnovniPodaci: {ex.Message}");
                _logger.LogInformation($"[ERROR] Stack trace: {ex.StackTrace}");
                return new List<PaletniListDetalji>();
            }
        }

        /// <summary>
        /// DOWNSTREAM - Učitava samo osnovne podatke paletnih listova BEZ JOIN-ova na dokumente (za P- pretragu)
        /// </summary>
        private async Task<List<PaletniListDetalji>> UcitajPaletneListoveOsnovniPodaciDown(List<long> paletniListoviIDs)
        {
            try
            {
                _logger.LogInformation($"[DOWNSTREAM] UcitajPaletneListoveOsnovniPodaciDown: Tražim {paletniListoviIDs.Count} PL: [{string.Join(", ", paletniListoviIDs)}]");

                if (!paletniListoviIDs.Any())
                    return new List<PaletniListDetalji>();

                var sql = @"
                    SELECT
                        pl.ID,
                        pl.Sifra,
                        pl.Tezina,
                        pl.Aktivan,
                        pl.DatumKreiranja,
                        pl.DokumentStatus,
                        pl.ArtikalID,
                        a.Naziv as ArtikalNaziv,
                        a.MagacinID as ArtikalMagacinID,
                        pl.KomitentID,
                        k.Naziv as KomitentNaziv,
                        pl.PakovanjeID,
                        pak.Naziv as PakovanjeNaziv,
                        pl.EvidencijaRadaID,
                        pl.OtpremnicaStavkaID,
                        pl.PrijemnicaStavkaID,
                        pl.RadniNalogID,
                        pl.OtpremnicaDobavljaca,
                        pl.LotDobavljaca,
                        pr.Sifra as PrijemnicaSifra
                    FROM PaletniList pl
                    LEFT JOIN Artikal a ON pl.ArtikalID = a.ID
                    LEFT JOIN Komitent k ON pl.KomitentID = k.ID
                    LEFT JOIN Pakovanje pak ON pl.PakovanjeID = pak.ID
                    LEFT JOIN PrijemnicaStavka ps ON pl.PrijemnicaStavkaID = ps.ID
                    LEFT JOIN Prijemnica pr ON ps.PrijemnicaID = pr.ID
                    WHERE pl.ID IN @IDs
                ";

                var rezultat = await _databaseService.QueryAsync<PaletniListDetalji>(sql, new { IDs = paletniListoviIDs });
                var lista = rezultat.ToList();

                _logger.LogInformation($"[DOWNSTREAM] UcitajPaletneListoveOsnovniPodaciDown: Učitano {lista.Count} PL");

                foreach (var pl in lista)
                {
                    pl.StatusNaziv = DocumentStatus.GetDisplayName(pl.DokumentStatus ?? 0);
                    _logger.LogInformation($"[DOWNSTREAM]   - PL ID={pl.ID}, Sifra={pl.Sifra}, PrijemnicaStavkaID={pl.PrijemnicaStavkaID?.ToString() ?? "NULL"}, PrijemnicaSifra={pl.PrijemnicaSifra ?? "NULL"}");
                }

                return lista;
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"[ERROR] Greška u UcitajPaletneListoveOsnovniPodaciDown: {ex.Message}");
                _logger.LogInformation($"[ERROR] Stack trace: {ex.StackTrace}");
                return new List<PaletniListDetalji>();
            }
        }

        /// <summary>
        /// NOVI SERVIS - Direktno učitava prijemnice po PrijemnicaStavkaID
        /// </summary>
        private async Task<List<PrijemnicaDetalji>> UcitajPrijemnicePoStavkama(List<long> prijemnicaStavkaIDs)
        {
            try
            {
                _logger.LogInformation($"[DEBUG] UcitajPrijemnicePoStavkama: Tražim prijemnice za {prijemnicaStavkaIDs.Count} PrijemnicaStavka IDs: [{string.Join(", ", prijemnicaStavkaIDs)}]");

                if (!prijemnicaStavkaIDs.Any())
                {
                    _logger.LogInformation($"[DEBUG] UcitajPrijemnicePoStavkama: Nema PrijemnicaStavkaID - vraćam praznu listu");
                    return new List<PrijemnicaDetalji>();
                }

                // Direktan JOIN - PrijemnicaStavka → Prijemnica
                var sql = @"
                    SELECT DISTINCT
                        pr.ID,
                        pr.Sifra,
                        pr.Datum,
                        pr.Otpremnica,
                        pr.KomitentID,
                        k.Naziv as KomitentNaziv,
                        pr.MagacinID,
                        pr.Vozilo,
                        pr.DokumentStatus
                    FROM PrijemnicaStavka ps
                    INNER JOIN Prijemnica pr ON ps.PrijemnicaID = pr.ID
                    LEFT JOIN Komitent k ON pr.KomitentID = k.ID
                    WHERE ps.ID IN @StavkaIDs
                ";

                _logger.LogInformation($"[DEBUG] UcitajPrijemnicePoStavkama: Izvršavam SQL sa StavkaIDs IN ({string.Join(", ", prijemnicaStavkaIDs)})");

                var prijemnice = await _databaseService.QueryAsync<PrijemnicaDetalji>(sql, new { StavkaIDs = prijemnicaStavkaIDs });
                var prijemniceList = prijemnice.ToList();

                _logger.LogInformation($"[DEBUG] UcitajPrijemnicePoStavkama: Učitano {prijemniceList.Count} prijemnica");

                foreach (var pr in prijemniceList)
                {
                    pr.StatusNaziv = DocumentStatus.GetDisplayName(pr.DokumentStatus ?? 0);
                    _logger.LogInformation($"[DEBUG]   - Prijemnica ID={pr.ID}, Sifra={pr.Sifra}, Komitent={pr.KomitentNaziv}");
                }

                return prijemniceList;
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"[ERROR] Greška u UcitajPrijemnicePoStavkama: {ex.Message}");
                _logger.LogInformation($"[ERROR] Stack trace: {ex.StackTrace}");
                return new List<PrijemnicaDetalji>();
            }
        }

        private async Task<List<PrijemnicaDetalji>> UcitajPrijemniceZaPaletneListove(List<PaletniListDetalji> paletniListovi)
        {
            try
            {
                _logger.LogInformation($"[DEBUG] UcitajPrijemniceZaPaletneListove: Ulazno {paletniListovi.Count} paletnih listova");

                // Detaljno logovanje svakog paletnog lista
                foreach (var pl in paletniListovi)
                {
                    _logger.LogInformation($"[DEBUG]   PL {pl.Sifra}: PrijemnicaStavkaID = {pl.PrijemnicaStavkaID?.ToString() ?? "NULL"}");
                }

                var prijemnicaStavkaIDs = paletniListovi
                    .Where(pl => pl.PrijemnicaStavkaID.HasValue)
                    .Select(pl => pl.PrijemnicaStavkaID!.Value)
                    .Distinct()
                    .ToList();

                _logger.LogInformation($"[DEBUG] UcitajPrijemniceZaPaletneListove: Pronađeno {prijemnicaStavkaIDs.Count} PrijemnicaStavka IDs: [{string.Join(", ", prijemnicaStavkaIDs)}]");

                if (!prijemnicaStavkaIDs.Any())
                {
                    _logger.LogInformation($"[DEBUG] UcitajPrijemniceZaPaletneListove: Nema PrijemnicaStavkaID - vraćam praznu listu");
                    return new List<PrijemnicaDetalji>();
                }

                // Prvo učitaj ID-jeve prijemnica
                var sqlStavke = @"
                    SELECT DISTINCT PrijemnicaID
                    FROM PrijemnicaStavka
                    WHERE ID IN @IDs
                ";

                _logger.LogInformation($"[DEBUG] UcitajPrijemniceZaPaletneListove: SQL za PrijemnicaStavka - tražim stavke sa ID IN ({string.Join(", ", prijemnicaStavkaIDs)})");

                var prijemnicaIDs = await _databaseService.QueryAsync<long>(sqlStavke, new { IDs = prijemnicaStavkaIDs });
                var prijemnicaIDsList = prijemnicaIDs.ToList();

                _logger.LogInformation($"[DEBUG] UcitajPrijemniceZaPaletneListove: Iz PrijemnicaStavka dobio {prijemnicaIDsList.Count} PrijemnicaID: [{string.Join(", ", prijemnicaIDsList)}]");

                if (!prijemnicaIDsList.Any())
                {
                    _logger.LogInformation($"[DEBUG] UcitajPrijemniceZaPaletneListove: Nema PrijemnicaID u PrijemnicaStavka tabeli - vraćam praznu listu");
                    return new List<PrijemnicaDetalji>();
                }

                // Učitaj prijemnice
                var sql = @"
                    SELECT
                        pr.ID,
                        pr.Sifra,
                        pr.Datum,
                        pr.Otpremnica,
                        pr.KomitentID,
                        k.Naziv as KomitentNaziv,
                        pr.MagacinID,
                        pr.Vozilo,
                        pr.DokumentStatus
                    FROM Prijemnica pr
                    LEFT JOIN Komitent k ON pr.KomitentID = k.ID
                    WHERE pr.ID IN @IDs
                ";

                _logger.LogInformation($"[DEBUG] UcitajPrijemniceZaPaletneListove: SQL za Prijemnica - tražim prijemnice sa ID IN ({string.Join(", ", prijemnicaIDsList)})");

                var prijemnice = await _databaseService.QueryAsync<PrijemnicaDetalji>(sql, new { IDs = prijemnicaIDsList });
                var prijemniceList = prijemnice.ToList();

                _logger.LogInformation($"[DEBUG] UcitajPrijemniceZaPaletneListove: Učitano {prijemniceList.Count} prijemnica iz Prijemnica tabele");

                foreach (var pr in prijemniceList)
                {
                    pr.StatusNaziv = DocumentStatus.GetDisplayName(pr.DokumentStatus ?? 0);
                    _logger.LogInformation($"[DEBUG]   - Prijemnica: {pr.Sifra}, Komitent: {pr.KomitentNaziv}");
                }

                return prijemniceList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajPrijemniceZaPaletneListove");
                return new List<PrijemnicaDetalji>();
            }
        }

        private async Task<PrijemnicaDetalji?> UcitajPrijemnicu(long prijemnicaStavkaId)
        {
            try
            {
                // Prvo nađi PrijemnicaID
                var sqlStavka = @"
                    SELECT PrijemnicaID
                    FROM PrijemnicaStavka
                    WHERE ID = @ID
                    LIMIT 1
                ";

                var prijemnicaId = await _databaseService.ExecuteScalarAsync<long?>(sqlStavka, new { ID = prijemnicaStavkaId });
                
                if (!prijemnicaId.HasValue)
                    return null;

                var sql = @"
                    SELECT 
                        pr.ID,
                        pr.Sifra,
                        pr.Datum,
                        pr.Otpremnica,
                        pr.KomitentID,
                        k.Naziv as KomitentNaziv,
                        pr.MagacinID,
                        pr.Vozilo,
                        pr.DokumentStatus
                    FROM Prijemnica pr
                    LEFT JOIN Komitent k ON pr.KomitentID = k.ID
                    WHERE pr.ID = @ID
                    LIMIT 1
                ";

                var rezultat = await _databaseService.QueryFirstOrDefaultAsync<PrijemnicaDetalji>(sql, new { ID = prijemnicaId.Value });
                
                if (rezultat != null)
                {
                    rezultat.StatusNaziv = DocumentStatus.GetDisplayName(rezultat.DokumentStatus ?? 0);
                }
                
                return rezultat;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajPrijemnicu");
                return null;
            }
        }

        /// <summary>
        /// Učitava otpremnice za radni nalog
        /// </summary>
        private async Task<List<OtpremnicaDetalji>> UcitajOtpremniceZaRadniNalog(long radniNalogId)
        {
            try
            {
                var sql = @"
                    SELECT 
                        o.ID,
                        o.Sifra,
                        o.Datum,
                        o.Vozilo,
                        o.KomitentID,
                        k.Naziv as KomitentNaziv,
                        o.RadniNalogID,
                        o.DokumentStatus
                    FROM Otpremnica o
                    LEFT JOIN Komitent k ON o.KomitentID = k.ID
                    WHERE o.RadniNalogID = @RadniNalogID
                    ORDER BY o.Datum
                ";

                var rezultat = await _databaseService.QueryAsync<OtpremnicaDetalji>(sql, new { RadniNalogID = radniNalogId });
                
                foreach (var otp in rezultat)
                {
                    otp.StatusNaziv = DocumentStatus.GetDisplayName(otp.DokumentStatus ?? 0);
                }
                
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajOtpremniceZaRadniNalog");
                return new List<OtpremnicaDetalji>();
            }
        }

        private async Task<OtpremnicaDetalji?> UcitajOtpremnicu(long otpremnicaStavkaId)
        {
            try
            {
                // Prvo nađi OtpremnicaID
                var sqlStavka = @"
                    SELECT OtpremnicaID
                    FROM OtpremnicaStavka
                    WHERE ID = @ID
                    LIMIT 1
                ";

                var otpremnicaId = await _databaseService.ExecuteScalarAsync<long?>(sqlStavka, new { ID = otpremnicaStavkaId });
                
                if (!otpremnicaId.HasValue)
                    return null;

                var sql = @"
                    SELECT 
                        o.ID,
                        o.Sifra,
                        o.Datum,
                        o.Vozilo,
                        o.KomitentID,
                        k.Naziv as KomitentNaziv,
                        o.RadniNalogID,
                        o.DokumentStatus
                    FROM Otpremnica o
                    LEFT JOIN Komitent k ON o.KomitentID = k.ID
                    WHERE o.ID = @ID
                    LIMIT 1
                ";

                var rezultat = await _databaseService.QueryFirstOrDefaultAsync<OtpremnicaDetalji>(sql, new { ID = otpremnicaId.Value });
                
                if (rezultat != null)
                {
                    rezultat.StatusNaziv = DocumentStatus.GetDisplayName(rezultat.DokumentStatus ?? 0);
                }
                
                return rezultat;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajOtpremnicu");
                return null;
            }
        }

        #endregion

        #region Autocomplete i pretraga

        /// <summary>
        /// Univerzalna pretraga za autocomplete
        /// </summary>
        public async Task<List<DokumentSearchResultModel>> PretraziDokumente(string searchTerm, int limit = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return new List<DokumentSearchResultModel>();

                var rezultat = new List<DokumentSearchResultModel>();

                // Pretraži Radne Naloge
                var radniNalozi = await PretraziRadneNaloge(searchTerm, limit / 2);
                rezultat.AddRange(radniNalozi);

                // Pretraži Paletne Listove
                var paletniListovi = await PretraziPaletneListove(searchTerm, limit / 2);
                rezultat.AddRange(paletniListovi);

                return rezultat.Take(limit).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u PretraziDokumente");
                return new List<DokumentSearchResultModel>();
            }
        }

        private async Task<List<DokumentSearchResultModel>> PretraziRadneNaloge(string searchTerm, int limit)
        {
            try
            {
                var sql = @"
                    SELECT 
                        rn.Sifra,
                        'RadniNalog' as Tip,
                        rn.ID,
                        CONCAT('Kupac: ', COALESCE(k.Naziv, 'Nepoznato'), ', ', COALESCE(rn.Kolicina, 0), ' kg') as Opis
                    FROM RadniNalog rn
                    LEFT JOIN Komitent k ON rn.KomitentID = k.ID
                    WHERE rn.Aktivno = 1
                      AND rn.Sifra LIKE @SearchTerm
                    ORDER BY rn.DatumIsporuke DESC
                    LIMIT @Limit
                ";

                var rezultat = await _databaseService.QueryAsync<DokumentSearchResultModel>(sql, 
                    new { SearchTerm = $"%{searchTerm}%", Limit = limit });

                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u PretraziRadneNaloge");
                return new List<DokumentSearchResultModel>();
            }
        }

        private async Task<List<DokumentSearchResultModel>> PretraziPaletneListove(string searchTerm, int limit)
        {
            try
            {
                var sql = @"
                    SELECT 
                        pl.Sifra,
                        'PaletniList' as Tip,
                        pl.ID,
                        CONCAT(COALESCE(a.Naziv, 'Nepoznato'), ', ', COALESCE(pl.Tezina, 0), ' kg') as Opis
                    FROM PaletniList pl
                    LEFT JOIN Artikal a ON pl.ArtikalID = a.ID
                    WHERE  pl.Sifra LIKE @SearchTerm
                    ORDER BY pl.DatumKreiranja DESC
                    LIMIT @Limit
                ";

                var rezultat = await _databaseService.QueryAsync<DokumentSearchResultModel>(sql, 
                    new { SearchTerm = $"%{searchTerm}%", Limit = limit });

                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u PretraziPaletneListove");
                return new List<DokumentSearchResultModel>();
            }
        }

        /// <summary>
        /// Učitava šifre Radnih Naloga za autocomplete
        /// </summary>
        public async Task<List<string>> UcitajSifreRadnihNaloga(string searchTerm = "", int limit = 50)
        {
            try
            {
                var sql = @"
                    SELECT Sifra
                    FROM RadniNalog
                    WHERE Aktivno = 1
                ";

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    sql += " AND Sifra LIKE @SearchTerm";
                }

                sql += @"
                    ORDER BY DatumIsporuke DESC
                    LIMIT @Limit
                ";

                var rezultat = await _databaseService.QueryAsync<string>(sql, 
                    new { SearchTerm = $"%{searchTerm}%", Limit = limit });

                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajSifreRadnihNaloga");
                return new List<string>();
            }
        }

        /// <summary>
        /// Učitava šifre Paletnih Listova za autocomplete
        /// </summary>
        public async Task<List<string>> UcitajSifrePaletnihListova(string searchTerm = "", int limit = 50)
        {
            try
            {
                var sql = @"
                    SELECT Sifra
                    FROM PaletniList
                ";

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    sql += " AND Sifra LIKE @SearchTerm";
                }

                sql += @"
                    ORDER BY DatumKreiranja DESC
                    LIMIT @Limit
                ";

                var rezultat = await _databaseService.QueryAsync<string>(sql, 
                    new { SearchTerm = $"%{searchTerm}%", Limit = limit });

                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajSifrePaletnihListova");
                return new List<string>();
            }
        }

        #endregion
        
        #region PaletniListoviPracenje metode
        
        /// <summary>
        /// Učitava ID-jeve povezanih paletnih listova iz PaletniListoviPracenje tabele
        /// </summary>
        private async Task<List<long>> UcitajPovezanePaletneListoveIDs(long paletniListID)
        {
            try
            {
                var sql = @"
                    SELECT TPaletniListID
                    FROM PaletniListoviPracenje
                    WHERE PaletniListID = @PaletniListID
                ";

                var rezultat = await _databaseService.QueryAsync<long>(sql, new { PaletniListID = paletniListID });
                var lista = rezultat.ToList();
                _logger.LogInformation($"[DEBUG] UcitajPovezanePaletneListoveIDs za PL ID {paletniListID}: Pronađeno {lista.Count} povezanih PL");
                return lista;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajPovezanePaletneListoveIDs");
                return new List<long>();
            }
        }

        /// <summary>
        /// Učitava šifre povezanih paletnih listova iz PaletniListoviPracenje tabele
        /// </summary>
        private async Task<List<string>> UcitajPovezanePaletneListoveSifre(long paletniListID)
        {
            try
            {
                var sql = @"
                    SELECT pl.Sifra
                    FROM PaletniListoviPracenje plp
                    INNER JOIN PaletniList pl ON plp.TPaletniListID = pl.ID
                    WHERE plp.PaletniListID = @PaletniListID
                ";

                var rezultat = await _databaseService.QueryAsync<string>(sql, new { PaletniListID = paletniListID });
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajPovezanePaletneListoveSifre");
                return new List<string>();
            }
        }

        /// <summary>
        /// Učitava SVE povezane paletne listove za listu paletnih listova u JEDNOM upitu (optimizacija performance)
        /// </summary>
        private async Task<Dictionary<long, (List<long>, List<string>)>> UcitajSvePovezanePaletneListove(List<long> paletniListoviIDs)
        {
            try
            {
                if (!paletniListoviIDs.Any())
                    return new Dictionary<long, (List<long>, List<string>)>();

                var sql = @"
                    SELECT
                        plp.PaletniListID,
                        plp.TPaletniListID,
                        pl.Sifra as TPaletniListSifra
                    FROM PaletniListoviPracenje plp
                    INNER JOIN PaletniList pl ON plp.TPaletniListID = pl.ID
                    WHERE plp.PaletniListID IN @IDs
                ";

                var rezultat = await _databaseService.QueryAsync<dynamic>(sql, new { IDs = paletniListoviIDs });

                // Grupiši rezultate po PaletniListID
                var dictionary = new Dictionary<long, (List<long>, List<string>)>();

                foreach (var row in rezultat)
                {
                    long paletniListID = row.PaletniListID;
                    long tPaletniListID = row.TPaletniListID;
                    string tPaletniListSifra = row.TPaletniListSifra;

                    if (!dictionary.ContainsKey(paletniListID))
                    {
                        dictionary[paletniListID] = (new List<long>(), new List<string>());
                    }

                    dictionary[paletniListID].Item1.Add(tPaletniListID);
                    dictionary[paletniListID].Item2.Add(tPaletniListSifra);
                }

                return dictionary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajSvePovezanePaletneListove");
                return new Dictionary<long, (List<long>, List<string>)>();
            }
        }

        /// <summary>
        /// Učitava evidencije rada, smene i radne naloge gde je paletni list korišćen
        /// SAMO za specifičan radni nalog (filteruje po radniNalogId)
        /// </summary>
        private async Task<Dictionary<long, (List<string>, List<string>, List<string>)>> UcitajGdeJePaletniListKoriscen(List<long> paletniListoviIDs, long radniNalogId)
        {
            try
            {
                if (!paletniListoviIDs.Any())
                    return new Dictionary<long, (List<string>, List<string>, List<string>)>();

                var sql = @"
                    SELECT
                        erupl.PaletniListID,
                        er.Sifra as EvidencijaSifra,
                        si.Broj as SmenskiIzvestajSifra,
                        rn.Sifra as RadniNalogSifra
                    FROM EvidencijaRada_UtroseniPaletniListovi erupl
                    INNER JOIN EvidencijaRada er ON erupl.EvidencijaRadaID = er.ID
                    LEFT JOIN SmenskiIzvestaj si ON er.SmenskiIzvestajID = si.ID
                    LEFT JOIN RadniNalog rn ON er.RadniNalogID = rn.ID
                    WHERE erupl.PaletniListID IN @IDs
                      AND er.Obrisan = 0
                      AND er.RadniNalogID = @RadniNalogID
                ";

                var rezultat = await _databaseService.QueryAsync<dynamic>(sql, new { IDs = paletniListoviIDs, RadniNalogID = radniNalogId });

                // Grupiši rezultate po PaletniListID
                var dictionary = new Dictionary<long, (List<string>, List<string>, List<string>)>();

                foreach (var row in rezultat)
                {
                    long paletniListID = row.PaletniListID;
                    string evidencijaSifra = row.EvidencijaSifra;
                    string? smenskiIzvestajSifra = row.SmenskiIzvestajSifra;
                    string? radniNalogSifra = row.RadniNalogSifra;

                    if (!dictionary.ContainsKey(paletniListID))
                    {
                        dictionary[paletniListID] = (new List<string>(), new List<string>(), new List<string>());
                    }

                    if (!string.IsNullOrEmpty(evidencijaSifra))
                        dictionary[paletniListID].Item1.Add(evidencijaSifra);

                    if (!string.IsNullOrEmpty(smenskiIzvestajSifra) && !dictionary[paletniListID].Item2.Contains(smenskiIzvestajSifra))
                        dictionary[paletniListID].Item2.Add(smenskiIzvestajSifra);

                    if (!string.IsNullOrEmpty(radniNalogSifra) && !dictionary[paletniListID].Item3.Contains(radniNalogSifra))
                        dictionary[paletniListID].Item3.Add(radniNalogSifra);
                }

                return dictionary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajGdeJePaletniListKoriscen");
                return new Dictionary<long, (List<string>, List<string>, List<string>)>();
            }
        }
        
        /// <summary>
        /// Učitava detalje povezanih paletnih listova za određeni paletni list (iz PaletniListoviPracenje)
        /// </summary>
        private async Task<List<PaletniListDetalji>> UcitajPovezanePaletneListoveDetalje(long paletniListID)
        {
            try
            {
                // Učitaj ID-jeve povezanih paletnih listova
                var povezaniIDs = await UcitajPovezanePaletneListoveIDs(paletniListID);

                if (!povezaniIDs.Any())
                    return new List<PaletniListDetalji>();

                // Učitaj detalje za te paletne listove
                return await UcitajPovezanePaletneListove(povezaniIDs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajPovezanePaletneListoveDetalje");
                return new List<PaletniListDetalji>();
            }
        }

        /// <summary>
        /// Učitava detalje povezanih paletnih listova
        /// </summary>
        private async Task<List<PaletniListDetalji>> UcitajPovezanePaletneListove(List<long> paletniListoviIDs)
        {
            try
            {
                _logger.LogInformation($"[DEBUG] UcitajPovezanePaletneListove: Tražim detalje za {paletniListoviIDs.Count} PL IDs");
                _logger.LogInformation($"[DEBUG] UcitajPovezanePaletneListove: IDs = [{string.Join(", ", paletniListoviIDs)}]");

                if (!paletniListoviIDs.Any())
                    return new List<PaletniListDetalji>();

                var sql = @"
                    SELECT
                        pl.ID,
                        pl.Sifra,
                        pl.Tezina,
                        pl.Aktivan,
                        pl.DatumKreiranja,
                        pl.DokumentStatus,
                        pl.ArtikalID,
                        a.Naziv as ArtikalNaziv,
                        pl.KomitentID,
                        k.Naziv as KomitentNaziv,
                        pl.PakovanjeID,
                        pak.Naziv as PakovanjeNaziv,
                        pl.EvidencijaRadaID,
                        pl.OtpremnicaStavkaID,
                        pl.PrijemnicaStavkaID,
                        pl.RadniNalogID,
                        pl.OtpremnicaDobavljaca,
                        pl.LotDobavljaca,
                        er.Sifra as EvidencijaRadaSifra,
                        pr.Sifra as PrijemnicaSifra,
                        otp.Sifra as OtpremnicaSifra,
                        rn.Sifra as RadniNalogSifra,
                        si.Broj as SmenskiIzvestajSifra
                    FROM PaletniList pl
                    LEFT JOIN Artikal a ON pl.ArtikalID = a.ID
                    LEFT JOIN Komitent k ON pl.KomitentID = k.ID
                    LEFT JOIN Pakovanje pak ON pl.PakovanjeID = pak.ID
                    LEFT JOIN EvidencijaRada er ON pl.EvidencijaRadaID = er.ID
                    LEFT JOIN PrijemnicaStavka ps ON pl.PrijemnicaStavkaID = ps.ID
                    LEFT JOIN Prijemnica pr ON ps.PrijemnicaID = pr.ID
                    LEFT JOIN OtpremnicaStavka os ON pl.OtpremnicaStavkaID = os.ID
                    LEFT JOIN Otpremnica otp ON os.OtpremnicaID = otp.ID
                    LEFT JOIN RadniNalog rn ON pl.RadniNalogID = rn.ID
                    LEFT JOIN SmenskiIzvestaj si ON er.SmenskiIzvestajID = si.ID
                    WHERE pl.ID IN @IDs
                ";

                var rezultat = await _databaseService.QueryAsync<PaletniListDetalji>(sql, new { IDs = paletniListoviIDs });
                var lista = rezultat.ToList();

                _logger.LogInformation($"[DEBUG] UcitajPovezanePaletneListove: Učitano {lista.Count} PL detalja");

                foreach (var pl in lista)
                {
                    pl.StatusNaziv = DocumentStatus.GetDisplayName(pl.DokumentStatus ?? 0);
                    _logger.LogInformation($"[DEBUG]   - PL ID={pl.ID}, Sifra={pl.Sifra}, Artikal={pl.ArtikalNaziv}");
                    _logger.LogInformation($"[DEBUG]     PrijemnicaStavkaID={pl.PrijemnicaStavkaID?.ToString() ?? "NULL"}, PrijemnicaSifra={pl.PrijemnicaSifra ?? "NULL"}");
                }

                return lista;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajPovezanePaletneListove");
                _logger.LogInformation($"Stack trace: {ex.StackTrace}");
                return new List<PaletniListDetalji>();
            }
        }

        #endregion

        #region Excel Export

        public async Task<byte[]> GenerisiUpstreamExcel(string sifra)
        {
            var sledljivost = await UcitajSledljivostPoRadnomNalogu(sifra);
            if (sledljivost == null)
                throw new Exception($"Radni nalog '{sifra}' nije pronađen.");
            return _excelService.GenerisiUpstreamExcel(sledljivost);
        }

        public async Task<byte[]> GenerisiDownstreamExcel(string sifra)
        {
            var sledljivost = await UcitajSledljivostPoPaletnomListu(sifra);
            if (sledljivost == null)
                throw new Exception($"Paletni list '{sifra}' nije pronađen.");
            return _excelService.GenerisiDownstreamExcel(sledljivost);
        }

        #endregion

        #region HTML Export

        public async Task<byte[]> GenerisiUpstreamHtml(string sifra)
        {
            var sledljivost = await UcitajSledljivostPoRadnomNalogu(sifra);
            if (sledljivost == null)
                throw new Exception($"Radni nalog '{sifra}' nije pronađen.");
            return _htmlService.GenerisiUpstreamHtml(sledljivost);
        }

        public async Task<byte[]> GenerisiDownstreamHtml(string sifra)
        {
            var sledljivost = await UcitajSledljivostPoPaletnomListu(sifra);
            if (sledljivost == null)
                throw new Exception($"Paletni list '{sifra}' nije pronađen.");
            return _htmlService.GenerisiDownstreamHtml(sledljivost);
        }

        #endregion

        #region Prijem Sledljivost

        /// <summary>
        /// Učitava sledljivost za sve paletne listove jedne prijemnice
        /// </summary>
        public async Task<PrijemSledljivostModel?> UcitajPrijemSledljivost(string sifra)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sifra))
                    return null;

                // 1. Učitaj prijemnicu po šifri
                var sqlPrijemnica = @"
                    SELECT
                        pr.ID,
                        pr.Sifra,
                        pr.Datum,
                        pr.Otpremnica,
                        pr.KomitentID,
                        k.Naziv as KomitentNaziv,
                        pr.DokumentStatus
                    FROM Prijemnica pr
                    LEFT JOIN Komitent k ON pr.KomitentID = k.ID
                    WHERE pr.Sifra = @Sifra
                    LIMIT 1
                ";

                var prijemnica = await _databaseService.QueryFirstOrDefaultAsync<dynamic>(sqlPrijemnica, new { Sifra = sifra.Trim() });
                if (prijemnica == null)
                    return null;

                long prijemnicaId = prijemnica.ID;

                var model = new PrijemSledljivostModel
                {
                    PrijemnicaID = prijemnicaId,
                    PrijemnicaSifra = prijemnica.Sifra,
                    Datum = prijemnica.Datum,
                    KomitentNaziv = prijemnica.KomitentNaziv,
                    OtpremnicaDobavljaca = prijemnica.Otpremnica,
                    DokumentStatus = prijemnica.DokumentStatus,
                    StatusNaziv = DocumentStatus.GetDisplayName((int)(prijemnica.DokumentStatus ?? 0))
                };

                // 2. Učitaj sve paletne listove te prijemnice
                var sqlPL = @"
                    SELECT
                        pl.ID,
                        pl.Sifra,
                        pl.Tezina,
                        pl.LotDobavljaca,
                        a.Naziv as ArtikalNaziv
                    FROM PaletniList pl
                    INNER JOIN PrijemnicaStavka ps ON pl.PrijemnicaStavkaID = ps.ID
                    LEFT JOIN Artikal a ON pl.ArtikalID = a.ID
                    WHERE ps.PrijemnicaID = @PrijemnicaID
                    ORDER BY pl.Sifra
                ";

                var paletniListovi = (await _databaseService.QueryAsync<dynamic>(sqlPL, new { PrijemnicaID = prijemnicaId })).ToList();

                if (!paletniListovi.Any())
                    return model;

                var plIDs = paletniListovi.Select(pl => (long)pl.ID).ToList();

                // 3. Učitaj SVE veze u proizvodnji za sve PL odjednom (bez filtera po RN)
                var sqlVeze = @"
                    SELECT
                        erupl.PaletniListID,
                        er.Sifra as EvidencijaSifra,
                        si.Broj as SmenskiIzvestajSifra,
                        rn.Sifra as RadniNalogSifra,
                        k.Naziv as KomitentNaziv
                    FROM EvidencijaRada_UtroseniPaletniListovi erupl
                    INNER JOIN EvidencijaRada er ON erupl.EvidencijaRadaID = er.ID
                    LEFT JOIN SmenskiIzvestaj si ON er.SmenskiIzvestajID = si.ID
                    LEFT JOIN RadniNalog rn ON er.RadniNalogID = rn.ID
                    LEFT JOIN Komitent k ON rn.KomitentID = k.ID
                    WHERE erupl.PaletniListID IN @IDs
                      AND er.Obrisan = 0
                ";

                var vezeRezultat = (await _databaseService.QueryAsync<dynamic>(sqlVeze, new { IDs = plIDs })).ToList();

                // Grupiši veze po PaletniListID
                var vezeDict = new Dictionary<long, (List<string> evidencije, List<string> smene, List<RadniNalogInfo> radniNalozi)>();
                foreach (var row in vezeRezultat)
                {
                    long plId = row.PaletniListID;
                    if (!vezeDict.ContainsKey(plId))
                        vezeDict[plId] = (new List<string>(), new List<string>(), new List<RadniNalogInfo>());

                    string? ev = row.EvidencijaSifra;
                    string? si = row.SmenskiIzvestajSifra;
                    string? rn = row.RadniNalogSifra;
                    string? komitent = row.KomitentNaziv;

                    if (!string.IsNullOrEmpty(ev) && !vezeDict[plId].evidencije.Contains(ev))
                        vezeDict[plId].evidencije.Add(ev);
                    if (!string.IsNullOrEmpty(si) && !vezeDict[plId].smene.Contains(si))
                        vezeDict[plId].smene.Add(si);
                    if (!string.IsNullOrEmpty(rn) && !vezeDict[plId].radniNalozi.Any(x => x.Sifra == rn))
                        vezeDict[plId].radniNalozi.Add(new RadniNalogInfo { Sifra = rn, KomitentNaziv = komitent });
                }

                // 4. Učitaj gotove PL:
                //    - Sirovi PL korišćen u RN (vezeDict.radniNalozi)
                //    - Za taj RN uzmi sve gotove PL (pl.RadniNalogID = rn.ID)
                //    - Gotovi PL koji imaju sirovi PL u svom PaletniListoviPracenje
                //      (plp.PaletniListID = gotoviPL.ID AND plp.TPaletniListID = siroviPL.ID)
                var sqlGotovi = @"
                    SELECT
                        plp.TPaletniListID as SiroviPLID,
                        gpl.Sifra as PLSifra,
                        a.Naziv as ArtikalNaziv,
                        gpl.Tezina,
                        otp.Sifra as OtpremnicaSifra,
                        k.Naziv as KomitentNaziv
                    FROM PaletniList gpl
                    INNER JOIN RadniNalog rn ON gpl.RadniNalogID = rn.ID
                    INNER JOIN EvidencijaRada er ON er.RadniNalogID = rn.ID
                    INNER JOIN EvidencijaRada_UtroseniPaletniListovi erupl ON erupl.EvidencijaRadaID = er.ID
                        AND erupl.PaletniListID IN @SiroviIDs
                    INNER JOIN PaletniListoviPracenje plp ON plp.PaletniListID = gpl.ID
                        AND plp.TPaletniListID = erupl.PaletniListID
                    LEFT JOIN Artikal a ON gpl.ArtikalID = a.ID
                    LEFT JOIN Otpremnica otp ON otp.RadniNalogID = rn.ID
                    LEFT JOIN Komitent k ON otp.KomitentID = k.ID
                    WHERE er.Obrisan = 0
                    ORDER BY gpl.Sifra
                ";

                var gotoviRez = (await _databaseService.QueryAsync<dynamic>(sqlGotovi, new { SiroviIDs = plIDs })).ToList();

                var gotoviDict = new Dictionary<long, List<GotoviPLInfo>>();
                foreach (var row in gotoviRez)
                {
                    long siroviId = row.SiroviPLID;
                    if (!gotoviDict.ContainsKey(siroviId))
                        gotoviDict[siroviId] = new List<GotoviPLInfo>();
                    string gSifra = row.PLSifra;
                    if (!gotoviDict[siroviId].Any(x => x.Sifra == gSifra))
                        gotoviDict[siroviId].Add(new GotoviPLInfo
                        {
                            Sifra = gSifra,
                            ArtikalNaziv = row.ArtikalNaziv,
                            Tezina = row.Tezina,
                            OtpremnicaSifra = row.OtpremnicaSifra,
                            KomitentNaziv = row.KomitentNaziv
                        });
                }

                // 5. Složi redove
                foreach (var pl in paletniListovi)
                {
                    long plId = pl.ID;
                    vezeDict.TryGetValue(plId, out var veze);
                    gotoviDict.TryGetValue(plId, out var gotovi);

                    model.PaletniListovi.Add(new PrijemPaletniListRow
                    {
                        PaletniListID = plId,
                        Sifra = pl.Sifra,
                        ArtikalNaziv = pl.ArtikalNaziv,
                        Tezina = pl.Tezina,
                        LotDobavljaca = pl.LotDobavljaca,
                        EvidencijeRada = veze.evidencije ?? new List<string>(),
                        SmenskiIzvestaji = veze.smene ?? new List<string>(),
                        RadniNalozi = veze.radniNalozi ?? new List<RadniNalogInfo>(),
                        GotoviPaletniListovi = gotovi ?? new List<GotoviPLInfo>()
                    });
                }

                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajPrijemSledljivost");
                return null;
            }
        }

        public async Task<byte[]> GenerisiPrijemExcel(string sifra)
        {
            var model = await UcitajPrijemSledljivost(sifra);
            if (model == null)
                throw new Exception($"Prijemnica '{sifra}' nije pronađena.");
            return _excelService.GenerisiPrijemExcel(model);
        }

        public async Task<byte[]> GenerisiPrijemHtml(string sifra)
        {
            var model = await UcitajPrijemSledljivost(sifra);
            if (model == null)
                throw new Exception($"Prijemnica '{sifra}' nije pronađena.");
            return _htmlService.GenerisiPrijemHtml(model);
        }

        #endregion
    }
}
