using FruitSysWeb.Models;
using FruitSysWeb.Services.Interfaces;
using FruitSysWeb.Services.Models.Requests;
using Dapper;
using System.Text;

namespace FruitSysWeb.Services.Implementations.IzvestajService
{
    public class PreradaService : IPreradaService
    {
        private readonly DatabaseService _databaseService;

        public PreradaService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        #region GLAVNI IZVEŠTAJI - NOVI MODELI

        /// <summary>
        /// Učitaj izveštaj radnih naloga AGREGIRANO PO SMENSKOM IZVEŠTAJU
        /// Sve evidencije rada jednog smenskog izveštaja se sabiru u jedan red
        /// </summary>
        public async Task<List<RadniNalogIzvestajModel>> UcitajRadniNalogIzvestaj(FilterRequest filter)
        {
            try
            {
                Console.WriteLine("🚀 SQL UPIT - Učitavam radni nalog izveštaj AGREGIRANO PO SMENSKOM IZVEŠTAJU...");

                var sql = @"
                    SELECT 
                        MIN(er.ID) as ID,
                        MIN(er.Sifra) as SifraEvidencije,
                        COALESCE(rn.Sifra, 'N/A') as RadniNalog,
                        si.Datum,
                        si.DokumentStatus,
                        COALESCE(
                            TRIM(TRAILING '+' FROM TRIM(TRAILING '-' FROM a.Naziv)),
                            'Gotov proizvod'
                        ) as VrstaArtikla,
                        COALESCE(k.Naziv, 'Nepoznato') as Komitent,
                        SUM(er.BrojRadnika) as BrojRadnika,
                        SUM(er.BrojRadnihSati) as BrojRadnihSati,
                        SUM(er.CenaKostanjaDirektanRad) as TrosakPoRadnomNalogu,
                        COALESCE(MAX(verm.Mnozilac) * 100, 75.0) as ProcenatIskoriscenja,
                        er.RadniNalogID,
                        er.SmenskiIzvestajID,
                        MIN(er.RadniProcesID) as RadniProcesID,
                        MIN(er.RezijaID) as RezijaID,
                        COALESCE(rn.KomitentID, 0) as KomitentID,
                        MIN(er.RezijskiProces) as RezijskiProces,
                        MIN(er.CenaSataPoReziji) as CenaSataPoReziji,
                        MIN(er.Kreirano) as Kreirano,
                        MAX(er.Azurirano) as Azurirano,
                        MAX(er.Version) as Version,
                        0 as Obrisan,
                        COALESCE(si.Broj, 'N/A') as BrojIzvestaja,
                        COALESCE(si.Smena, 1) as Smena
                    FROM EvidencijaRada er
                    INNER JOIN SmenskiIzvestaj si ON er.SmenskiIzvestajID = si.ID
                    LEFT JOIN RadniNalog rn ON er.RadniNalogID = rn.ID
                    LEFT JOIN Komitent k ON rn.KomitentID = k.ID
                    LEFT JOIN vEvidencijaRadaPreradaMnozilac verm ON er.ID = verm.EvidencijaRadaID
                    LEFT JOIN (
                        SELECT DISTINCT 
                            vpp.RadniNalogID, 
                            TRIM(TRAILING '+' FROM TRIM(TRAILING '-' FROM a.Naziv)) as Naziv
                        FROM vPreradaPregled vpp
                        LEFT JOIN Artikal a ON vpp.ArtikalID = a.ID
                        WHERE a.MagacinID = 6
                        GROUP BY vpp.RadniNalogID, TRIM(TRAILING '+' FROM TRIM(TRAILING '-' FROM a.Naziv))
                    ) a ON rn.ID = a.RadniNalogID
                    WHERE er.Obrisan = 0
                      AND er.DirektanRadObracunat = 1
                      AND rn.Aktivno = 1
                      AND rn.Sifra NOT LIKE 'ST-%'
                      AND rn.Sifra IS NOT NULL
                      AND si.Broj IS NOT NULL";

                var parameters = new DynamicParameters();

                if (filter.OdDatum.HasValue)
                {
                    sql += " AND si.Datum >= @OdDatum";
                    parameters.Add("@OdDatum", filter.OdDatum.Value);
                }

                if (filter.DoDatum.HasValue)
                {
                    sql += " AND si.Datum <= @DoDatum";
                    parameters.Add("@DoDatum", filter.DoDatum.Value);
                }

                if (!string.IsNullOrEmpty(filter.RadniNalog))
                {
                    sql += " AND rn.Sifra LIKE @RadniNalog";
                    parameters.Add("@RadniNalog", $"%{filter.RadniNalog}%");
                }

                if (filter.KomitentId.HasValue)
                {
                    sql += " AND rn.KomitentID = @KomitentId";
                    parameters.Add("@KomitentId", filter.KomitentId.Value);
                }

                if (filter.DokumentStatus.HasValue)
                {
                    sql += " AND si.DokumentStatus = @DokumentStatus";
                    parameters.Add("@DokumentStatus", filter.DokumentStatus.Value);
                }

                sql += " AND si.Datum >= DATE_SUB(NOW(), INTERVAL 6 MONTH)";

                // ✅ KLJUČNO: GROUP BY po SmenskiIzvestajID i RadniNalogID
                // ✅ VAZNO: Grupišemo po očišćenom nazivu artikla (bez + i -)
                sql += " GROUP BY er.SmenskiIzvestajID, er.RadniNalogID, si.Datum, si.DokumentStatus, si.Broj, si.Smena, rn.Sifra, rn.KomitentID, k.Naziv, TRIM(TRAILING '+' FROM TRIM(TRAILING '-' FROM a.Naziv))";
                sql += " ORDER BY si.Datum DESC, si.Broj LIMIT 500";

                Console.WriteLine($"🔍 Izvršavam SQL (agregacija po smenskom izveštaju)...");

                var rezultat = await _databaseService.QueryAsync<RadniNalogIzvestajModel>(sql, parameters);

                Console.WriteLine($"✅ SQL završen, dobijeno {rezultat?.Count() ?? 0} AGREGIRANIH redova");
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SQL GREŠKA: {ex.Message}");
                Console.WriteLine($"❌ STACK TRACE: {ex.StackTrace}");
                Console.WriteLine($"❌ INNER: {ex.InnerException?.Message}");
                return new List<RadniNalogIzvestajModel>();
            }
        }

