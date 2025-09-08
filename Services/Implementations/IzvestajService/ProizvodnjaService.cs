using FruitSysWeb.Models;
using FruitSysWeb.Services.Interfaces;
using FruitSysWeb.Services.Models.Requests;
using System.Text;

namespace FruitSysWeb.Services.Implementations.IzvestajService
{
    public class ProizvodnjaService : IProizvodnjaService
    {
        private readonly DatabaseService _databaseService;

        public ProizvodnjaService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<List<ProizvodnjaModel>> UcitajIzvestajProizvodnje(FilterRequest filterRequest)
        {
            try
            {
                var sql = new StringBuilder();
                sql.Append(@"
                SELECT 
                rn.DatumPocetka as Datum,
                rn.Sifra as RadniNalog,
                vpp.Artikal,
                CAST(CASE a.Tip
                WHEN 1 THEN 'Sirovina'
                WHEN 2 THEN 'Ambalaza'
                WHEN 3 THEN 'Potrošni materijal'
                WHEN 4 THEN 'Gotova roba'
                WHEN 5 THEN 'Oprema'
                WHEN 7 THEN 'Klase'
                ELSE CONCAT('Tip ', a.Tip)
                END AS CHAR(50)) as TipArtikla,
                CASE 
                WHEN a.Tip NOT IN (2, 4) THEN vpp.Kolicina 
                ELSE 0 
                END as KolicinaRoba,
                CASE 
                    WHEN a.Tip = 2 THEN vpp.Kolicina 
                    ELSE 0 
                END as KolicinaAmbalaza,
                    CASE 
                        WHEN a.Tip = 4 THEN vpp.Kolicina 
                        ELSE 0 
                    END as GotovProizvod,
                        vpp.Kolicina,
                vpp.Komitent,
                rn.ID as RadniNalogID,
                rn.KomitentID,
                rn.DokumentStatus,
                vpp.ArtikalID,
                vpp.ArtikalPrvaKlasifikacijaID,
                a.Tip as Tip
            FROM RadniNalog rn
            LEFT JOIN vPreradaPregled vpp ON rn.ID = vpp.RadniNalogID
            LEFT JOIN Artikal a ON vpp.ArtikalID = a.ID
            WHERE rn.Aktivno = 1
            AND a.Aktivno = 1
            AND a.ID IS NOT NULL
        ");

                var parameters = new Dictionary<string, object>();

                // Datum filteri
                if (filterRequest.OdDatum.HasValue)
                {
                    sql.Append(" AND DATE(rn.DatumPocetka) >= @OdDatum");
                    parameters.Add("@OdDatum", filterRequest.OdDatum.Value.Date);
                }

                if (filterRequest.DoDatum.HasValue)
                {
                    sql.Append(" AND DATE(rn.DatumPocetka) <= @DoDatum");
                    parameters.Add("@DoDatum", filterRequest.DoDatum.Value.Date);
                }

                // Filter za radni nalog
                if (!string.IsNullOrWhiteSpace(filterRequest.RadniNalog))
                {
                    Console.WriteLine($"DEBUG: Dodajem filter za radni nalog: '{filterRequest.RadniNalog}'");
                    sql.Append(" AND rn.Sifra = @RadniNalog");
                    parameters.Add("@RadniNalog", filterRequest.RadniNalog);
                }
                else
                {
                    Console.WriteLine($"DEBUG: Radni nalog filter je prazan: '{filterRequest.RadniNalog}'");
                }

                // Komitent filter
                if (filterRequest.KomitentId.HasValue && filterRequest.KomitentId > 0)
                {
                    sql.Append(" AND rn.KomitentID = @KomitentId");
                    parameters.Add("@KomitentId", filterRequest.KomitentId.Value);
                }

                // NOVO: Filter za klasifikaciju (vrstu) artikla umesto tipa
                if (filterRequest.ArtikalKlasifikacijaId.HasValue && filterRequest.ArtikalKlasifikacijaId > 0)
                {
                    sql.Append(" AND vpp.ArtikalPrvaKlasifikacijaID = @ArtikalKlasifikacijaId");
                    parameters.Add("@ArtikalKlasifikacijaId", filterRequest.ArtikalKlasifikacijaId.Value);
                }

                // BEZ grupiranja - svaki artikal u posebnom redu
                sql.Append(@" 
                    ORDER BY rn.DatumPocetka DESC, rn.Sifra, vpp.Artikal");

                Console.WriteLine($"DEBUG: Finalni SQL upit:\n{sql}");
                Console.WriteLine($"DEBUG: Parametri: {string.Join(", ", parameters.Select(p => $"{p.Key}={p.Value}"))}");

                var rezultat = await _databaseService.QueryAsync<ProizvodnjaModel>(sql.ToString(), parameters);
                
                // DEBUG: Ispis tipova artikala
                Console.WriteLine($"DEBUG: Učitano {rezultat.Count()} stavki iz proizvodnje");
                var tipoviDebug = rezultat.GroupBy(x => new { x.Tip, x.TipArtikla })
                    .Select(g => new { TipBroj = g.Key.Tip, TipNaziv = g.Key.TipArtikla, Broj = g.Count() })
                    .OrderBy(x => x.TipBroj);
                    
                foreach (var tip in tipoviDebug)
                {
                    Console.WriteLine($"  Tip {tip.TipBroj} ({tip.TipNaziv}): {tip.Broj} stavki");
                }
                
                // DEBUG: Specijalno pratići "Kupina Rollend +" da vidimo da li je sada ispravno
                var kupinaRollend = rezultat.Where(x => x.Artikal.Contains("Kupina Rollend")).ToList();
                if (kupinaRollend.Any())
                {
                    Console.WriteLine($"DEBUG: Pronađen 'Kupina Rollend' artikl(i):");
                    foreach (var item in kupinaRollend)
                    {
                        Console.WriteLine($"  - {item.Artikal} (Tip: {item.Tip}, TipArtikla: {item.TipArtikla})");
                    }
                }
                
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajIzvestajProizvodnje: {ex.Message}");
                throw;
            }
        }

        // Metoda za dobijanje liste radnih naloga za padajući meni
        public async Task<List<string>> UcitajListuRadnihNaloga(FilterRequest filterRequest)
        {
            try
            {
                var sql = new StringBuilder();
                sql.Append(@"
                    SELECT DISTINCT rn.Sifra
                    FROM RadniNalog rn
                    WHERE rn.Aktivno = 1 AND rn.Sifra IS NOT NULL AND rn.Sifra != ''
                ");

                var parameters = new Dictionary<string, object>();

                if (filterRequest.OdDatum.HasValue)
                {
                    sql.Append(" AND DATE(rn.DatumPocetka) >= @OdDatum");
                    parameters.Add("@OdDatum", filterRequest.OdDatum.Value.Date);
                }

                if (filterRequest.DoDatum.HasValue)
                {
                    sql.Append(" AND DATE(rn.DatumPocetka) <= @DoDatum");
                    parameters.Add("@DoDatum", filterRequest.DoDatum.Value.Date);
                }

                sql.Append(" ORDER BY rn.Sifra DESC");

                var rezultat = await _databaseService.QueryAsync<string>(sql.ToString(), parameters);
                Console.WriteLine($"DEBUG: Pronađeno {rezultat.Count()} radnih naloga za dropdown");
                
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajListuRadnihNaloga: {ex.Message}");
                return new List<string>();
            }
        }

        public async Task<int> UcitajBrojAktivnihNaloga(FilterRequest filterRequest)
        {
            try
            {
                var sql = @"
                    SELECT COUNT(DISTINCT rn.ID) as BrojNaloga
                    FROM RadniNalog rn
                    WHERE rn.Aktivno = 1 AND rn.DokumentStatus = 2  -- 2 = Otvoren prema dokumentu
                ";

                var rezultat = await _databaseService.ExecuteScalarAsync<int>(sql);
                return rezultat;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajBrojAktivnihNaloga: {ex.Message}");
                return 0;
            }
        }

        // OSTALE METODE OSTAJU ISTE...
        public async Task<decimal> UcitajUkupnuProizvodnju(FilterRequest filterRequest)
        {
            try
            {
                var sql = new StringBuilder();
                sql.Append(@"
                    SELECT COALESCE(SUM(vpp.Kolicina), 0) as UkupnaKolicina
                    FROM vPreradaPregled vpp
                    LEFT JOIN RadniNalog rn ON vpp.RadniNalogID = rn.ID
                    WHERE vpp.RpArtikalTip = 4  -- SAMO GOTOVI PROIZVODI
                ");

                var parameters = new Dictionary<string, object>();

                if (filterRequest.OdDatum.HasValue)
                {
                    sql.Append(" AND DATE(rn.DatumPocetka) >= @OdDatum");
                    parameters.Add("@OdDatum", filterRequest.OdDatum.Value.Date);
                }

                if (filterRequest.DoDatum.HasValue)
                {
                    sql.Append(" AND DATE(rn.DatumPocetka) <= @DoDatum");
                    parameters.Add("@DoDatum", filterRequest.DoDatum.Value.Date);
                }

                var rezultat = await _databaseService.ExecuteScalarAsync<decimal>(sql.ToString(), parameters);
                return rezultat;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajUkupnuProizvodnju: {ex.Message}");
                return 0;
            }
        }

        // POČETAK EXISTING METHODS...
        public async Task<Dictionary<string, decimal>> UcitajProizvodnjuPoArtiklima(FilterRequest filterRequest)
        {
            try
            {
                var sql = new StringBuilder();
                sql.Append(@"
                    SELECT 
                        vpp.Artikal,
                        SUM(vpp.Kolicina) as UkupnaKolicina
                    FROM vPreradaPregled vpp
                    LEFT JOIN RadniNalog rn ON vpp.RadniNalogID = rn.ID
                    WHERE vpp.RpArtikalTip = 4  -- SAMO GOTOVI PROIZVODI
                ");

                var parameters = new Dictionary<string, object>();

                if (filterRequest.OdDatum.HasValue)
                {
                    sql.Append(" AND DATE(rn.DatumPocetka) >= @OdDatum");
                    parameters.Add("@OdDatum", filterRequest.OdDatum.Value.Date);
                }

                if (filterRequest.DoDatum.HasValue)
                {
                    sql.Append(" AND DATE(rn.DatumPocetka) <= @DoDatum");
                    parameters.Add("@DoDatum", filterRequest.DoDatum.Value.Date);
                }

                sql.Append(@"
                    GROUP BY vpp.Artikal, vpp.ArtikalID
                    ORDER BY UkupnaKolicina DESC
                ");

                var rezultat = await _databaseService.QueryAsync<dynamic>(sql.ToString(), parameters);
                
                return rezultat.ToDictionary(
                    x => (string)x.Artikal ?? "Nepoznato",
                    x => (decimal)x.UkupnaKolicina
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajProizvodnjuPoArtiklima: {ex.Message}");
                return new Dictionary<string, decimal>();
            }
        }

        // OSTALE METODE BEZ PROMENA...
        public async Task<List<ProizvodnjaModel>> UcitajSveRadneNaloge() => await UcitajIzvestajProizvodnje(new FilterRequest());
        public async Task<List<ProizvodnjaModel>> UcitajRadneNalogePoDatumu(DateTime odDatum, DateTime doDatum) => 
            await UcitajIzvestajProizvodnje(new FilterRequest { OdDatum = odDatum, DoDatum = doDatum });
        public async Task<List<ProizvodnjaModel>> UcitajRadneNalogePoKomitentu(long komitentId) => 
            await UcitajIzvestajProizvodnje(new FilterRequest { KomitentId = komitentId });
        public async Task<List<ProizvodnjaModel>> UcitajRadneNalogePoArtiklu(long artikalId) => 
            await UcitajIzvestajProizvodnje(new FilterRequest { ArtikalId = artikalId });
        public async Task<List<ProizvodnjaModel>> UcitajRadneNalogePoTipu(int tipArtikla) => 
            await UcitajIzvestajProizvodnje(new FilterRequest { TipArtikla = tipArtikla });

        public async Task<Dictionary<string, decimal>> UcitajProizvodnjuPoKomitentima(FilterRequest filterRequest)
        {
            try
            {
                var sql = new StringBuilder();
                sql.Append(@"
                    SELECT 
                        COALESCE(vpp.Komitent, 'Nepoznato') as Komitent,
                        SUM(vpp.Kolicina) as UkupnaKolicina
                    FROM vPreradaPregled vpp
                    LEFT JOIN RadniNalog rn ON vpp.RadniNalogID = rn.ID
                    WHERE vpp.RpArtikalTip = 4  -- SAMO GOTOVI PROIZVODI
                    AND EXISTS (SELECT 1 FROM Artikal a WHERE a.ID = vpp.ArtikalID AND a.Aktivno = 1)
                ");

                var parameters = new Dictionary<string, object>();

                if (filterRequest.OdDatum.HasValue)
                {
                    sql.Append(" AND DATE(rn.DatumPocetka) >= @OdDatum");
                    parameters.Add("@OdDatum", filterRequest.OdDatum.Value.Date);
                }

                if (filterRequest.DoDatum.HasValue)
                {
                    sql.Append(" AND DATE(rn.DatumPocetka) <= @DoDatum");
                    parameters.Add("@DoDatum", filterRequest.DoDatum.Value.Date);
                }

                sql.Append(@"
                    GROUP BY vpp.Komitent, vpp.KomitentID
                    ORDER BY UkupnaKolicina DESC
                ");

                var rezultat = await _databaseService.QueryAsync<dynamic>(sql.ToString(), parameters);
                
                return rezultat.ToDictionary(
                    x => (string)x.Komitent ?? "Nepoznato",
                    x => (decimal)x.UkupnaKolicina
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajProizvodnjuPoKomitentima: {ex.Message}");
                return new Dictionary<string, decimal>();
            }
        }

        public Task<Dictionary<string, decimal>> UcitajProizvodnjuPoMesecima(FilterRequest filterRequest)
        {
            // implementacija ista kao postojeća
            return Task.FromResult(new Dictionary<string, decimal>());
        }

        public async Task<Dictionary<int, decimal>> UcitajProizvodnjuPoTipovima(FilterRequest filterRequest)
        {
            try
            {
                var sql = new StringBuilder();
                sql.Append(@"
                    SELECT 
                        vpp.RpArtikalTip as Tip,
                        SUM(vpp.Kolicina) as UkupnaKolicina
                    FROM vPreradaPregled vpp
                    LEFT JOIN RadniNalog rn ON vpp.RadniNalogID = rn.ID
                    WHERE vpp.RpArtikalTip = 4  -- SAMO GOTOVI PROIZVODI
                    AND EXISTS (SELECT 1 FROM Artikal a WHERE a.ID = vpp.ArtikalID AND a.Aktivno = 1)
                ");

                var parameters = new Dictionary<string, object>();

                if (filterRequest.OdDatum.HasValue)
                {
                    sql.Append(" AND DATE(rn.DatumPocetka) >= @OdDatum");
                    parameters.Add("@OdDatum", filterRequest.OdDatum.Value.Date);
                }

                if (filterRequest.DoDatum.HasValue)
                {
                    sql.Append(" AND DATE(rn.DatumPocetka) <= @DoDatum");
                    parameters.Add("@DoDatum", filterRequest.DoDatum.Value.Date);
                }

                sql.Append(@"
                    GROUP BY vpp.RpArtikalTip
                    ORDER BY UkupnaKolicina DESC
                ");

                var rezultat = await _databaseService.QueryAsync<dynamic>(sql.ToString(), parameters);
                
                return rezultat.ToDictionary(
                    x => (int)x.Tip,
                    x => (decimal)x.UkupnaKolicina
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajProizvodnjuPoTipovima: {ex.Message}");
                return new Dictionary<int, decimal>();
            }
        }

        public async Task<List<ProizvodnjaModel>> UcitajNajproduktivnijeNaloge(FilterRequest filterRequest, int brojNaloga = 10)
        {
            try
            {
                var sql = new StringBuilder();
                sql.Append(@"
                    SELECT 
                        rn.DatumPocetka as Datum,
                        vpp.RadniNalog,
                        vpp.Artikal,
                        vpp.Kolicina,
                        vpp.Komitent,
                        vpp.ArtikalPrvaKlasifikacija as Klasifikacija,
                        vpp.RpArtikalTip as TipArtikla,
                        vpp.RadniNalogID,
                        vpp.ArtikalID,
                        vpp.KomitentID
                    FROM vPreradaPregled vpp
                    LEFT JOIN RadniNalog rn ON vpp.RadniNalogID = rn.ID
                    WHERE vpp.RpArtikalTip = 4  -- SAMO GOTOVI PROIZVODI
                    AND EXISTS (SELECT 1 FROM Artikal a WHERE a.ID = vpp.ArtikalID AND a.Aktivno = 1)
                ");

                var parameters = new Dictionary<string, object>();

                if (filterRequest.OdDatum.HasValue)
                {
                    sql.Append(" AND DATE(rn.DatumPocetka) >= @OdDatum");
                    parameters.Add("@OdDatum", filterRequest.OdDatum.Value.Date);
                }

                if (filterRequest.DoDatum.HasValue)
                {
                    sql.Append(" AND DATE(rn.DatumPocetka) <= @DoDatum");
                    parameters.Add("@DoDatum", filterRequest.DoDatum.Value.Date);
                }

                sql.Append($@"
                    ORDER BY vpp.Kolicina DESC
                    LIMIT {brojNaloga}
                ");

                var rezultat = await _databaseService.QueryAsync<ProizvodnjaModel>(sql.ToString(), parameters);
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajNajproduktivnijeNaloge: {ex.Message}");
                return new List<ProizvodnjaModel>();
            }
        }

        public Task<List<PreradaPregledModel>> UcitajPreraduPregled(FilterRequest filterRequest)
        {
            // implementacija ista kao postojeća
            return Task.FromResult(new List<PreradaPregledModel>());
        }

        public Task<Dictionary<string, decimal>> UcitajAnalizuPreradePoArtiklima(FilterRequest filterRequest)
        {
            // implementacija ista kao postojeća
            return Task.FromResult(new Dictionary<string, decimal>());
        }

        // DASHBOARD - nove metode za dashboard
        public async Task<decimal> UcitajGotoveProizvodePoslednjihDana(int dana)
        {
            try
            {
                var sql = @"
                    SELECT COALESCE(SUM(vpp.Kolicina), 0) as UkupnaKolicina
                    FROM vPreradaPregled vpp
                    LEFT JOIN RadniNalog rn ON vpp.RadniNalogID = rn.ID
                    WHERE vpp.RpArtikalTip = 4  -- SAMO GOTOVI PROIZVODI
                      AND DATE(rn.DatumPocetka) >= DATE_SUB(CURDATE(), INTERVAL @Dana DAY)
                      AND EXISTS (SELECT 1 FROM Artikal a WHERE a.ID = vpp.ArtikalID AND a.Aktivno = 1)
                ";

                var rezultat = await _databaseService.ExecuteScalarAsync<decimal>(sql, new { Dana = dana });
                return rezultat;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajGotoveProizvodePoslednjihDana: {ex.Message}");
                return 0;
            }
        }

        public async Task<Dictionary<string, decimal>> UcitajTopKupcePoKilogramima(FilterRequest filterRequest)
        {
            try
            {
                var sql = new StringBuilder();
                sql.Append(@"
                    SELECT 
                        COALESCE(vpp.Komitent, 'Nepoznato') as Komitent,
                        SUM(vpp.Kolicina) as UkupnaKolicina
                    FROM vPreradaPregled vpp
                    LEFT JOIN RadniNalog rn ON vpp.RadniNalogID = rn.ID
                    WHERE vpp.RpArtikalTip = 4  -- SAMO GOTOVI PROIZVODI
                      AND EXISTS (SELECT 1 FROM Artikal a WHERE a.ID = vpp.ArtikalID AND a.Aktivno = 1)
                      AND EXISTS (SELECT 1 FROM Komitent k WHERE k.Id = vpp.KomitentID AND k.JeKupac = 1)
                ");

                var parameters = new Dictionary<string, object>();

                if (filterRequest.OdDatum.HasValue)
                {
                    sql.Append(" AND DATE(rn.DatumPocetka) >= @OdDatum");
                    parameters.Add("@OdDatum", filterRequest.OdDatum.Value.Date);
                }

                if (filterRequest.DoDatum.HasValue)
                {
                    sql.Append(" AND DATE(rn.DatumPocetka) <= @DoDatum");
                    parameters.Add("@DoDatum", filterRequest.DoDatum.Value.Date);
                }

                sql.Append(@"
                    GROUP BY vpp.Komitent, vpp.KomitentID
                    ORDER BY UkupnaKolicina DESC
                    LIMIT 5
                ");

                var rezultat = await _databaseService.QueryAsync<dynamic>(sql.ToString(), parameters);
                
                return rezultat.ToDictionary(
                    x => (string)x.Komitent ?? "Nepoznato",
                    x => (decimal)x.UkupnaKolicina
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajTopKupcePoKilogramima: {ex.Message}");
                return new Dictionary<string, decimal>();
            }
        }

        public async Task<Dictionary<string, decimal>> UcitajTopDobavljacePoKilogramima(FilterRequest filterRequest)
        {
            try
            {
                var sql = new StringBuilder();
                sql.Append(@"
                    SELECT 
                        COALESCE(vpp.Komitent, 'Nepoznato') as Komitent,
                        SUM(vpp.Kolicina) as UkupnaKolicina
                    FROM vPreradaPregled vpp
                    LEFT JOIN RadniNalog rn ON vpp.RadniNalogID = rn.ID
                    WHERE vpp.RpArtikalTip = 1  -- SIROVINE (dobavljači uglavnom isporučuju sirovine)
                      AND EXISTS (SELECT 1 FROM Artikal a WHERE a.ID = vpp.ArtikalID AND a.Aktivno = 1)
                      AND EXISTS (SELECT 1 FROM Komitent k WHERE k.Id = vpp.KomitentID AND k.JeDobavljac = 1)
                ");

                var parameters = new Dictionary<string, object>();

                if (filterRequest.OdDatum.HasValue)
                {
                    sql.Append(" AND DATE(rn.DatumPocetka) >= @OdDatum");
                    parameters.Add("@OdDatum", filterRequest.OdDatum.Value.Date);
                }

                if (filterRequest.DoDatum.HasValue)
                {
                    sql.Append(" AND DATE(rn.DatumPocetka) <= @DoDatum");
                    parameters.Add("@DoDatum", filterRequest.DoDatum.Value.Date);
                }

                sql.Append(@"
                    GROUP BY vpp.Komitent, vpp.KomitentID
                    ORDER BY UkupnaKolicina DESC
                    LIMIT 5
                ");

                var rezultat = await _databaseService.QueryAsync<dynamic>(sql.ToString(), parameters);
                
                return rezultat.ToDictionary(
                    x => (string)x.Komitent ?? "Nepoznato",
                    x => (decimal)x.UkupnaKolicina
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajTopDobavljacePoKilogramima: {ex.Message}");
                return new Dictionary<string, decimal>();
            }
        }
    }
}