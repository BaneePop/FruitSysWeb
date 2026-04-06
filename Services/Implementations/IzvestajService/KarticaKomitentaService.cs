using FruitSysWeb.Models;
using FruitSysWeb.Services.Interfaces;
using FruitSysWeb.Services.Models.Requests;
using FruitSysWeb.Services.Core;
using Microsoft.Extensions.Logging;

namespace FruitSysWeb.Services.Implementations.IzvestajService
{
    public class KarticaKomitentaService : BaseService, IKarticaKomitentaService
    {
        private readonly DatabaseService _db;
        private readonly ILogger<KarticaKomitentaService> _logger;

        public KarticaKomitentaService(DatabaseService db, ILogger<KarticaKomitentaService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<List<KarticaStavkaModel>> UcitajKarticu(long komitentId, FilterRequest filterRequest)
        {
            try
            {
                var sql = CreateSqlBuilder(@"
                    SELECT
                        vp.ID,
                        vp.KomitentID,
                        vp.Komitent,
                        vp.Datum,
                        vp.Dokument,
                        vp.DokumentTip,
                        vp.DokumentStatus,
                        vp.Artikal,
                        COALESCE(vp.Kolicina, 0)  AS Kolicina,
                        COALESCE(vp.Potrazuje, 0) AS Potrazuje,
                        COALESCE(vp.Duguje, 0)    AS Duguje
                    FROM vPrometFinansijev9 vp
                    WHERE vp.KomitentID = @KomitentId
                      AND vp.DokumentStatus != 4");

                var parameters = CreateParameters();
                parameters.Add("@KomitentId", komitentId);

                ApplyDateFilter(sql, parameters, filterRequest, "vp.Datum");

                sql.Append(" ORDER BY vp.Datum ASC, vp.ID ASC");

                var stavke = (await _db.QueryAsync<KarticaStavkaModel>(sql.ToString(), parameters)).ToList();

                // Tekući (kumulativni) saldo: potražuje = komitent duguje nama (+), duguje = mi dugujemo njima (-)
                decimal saldo = 0;
                foreach (var s in stavke)
                {
                    saldo += s.Potrazuje - s.Duguje;
                    s.TekuciSaldo = saldo;
                }

                return stavke;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajKarticu za KomitentId={Id}", komitentId);
                return new List<KarticaStavkaModel>();
            }
        }

        public async Task<KarticaKomitentaSumaModel?> UcitajSumu(long komitentId, FilterRequest filterRequest)
        {
            try
            {
                var sql = CreateSqlBuilder(@"
                    SELECT
                        vp.KomitentID,
                        vp.Komitent,
                        COALESCE(SUM(vp.Potrazuje), 0) AS UkupnoPotrazuje,
                        COALESCE(SUM(vp.Duguje),    0) AS UkupnoDuguje,
                        COUNT(*)                        AS BrojTransakcija
                    FROM vPrometFinansijev9 vp
                    WHERE vp.KomitentID = @KomitentId
                      AND vp.DokumentStatus != 4");

                var parameters = CreateParameters();
                parameters.Add("@KomitentId", komitentId);

                ApplyDateFilter(sql, parameters, filterRequest, "vp.Datum");

                sql.Append(" GROUP BY vp.KomitentID, vp.Komitent");

                var suma = await _db.QueryFirstOrDefaultAsync<KarticaKomitentaSumaModel>(sql.ToString(), parameters);

                if (suma != null)
                    suma.ZavrsniSaldo = suma.UkupnoPotrazuje - suma.UkupnoDuguje;

                return suma;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajSumu za KomitentId={Id}", komitentId);
                return null;
            }
        }
    }
}
