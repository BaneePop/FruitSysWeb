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

        public async Task<Dictionary<string, decimal>> UcitajTopDuznikeAsync(
            string tipAmbalazeFilter,
            FilterRequest filterRequest)
        {
            try
            {
                var sql = CreateSqlBuilder();
                sql.Append(@"
                    SELECT
                        COALESCE(k.Naziv, 'Nepoznato') as Komitent,
                        SUM(vrp.Izlaz) - SUM(vrp.Ulaz) as Duguje
                    FROM vPrometRobav5 vrp
                    INNER JOIN Artikal a ON vrp.ArtikalID = a.ID
                    LEFT JOIN Komitent k ON vrp.KomitentID = k.ID
                    WHERE vrp.DokumentStatus = 3
                        AND a.MagacinID = 4  -- Ambalaža
                        AND a.PovratnaAmbalaza = 1
                        AND a.PrijemnaAmbalaza = 1
                        AND a.AmbalazaTip IN (1, 3)  -- Gajba ili Kutija
                ");

                var parameters = CreateParameters();

                // Filter po tipu ambalaže
                if (!string.IsNullOrWhiteSpace(tipAmbalazeFilter))
                {
                    sql.Append(" AND a.Naziv LIKE @TipFilter");
                    parameters.Add("@TipFilter", $"%{tipAmbalazeFilter}%");
                }

                // Datum filter
                ApplyDateFilter(sql, parameters, filterRequest, "vrp.Datum");

                sql.Append(@"
                    GROUP BY k.ID, k.Naziv
                    HAVING SUM(vrp.Izlaz) - SUM(vrp.Ulaz) > 0
                    ORDER BY Duguje DESC
                    LIMIT 5
                ");

                var rezultat = await _databaseService.QueryAsync<dynamic>(sql.ToString(), parameters);

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

        public async Task<Dictionary<string, decimal>> UcitajTopPotrazujuceAsync(
            string tipAmbalazeFilter,
            FilterRequest filterRequest)
        {
            try
            {
                var sql = CreateSqlBuilder();
                sql.Append(@"
                    SELECT
                        COALESCE(k.Naziv, 'Nepoznato') as Komitent,
                        SUM(vrp.Ulaz) - SUM(vrp.Izlaz) as Potrazuje
                    FROM vPrometRobav5 vrp
                    INNER JOIN Artikal a ON vrp.ArtikalID = a.ID
                    LEFT JOIN Komitent k ON vrp.KomitentID = k.ID
                    WHERE vrp.DokumentStatus = 3
                        AND a.MagacinID = 4  -- Ambalaža
                        AND a.PovratnaAmbalaza = 1
                        AND a.PrijemnaAmbalaza = 1
                        AND a.AmbalazaTip IN (1, 3)  -- Gajba ili Kutija
                ");

                var parameters = CreateParameters();

                // Filter po tipu ambalaže
                if (!string.IsNullOrWhiteSpace(tipAmbalazeFilter))
                {
                    sql.Append(" AND a.Naziv LIKE @TipFilter");
                    parameters.Add("@TipFilter", $"%{tipAmbalazeFilter}%");
                }

                // Datum filter
                ApplyDateFilter(sql, parameters, filterRequest, "vrp.Datum");

                sql.Append(@"
                    GROUP BY k.ID, k.Naziv
                    HAVING SUM(vrp.Ulaz) - SUM(vrp.Izlaz) > 0
                    ORDER BY Potrazuje DESC
                    LIMIT 5
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

        public async Task<decimal> UcitajStanjeNaLageruAsync(
            string tipAmbalazeFilter,
            FilterRequest filterRequest)
        {
            try
            {
                var sql = CreateSqlBuilder();
                sql.Append(@"
                    SELECT
                        COALESCE(SUM(vrp.Ulaz) - SUM(vrp.Izlaz), 0) as Stanje
                    FROM vPrometRobav5 vrp
                    INNER JOIN Artikal a ON vrp.ArtikalID = a.ID
                    WHERE vrp.DokumentStatus = 3
                        AND a.MagacinID = 4  -- Ambalaža
                        AND a.PovratnaAmbalaza = 1
                        AND a.PrijemnaAmbalaza = 1
                        AND a.AmbalazaTip IN (1, 3)  -- Gajba ili Kutija
                ");

                var parameters = CreateParameters();

                // Filter po tipu ambalaže
                if (!string.IsNullOrWhiteSpace(tipAmbalazeFilter))
                {
                    sql.Append(" AND a.Naziv LIKE @TipFilter");
                    parameters.Add("@TipFilter", $"%{tipAmbalazeFilter}%");
                }

                // Datum filter
                ApplyDateFilter(sql, parameters, filterRequest, "vrp.Datum");

                var rezultat = await _databaseService.ExecuteScalarAsync<decimal>(sql.ToString(), parameters);
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
                        AND a.MagacinID = 4  -- Ambalaža
                        AND a.PovratnaAmbalaza = 1
                        AND a.PrijemnaAmbalaza = 1
                        AND a.AmbalazaTip IN (1, 3)  -- Gajba ili Kutija
                ");

                var parameters = CreateParameters();

                // Datum filter
                ApplyDateFilter(sql, parameters, filterRequest, "vrp.Datum");

                // Komitent filter
                if (filterRequest.KomitentId.HasValue && filterRequest.KomitentId > 0)
                {
                    sql.Append(" AND vrp.KomitentID = @KomitentId");
                    parameters.Add("@KomitentId", filterRequest.KomitentId.Value);
                }

                // Artikal filter
                if (filterRequest.ArtikalId.HasValue && filterRequest.ArtikalId > 0)
                {
                    sql.Append(" AND vrp.ArtikalID = @ArtikalId");
                    parameters.Add("@ArtikalId", filterRequest.ArtikalId.Value);
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
