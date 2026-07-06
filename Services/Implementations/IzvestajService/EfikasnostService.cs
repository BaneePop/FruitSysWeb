using FruitSysWeb.Models;
using FruitSysWeb.Services.Interfaces;
using Dapper;

namespace FruitSysWeb.Services.Implementations.IzvestajService
{
    public class EfikasnostService : IEfikasnostService
    {
        private readonly FruitSysWeb.Services.DatabaseService _db;
        private readonly ILogger<EfikasnostService> _logger;

        public EfikasnostService(FruitSysWeb.Services.DatabaseService db, ILogger<EfikasnostService> logger)
        {
            _db = db;
            _logger = logger;
        }

        // ─────────────────────────────────────────────────────────────
        // Kumulativ po poslovođi za period
        // ─────────────────────────────────────────────────────────────
        public async Task<List<EfikasnostPoslovodjeModel>> UcitajEfikasnostPoslovodja(EfikasnostFilter filter)
        {
            try
            {
                var sql = new System.Text.StringBuilder(@"
                    SELECT
                        r.ID AS PoslovodjaID,
                        r.ImePrezime AS Poslovodja,
                        COUNT(DISTINCT si.ID) AS BrojSmena,
                        COALESCE(SUM(er.BrojRadnihSati), 0) AS UkupnoRadnikaSati,
                        COUNT(er.ID) AS BrojEvidencija,
                        COUNT(DISTINCT er.RadniNalogID) AS BrojRadnihNaloga,
                        COALESCE(SUM(gp.KolicinaGP), 0) AS GotovProizvod
                    FROM SmenskiIzvestaj si
                    JOIN Radnik r ON si.PoslovodjaID = r.ID
                    LEFT JOIN EvidencijaRada er ON er.SmenskiIzvestajID = si.ID AND er.Obrisan = 0
                    LEFT JOIN (
                        SELECT EvidencijaRadaID, SUM(Kolicina) AS KolicinaGP
                        FROM RadniProcesArtikal
                        WHERE RpArtikalTip = 2 AND Storno = 0 AND RadniNalogID IS NULL
                        GROUP BY EvidencijaRadaID
                    ) gp ON gp.EvidencijaRadaID = er.ID
                    WHERE r.JePoslovodja = 1");

                var p = new DynamicParameters();

                if (filter.OdDatum.HasValue)
                {
                    sql.Append(" AND si.Datum >= @OdDatum");
                    p.Add("OdDatum", filter.OdDatum.Value.Date);
                }
                if (filter.DoDatum.HasValue)
                {
                    sql.Append(" AND si.Datum < @DoDatum");
                    p.Add("DoDatum", filter.DoDatum.Value.Date.AddDays(1));
                }
                if (filter.PoslovodjaID.HasValue)
                {
                    sql.Append(" AND r.ID = @PoslovodjaID");
                    p.Add("PoslovodjaID", filter.PoslovodjaID.Value);
                }
                if (filter.Smena.HasValue)
                {
                    sql.Append(" AND si.Smena = @Smena");
                    p.Add("Smena", filter.Smena.Value);
                }

                sql.Append(@"
                    GROUP BY r.ID, r.ImePrezime
                    ORDER BY UkupnoRadnikaSati DESC");

                return (await _db.QueryAsync<EfikasnostPoslovodjeModel>(sql.ToString(), p)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju efikasnosti poslovođa");
                throw;
            }
        }

        // ─────────────────────────────────────────────────────────────
        // Detalji po smeni za jednog poslovođu
        // ─────────────────────────────────────────────────────────────
        public async Task<List<EfikasnostSmenaDetaljiModel>> UcitajDetaljeSmena(long poslovodjaID, EfikasnostFilter filter)
        {
            try
            {
                var sql = new System.Text.StringBuilder(@"
                    SELECT
                        si.ID AS SmenskiIzvestajID,
                        si.Broj AS SmenskiBroj,
                        si.Datum,
                        si.Smena,
                        COUNT(er.ID) AS BrojEvidencija,
                        COUNT(DISTINCT er.RadniNalogID) AS BrojRadnihNaloga,
                        COALESCE(SUM(er.BrojRadnihSati), 0) AS UkupnoRadnikaSati,
                        COALESCE(SUM(er.BrojRadnika), 0) AS UkupnoBrojRadnika
                    FROM SmenskiIzvestaj si
                    LEFT JOIN EvidencijaRada er ON er.SmenskiIzvestajID = si.ID AND er.Obrisan = 0
                    WHERE si.PoslovodjaID = @PoslovodjaID");

                var p = new DynamicParameters();
                p.Add("PoslovodjaID", poslovodjaID);

                if (filter.OdDatum.HasValue)
                {
                    sql.Append(" AND si.Datum >= @OdDatum");
                    p.Add("OdDatum", filter.OdDatum.Value.Date);
                }
                if (filter.DoDatum.HasValue)
                {
                    sql.Append(" AND si.Datum < @DoDatum");
                    p.Add("DoDatum", filter.DoDatum.Value.Date.AddDays(1));
                }
                if (filter.Smena.HasValue)
                {
                    sql.Append(" AND si.Smena = @Smena");
                    p.Add("Smena", filter.Smena.Value);
                }

                sql.Append(@"
                    GROUP BY si.ID, si.Broj, si.Datum, si.Smena
                    ORDER BY si.Datum DESC
                    LIMIT 500");

                return (await _db.QueryAsync<EfikasnostSmenaDetaljiModel>(sql.ToString(), p)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju detalja smena za poslovođu {ID}", poslovodjaID);
                throw;
            }
        }

        // ─────────────────────────────────────────────────────────────
        // Dropdown — samo poslovođe
        // ─────────────────────────────────────────────────────────────
        public async Task<List<(long ID, string ImePrezime)>> UcitajPoslovodje()
        {
            const string sql = @"
                SELECT ID, ImePrezime
                FROM Radnik
                WHERE JePoslovodja = 1
                ORDER BY ImePrezime";
            var result = await _db.QueryAsync<(long ID, string ImePrezime)>(sql);
            return result.ToList();
        }
    }
}
