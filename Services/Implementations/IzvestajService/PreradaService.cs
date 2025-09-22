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
        /// Učitaj izveštaj radnih naloga po evidenciji rada
        /// Kombinuje EvidencijaRada, RadniNalog, RadniProces, Komitent, vPreradaSaProcentima
        /// </summary>
        public async Task<List<RadniNalogIzvestajModel>> UcitajRadniNalogIzvestaj(FilterRequest filter)
        {
            try
            {
                var sql = @"
                    SELECT 
                        er.ID,
                        er.Sifra as SifraEvidencije,
                        COALESCE(rn.Sifra, 'N/A') as RadniNalog,
                        er.Datum,
                        er.DokumentStatus,
                        COALESCE(rp.Naziv, 'Nepoznato') as RadniProces,
                        COALESCE(k.Naziv, 'Nepoznato') as Komitent,
                        er.BrojRadnika,
                        er.BrojRadnihSati,
                        er.CenaKostanjaDirektanRad as TrosakPoRadnomNalogu,
                        COALESCE(vp.Procenat, 0) as ProcenatIskoriscenja,
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
                        er.Obrisan
                    FROM EvidencijaRada er
                    LEFT JOIN RadniNalog rn ON er.RadniNalogID = rn.ID
                    LEFT JOIN RadniProces rp ON er.RadniProcesID = rp.ID
                    LEFT JOIN Komitent k ON rn.KomitentID = k.ID
                    LEFT JOIN vPreradaSaProcentima vp ON er.RadniNalogID = vp.RadniNalogID
                    WHERE er.Obrisan = 0";

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

                if (filter.RadniProcesID.HasValue)
                {
                    sql += " AND er.RadniProcesID = @RadniProcesID";
                    parameters.Add("@RadniProcesID", filter.RadniProcesID.Value);
                }

                if (filter.DokumentStatus.HasValue)
                {
                    sql += " AND er.DokumentStatus = @DokumentStatus";
                    parameters.Add("@DokumentStatus", filter.DokumentStatus.Value);
                }

                if (filter.KomitentId.HasValue)
                {
                    sql += " AND rn.KomitentID = @KomitentId";
                    parameters.Add("@KomitentId", filter.KomitentId.Value);
                }

                sql += " ORDER BY er.Datum DESC, er.Sifra LIMIT 500";

                var rezultat = await _databaseService.QueryAsync<RadniNalogIzvestajModel>(sql, parameters);
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajRadniNalogIzvestaj: {ex.Message}");
                return new List<RadniNalogIzvestajModel>();
            }
        }

        /// <summary>
        /// Učitaj izveštaj smenskog rada po danima i nedeljama
        /// </summary>
        /* public async Task<List<SmeneDaniIzvestajModel>> UcitajSmeneDaniIzvestaj(FilterRequest filter)
        {
            try
            {
                var sql = @"
                    SELECT 
                        si.ID,
                        COALESCE(si.Broj, 'N/A') as BrojIzvestaja,
                        si.Datum,
                        si.Smena,
                        si.DokumentStatus,
                        COALESCE(k.Naziv, 'Nepoznato') as Smenovoda,
                        COALESCE(pp.Naziv, 'Nepoznato') as ProizvodniProces,
                        COALESCE(SUM(er.BrojRadnihSati), 0) as UkupnoSati,
                        COALESCE(SUM(er.CenaKostanjaDirektanRad), 0) as UkupanTrosak,
                        COALESCE(SUM(er.BrojRadnika), 0) as UkupnoBrojRadnika,
                        COALESCE(AVG(vp.Procenat), 0) as Produktivnost,
                        COALESCE(AVG(vp.Procenat), 0) as Efikasnost,
                        COALESCE(SUM(vp.Kolicina), 0) as UkupnaKolicina,
                        si.ID as SmenskiIzvestajID,
                        si.PoslovodjaID,
                        DAYOFWEEK(si.Datum) as DanUNedelji,
                        WEEK(si.Datum) as NedeljaUGodini,
                        MONTH(si.Datum) as MesecUGodini,
                        si.Kreirano,
                        si.Azurirano,
                        si.Version
                    FROM SmenskiIzvestaj si
                    LEFT JOIN EvidencijaRada er ON si.ID = er.SmenskiIzvestajID AND er.Obrisan = 0
                    LEFT JOIN Komitent k ON si.PoslovodjaID = k.ID
                    LEFT JOIN ProizvodniProces pp ON pp.ID = 1
                    LEFT JOIN vPreradaSaProcentima vp ON si.ID = vp.SmenskiIzvestajID
                    WHERE 1=1";

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

                if (filter.Smena.HasValue)
                {
                    sql += " AND si.Smena = @Smena";
                    parameters.Add("@Smena", filter.Smena.Value);
                }

                if (filter.DokumentStatus.HasValue)
                {
                    sql += " AND si.DokumentStatus = @DokumentStatus";
                    parameters.Add("@DokumentStatus", filter.DokumentStatus.Value);
                }

                if (filter.KomitentId.HasValue)
                {
                    sql += " AND si.PoslovodjaID = @KomitentId";
                    parameters.Add("@KomitentId", filter.KomitentId.Value);
                }

                sql += @" 
                    GROUP BY si.ID, si.Broj, si.Datum, si.Smena, si.DokumentStatus, 
                             k.Naziv, pp.Naziv, si.PoslovodjaID, si.Kreirano, si.Azurirano, si.Version
                    ORDER BY si.Datum DESC, si.Smena 
                    LIMIT 500";

                var rezultat = await _databaseService.QueryAsync<SmeneDaniIzvestajModel>(sql, parameters);
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajSmeneDaniIzvestaj: {ex.Message}");
                return new List<SmeneDaniIzvestajModel>();
            }
        } */

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
            await Task.Delay(100); // Simuliranje async poziva
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
            await Task.Delay(100); // Simuliranje async poziva
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
    }
}