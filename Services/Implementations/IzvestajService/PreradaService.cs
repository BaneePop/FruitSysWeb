using FruitSysWeb.Models;
using FruitSysWeb.Services.Interfaces;
using FruitSysWeb.Services.Models.Requests;
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

        #region EvidencijaRada metode

        public async Task<List<EvidencijaRadaModel>> UcitajSveEvidencijeRada()
        {
            try
            {
                var sql = @"
                    SELECT 
                        er.ID,
                        er.Version,
                        er.Naziv,
                        er.Kreirano,
                        er.Azurirano,
                        er.RezijaID,
                        r.Naziv as Rezija
                    FROM EvidencijaRada er
                    LEFT JOIN Rezija r ON er.RezijaID = r.ID
                    WHERE er.Naziv NOT LIKE '###########%'
                    ORDER BY er.Naziv";

                return (await _databaseService.QueryAsync<EvidencijaRadaModel>(sql)).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Greška pri učitavanju evidencije rada: {ex.Message}", ex);
            }
        }

        public async Task<EvidencijaRadaModel?> UcitajEvidencijuRadaPoId(long id)
        {
            try
            {
                var sql = @"
                    SELECT 
                        er.ID,
                        er.Version,
                        er.Naziv,
                        er.Kreirano,
                        er.Azurirano,
                        er.RezijaID,
                        r.Naziv as Rezija
                    FROM EvidencijaRada er
                    LEFT JOIN Rezija r ON er.RezijaID = r.ID
                    WHERE er.ID = @id";

                var parameters = new { id };
                return await _databaseService.QueryFirstOrDefaultAsync<EvidencijaRadaModel>(sql, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"Greška pri učitavanju evidencije rada po ID: {ex.Message}", ex);
            }
        }

        public async Task<List<EvidencijaRadaModel>> UcitajEvidencijeRadaPoReziji(long rezijaId)
        {
            try
            {
                var sql = @"
                    SELECT 
                        er.ID,
                        er.Version,
                        er.Naziv,
                        er.Kreirano,
                        er.Azurirano,
                        er.RezijaID,
                        r.Naziv as Rezija
                    FROM EvidencijaRada er
                    LEFT JOIN Rezija r ON er.RezijaID = r.ID
                    WHERE er.RezijaID = @rezijaId
                    ORDER BY er.Naziv";

                var parameters = new { rezijaId };
                return (await _databaseService.QueryAsync<EvidencijaRadaModel>(sql, parameters)).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Greška pri učitavanju evidencije rada po reziji: {ex.Message}", ex);
            }
        }

        public async Task<List<EvidencijaRadaModel>> UcitajEvidencijeRadaPoNazivu(string naziv)
        {
            try
            {
                var sql = @"
                    SELECT 
                        er.ID,
                        er.Version,
                        er.Naziv,
                        er.Kreirano,
                        er.Azurirano,
                        er.RezijaID,
                        r.Naziv as Rezija
                    FROM EvidencijaRada er
                    LEFT JOIN Rezija r ON er.RezijaID = r.ID
                    WHERE er.Naziv LIKE @naziv
                    ORDER BY er.Naziv";

                var parameters = new { naziv = $"%{naziv}%" };
                return (await _databaseService.QueryAsync<EvidencijaRadaModel>(sql, parameters)).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Greška pri učitavanju evidencije rada po nazivu: {ex.Message}", ex);
            }
        }

        #endregion

        #region RadniProces metode

        public async Task<List<RadniProcesModel>> UcitajSveRadneProcese()
        {
            try
            {
                var sql = @"
                    SELECT 
                        rp.ID,
                        rp.Naziv,
                        rp.Kreirano,
                        rp.Azurirano,
                        rp.Version
                    FROM RadniProces rp
                    ORDER BY rp.Naziv";

                return (await _databaseService.QueryAsync<RadniProcesModel>(sql)).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Greška pri učitavanju radnih procesa: {ex.Message}", ex);
            }
        }

        public async Task<RadniProcesModel?> UcitajRadniProcesPoId(long id)
        {
            try
            {
                var sql = @"
                    SELECT 
                        rp.ID,
                        rp.Naziv,
                        rp.Kreirano,
                        rp.Azurirano,
                        rp.Version
                    FROM RadniProces rp
                    WHERE rp.ID = @id";

                var parameters = new { id };
                return await _databaseService.QueryFirstOrDefaultAsync<RadniProcesModel>(sql, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"Greška pri učitavanju radnog procesa po ID: {ex.Message}", ex);
            }
        }

        public async Task<List<RadniProcesModel>> UcitajRadneProcesePoNazivu(string naziv)
        {
            try
            {
                var sql = @"
                    SELECT 
                        rp.ID,
                        rp.Naziv,
                        rp.Kreirano,
                        rp.Azurirano,
                        rp.Version
                    FROM RadniProces rp
                    WHERE rp.Naziv LIKE @naziv
                    ORDER BY rp.Naziv";

                var parameters = new { naziv = $"%{naziv}%" };
                return (await _databaseService.QueryAsync<RadniProcesModel>(sql, parameters)).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Greška pri učitavanju radnih procesa po nazivu: {ex.Message}", ex);
            }
        }

        public async Task<List<RadniProcesModel>> UcitajNoveRadneProcese(int dana = 30)
        {
            try
            {
                var sql = @"
                    SELECT 
                        rp.ID,
                        rp.Naziv,
                        rp.Kreirano,
                        rp.Azurirano,
                        rp.Version
                    FROM RadniProces rp
                    WHERE rp.Kreirano >= DATE_SUB(NOW(), INTERVAL @dana DAY)
                    ORDER BY rp.Kreirano DESC";

                var parameters = new { dana };
                return (await _databaseService.QueryAsync<RadniProcesModel>(sql, parameters)).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Greška pri učitavanju novih radnih procesa: {ex.Message}", ex);
            }
        }

        #endregion

        #region ProizvodniProces metode

        public async Task<List<ProizvodniProcesModel>> UcitajSveProizvodneProcese()
        {
            try
            {
                var sql = @"
                    SELECT 
                        pp.ID,
                        pp.Naziv,
                        pp.Kreirano,
                        pp.Azurirano,
                        pp.Version
                    FROM ProizvodniProces pp
                    ORDER BY pp.Naziv";

                return (await _databaseService.QueryAsync<ProizvodniProcesModel>(sql)).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Greška pri učitavanju proizvodnih procesa: {ex.Message}", ex);
            }
        }

        public async Task<ProizvodniProcesModel?> UcitajProizvodniProcesPoId(long id)
        {
            try
            {
                var sql = @"
                    SELECT 
                        pp.ID,
                        pp.Naziv,
                        pp.Kreirano,
                        pp.Azurirano,
                        pp.Version
                    FROM ProizvodniProces pp
                    WHERE pp.ID = @id";

                var parameters = new { id };
                return await _databaseService.QueryFirstOrDefaultAsync<ProizvodniProcesModel>(sql, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"Greška pri učitavanju proizvodnog procesa po ID: {ex.Message}", ex);
            }
        }

        public async Task<List<ProizvodniProcesModel>> UcitajProizvodneProcesePoKategoriji(string kategorija)
        {
            try
            {
                var sql = @"
                    SELECT 
                        pp.ID,
                        pp.Naziv,
                        pp.Kreirano,
                        pp.Azurirano,
                        pp.Version
                    FROM ProizvodniProces pp
                    WHERE pp.Naziv LIKE @kategorija
                    ORDER BY pp.Naziv";

                var parameters = new { kategorija = $"%{kategorija}%" };
                return (await _databaseService.QueryAsync<ProizvodniProcesModel>(sql, parameters)).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Greška pri učitavanju proizvodnih procesa po kategoriji: {ex.Message}", ex);
            }
        }

        public async Task<List<ProizvodniProcesModel>> UcitajProizvodneProcesePoNazivu(string naziv)
        {
            try
            {
                var sql = @"
                    SELECT 
                        pp.ID,
                        pp.Naziv,
                        pp.Kreirano,
                        pp.Azurirano,
                        pp.Version
                    FROM ProizvodniProces pp
                    WHERE pp.Naziv LIKE @naziv
                    ORDER BY pp.Naziv";

                var parameters = new { naziv = $"%{naziv}%" };
                return (await _databaseService.QueryAsync<ProizvodniProcesModel>(sql, parameters)).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Greška pri učitavanju proizvodnih procesa po nazivu: {ex.Message}", ex);
            }
        }

        #endregion

        #region SmenskiIzvestaj metode

        public async Task<List<SmenskiIzvestajModel>> UcitajSveSmenskeIzvestaje(FilterRequest filterRequest)
        {
            try
            {
                var sql = new StringBuilder();
                sql.Append(@"
                    SELECT 
                        si.ID,
                        si.Broj,
                        si.Datum,
                        si.Smena,
                        si.DokumentStatus,
                        si.Kreirano,
                        si.Azurirano,
                        si.Version,
                        si.PoslovodjaID,
                        k.Naziv as Poslovodja
                    FROM SmenskiIzvestaj si
                    LEFT JOIN Komitent k ON si.PoslovodjaID = k.ID
                    WHERE 1=1");

                if (filterRequest.OdDatum.HasValue)
                {
                    sql.Append(" AND si.Datum >= @odDatum");
                }

                if (filterRequest.DoDatum.HasValue)
                {
                    sql.Append(" AND si.Datum <= @doDatum");
                }

                if (filterRequest.KomitentId.HasValue)
                {
                    sql.Append(" AND si.PoslovodjaID = @komitentId");
                }

                sql.Append(" ORDER BY si.Datum DESC, si.Smena");

                var parameters = new
                {
                    odDatum = filterRequest.OdDatum,
                    doDatum = filterRequest.DoDatum,
                    komitentId = filterRequest.KomitentId
                };

                return (await _databaseService.QueryAsync<SmenskiIzvestajModel>(sql.ToString(), parameters)).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Greška pri učitavanju smenskih izveštaja: {ex.Message}", ex);
            }
        }

        public async Task<SmenskiIzvestajModel?> UcitajSmenskiIzvestajPoId(long id)
        {
            try
            {
                var sql = @"
                    SELECT 
                        si.ID,
                        si.Broj,
                        si.Datum,
                        si.Smena,
                        si.DokumentStatus,
                        si.Kreirano,
                        si.Azurirano,
                        si.Version,
                        si.PoslovodjaID,
                        k.Naziv as Poslovodja
                    FROM SmenskiIzvestaj si
                    LEFT JOIN Komitent k ON si.PoslovodjaID = k.ID
                    WHERE si.ID = @id";

                var parameters = new { id };
                return await _databaseService.QueryFirstOrDefaultAsync<SmenskiIzvestajModel>(sql, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"Greška pri učitavanju smenskog izveštaja po ID: {ex.Message}", ex);
            }
        }

        public async Task<List<SmenskiIzvestajModel>> UcitajSmenskeIzvestajePoDatumu(DateTime odDatum, DateTime doDatum)
        {
            try
            {
                var sql = @"
                    SELECT 
                        si.ID,
                        si.Broj,
                        si.Datum,
                        si.Smena,
                        si.DokumentStatus,
                        si.Kreirano,
                        si.Azurirano,
                        si.Version,
                        si.PoslovodjaID,
                        k.Naziv as Poslovodja
                    FROM SmenskiIzvestaj si
                    LEFT JOIN Komitent k ON si.PoslovodjaID = k.ID
                    WHERE si.Datum BETWEEN @odDatum AND @doDatum
                    ORDER BY si.Datum DESC, si.Smena";

                var parameters = new { odDatum, doDatum };
                return (await _databaseService.QueryAsync<SmenskiIzvestajModel>(sql, parameters)).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Greška pri učitavanju smenskih izveštaja po datumu: {ex.Message}", ex);
            }
        }

        public async Task<List<SmenskiIzvestajModel>> UcitajSmenskeIzvestajePoSmeni(int smena)
        {
            try
            {
                var sql = @"
                    SELECT 
                        si.ID,
                        si.Broj,
                        si.Datum,
                        si.Smena,
                        si.DokumentStatus,
                        si.Kreirano,
                        si.Azurirano,
                        si.Version,
                        si.PoslovodjaID,
                        k.Naziv as Poslovodja
                    FROM SmenskiIzvestaj si
                    LEFT JOIN Komitent k ON si.PoslovodjaID = k.ID
                    WHERE si.Smena = @smena
                    ORDER BY si.Datum DESC";

                var parameters = new { smena };
                return (await _databaseService.QueryAsync<SmenskiIzvestajModel>(sql, parameters)).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Greška pri učitavanju smenskih izveštaja po smeni: {ex.Message}", ex);
            }
        }

        public async Task<List<SmenskiIzvestajModel>> UcitajSmenskeIzvestajePoPoslovodji(long poslovodjaId)
        {
            try
            {
                var sql = @"
                    SELECT 
                        si.ID,
                        si.Broj,
                        si.Datum,
                        si.Smena,
                        si.DokumentStatus,
                        si.Kreirano,
                        si.Azurirano,
                        si.Version,
                        si.PoslovodjaID,
                        k.Naziv as Poslovodja
                    FROM SmenskiIzvestaj si
                    LEFT JOIN Komitent k ON si.PoslovodjaID = k.ID
                    WHERE si.PoslovodjaID = @poslovodjaId
                    ORDER BY si.Datum DESC";

                var parameters = new { poslovodjaId };
                return (await _databaseService.QueryAsync<SmenskiIzvestajModel>(sql, parameters)).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Greška pri učitavanju smenskih izveštaja po poslovođi: {ex.Message}", ex);
            }
        }

        public async Task<List<SmenskiIzvestajModel>> UcitajOtvoreneSmenskeIzvestaje()
        {
            try
            {
                var sql = @"
                    SELECT 
                        si.ID,
                        si.Broj,
                        si.Datum,
                        si.Smena,
                        si.DokumentStatus,
                        si.Kreirano,
                        si.Azurirano,
                        si.Version,
                        si.PoslovodjaID,
                        k.Naziv as Poslovodja
                    FROM SmenskiIzvestaj si
                    LEFT JOIN Komitent k ON si.PoslovodjaID = k.ID
                    WHERE si.DokumentStatus = 2
                    ORDER BY si.Datum DESC";

                return (await _databaseService.QueryAsync<SmenskiIzvestajModel>(sql)).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Greška pri učitavanju otvorenih smenskih izveštaja: {ex.Message}", ex);
            }
        }

        public async Task<List<SmenskiIzvestajModel>> UcitajZakljuceneSmenskeIzvestaje()
        {
            try
            {
                var sql = @"
                    SELECT 
                        si.ID,
                        si.Broj,
                        si.Datum,
                        si.Smena,
                        si.DokumentStatus,
                        si.Kreirano,
                        si.Azurirano,
                        si.Version,
                        si.PoslovodjaID,
                        k.Naziv as Poslovodja
                    FROM SmenskiIzvestaj si
                    LEFT JOIN Komitent k ON si.PoslovodjaID = k.ID
                    WHERE si.DokumentStatus = 3
                    ORDER BY si.Datum DESC";

                return (await _databaseService.QueryAsync<SmenskiIzvestajModel>(sql)).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Greška pri učitavanju zaključenih smenskih izveštaja: {ex.Message}", ex);
            }
        }

        #endregion

        #region Analitičke metode

        public async Task<Dictionary<string, int>> UcitajStatistikuPoSmenama(FilterRequest filterRequest)
        {
            try
            {
                var sql = new StringBuilder();
                sql.Append(@"
                    SELECT 
                        CASE si.Smena
                            WHEN 1 THEN 'Prva smena'
                            WHEN 2 THEN 'Druga smena'
                            WHEN 3 THEN 'Treća smena'
                            ELSE CONCAT('Smena ', si.Smena)
                        END as Smena,
                        COUNT(*) as Broj
                    FROM SmenskiIzvestaj si
                    WHERE 1=1");

                if (filterRequest.OdDatum.HasValue)
                {
                    sql.Append(" AND si.Datum >= @odDatum");
                }

                if (filterRequest.DoDatum.HasValue)
                {
                    sql.Append(" AND si.Datum <= @doDatum");
                }

                sql.Append(" GROUP BY si.Smena ORDER BY si.Smena");

                var parameters = new
                {
                    odDatum = filterRequest.OdDatum,
                    doDatum = filterRequest.DoDatum
                };

                var results = await _databaseService.QueryAsync<dynamic>(sql.ToString(), parameters);
                return results.ToDictionary(x => (string)x.Smena, x => (int)x.Broj);
            }
            catch (Exception ex)
            {
                throw new Exception($"Greška pri učitavanju statistike po smenama: {ex.Message}", ex);
            }
        }

        public async Task<Dictionary<string, int>> UcitajStatistikuPoPoslovodjama(FilterRequest filterRequest)
        {
            try
            {
                var sql = new StringBuilder();
                sql.Append(@"
                    SELECT 
                        COALESCE(k.Naziv, 'Nepoznato') as Poslovodja,
                        COUNT(*) as Broj
                    FROM SmenskiIzvestaj si
                    LEFT JOIN Komitent k ON si.PoslovodjaID = k.ID
                    WHERE 1=1");

                if (filterRequest.OdDatum.HasValue)
                {
                    sql.Append(" AND si.Datum >= @odDatum");
                }

                if (filterRequest.DoDatum.HasValue)
                {
                    sql.Append(" AND si.Datum <= @doDatum");
                }

                sql.Append(" GROUP BY si.PoslovodjaID, k.Naziv ORDER BY Broj DESC");

                var parameters = new
                {
                    odDatum = filterRequest.OdDatum,
                    doDatum = filterRequest.DoDatum
                };

                var results = await _databaseService.QueryAsync<dynamic>(sql.ToString(), parameters);
                return results.ToDictionary(x => (string)x.Poslovodja, x => (int)x.Broj);
            }
            catch (Exception ex)
            {
                throw new Exception($"Greška pri učitavanju statistike po poslovođama: {ex.Message}", ex);
            }
        }

        public async Task<Dictionary<string, int>> UcitajStatistikuPoStatusima(FilterRequest filterRequest)
        {
            try
            {
                var sql = new StringBuilder();
                sql.Append(@"
                    SELECT 
                        CASE si.DokumentStatus
                            WHEN 2 THEN 'Otvoren'
                            WHEN 3 THEN 'Zaključen'
                            WHEN 4 THEN 'Storno'
                            ELSE 'Nepoznato'
                        END as Status,
                        COUNT(*) as Broj
                    FROM SmenskiIzvestaj si
                    WHERE 1=1");

                if (filterRequest.OdDatum.HasValue)
                {
                    sql.Append(" AND si.Datum >= @odDatum");
                }

                if (filterRequest.DoDatum.HasValue)
                {
                    sql.Append(" AND si.Datum <= @doDatum");
                }

                sql.Append(" GROUP BY si.DokumentStatus ORDER BY si.DokumentStatus");

                var parameters = new
                {
                    odDatum = filterRequest.OdDatum,
                    doDatum = filterRequest.DoDatum
                };

                var results = await _databaseService.QueryAsync<dynamic>(sql.ToString(), parameters);
                return results.ToDictionary(x => (string)x.Status, x => (int)x.Broj);
            }
            catch (Exception ex)
            {
                throw new Exception($"Greška pri učitavanju statistike po statusima: {ex.Message}", ex);
            }
        }

        public async Task<int> UcitajUkupanBrojSmenskihIzvestaja(FilterRequest filterRequest)
        {
            try
            {
                var sql = new StringBuilder();
                sql.Append("SELECT COUNT(*) FROM SmenskiIzvestaj si WHERE 1=1");

                if (filterRequest.OdDatum.HasValue)
                {
                    sql.Append(" AND si.Datum >= @odDatum");
                }

                if (filterRequest.DoDatum.HasValue)
                {
                    sql.Append(" AND si.Datum <= @doDatum");
                }

                var parameters = new
                {
                    odDatum = filterRequest.OdDatum,
                    doDatum = filterRequest.DoDatum
                };

                return await _databaseService.QuerySingleAsync<int>(sql.ToString(), parameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"Greška pri učitavanju ukupnog broja smenskih izveštaja: {ex.Message}", ex);
            }
        }

        public async Task<int> UcitajBrojOtvorenihSmenskihIzvestaja()
        {
            try
            {
                var sql = "SELECT COUNT(*) FROM SmenskiIzvestaj WHERE DokumentStatus = 2";
                return await _databaseService.QuerySingleAsync<int>(sql);
            }
            catch (Exception ex)
            {
                throw new Exception($"Greška pri učitavanju broja otvorenih smenskih izveštaja: {ex.Message}", ex);
            }
        }

        public async Task<int> UcitajBrojZakljucenihSmenskihIzvestaja()
        {
            try
            {
                var sql = "SELECT COUNT(*) FROM SmenskiIzvestaj WHERE DokumentStatus = 3";
                return await _databaseService.QuerySingleAsync<int>(sql);
            }
            catch (Exception ex)
            {
                throw new Exception($"Greška pri učitavanju broja zaključenih smenskih izveštaja: {ex.Message}", ex);
            }
        }

        #endregion

        #region Dashboard metode

        public async Task<Dictionary<string, decimal>> UcitajTopRadneProcese(FilterRequest filterRequest)
        {
            try
            {
                // Ova metoda bi trebalo da se implementira kada budemo imali podatke o korišćenju procesa
                // Za sada vraćamo prazan dictionary
                return new Dictionary<string, decimal>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Greška pri učitavanju top radnih procesa: {ex.Message}", ex);
            }
        }

        public async Task<Dictionary<string, decimal>> UcitajTopProizvodneProcese(FilterRequest filterRequest)
        {
            try
            {
                // Ova metoda bi trebalo da se implementira kada budemo imali podatke o korišćenju procesa
                // Za sada vraćamo prazan dictionary
                return new Dictionary<string, decimal>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Greška pri učitavanju top proizvodnih procesa: {ex.Message}", ex);
            }
        }

        public async Task<decimal> UcitajUkupnuAktivnost(FilterRequest filterRequest)
        {
            try
            {
                // Ova metoda bi trebalo da se implementira kada budemo imali podatke o aktivnosti
                // Za sada vraćamo 0
                return 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Greška pri učitavanju ukupne aktivnosti: {ex.Message}", ex);
            }
        }

        #endregion
    }
}
