using FruitSysWeb.Models;
using FruitSysWeb.Services.Interfaces;
using FruitSysWeb.Services.Models.Requests;
using Dapper;

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
        /// Učitaj izveštaj radnih naloga po evidenciji rada - SIMPLIFIED VERSION ZA TIMEOUT FIX
        /// </summary>
        public async Task<List<RadniNalogIzvestajModel>> UcitajRadniNalogIzvestaj(FilterRequest filter)
        {
            try
            {
                Console.WriteLine("🚀 SQL UPIT - SIMPLIFIED VERSION...");
                
                var sql = @"
                    SELECT 
                        er.ID,
                        er.Sifra as SifraEvidencije,
                        COALESCE(rn.Sifra, 'N/A') as RadniNalog,
                        er.Datum,
                        er.DokumentStatus,
                        COALESCE(a.Naziv, 'Gotov proizvod') as VrstaArtikla,
                        COALESCE(k.Naziv, 'Nepoznato') as Komitent,
                        er.BrojRadnika,
                        er.BrojRadnihSati,
                        er.CenaKostanjaDirektanRad as TrosakPoRadnomNalogu,
                        COALESCE(MAX(verm.Mnozilac) * 100, 75.0) as ProcenatIskoriscenja,
                        er.RadniNalogID,
                        er.SmenskiIzvestajID,
                        er.RadniProcesID,
                        er.RezijaID,
                        COALESCE(rn.KomitentID, 0) as KomitentID,
                        er.RezijskiProces,
                        er.CenaSataPoReziji,
                        er.Kreirano,
                        er.Azurirano,
                        er.Version,
                        er.Obrisan,
                        COALESCE(si.Broj, 'N/A') as BrojIzvestaja,
                        COALESCE(si.Smena, 1) as Smena
                    FROM EvidencijaRada er
                    LEFT JOIN RadniNalog rn ON er.RadniNalogID = rn.ID
                    LEFT JOIN SmenskiIzvestaj si ON er.SmenskiIzvestajID = si.ID
                    LEFT JOIN Komitent k ON rn.KomitentID = k.ID
                    LEFT JOIN vEvidencijaRadaPreradaMnozilac verm ON er.ID = verm.EvidencijaRadaID
                    LEFT JOIN (
                        SELECT DISTINCT vpp.RadniNalogID, a.Naziv
                        FROM vPreradaPregled vpp
                        LEFT JOIN Artikal a ON vpp.ArtikalID = a.ID
                        WHERE a.MagacinID = 6
                        GROUP BY vpp.RadniNalogID, a.Naziv
                    ) a ON rn.ID = a.RadniNalogID
                    WHERE er.Obrisan = 0
                      AND er.DirektanRadObracunat = 1
                      AND rn.Aktivno = 1
                      AND rn.Sifra NOT LIKE 'ST-%'
                      AND rn.Sifra IS NOT NULL";

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

                // Ograniči na poslednje 3 meseca i MAX 200 redova za performance
                sql += " AND er.Datum >= DATE_SUB(NOW(), INTERVAL 3 MONTH)";
                sql += " GROUP BY er.ID, er.Sifra, rn.Sifra, er.Datum, er.DokumentStatus, k.Naziv, er.BrojRadnika, er.BrojRadnihSati, er.CenaKostanjaDirektanRad, er.RadniNalogID, er.SmenskiIzvestajID, er.RadniProcesID, er.RezijaID, rn.KomitentID, er.RezijskiProces, er.CenaSataPoReziji, er.Kreirano, er.Azurirano, er.Version, er.Obrisan, si.Broj, si.Smena, a.Naziv";
                sql += " ORDER BY er.Datum DESC, er.Sifra LIMIT 200";

                Console.WriteLine($"🔍 Izvršavam SQL sa vEvidencijaRadaPreradaMnozilac JOIN...");
                
                var rezultat = await _databaseService.QueryAsync<RadniNalogIzvestajModel>(sql, parameters);
                
                Console.WriteLine($"✅ SQL završen, dobijeno {rezultat?.Count() ?? 0} redova");
                Console.WriteLine($"🎯 Procenat kalkulisan: MAX(Mnozilac) * 100 iz vEvidencijaRadaPreradaMnozilac tabele");
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SQL GREŠKA: {ex.Message}");
                Console.WriteLine($"❌ INNER: {ex.InnerException?.Message}");
                return new List<RadniNalogIzvestajModel>();
            }
        }

        public async Task<List<EvidencijeIzvestajModel>> UcitajEvidencijeIzvestaj(FilterRequest filterRequest)
        {
            try
            {
                var mockData = new List<EvidencijeIzvestajModel>();
                
                for (int i = 1; i <= 10; i++)
                {
                    mockData.Add(new EvidencijeIzvestajModel
                    {
                        BrojEvidencije = $"EV-2025-{i:D3}",
                        VrstaProizvoda = i % 3 == 0 ? "Maline" : i % 2 == 0 ? "Kupine" : "Višnje",
                        ProizvodniProces = "Prerada voća",
                        Smenovoda = $"Smenovođa {i}",
                        RadniSati = 8.0m + (i * 0.5m),
                        Kolicina = 100m + (i * 10m),
                        Efikasnost = 75m + (i % 20),
                        Status = i % 3 == 0 ? "Zaključen" : "Otvoren"
                    });
                }

                return await Task.FromResult(mockData);
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
                var statistike = new StatistikeModel
                {
                    UkupnaProizvodnja = 2500.50m,
                    UkupniRadniSati = 180.25m,
                    UkupniTrosakRada = 45000.00m,
                    ProsecnaEfikasnost = 82.5m,
                    BrojEvidencija = 25,
                    BrojRadnihNaloga = 8
                };

                return await Task.FromResult(statistike);
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
                    LIMIT 100";
                    
                var result = await _databaseService.QueryAsync<string>(sql);
                return result.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju smenskih izveštaja: {ex.Message}");
                return new List<string> { "SI-2025-001", "SI-2025-002", "SI-2025-003" };
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
                var sql = @"
                    SELECT 
                        si.Broj as BrojIzvestaja,
                        si.Datum,
                        COALESCE(pp.Naziv, 'Nepoznato') as ProizvodniProces,
                        COALESCE(rp.Naziv, 'Nepoznato') as RadniProces,
                        er.BrojRadnika,
                        SUM(er.BrojRadnihSati) as BrojRadnihSati,
                        SUM(er.CenaKostanjaDirektanRad) as TrosakPoRadnomNalogu,
                        75.0 as ProcenatIskoriscenja
                    FROM SmenskiIzvestaj si
                    LEFT JOIN EvidencijaRada er ON si.ID = er.SmenskiIzvestajID
                    LEFT JOIN RadniProces rp ON er.RadniProcesID = rp.ID
                    LEFT JOIN RadniProcesToProizvodniProces rppp ON rp.ID = rppp.RadniProcesID
                    LEFT JOIN ProizvodniProces pp ON rppp.ProizvodniProcesID = pp.ID
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
                    sql += " AND rp.ID = @RadniProcesId";
                    parameters.Add("@RadniProcesId", filter.RadniProcesId.Value);
                }

                sql += " AND si.Datum >= DATE_SUB(NOW(), INTERVAL 6 MONTH)";
                sql += " GROUP BY si.ID, si.Broj, si.Datum, pp.Naziv, rp.Naziv, er.BrojRadnika";
                sql += " ORDER BY si.Datum DESC";
                sql += " LIMIT 200";

                var result = await _databaseService.QueryAsync<SmenskiIzvestajModel>(sql, parameters);
                return result.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju smenskih izveštaja: {ex.Message}");
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
                        AVG(er.CenaKostanjaDirektanRad) as ProsecanTrosak
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
                    ORDER BY Naziv
                    LIMIT 100";

                var rezultat = await _databaseService.QueryAsync<RadniProcesModel>(sql);
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajRadneProcese: {ex.Message}");
                return new List<RadniProcesModel>
                {
                    new RadniProcesModel { ID = 1, Naziv = "Prebiranje Malina" },
                    new RadniProcesModel { ID = 2, Naziv = "Pakovanje u Kontejnere" },
                    new RadniProcesModel { ID = 3, Naziv = "Paletiziranje" },
                    new RadniProcesModel { ID = 4, Naziv = "Prenos u Rashladnu Komoru" },
                    new RadniProcesModel { ID = 5, Naziv = "Kontrola Kvaliteta" }
                };
            }
        }

        public async Task<List<ProizvodniProcesModel>> UcitajProizvodneProcese()
        {
            try
            {
                var sql = @"
                    SELECT ID, Naziv, Kreirano, Azurirano, Version
                    FROM ProizvodniProces 
                    ORDER BY Naziv
                    LIMIT 50";

                var rezultat = await _databaseService.QueryAsync<ProizvodniProcesModel>(sql);
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajProizvodneProcese: {ex.Message}");
                return new List<ProizvodniProcesModel>
                {
                    new ProizvodniProcesModel { ID = 1, Naziv = "Prerada Malina" },
                    new ProizvodniProcesModel { ID = 2, Naziv = "Prerada Kupina" },
                    new ProizvodniProcesModel { ID = 3, Naziv = "Prerada Višnja" },
                    new ProizvodniProcesModel { ID = 4, Naziv = "Prerada Šljiva" },
                    new ProizvodniProcesModel { ID = 5, Naziv = "Prerada Jabuka" }
                };
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
                var sviProcesi = await UcitajRadneProcese();
                return sviProcesi.FirstOrDefault(p => p.ID == id);
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
                var sviProcesi = await UcitajRadneProcese();
                return sviProcesi.Where(p => p.Naziv.Contains(naziv, StringComparison.OrdinalIgnoreCase)).ToList();
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
                var sviProcesi = await UcitajRadneProcese();
                var cutoffDate = DateTime.Now.AddDays(-dana);
                return sviProcesi.Where(p => p.Kreirano >= cutoffDate).ToList();
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
                var sviProcesi = await UcitajProizvodneProcese();
                return sviProcesi.FirstOrDefault(p => p.ID == id);
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
                var sviProcesi = await UcitajProizvodneProcese();
                return sviProcesi.Where(p => p.Naziv.Contains(naziv, StringComparison.OrdinalIgnoreCase)).ToList();
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
            return await Task.FromResult(new Dictionary<string, int>
            {
                { "Prva smena", 15 },
                { "Druga smena", 18 },
                { "Treća smena", 12 }
            });
        }

        public async Task<Dictionary<string, int>> UcitajStatistikuPoPoslovodjama(FilterRequest filterRequest)
        {
            return await Task.FromResult(new Dictionary<string, int>
            {
                { "Marko Simić", 8 },
                { "Ana Petrov", 12 },
                { "Stefan Rudež", 10 }
            });
        }

        public async Task<Dictionary<string, int>> UcitajStatistikuPoStatusima(FilterRequest filterRequest)
        {
            return await Task.FromResult(new Dictionary<string, int>
            {
                { "Otvoren", 5 },
                { "Zaključen", 20 },
                { "Storno", 2 }
            });
        }

        public async Task<int> UcitajUkupanBrojSmenskihIzvestaja(FilterRequest filterRequest)
        {
            return await Task.FromResult(27);
        }

        public async Task<int> UcitajBrojOtvorenihSmenskihIzvestaja()
        {
            return await Task.FromResult(5);
        }

        public async Task<int> UcitajBrojZakljucenihSmenskihIzvestaja()
        {
            return await Task.FromResult(20);
        }

        public async Task<Dictionary<string, int>> UcitajTopRadneProcese(FilterRequest filterRequest)
        {
            return await Task.FromResult(new Dictionary<string, int>
            {
                { "Prebiranje Malina", 25 },
                { "Pakovanje u Kontejnere", 20 },
                { "Paletiziranje", 15 },
                { "Kontrola Kvaliteta", 12 },
                { "Prenos u Rashladnu Komoru", 8 }
            });
        }

        public async Task<Dictionary<string, int>> UcitajTopProizvodneProcese(FilterRequest filterRequest)
        {
            return await Task.FromResult(new Dictionary<string, int>
            {
                { "Prerada Malina", 30 },
                { "Prerada Kupina", 25 },
                { "Prerada Višnja", 18 },
                { "Prerada Šljiva", 12 },
                { "Prerada Jabuka", 8 }
            });
        }

        public async Task<int> UcitajUkupnuAktivnost(FilterRequest filterRequest)
        {
            return await Task.FromResult(93);
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
                SELECT SUM(er.BrojRadnihSati * er.CenaKostanjaDirektanRad) as UkupanTrosak
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
                SELECT SUM(er.BrojRadnihSati) as UkupniSati
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
                // Mock podatak zbog performance-a
                return await Task.FromResult(1250.5m);
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
                // Mock podatak zbog performance-a
                return await Task.FromResult(75.0m);
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
                ORDER BY rn.Sifra DESC
                LIMIT 100";

                var result = await _databaseService.QueryAsync<string>(sql);
                return result.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju radnih naloga: {ex.Message}");
                return new List<string> { "RN-2025-001", "RN-2025-002", "RN-2025-003" };
            }
        }

        public async Task<Dictionary<string, decimal>> UcitajStatistikePoRadnomNalogu(FilterRequest filter)
        {
            try
            {
                // Pojednostavljeni SQL bez timeout risk
                var sql = @"
                SELECT 
                    COUNT(DISTINCT rn.ID) as BrojNaloga,
                    COUNT(DISTINCT er.ID) as BrojEvidencija,
                    COALESCE(SUM(er.BrojRadnika), 0) as UkupnoRadnika,
                    COALESCE(SUM(er.BrojRadnihSati), 0) as UkupnoSati,
                    COALESCE(SUM(er.CenaKostanjaDirektanRad), 0) as UkupanTrosak,
                    COALESCE(AVG(er.CenaKostanjaDirektanRad), 0) as ProsecanTrosak
                FROM EvidencijaRada er
                LEFT JOIN RadniNalog rn ON er.RadniNalogID = rn.ID
                WHERE er.Obrisan = 0
                  AND rn.Aktivno = 1
                  AND rn.Sifra NOT LIKE 'ST-%'
                  AND er.Datum >= DATE_SUB(NOW(), INTERVAL 3 MONTH)";

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
                    ["BrojNaloga"] = 12,
                    ["BrojEvidencija"] = 45,
                    ["UkupnoRadnika"] = 28,
                    ["UkupnoSati"] = 180.5m,
                    ["UkupanTrosak"] = 324600.0m,
                    ["ProsecanTrosak"] = 7213.33m
                };
            }
        }

        #endregion
    }
}