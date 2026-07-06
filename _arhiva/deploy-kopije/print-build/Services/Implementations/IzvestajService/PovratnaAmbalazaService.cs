using FruitSysWeb.Models;
using FruitSysWeb.Services.Interfaces;
using FruitSysWeb.Services.Models.Requests;
using FruitSysWeb.Services.Core;
using Microsoft.Extensions.Logging;

namespace FruitSysWeb.Services.Implementations.IzvestajService
{
    public class PovratnaAmbalazaService : BaseService, IPovratnaAmbalazaService
    {
        private readonly DatabaseService _databaseService;
        private readonly ILogger<PovratnaAmbalazaService> _logger;

        public PovratnaAmbalazaService(
            DatabaseService databaseService,
            ILogger<PovratnaAmbalazaService> logger)
        {
            _databaseService = databaseService;
            _logger = logger;
        }

        /// <summary>
        /// JEDAN upit za SVE podatke - učitava sve komitenate + artikle agregat
        /// </summary>
        public async Task<List<PovratnaAmbalazaAgregat>> UcitajSveAgregiraneAsync(FilterRequest filterRequest)
        {
            try
            {
                var sql = CreateSqlBuilder();
                var parameters = CreateParameters();

                // JEDAN upit za SVE artikle PVC ambalaže
                sql.Append(@"
                    SELECT
                        agg.KomitentID,
                        COALESCE(k.Naziv, 'Nepoznato') as Komitent,
                        agg.ArtikalID,
                        agg.UkupnoUlaz,
                        agg.UkupnoIzlaz
                    FROM (
                        SELECT
                            vrp.KomitentID,
                            vrp.ArtikalID,
                            SUM(vrp.Ulaz) as UkupnoUlaz,
                            SUM(vrp.Izlaz) as UkupnoIzlaz
                        FROM vPrometRobav5 vrp
                        WHERE vrp.DokumentStatus = 3
                            AND vrp.ArtikalID IN (206, 75, 79, 281)  -- PVC 4/1, 6/1, 12/1, 10/1
                ");

                // Datum filter
                ApplyDateFilter(sql, parameters, filterRequest, "vrp.Datum");

                sql.Append(@"
                        GROUP BY vrp.KomitentID, vrp.ArtikalID
                    ) agg
                    LEFT JOIN Komitent k ON agg.KomitentID = k.ID
                    ORDER BY agg.KomitentID, agg.ArtikalID
                ");

                _logger.LogInformation("Učitavam SVE agregate jednim upitom");

                var rezultat = await _databaseService.QueryAsync<PovratnaAmbalazaAgregat>(sql.ToString(), parameters);

                _logger.LogInformation("Učitano {Count} agregata", rezultat.Count());

                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajSveAgregiraneAsync");
                return new List<PovratnaAmbalazaAgregat>();
            }
        }

        public async Task<Dictionary<string, decimal>> UcitajTopDuznikeAsync(
            string tipAmbalazeFilter,
            FilterRequest filterRequest)
        {
            try
            {
                // Map tip naziva na Artikal ID
                long? artikalId = tipAmbalazeFilter switch
                {
                    "PVC 4/1" => 206,
                    "PVC 6/1" => 75,
                    "PVC 12/1" => 79,
                    "PVC 10/1" => 281,
                    _ => null
                };

                if (!artikalId.HasValue)
                {
                    _logger.LogWarning("Nepoznat tip ambalaže: {Tip}", tipAmbalazeFilter);
                    return new Dictionary<string, decimal>();
                }

                var sql = CreateSqlBuilder();
                var parameters = CreateParameters();

                // ULTRA-optimizovan SQL: Agrega cija BEZ JOIN-a, onda JOIN samo za TOP 5!
                sql.Append(@"
                    SELECT
                        agg.KomitentID,
                        COALESCE(k.Naziv, 'Nepoznato') as Komitent,
                        agg.Duguje
                    FROM (
                        SELECT
                            vrp.KomitentID,
                            SUM(vrp.Izlaz - vrp.Ulaz) as Duguje
                        FROM vPrometRobav5 vrp
                        WHERE vrp.DokumentStatus = 3
                            AND vrp.ArtikalID = @ArtikalId
                ");

                parameters.Add("@ArtikalId", artikalId.Value);

                // Datum filter
                ApplyDateFilter(sql, parameters, filterRequest, "vrp.Datum");

                sql.Append(@"
                        GROUP BY vrp.KomitentID
                        HAVING Duguje > 0
                        ORDER BY Duguje DESC
                        LIMIT 5
                    ) agg
                    LEFT JOIN Komitent k ON agg.KomitentID = k.ID
                    ORDER BY agg.Duguje DESC
                ");

                _logger.LogInformation("SQL Upit za {Tip} (ID={Id}): {Sql}", tipAmbalazeFilter, artikalId, sql.ToString());

                var rezultat = await _databaseService.QueryAsync<dynamic>(sql.ToString(), parameters);

                _logger.LogInformation("Pronađeno {Count} dužnika za {Tip}", rezultat.Count(), tipAmbalazeFilter);

                return rezultat.ToDictionary(
                    x => (string)x.Komitent ?? "Nepoznato",
                    x => (decimal)x.Duguje
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajTopDuznikeAsync za tip: {Tip}", tipAmbalazeFilter);
                return new Dictionary<string, decimal>();
            }
        }

        public async Task<Dictionary<string, decimal>> UcitajTopDuznikeKombinovanoAsync(
            List<string> tipAmbalazeFiltri,
            FilterRequest filterRequest)
        {
            try
            {
                if (tipAmbalazeFiltri == null || tipAmbalazeFiltri.Count == 0)
                {
                    return [];
                }

                // Map tipove na Artikal IDs
                var artikalIds = new List<long>();
                foreach (var tip in tipAmbalazeFiltri)
                {
                    var id = tip switch
                    {
                        "PVC 4/1" => 206,
                        "PVC 6/1" => 75,
                        "PVC 12/1" => 79,
                        "PVC 10/1" => 281,
                        _ => (long?)null
                    };

                    if (id.HasValue)
                        artikalIds.Add(id.Value);
                }

                if (artikalIds.Count == 0)
                {
                    _logger.LogWarning("Nijedan validan tip ambalaže u listi: {Tipovi}", string.Join(", ", tipAmbalazeFiltri));
                    return new Dictionary<string, decimal>();
                }

                var sql = CreateSqlBuilder();
                var parameters = CreateParameters();

                // ULTRA-optimizovan SQL: Agregacija BEZ JOIN-a, onda JOIN samo za TOP 5!
                sql.Append(@"
                    SELECT
                        agg.KomitentID,
                        COALESCE(k.Naziv, 'Nepoznato') as Komitent,
                        agg.Duguje
                    FROM (
                        SELECT
                            vrp.KomitentID,
                            SUM(vrp.Izlaz - vrp.Ulaz) as Duguje
                        FROM vPrometRobav5 vrp
                        WHERE vrp.DokumentStatus = 3
                            AND vrp.ArtikalID IN @ArtikalIds
                ");

                parameters.Add("@ArtikalIds", artikalIds);

                // Datum filter
                ApplyDateFilter(sql, parameters, filterRequest, "vrp.Datum");

                sql.Append(@"
                        GROUP BY vrp.KomitentID
                        HAVING Duguje > 0
                        ORDER BY Duguje DESC
                        LIMIT 5
                    ) agg
                    LEFT JOIN Komitent k ON agg.KomitentID = k.ID
                    ORDER BY agg.Duguje DESC
                ");

                _logger.LogInformation("SQL Upit za {Tipovi} (IDs={Ids}): {Sql}", string.Join(", ", tipAmbalazeFiltri), string.Join(",", artikalIds), sql.ToString());

                var rezultat = await _databaseService.QueryAsync<dynamic>(sql.ToString(), parameters);

                _logger.LogInformation("Pronađeno {Count} dužnika za {Tipovi}", rezultat.Count(), string.Join(", ", tipAmbalazeFiltri));

                return rezultat.ToDictionary(
                    x => (string)x.Komitent ?? "Nepoznato",
                    x => (decimal)x.Duguje
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajTopDuznikeKombinovanoAsync za tipove: {Tipovi}", string.Join(", ", tipAmbalazeFiltri));
                return new Dictionary<string, decimal>();
            }
        }

        public async Task<Dictionary<string, decimal>> UcitajTopPotrazujuceAsync(
            string tipAmbalazeFilter,
            FilterRequest filterRequest)
        {
            try
            {
                // Map tip naziva na Artikal ID
                long? artikalId = tipAmbalazeFilter switch
                {
                    "PVC 4/1" => 206,
                    "PVC 6/1" => 75,
                    "PVC 12/1" => 79,
                    "PVC 10/1" => 281,
                    _ => null
                };

                if (!artikalId.HasValue)
                {
                    _logger.LogWarning("Nepoznat tip ambalaže: {Tip}", tipAmbalazeFilter);
                    return new Dictionary<string, decimal>();
                }

                var sql = CreateSqlBuilder();
                var parameters = CreateParameters();

                // ULTRA-optimizovan SQL: Agregacija BEZ JOIN-a, onda JOIN samo za TOP 5!
                sql.Append(@"
                    SELECT
                        agg.KomitentID,
                        COALESCE(k.Naziv, 'Nepoznato') as Komitent,
                        agg.Potrazuje
                    FROM (
                        SELECT
                            vrp.KomitentID,
                            SUM(vrp.Ulaz - vrp.Izlaz) as Potrazuje
                        FROM vPrometRobav5 vrp
                        WHERE vrp.DokumentStatus = 3
                            AND vrp.ArtikalID = @ArtikalId
                ");

                parameters.Add("@ArtikalId", artikalId.Value);

                // Datum filter
                ApplyDateFilter(sql, parameters, filterRequest, "vrp.Datum");

                sql.Append(@"
                        GROUP BY vrp.KomitentID
                        HAVING Potrazuje > 0
                        ORDER BY Potrazuje DESC
                        LIMIT 5
                    ) agg
                    LEFT JOIN Komitent k ON agg.KomitentID = k.ID
                    ORDER BY agg.Potrazuje DESC
                ");

                var rezultat = await _databaseService.QueryAsync<dynamic>(sql.ToString(), parameters);

                return rezultat.ToDictionary(
                    x => (string)x.Komitent ?? "Nepoznato",
                    x => (decimal)x.Potrazuje
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajTopPotrazujuceAsync za tip: {Tip}", tipAmbalazeFilter);
                return new Dictionary<string, decimal>();
            }
        }

        public async Task<Dictionary<string, decimal>> UcitajTopPotrazujuceKombinovanoAsync(
            List<string> tipAmbalazeFiltri,
            FilterRequest filterRequest)
        {
            try
            {
                if (tipAmbalazeFiltri == null || tipAmbalazeFiltri.Count == 0)
                {
                    return [];
                }

                // Map tipove na Artikal IDs
                var artikalIds = new List<long>();
                foreach (var tip in tipAmbalazeFiltri)
                {
                    var id = tip switch
                    {
                        "PVC 4/1" => 206,
                        "PVC 6/1" => 75,
                        "PVC 12/1" => 79,
                        "PVC 10/1" => 281,
                        _ => (long?)null
                    };

                    if (id.HasValue)
                        artikalIds.Add(id.Value);
                }

                if (artikalIds.Count == 0)
                {
                    _logger.LogWarning("Nijedan validan tip ambalaže u listi: {Tipovi}", string.Join(", ", tipAmbalazeFiltri));
                    return new Dictionary<string, decimal>();
                }

                var sql = CreateSqlBuilder();
                var parameters = CreateParameters();

                // ULTRA-optimizovan SQL: Agregacija BEZ JOIN-a, onda JOIN samo za TOP 5!
                sql.Append(@"
                    SELECT
                        agg.KomitentID,
                        COALESCE(k.Naziv, 'Nepoznato') as Komitent,
                        agg.Potrazuje
                    FROM (
                        SELECT
                            vrp.KomitentID,
                            SUM(vrp.Ulaz - vrp.Izlaz) as Potrazuje
                        FROM vPrometRobav5 vrp
                        WHERE vrp.DokumentStatus = 3
                            AND vrp.ArtikalID IN @ArtikalIds
                ");

                parameters.Add("@ArtikalIds", artikalIds);

                // Datum filter
                ApplyDateFilter(sql, parameters, filterRequest, "vrp.Datum");

                sql.Append(@"
                        GROUP BY vrp.KomitentID
                        HAVING Potrazuje > 0
                        ORDER BY Potrazuje DESC
                        LIMIT 5
                    ) agg
                    LEFT JOIN Komitent k ON agg.KomitentID = k.ID
                    ORDER BY agg.Potrazuje DESC
                ");

                var rezultat = await _databaseService.QueryAsync<dynamic>(sql.ToString(), parameters);

                return rezultat.ToDictionary(
                    x => (string)x.Komitent ?? "Nepoznato",
                    x => (decimal)x.Potrazuje
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajTopPotrazujuceKombinovanoAsync za tipove: {Tipovi}", string.Join(", ", tipAmbalazeFiltri));
                return new Dictionary<string, decimal>();
            }
        }

        public async Task<decimal> UcitajStanjeNaLageruAsync(
            string tipAmbalazeFilter,
            FilterRequest filterRequest)
        {
            try
            {
                // Map tip naziva na Artikal ID
                long? artikalId = tipAmbalazeFilter switch
                {
                    "PVC 4/1" => 206,
                    "PVC 6/1" => 75,
                    "PVC 12/1" => 79,
                    "PVC 10/1" => 281,
                    _ => null
                };

                if (!artikalId.HasValue)
                {
                    _logger.LogWarning("Nepoznat tip ambalaže: {Tip}", tipAmbalazeFilter);
                    return 0;
                }

                // Koristi vwMagacinLager za trenutno stanje na lageru (kao MagacinLagerService)
                var sql = @"
                    SELECT
                        COALESCE(SUM(ml.Kolicina), 0) as Stanje
                    FROM vwMagacinLager ml
                    WHERE ml.ArtikalID = @ArtikalId
                      AND ml.Kolicina IS NOT NULL
                ";

                var parameters = CreateParameters();
                parameters.Add("@ArtikalId", artikalId.Value);

                var rezultat = await _databaseService.ExecuteScalarAsync<decimal>(sql, parameters);

                _logger.LogInformation("Stanje za {Tip} (ID={Id}): {Stanje}", tipAmbalazeFilter, artikalId, rezultat);

                return rezultat;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajStanjeNaLageruAsync za tip: {Tip}", tipAmbalazeFilter);
                return 0;
            }
        }

        public async Task<List<PovratnaAmbalazaModel>> UcitajDetaljniIzvestajAsync(FilterRequest filterRequest)
        {
            try
            {
                var sql = CreateSqlBuilder();
                var parameters = CreateParameters();

                // SQL SAMO za 4 PVC artikla (206, 75, 79, 281)
                sql.Append(@"
                    SELECT
                        vrp.Datum,
                        vrp.KomitentID,
                        COALESCE(k.Naziv, 'Nepoznato') as Komitent,
                        vrp.Dokument,
                        vrp.ArtikalID,
                        a.Naziv as Artikal,
                        vrp.Ulaz,
                        vrp.Izlaz
                    FROM vPrometRobav5 vrp
                    INNER JOIN Artikal a ON vrp.ArtikalID = a.ID
                    LEFT JOIN Komitent k ON vrp.KomitentID = k.ID
                    WHERE vrp.DokumentStatus = 3
                ");

                // Artikal filter - ako je izabran, samo taj artikal; inače svi PVC
                if (filterRequest.ArtikalId.HasValue && filterRequest.ArtikalId > 0)
                {
                    sql.Append(" AND vrp.ArtikalID = @ArtikalId");
                    parameters.Add("@ArtikalId", filterRequest.ArtikalId.Value);
                }
                else
                {
                    // Ako nije izabran, prikaži samo 4 PVC artikla
                    sql.Append(" AND vrp.ArtikalID IN (206, 75, 79, 281)");
                }

                // Datum filter
                ApplyDateFilter(sql, parameters, filterRequest, "vrp.Datum");

                // Komitent filter
                if (filterRequest.KomitentId.HasValue && filterRequest.KomitentId > 0)
                {
                    sql.Append(" AND vrp.KomitentID = @KomitentId");
                    parameters.Add("@KomitentId", filterRequest.KomitentId.Value);
                }

                sql.Append(" ORDER BY vrp.Datum DESC, k.Naziv, a.Naziv");

                var rezultat = await _databaseService.QueryAsync<PovratnaAmbalazaModel>(sql.ToString(), parameters);
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajDetaljniIzvestajAsync");
                return new List<PovratnaAmbalazaModel>();
            }
        }
    }
}