        public async Task<List<EvidencijeIzvestajModel>> UcitajEvidencijeIzvestaj(FilterRequest filterRequest)
        {
            try
            {
                var sql = new StringBuilder();
                sql.Append(@"
                    SELECT
                        er.Sifra as BrojEvidencije,
                        COALESCE(a.Naziv, 'Nepoznato') as VrstaProizvoda,
                        COALESCE(pp.Naziv, 'Nepoznato') as ProizvodniProces,
                        COALESCE(rp.Naziv, 'Nepoznato') as RadniProces,
                        COALESCE(k.Naziv, 'Nepoznato') as Smenovoda,
                        SUM(er.BrojRadnika) as BrojRadnika,
                        er.BrojRadnihSati as RadniSati,
                        COALESCE(vpp.Kolicina, 0) as Kolicina,
                        CASE
                            WHEN er.BrojRadnihSati > 0 THEN (vpp.Kolicina / er.BrojRadnihSati) * 100
                            ELSE 0
                        END as Efikasnost,
                        er.CenaKostanjaDirektanRad,
                        CASE
                            WHEN rn.DokumentStatus = 2 THEN 'Otvoren'
                            WHEN rn.DokumentStatus = 3 THEN 'Zaključen'
                            WHEN rn.DokumentStatus = 4 THEN 'Storno'
                            ELSE 'Nepoznato'
                        END as Status
                    FROM EvidencijaRada er
                    LEFT JOIN RadniNalog rn ON er.RadniNalogID = rn.ID
                    LEFT JOIN SmenskiIzvestaj si ON er.SmenskiIzvestajID = si.ID
                    LEFT JOIN Komitent k ON si.PoslovodjaID = k.ID
                    LEFT JOIN vPreradaPregled vpp ON rn.ID = vpp.RadniNalogID
                    LEFT JOIN Artikal a ON vpp.ArtikalID = a.ID
                    LEFT JOIN ProizvodniProces pp ON er.ProizvodniProcesID = pp.ID
                    LEFT JOIN RadniProces rp ON er.RadniProcesID = rp.ID
                    WHERE er.Obrisan = 0
                      AND rn.Aktivno = 1
                      AND a.MagacinID = 6
                ");

                var parameters = new DynamicParameters();

                if (filterRequest.OdDatum.HasValue)
                {
                    sql.Append(" AND DATE(er.Datum) >= @OdDatum");
                    parameters.Add("@OdDatum", filterRequest.OdDatum.Value.Date);
                }

                if (filterRequest.DoDatum.HasValue)
                {
                    sql.Append(" AND DATE(er.Datum) <= @DoDatum");
                    parameters.Add("@DoDatum", filterRequest.DoDatum.Value.Date);
                }

                sql.Append(" GROUP BY er.ID ORDER BY er.Datum DESC LIMIT 200");

                var rezultat = await _databaseService.QueryAsync<EvidencijeIzvestajModel>(sql.ToString(), parameters);
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajEvidencijeIzvestaj: {ex.Message}");
                return new List<EvidencijeIzvestajModel>();
            }
        }

        public async Task<StatistikeModel> UcitajStatistike(FilterRequest filterRequest)
        {
            try
            {
                var sql = new StringBuilder();
                sql.Append(@"
                    SELECT 
                        COALESCE(SUM(vpp.Kolicina), 0) as UkupnaProizvodnja,
                        COALESCE(SUM(er.BrojRadnihSati), 0) as UkupniRadniSati,
                        COALESCE(SUM(er.CenaKostanjaDirektanRad), 0) as UkupniTrosakRada,
                        COALESCE(AVG(
                            CASE 
                                WHEN er.BrojRadnihSati > 0 THEN (vpp.Kolicina / er.BrojRadnihSati) * 100
                                ELSE 0 
                            END
                        ), 0) as ProsecnaEfikasnost,
                        COUNT(DISTINCT er.ID) as BrojEvidencija,
                        COUNT(DISTINCT rn.ID) as BrojRadnihNaloga
                    FROM EvidencijaRada er
                    LEFT JOIN RadniNalog rn ON er.RadniNalogID = rn.ID
                    LEFT JOIN vPreradaPregled vpp ON rn.ID = vpp.RadniNalogID
                    LEFT JOIN Artikal a ON vpp.ArtikalID = a.ID
                    WHERE er.Obrisan = 0
                      AND rn.Aktivno = 1
                      AND a.MagacinID = 6
                ");

                var parameters = new DynamicParameters();

                if (filterRequest.OdDatum.HasValue)
                {
                    sql.Append(" AND DATE(er.Datum) >= @OdDatum");
                    parameters.Add("@OdDatum", filterRequest.OdDatum.Value.Date);
                }

                if (filterRequest.DoDatum.HasValue)
                {
                    sql.Append(" AND DATE(er.Datum) <= @DoDatum");
                    parameters.Add("@DoDatum", filterRequest.DoDatum.Value.Date);
                }

                var rezultat = await _databaseService.QuerySingleOrDefaultAsync<StatistikeModel>(sql.ToString(), parameters);
                return rezultat ?? new StatistikeModel();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajStatistike: {ex.Message}");
                return new StatistikeModel();
            }
        }

        #endregion

        #region SMENSKI IZVESTAJI METODE

        public async Task<List<string>> UcitajSveSmenskeIzvestaje()
        {
            try
            {
                var sql = @"
                    SELECT DISTINCT si.Broj
                    FROM SmenskiIzvestaj si
                    WHERE si.Broj IS NOT NULL
                    ORDER BY si.Broj DESC
                    LIMIT 200";

                var result = await _databaseService.QueryAsync<string>(sql);
                return result.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju smenskih izveštaja: {ex.Message}");
                return new List<string>();
            }
        }

        public async Task<List<RadniProcesModel>> UcitajRadneProcesePoPorizvodnomProcesu(int proizvodniProcesId)
        {
            try
            {
                var sql = @"
                    SELECT DISTINCT rp.ID, rp.Naziv
                    FROM RadniProces rp
                    INNER JOIN RadniProcesToProizvodniProces rppp ON rp.ID = rppp.RadniProcesID
                    WHERE rppp.ProizvodniProcesID = @ProizvodniProcesId
                      AND rp.Naziv NOT LIKE '%#%'
                    ORDER BY rp.Naziv";

                var result = await _databaseService.QueryAsync<RadniProcesModel>(sql, new { ProizvodniProcesId = proizvodniProcesId });
                return result.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju povezanih radnih procesa: {ex.Message}");
                return new List<RadniProcesModel>();
            }
        }
        public async Task<List<SmenskiIzvestajModel>> UcitajSmenskeIzvestaje(FilterRequest filter)
        {
            try
            {
                Console.WriteLine("🔍 Učitavam smenske izveštaje...");

                var sql = @"
            SELECT 
                si.Broj as BrojIzvestaja,
                si.Datum,
                pp.Naziv as ProizvodniProces,
                SUM(er.BrojRadnihSati) as BrojRadnihSati,
                SUM(er.BrojRadnika) as BrojRadnika,
                SUM(er.CenaKostanjaDirektanRad) as TrosakPoRadnomNalogu
            FROM SmenskiIzvestaj si
            INNER JOIN EvidencijaRada er ON si.ID = er.SmenskiIzvestajID AND er.Obrisan = 0
            INNER JOIN RadniNalog rn ON er.RadniNalogID = rn.ID
            INNER JOIN RadniProcesToProizvodniProces rppp ON er.RadniProcesID = rppp.RadniProcesID
            INNER JOIN ProizvodniProces pp ON rppp.ProizvodniProcesID = pp.ID
            WHERE si.Broj IS NOT NULL
                AND er.Obrisan = 0
                AND er.DokumentStatus = 3
                AND er.BrojRadnihSati > 1";

                var parameters = new DynamicParameters();

                if (filter.OdDatum.HasValue)
                {
                    sql += " AND si.Datum >= @OdDatum";
                    parameters.Add("@OdDatum", filter.OdDatum.Value);
                }

                if (filter.DoDatum.HasValue)
                {
                    sql += " AND si.Datum <= @DoDatum";
                    parameters.Add("@DoDatum", filter.DoDatum.Value);
                }

                if (!string.IsNullOrEmpty(filter.SmenskiIzvestaj))
                {
                    sql += " AND si.Broj LIKE @SmenskiIzvestaj";
                    parameters.Add("@SmenskiIzvestaj", $"%{filter.SmenskiIzvestaj}%");
                }

                if (filter.ProizvodniProcesId.HasValue)
                {
                    sql += " AND pp.ID = @ProizvodniProcesId";
                    parameters.Add("@ProizvodniProcesId", filter.ProizvodniProcesId.Value);
                }

                if (filter.RadniProcesId.HasValue)
                {
                    sql += " AND er.RadniProcesID = @RadniProcesId";
                    parameters.Add("@RadniProcesId", filter.RadniProcesId.Value);
                }

                sql += " AND si.Datum >= DATE_SUB(NOW(), INTERVAL 6 MONTH)";

                // GROUP BY po si.ID (umesto si.Broj) + rn.ID (umesto rn.Sifra) + pp.ID
                sql += @"
            GROUP BY si.ID, rn.ID, pp.ID
            HAVING SUM(er.BrojRadnihSati) > 0
            ORDER BY si.Datum DESC, si.Broj, pp.Naziv 
            LIMIT 200";

                var result = await _databaseService.QueryAsync<SmenskiIzvestajModel>(sql, parameters);
                Console.WriteLine($"✅ Dobijeno {result?.Count() ?? 0} redova");
                return result.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ GREŠKA: {ex.Message}");
                return new List<SmenskiIzvestajModel>();
            }
        }

        public async Task<Dictionary<string, decimal>> UcitajStatistikePoSmenskimIzvestajima(FilterRequest filter)
        {
            try
            {
                var sql = @"
                    SELECT 
                        COUNT(DISTINCT si.ID) as BrojIzvestaja,
                        SUM(er.BrojRadnihSati) as UkupnoSati,
                        SUM(er.BrojRadnika) as UkupnoRadnika,
                        SUM(er.CenaKostanjaDirektanRad) as ProsecanTrosak
                    FROM SmenskiIzvestaj si
                    LEFT JOIN EvidencijaRada er ON si.ID = er.SmenskiIzvestajID
                    WHERE si.Broj IS NOT NULL
                      AND er.Obrisan = 0";

                var parameters = new DynamicParameters();

                if (filter.OdDatum.HasValue)
                {
                    sql += " AND si.Datum >= @OdDatum";
                    parameters.Add("@OdDatum", filter.OdDatum.Value);
                }

                if (filter.DoDatum.HasValue)
                {
                    sql += " AND si.Datum <= @DoDatum";
                    parameters.Add("@DoDatum", filter.DoDatum.Value);
                }

                sql += " AND si.Datum >= DATE_SUB(NOW(), INTERVAL 6 MONTH)";

                var result = await _databaseService.QuerySingleOrDefaultAsync<dynamic>(sql, parameters);

                return new Dictionary<string, decimal>
                {
                    ["UkupnoIzvestaja"] = Convert.ToDecimal(result?.BrojIzvestaja ?? 0),
                    ["UkupnoSati"] = Convert.ToDecimal(result?.UkupnoSati ?? 0),
                    ["UkupnoRadnika"] = Convert.ToDecimal(result?.UkupnoRadnika ?? 0),
                    ["ProsecanTrosak"] = Convert.ToDecimal(result?.ProsecanTrosak ?? 0)
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri računanju statistika smenskih izveštaja: {ex.Message}");
                return new Dictionary<string, decimal>
                {
                    ["UkupnoIzvestaja"] = 0,
                    ["UkupnoSati"] = 0,
                    ["UkupnoRadnika"] = 0,
                    ["ProsecanTrosak"] = 0
                };
            }
        }

        #endregion

        #region HELPER METODE ZA DROPDOWN LISTE

        public async Task<List<RadniProcesModel>> UcitajRadneProcese()
        {
            try
            {
                var sql = @"
                    SELECT ID, Naziv, RezijaID, Kreirano, Azurirano, Version
                    FROM RadniProces 
                    WHERE Naziv NOT LIKE '%#%'
                    ORDER BY Naziv
                    LIMIT 200";

                var rezultat = await _databaseService.QueryAsync<RadniProcesModel>(sql);
                Console.WriteLine($"✅ Učitano {rezultat?.Count() ?? 0} korišćenih proizvodnih procesa");
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajRadneProcese: {ex.Message}");
                return new List<RadniProcesModel>();
            }
        }

        public async Task<List<ProizvodniProcesModel>> UcitajProizvodneProcese()
        {
            try
            {
                var sql = @"
            SELECT DISTINCT 
                pp.ID, 
                pp.Naziv, 
                pp.Kreirano, 
                pp.Azurirano, 
                pp.Version
            FROM ProizvodniProces pp
            INNER JOIN RadniProcesToProizvodniProces rppp ON pp.ID = rppp.ProizvodniProcesID
            INNER JOIN EvidencijaRada er ON rppp.RadniProcesID = er.RadniProcesID
            WHERE er.Obrisan = 0
              AND er.Datum >= DATE_SUB(NOW(), INTERVAL 6 MONTH)
            ORDER BY pp.Naziv
            LIMIT 100";

                var rezultat = await _databaseService.QueryAsync<ProizvodniProcesModel>(sql);
                Console.WriteLine($"✅ Učitano {rezultat?.Count() ?? 0} korišćenih proizvodnih procesa");
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajProizvodneProcese: {ex.Message}");
                return new List<ProizvodniProcesModel>();
            }
        }


        #endregion

        #region ORIGINALNE METODE - KOMPATIBILNOST

        public async Task<List<EvidencijaRadaModel>> UcitajSveEvidencijeRada()
        {
            await Task.Delay(100);
            return new List<EvidencijaRadaModel>();
        }

        public async Task<EvidencijaRadaModel?> UcitajEvidencijuRadaPoId(long id)
        {
            return await Task.FromResult<EvidencijaRadaModel?>(null);
        }

        public async Task<List<EvidencijaRadaModel>> UcitajEvidencijeRadaPoReziji(long rezijaId)
        {
            return await UcitajSveEvidencijeRada();
        }

        public async Task<List<EvidencijaRadaModel>> UcitajEvidencijeRadaPoNazivu(string naziv)
        {
            return await UcitajSveEvidencijeRada();
        }

        public async Task<List<RadniProcesModel>> UcitajSveRadneProcese()
        {
            return await UcitajRadneProcese();
        }

        public async Task<RadniProcesModel?> UcitajRadniProcesPoId(long id)
        {
            try
            {
                var sql = @"
                    SELECT ID, Naziv, RezijaID, Kreirano, Azurirano, Version
                    FROM RadniProces 
                    WHERE ID = @Id";

                var rezultat = await _databaseService.QueryFirstOrDefaultAsync<RadniProcesModel>(sql, new { Id = id });
                return rezultat;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajRadniProcesPoId: {ex.Message}");
                return null;
            }
        }

        public async Task<List<RadniProcesModel>> UcitajRadneProcesePoNazivu(string naziv)
        {
            try
            {
                var sql = @"
                    SELECT ID, Naziv, RezijaID, Kreirano, Azurirano, Version
                    FROM RadniProces 
                    WHERE Naziv LIKE @Naziv
                      AND Naziv NOT LIKE '%#%'
                    ORDER BY Naziv
                    LIMIT 50";

                var rezultat = await _databaseService.QueryAsync<RadniProcesModel>(sql, new { Naziv = $"%{naziv}%" });
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajRadneProcesePoNazivu: {ex.Message}");
                return new List<RadniProcesModel>();
            }
        }

        public async Task<List<RadniProcesModel>> UcitajNoveRadneProcese(int dana = 30)
        {
            try
            {
                var sql = @"
                    SELECT ID, Naziv, RezijaID, Kreirano, Azurirano, Version
                    FROM RadniProces 
                    WHERE Kreirano >= DATE_SUB(NOW(), INTERVAL @Dana DAY)
                      AND Naziv NOT LIKE '%#%'
                    ORDER BY Kreirano DESC
                    LIMIT 50";

                var rezultat = await _databaseService.QueryAsync<RadniProcesModel>(sql, new { Dana = dana });
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajNoveRadneProcese: {ex.Message}");
                return new List<RadniProcesModel>();
            }
        }

        public async Task<List<ProizvodniProcesModel>> UcitajSveProizvodneProcese()
        {
            return await UcitajProizvodneProcese();
        }

        public async Task<ProizvodniProcesModel?> UcitajProizvodniProcesPoId(long id)
        {
            try
            {
                var sql = @"
                    SELECT ID, Naziv, Kreirano, Azurirano, Version
                    FROM ProizvodniProces 
                    WHERE ID = @Id";

                var rezultat = await _databaseService.QueryFirstOrDefaultAsync<ProizvodniProcesModel>(sql, new { Id = id });
                return rezultat;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajProizvodniProcesPoId: {ex.Message}");
                return null;
            }
        }

        public async Task<List<ProizvodniProcesModel>> UcitajProizvodneProcesePoKategoriji(string kategorija)
        {
            return await UcitajProizvodneProcese();
        }

        public async Task<List<ProizvodniProcesModel>> UcitajProizvodneProcesePoNazivu(string naziv)
        {
            try
            {
                var sql = @"
                    SELECT ID, Naziv, Kreirano, Azurirano, Version
                    FROM ProizvodniProces 
                    WHERE Naziv LIKE @Naziv
                    ORDER BY Naziv
                    LIMIT 50";

                var rezultat = await _databaseService.QueryAsync<ProizvodniProcesModel>(sql, new { Naziv = $"%{naziv}%" });
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajProizvodneProcesePoNazivu: {ex.Message}");
                return new List<ProizvodniProcesModel>();
            }
        }

        public async Task<List<SmenskiIzvestajModel>> UcitajSveSmenskeIzvestaje(FilterRequest filterRequest)
        {
            await Task.Delay(100);
            return new List<SmenskiIzvestajModel>();
        }

        public async Task<SmenskiIzvestajModel?> UcitajSmenskiIzvestajPoId(long id)
        {
            return await Task.FromResult<SmenskiIzvestajModel?>(null);
        }

        public async Task<List<SmenskiIzvestajModel>> UcitajSmenskeIzvestajePoDatumu(DateTime odDatum, DateTime doDatum)
        {
            return await UcitajSveSmenskeIzvestaje(new FilterRequest { OdDatum = odDatum, DoDatum = doDatum });
        }

        public async Task<List<SmenskiIzvestajModel>> UcitajSmenskeIzvestajePoSmeni(int smena)
        {
            return await UcitajSveSmenskeIzvestaje(new FilterRequest { Smena = smena });
        }

        public async Task<List<SmenskiIzvestajModel>> UcitajSmenskeIzvestajePoPoslovodji(long poslovodjaId)
        {
            return await UcitajSveSmenskeIzvestaje(new FilterRequest { KomitentId = poslovodjaId });
        }

        public async Task<List<SmenskiIzvestajModel>> UcitajOtvoreneSmenskeIzvestaje()
        {
            return await UcitajSveSmenskeIzvestaje(new FilterRequest { DokumentStatus = 2 });
        }

        public async Task<List<SmenskiIzvestajModel>> UcitajZakljuceneSmenskeIzvestaje()
        {
            return await UcitajSveSmenskeIzvestaje(new FilterRequest { DokumentStatus = 3 });
        }

        public async Task<Dictionary<string, int>> UcitajStatistikuPoSmenama(FilterRequest filterRequest)
        {
            try
            {
                var sql = @"
                    SELECT 
                        CASE si.Smena
                            WHEN 1 THEN 'Prva smena'
                            WHEN 2 THEN 'Druga smena'
                            WHEN 3 THEN 'Treća smena'
                            ELSE 'Nepoznata smena'
                        END as Smena,
                        COUNT(*) as BrojIzvestaja
                    FROM SmenskiIzvestaj si
                    WHERE si.Broj IS NOT NULL
                    GROUP BY si.Smena
                    ORDER BY si.Smena
                ";

                var rezultat = await _databaseService.QueryAsync<dynamic>(sql);
                return rezultat.ToDictionary(x => (string)x.Smena, x => (int)x.BrojIzvestaja);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajStatistikuPoSmenama: {ex.Message}");
                return new Dictionary<string, int>();
            }
        }

        public async Task<Dictionary<string, int>> UcitajStatistikuPoPoslovodjama(FilterRequest filterRequest)
        {
            try
            {
                var sql = @"
                    SELECT 
                        COALESCE(k.Naziv, 'Nepoznato') as Poslovodja,
                        COUNT(*) as BrojIzvestaja
                    FROM SmenskiIzvestaj si
                    LEFT JOIN Komitent k ON si.PoslovodjaID = k.ID
                    WHERE si.Broj IS NOT NULL
                    GROUP BY k.ID, k.Naziv
                    ORDER BY BrojIzvestaja DESC
                    LIMIT 10
                ";

                var rezultat = await _databaseService.QueryAsync<dynamic>(sql);
                return rezultat.ToDictionary(x => (string)x.Poslovodja, x => (int)x.BrojIzvestaja);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajStatistikuPoPoslovodjama: {ex.Message}");
                return new Dictionary<string, int>();
            }
        }

        public async Task<Dictionary<string, int>> UcitajStatistikuPoStatusima(FilterRequest filterRequest)
        {
            try
            {
                var sql = @"
                    SELECT 
                        CASE si.DokumentStatus
                            WHEN 2 THEN 'Otvoren'
                            WHEN 3 THEN 'Zaključen'
                            WHEN 4 THEN 'Storno'
                            ELSE 'Nepoznato'
                        END as Status,
                        COUNT(*) as BrojIzvestaja
                    FROM SmenskiIzvestaj si
                    WHERE si.Broj IS NOT NULL
                    GROUP BY si.DokumentStatus
                ";

                var rezultat = await _databaseService.QueryAsync<dynamic>(sql);
                return rezultat.ToDictionary(x => (string)x.Status, x => (int)x.BrojIzvestaja);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajStatistikuPoStatusima: {ex.Message}");
                return new Dictionary<string, int>();
            }
        }

        public async Task<int> UcitajUkupanBrojSmenskihIzvestaja(FilterRequest filterRequest)
        {
            try
            {
                var sql = "SELECT COUNT(*) FROM SmenskiIzvestaj WHERE Broj IS NOT NULL";
                var rezultat = await _databaseService.ExecuteScalarAsync<int>(sql);
                return rezultat;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajUkupanBrojSmenskihIzvestaja: {ex.Message}");
                return 0;
            }
        }

        public async Task<int> UcitajBrojOtvorenihSmenskihIzvestaja()
        {
            try
            {
                var sql = "SELECT COUNT(*) FROM SmenskiIzvestaj WHERE Broj IS NOT NULL AND DokumentStatus = 2";
                var rezultat = await _databaseService.ExecuteScalarAsync<int>(sql);
                return rezultat;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajBrojOtvorenihSmenskihIzvestaja: {ex.Message}");
                return 0;
            }
        }

        public async Task<int> UcitajBrojZakljucenihSmenskihIzvestaja()
        {
            try
            {
                var sql = "SELECT COUNT(*) FROM SmenskiIzvestaj WHERE Broj IS NOT NULL AND DokumentStatus = 3";
                var rezultat = await _databaseService.ExecuteScalarAsync<int>(sql);
                return rezultat;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajBrojZakljucenihSmenskihIzvestaja: {ex.Message}");
                return 0;
            }
        }

        public async Task<Dictionary<string, int>> UcitajTopRadneProcese(FilterRequest filterRequest)
        {
            try
            {
                var sql = @"
                    SELECT 
                        COALESCE(rp.Naziv, 'Nepoznato') as RadniProces,
                        COUNT(*) as BrojEvidencija
                    FROM EvidencijaRada er
                    LEFT JOIN RadniProces rp ON er.RadniProcesID = rp.ID
                    WHERE er.Obrisan = 0
                    GROUP BY rp.ID, rp.Naziv
                    ORDER BY BrojEvidencija DESC
                    LIMIT 10
                ";

                var rezultat = await _databaseService.QueryAsync<dynamic>(sql);
                return rezultat.ToDictionary(x => (string)x.RadniProces, x => (int)x.BrojEvidencija);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajTopRadneProcese: {ex.Message}");
                return new Dictionary<string, int>();
            }
        }

        public async Task<Dictionary<string, int>> UcitajTopProizvodneProcese(FilterRequest filterRequest)
        {
            try
            {
                var sql = @"
                    SELECT 
                        COALESCE(pp.Naziv, 'Nepoznato') as ProizvodniProces,
                        COUNT(*) as BrojEvidencija
                    FROM EvidencijaRada er
                    LEFT JOIN ProizvodniProces pp ON er.ProizvodniProcesID = pp.ID
                    WHERE er.Obrisan = 0
                    GROUP BY pp.ID, pp.Naziv
                    ORDER BY BrojEvidencija DESC
                    LIMIT 10
                ";

                var rezultat = await _databaseService.QueryAsync<dynamic>(sql);
                return rezultat.ToDictionary(x => (string)x.ProizvodniProces, x => (int)x.BrojEvidencija);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajTopProizvodneProcese: {ex.Message}");
                return new Dictionary<string, int>();
            }
        }

        public async Task<int> UcitajUkupnuAktivnost(FilterRequest filterRequest)
        {
            try
            {
                var sql = "SELECT COUNT(*) FROM EvidencijaRada WHERE Obrisan = 0";
                var rezultat = await _databaseService.ExecuteScalarAsync<int>(sql);
                return rezultat;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajUkupnuAktivnost: {ex.Message}");
                return 0;
            }
        }

        #endregion

        #region NOVE METODE ZA RADNI NALOG IZVEŠTAJ

        public async Task<List<RadniNalogIzvestajModel>> UcitajRadniNalogIzvestajPoNalogu(string radniNalog)
        {
            var filter = new FilterRequest { RadniNalog = radniNalog };
            return await UcitajRadniNalogIzvestaj(filter);
        }

        public async Task<decimal> UcitajUkupanTrosakPoRadnomNalogu(string radniNalog)
        {
            try
            {
                var sql = @"
                SELECT COALESCE(SUM(er.CenaKostanjaDirektanRad), 0) as UkupanTrosak
                FROM EvidencijaRada er
                LEFT JOIN RadniNalog rn ON er.RadniNalogID = rn.ID
                WHERE rn.Sifra = @RadniNalog 
                  AND er.Obrisan = 0";

                var result = await _databaseService.QuerySingleOrDefaultAsync<decimal>(sql, new { RadniNalog = radniNalog });
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri računanju troška: {ex.Message}");
                return 0;
            }
        }

        public async Task<decimal> UcitajUkupneRadneSatePoNalogu(string radniNalog)
        {
            try
            {
                var sql = @"
                SELECT COALESCE(SUM(er.BrojRadnihSati), 0) as UkupniSati
                FROM EvidencijaRada er
                LEFT JOIN RadniNalog rn ON er.RadniNalogID = rn.ID
                WHERE rn.Sifra = @RadniNalog 
                  AND er.Obrisan = 0";

                var result = await _databaseService.QuerySingleOrDefaultAsync<decimal>(sql, new { RadniNalog = radniNalog });
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri računanju sati: {ex.Message}");
                return 0;
            }
        }

        public async Task<decimal> UcitajUkupnuRobuPoNalogu(string radniNalog)
        {
            try
            {
                var sql = @"
                SELECT COALESCE(SUM(vpp.Kolicina), 0) as UkupnaRoba
                FROM vPreradaPregled vpp
                LEFT JOIN RadniNalog rn ON vpp.RadniNalogID = rn.ID
                LEFT JOIN Artikal a ON vpp.ArtikalID = a.ID
                WHERE rn.Sifra = @RadniNalog
                  AND a.MagacinID = 6
                  AND rn.Aktivno = 1";

                var result = await _databaseService.QuerySingleOrDefaultAsync<decimal>(sql, new { RadniNalog = radniNalog });
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri računanju robe: {ex.Message}");
                return 0;
            }
        }

        public async Task<decimal> UcitajProcenatIskoriscenjaPoNalogu(string radniNalog)
        {
            try
            {
                var sql = @"
                SELECT COALESCE(MAX(verm.Mnozilac) * 100, 0) as Procenat
                FROM vEvidencijaRadaPreradaMnozilac verm
                LEFT JOIN EvidencijaRada er ON verm.EvidencijaRadaID = er.ID
                LEFT JOIN RadniNalog rn ON er.RadniNalogID = rn.ID
                WHERE rn.Sifra = @RadniNalog
                  AND er.Obrisan = 0";

                var result = await _databaseService.QuerySingleOrDefaultAsync<decimal>(sql, new { RadniNalog = radniNalog });
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri računanju procenta: {ex.Message}");
                return 0;
            }
        }

        public async Task<List<string>> UcitajSveRadneNaloge()
        {
            try
            {
                var sql = @"
                SELECT DISTINCT rn.Sifra
                FROM RadniNalog rn
                WHERE rn.Aktivno = 1
                  AND rn.DokumentStatus IN (2, 3)
                  AND rn.Sifra NOT LIKE 'ST-%'
                  AND rn.Sifra IS NOT NULL
                ORDER BY rn.Sifra DESC
                LIMIT 200";

                var result = await _databaseService.QueryAsync<string>(sql);
                return result.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju radnih naloga: {ex.Message}");
                return new List<string>();
            }
        }

        public async Task<Dictionary<string, decimal>> UcitajStatistikePoRadnomNalogu(FilterRequest filter)
        {
            try
            {
                var sql = @"
                SELECT 
                    COUNT(DISTINCT rn.ID) as BrojNaloga,
                    COUNT(DISTINCT er.ID) as BrojEvidencija,
                    COALESCE(SUM(er.BrojRadnika), 0) as UkupnoRadnika,
                    COALESCE(SUM(er.BrojRadnihSati), 0) as UkupnoSati,
                    COALESCE(SUM(er.CenaKostanjaDirektanRad), 0) as UkupanTrosak,
                    CASE 
                        WHEN COUNT(DISTINCT er.ID) > 0 
                        THEN COALESCE(SUM(er.CenaKostanjaDirektanRad) / COUNT(DISTINCT er.ID), 0)
                        ELSE 0 
                    END as ProsecanTrosak
                FROM EvidencijaRada er
                LEFT JOIN RadniNalog rn ON er.RadniNalogID = rn.ID
                WHERE er.Obrisan = 0
                  AND rn.Aktivno = 1
                  AND rn.Sifra NOT LIKE 'ST-%'
                  AND er.Datum >= DATE_SUB(NOW(), INTERVAL 6 MONTH)";

                var parameters = new DynamicParameters();

                if (filter.OdDatum.HasValue)
                {
                    sql += " AND er.Datum >= @OdDatum";
                    parameters.Add("@OdDatum", filter.OdDatum.Value);
                }

                if (filter.DoDatum.HasValue)
                {
                    sql += " AND er.Datum <= @DoDatum";
                    parameters.Add("@DoDatum", filter.DoDatum.Value);
                }

                var result = await _databaseService.QuerySingleOrDefaultAsync<dynamic>(sql, parameters);

                return new Dictionary<string, decimal>
                {
                    ["BrojNaloga"] = Convert.ToDecimal(result?.BrojNaloga ?? 0),
                    ["BrojEvidencija"] = Convert.ToDecimal(result?.BrojEvidencija ?? 0),
                    ["UkupnoRadnika"] = Convert.ToDecimal(result?.UkupnoRadnika ?? 0),
                    ["UkupnoSati"] = Convert.ToDecimal(result?.UkupnoSati ?? 0),
                    ["UkupanTrosak"] = Convert.ToDecimal(result?.UkupanTrosak ?? 0),
                    ["ProsecanTrosak"] = Convert.ToDecimal(result?.ProsecanTrosak ?? 0)
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Greška pri računanju statistika: {ex.Message}");
                return new Dictionary<string, decimal>
                {
                    ["BrojNaloga"] = 0,
                    ["BrojEvidencija"] = 0,
                    ["UkupnoRadnika"] = 0,
                    ["UkupnoSati"] = 0,
                    ["UkupanTrosak"] = 0,
                    ["ProsecanTrosak"] = 0
                };
            }
        }

        #endregion

        #region NAJAVLJENI UTOVARI ZA DASHBOARD

        public async Task<List<NajavljeniUtovarModel>> UcitajNajavljeneUtovare()
        {
            try
            {
                Console.WriteLine("🚚 Učitavam najavljene utovare...");

                var sql = @"
                    SELECT 
                        rn.ID,
                        rn.Sifra as RadniNalog,
                        rn.DatumIsporuke,
                        COALESCE(k.Naziv, 'Nepoznat kupac') as Kupac,
                        COALESCE(a.Naziv, 'Nepoznat artikal') as Artikal,
                        rn.Kolicina,
                        rn.LotNaloga,
                        COALESCE(up.BrojUgovora, '') as BrojUgovora,
                        rn.DokumentStatus,
                        rn.KomitentID,
                        rn.ArtikalInstancaID,
                        rn.UgovorProdajaID
                    FROM RadniNalog rn
                    LEFT JOIN Komitent k ON rn.KomitentID = k.ID
                    LEFT JOIN ArtikalInstanca ai ON rn.ArtikalInstancaID = ai.ID
                    LEFT JOIN Artikal a ON ai.ArtikalID = a.ID
                    LEFT JOIN UgovorProdaja up ON rn.UgovorProdajaID = up.ID
                    WHERE rn.DokumentStatus = 2
                      AND rn.Sifra LIKE 'RN-%'
                      AND rn.Aktivno = 1
                      AND rn.DatumIsporuke IS NOT NULL
                      AND rn.DatumIsporuke >= CURDATE()
                      AND rn.DatumIsporuke <= DATE_ADD(CURDATE(), INTERVAL 20 DAY)
                    ORDER BY rn.DatumIsporuke ASC, rn.Sifra
                    LIMIT 50";

                var rezultat = await _databaseService.QueryAsync<NajavljeniUtovarModel>(sql);

                Console.WriteLine($"✅ Učitano {rezultat?.Count() ?? 0} najavljenih utovara");
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Greška pri učitavanju najavljenih utovara: {ex.Message}");
                return new List<NajavljeniUtovarModel>();
            }
        }

        #endregion
    }
}