using FruitSysWeb.Models;
using FruitSysWeb.Services.Core;
using FruitSysWeb.Services.Interfaces;

namespace FruitSysWeb.Services.Implementations.IzvestajService
{
    public class PromenService : BaseService, IPromenService
    {
        private readonly DatabaseService _databaseService;
        private readonly ILogger<PromenService> _logger;

        public PromenService(DatabaseService databaseService, ILogger<PromenService> logger)
        {
            _databaseService = databaseService;
            _logger = logger;
        }

        public async Task<PromenPeriodData> UcitajPromene(PromenFilter filter)
        {
            try
            {
                var tasks = await Task.WhenAll(
                    UcitajUlaz(filter),
                    UcitajIzlaz(filter),
                    UcitajFinansije(filter)
                );

                var ulaz = tasks[0] as List<PromenUlazModel> ?? new();
                var izlaz = tasks[1] as List<PromenIzlazModel> ?? new();
                var finansije = tasks[2] as List<PromenFinansijeModel> ?? new();

                return new PromenPeriodData
                {
                    // MagacinID: 2=Sveza Roba, 3=Sirovine, 5=Poluproizvodi, 6=Gotova Roba
                    UlazSirovineProizvodi = ulaz.Where(x => x.MagacinID is 2 or 3 or 5 or 6).ToList(),
                    // MagacinID: 4=Ambalaza (nepovratna)
                    UlazAmbalaza = ulaz.Where(x => x.MagacinID == 4).ToList(),
                    // MagacinID: 9=Repromaterijal, 10=Djubriva
                    UlazRepromaterijal = ulaz.Where(x => x.MagacinID is 9 or 10).ToList(),

                    IzlazGotovaRobaSirovine = izlaz.Where(x => x.MagacinID is 2 or 3 or 5 or 6).ToList(),
                    IzlazAmbalaza = izlaz.Where(x => x.MagacinID == 4).ToList(),
                    IzlazRepromaterijal = izlaz.Where(x => x.MagacinID is 9 or 10).ToList(),

                    Finansije = finansije
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju promena za period {Od}-{Do}", filter.OdDatum, filter.DoDatum);
                return new PromenPeriodData();
            }
        }

        private async Task<object> UcitajUlaz(PromenFilter filter)
        {
            var sql = @"
                SELECT
                    fm.DokumentID,
                    fm.Dokument,
                    fm.Datum,
                    fm.Artikal AS VrstaProizvoda,
                    fm.Komitent AS Dobavljac,
                    ABS(COALESCE(fm.Kolicina, 0)) AS Kolicina,
                    ABS(COALESCE(fm.PCenaPrijem, 0)) AS Cena,
                    ABS(COALESCE(fm.Potrazuje, 0)) AS Vrednost,
                    COALESCE(a.MagacinID, 0) AS MagacinID
                FROM vPrometFinansijev9 fm
                LEFT JOIN Artikal a ON fm.ArtikalID = a.ID
                WHERE fm.DokumentStatus != 4
                  AND fm.Dokument LIKE 'KL-%'
                  AND fm.ArtikalID IS NOT NULL
                  AND DATE(fm.Datum) >= @OdDatum
                  AND DATE(fm.Datum) <= @DoDatum
                ORDER BY fm.Datum, fm.DokumentID, fm.Artikal";

            var parameters = new Dictionary<string, object>
            {
                { "@OdDatum", filter.OdDatum.Date },
                { "@DoDatum", filter.DoDatum.Date }
            };

            return await _databaseService.QueryAsync<PromenUlazModel>(sql, parameters);
        }

        private async Task<object> UcitajIzlaz(PromenFilter filter)
        {
            var sql = @"
                SELECT
                    fm.DokumentID,
                    fm.Dokument,
                    fm.Datum,
                    fm.Artikal AS VrstaProizvoda,
                    fm.Komitent AS Kupac,
                    ABS(COALESCE(fm.Kolicina, 0)) AS Kolicina,
                    ABS(COALESCE(fm.PCenaPrijem, 0)) AS Cena,
                    GREATEST(ABS(COALESCE(fm.Duguje, 0)), ABS(COALESCE(fm.Potrazuje, 0))) AS Vrednost,
                    COALESCE(a.MagacinID, 0) AS MagacinID
                FROM vPrometFinansijev9 fm
                LEFT JOIN Artikal a ON fm.ArtikalID = a.ID
                WHERE fm.DokumentStatus != 4
                  AND fm.Dokument LIKE 'FK-%'
                  AND fm.ArtikalID IS NOT NULL
                  AND DATE(fm.Datum) >= @OdDatum
                  AND DATE(fm.Datum) <= @DoDatum
                ORDER BY fm.Datum, fm.DokumentID, fm.Artikal";

            var parameters = new Dictionary<string, object>
            {
                { "@OdDatum", filter.OdDatum.Date },
                { "@DoDatum", filter.DoDatum.Date }
            };

            return await _databaseService.QueryAsync<PromenIzlazModel>(sql, parameters);
        }

        private async Task<object> UcitajFinansije(PromenFilter filter)
        {
            // UP- i IS- dokumenti se grupišu po DokumentuID jer mogu imati više redova
            // Vrednost je u Potrazuje za UP- i Duguje za IS-
            var sql = @"
                SELECT
                    fm.DokumentID,
                    MIN(fm.Dokument) AS Dokument,
                    MIN(fm.Datum)    AS Datum,
                    CASE
                        WHEN MIN(fm.Dokument) LIKE 'UP-%' THEN 'Uplata'
                        WHEN MIN(fm.Dokument) LIKE 'IS-%' THEN 'Isplata'
                        ELSE ''
                    END AS TipPrometa,
                    MIN(fm.Komitent) AS Komitent,
                    ABS(SUM(COALESCE(fm.Potrazuje, 0))) AS Uplata,
                    ABS(SUM(COALESCE(fm.Duguje, 0)))    AS Isplata
                FROM vPrometFinansijev9 fm
                WHERE fm.DokumentStatus != 4
                  AND (fm.Dokument LIKE 'UP-%' OR fm.Dokument LIKE 'IS-%')
                  AND DATE(fm.Datum) >= @OdDatum
                  AND DATE(fm.Datum) <= @DoDatum
                GROUP BY fm.DokumentID
                ORDER BY MIN(fm.Datum), fm.DokumentID";

            var parameters = new Dictionary<string, object>
            {
                { "@OdDatum", filter.OdDatum.Date },
                { "@DoDatum", filter.DoDatum.Date }
            };

            return await _databaseService.QueryAsync<PromenFinansijeModel>(sql, parameters);
        }
    }
}
