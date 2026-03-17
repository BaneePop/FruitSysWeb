using FruitSysWeb.Models;
using FruitSysWeb.Services.Interfaces;
using FruitSysWeb.Services.Models.Requests;
using FruitSysWeb.Services.Core;
using Microsoft.Extensions.Logging;

namespace FruitSysWeb.Services.Implementations.IzvestajService
{
    public class KontrolaService : BaseService, IKontrolaService
    {
        private readonly DatabaseService _db;
        private readonly ILogger<KontrolaService> _logger;

        public KontrolaService(DatabaseService db, ILogger<KontrolaService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<KontrolaRadniNalogModel> UcitajKontrole(long radniNalogId)
        {
            var result = new KontrolaRadniNalogModel();

            try
            {
                var tasks = new[]
                {
                    UcitajHeaderAsync(radniNalogId).ContinueWith(t => { result.Header = t.Result; }),
                    UcitajProizvodnjaAsync(radniNalogId).ContinueWith(t => { result.KontroleProzvodnje = t.Result; }),
                    UcitajMetalDetektorAsync(radniNalogId).ContinueWith(t => { result.MetalDetektor = t.Result; }),
                    UcitajTemperaturaAsync(radniNalogId).ContinueWith(t => { result.Temperature = t.Result; }),
                    UcitajTezineAsync(radniNalogId).ContinueWith(t => { result.Tezine = t.Result; }),
                    UcitajZavrsneAsync(radniNalogId).ContinueWith(t => { result.ZavrsneKontrole = t.Result; })
                };

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajKontrole za RadniNalogId={Id}", radniNalogId);
            }

            return result;
        }

        public async Task<List<KontrolaRadniNalogHeaderModel>> PretragaRadnihNaloga(FilterRequest filterRequest, string? komitent = null)
        {
            try
            {
                var sql = CreateSqlBuilder(@"
                    SELECT
                        rn.ID AS RadniNalogID,
                        rn.Sifra AS RadniNalogSifra,
                        COALESCE(k.Naziv, '') AS Komitent,
                        COALESCE(a.Naziv, '') AS Artikal,
                        COALESCE(p.Naziv, '') AS Pakovanje,
                        rn.LotNaloga,
                        MIN(si.Datum) AS DatumPocetka,
                        MAX(si.Datum) AS DatumZavrsetka
                    FROM RadniNalog rn
                    LEFT JOIN Komitent k ON rn.KomitentID = k.ID
                    LEFT JOIN ArtikalInstanca ai ON rn.ArtikalInstancaID = ai.ID
                    LEFT JOIN Artikal a ON ai.ArtikalID = a.ID
                    LEFT JOIN Pakovanje p ON ai.PakovanjeID = p.ID
                    LEFT JOIN EvidencijaRada er ON er.RadniNalogID = rn.ID
                    LEFT JOIN SmenskiIzvestaj si ON er.SmenskiIzvestajID = si.ID
                    WHERE rn.Aktivno = 1
                      AND rn.Sifra IS NOT NULL
                      AND rn.Sifra NOT LIKE 'ST-%'");

                var parameters = CreateParameters();

                if (!string.IsNullOrWhiteSpace(komitent))
                {
                    sql.Append(" AND k.Naziv LIKE @Komitent");
                    parameters.Add("@Komitent", $"%{komitent}%");
                }

                ApplyDateFilter(sql, parameters, filterRequest, "si.Datum");

                sql.Append(" GROUP BY rn.ID, rn.Sifra, k.Naziv, a.Naziv, p.Naziv, rn.LotNaloga");
                sql.Append(" ORDER BY DatumPocetka DESC");
                sql.Append(" LIMIT 200");

                return (await _db.QueryAsync<KontrolaRadniNalogHeaderModel>(sql.ToString(), parameters)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u PretragaRadnihNaloga");
                return new List<KontrolaRadniNalogHeaderModel>();
            }
        }

        private async Task<KontrolaRadniNalogHeaderModel?> UcitajHeaderAsync(long radniNalogId)
        {
            try
            {
                const string sql = @"
                    SELECT
                        rn.ID AS RadniNalogID,
                        rn.Sifra AS RadniNalogSifra,
                        COALESCE(k.Naziv, '') AS Komitent,
                        COALESCE(a.Naziv, '') AS Artikal,
                        COALESCE(p.Naziv, '') AS Pakovanje,
                        rn.LotNaloga,
                        MIN(si.Datum) AS DatumPocetka,
                        MAX(si.Datum) AS DatumZavrsetka
                    FROM RadniNalog rn
                    LEFT JOIN Komitent k ON rn.KomitentID = k.ID
                    LEFT JOIN ArtikalInstanca ai ON rn.ArtikalInstancaID = ai.ID
                    LEFT JOIN Artikal a ON ai.ArtikalID = a.ID
                    LEFT JOIN Pakovanje p ON ai.PakovanjeID = p.ID
                    LEFT JOIN EvidencijaRada er ON er.RadniNalogID = rn.ID
                    LEFT JOIN SmenskiIzvestaj si ON er.SmenskiIzvestajID = si.ID
                    WHERE rn.ID = @RadniNalogId
                    GROUP BY rn.ID, rn.Sifra, k.Naziv, a.Naziv, p.Naziv, rn.LotNaloga";

                var parameters = CreateParameters();
                parameters.Add("@RadniNalogId", radniNalogId);

                return await _db.QueryFirstOrDefaultAsync<KontrolaRadniNalogHeaderModel>(sql, parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajHeaderAsync za RadniNalogId={Id}", radniNalogId);
                return null;
            }
        }

        private async Task<List<KontrolaProizvodnjaModel>> UcitajProizvodnjaAsync(long radniNalogId)
        {
            try
            {
                const string sql = @"
                    SELECT
                        ID, Datum, Smena,
                        COALESCE(Komitent, '') AS Komitent,
                        COALESCE(Artikal, '') AS Artikal,
                        COALESCE(Pakovanje, '') AS Pakovanje,
                        COALESCE(Kolicina, 0) AS Kolicina,
                        MestoKontrole, Nalaz,
                        COALESCE(RezultatKontrole, 0) AS RezultatKontrole,
                        Pregledao, PrisutnoLice1, PrisutnoLice2, Napomena
                    FROM KontrolaProizvodnja
                    WHERE RadniNalogID = @RadniNalogId
                    ORDER BY Datum ASC";

                var parameters = CreateParameters();
                parameters.Add("@RadniNalogId", radniNalogId);

                return (await _db.QueryAsync<KontrolaProizvodnjaModel>(sql, parameters)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajProizvodnjaAsync za RadniNalogId={Id}", radniNalogId);
                return new List<KontrolaProizvodnjaModel>();
            }
        }

        private async Task<List<KontrolaMetalDetektorModel>> UcitajMetalDetektorAsync(long radniNalogId)
        {
            try
            {
                const string sql = @"
                    SELECT
                        ID, Datum, Smena, Artikal, Pakovanje,
                        COALESCE(Kolicina, 0) AS Kolicina,
                        Napomena,
                        H00, H01, H02, H03, H04, H05, H06, H07, H08, H09, H10, H11,
                        H12, H13, H14, H15, H16, H17, H18, H19, H20, H21, H22, H23
                    FROM KontrolaMetalDetektor
                    WHERE RadniNalogID = @RadniNalogId
                    ORDER BY Datum ASC, Smena ASC";

                var parameters = CreateParameters();
                parameters.Add("@RadniNalogId", radniNalogId);

                return (await _db.QueryAsync<KontrolaMetalDetektorModel>(sql, parameters)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajMetalDetektorAsync za RadniNalogId={Id}", radniNalogId);
                return new List<KontrolaMetalDetektorModel>();
            }
        }

        private async Task<List<KontrolaTemperaturaModel>> UcitajTemperaturaAsync(long radniNalogId)
        {
            try
            {
                const string sql = @"
                    SELECT
                        ID, RadniNalogID, Datum, Smena, Komitent, Artikal, Pakovanje,
                        BrojLota, VrstaPakovanja, Pregledao,
                        V00, V01, V02, V03, V04, V05, V06, V07, V08, V09, V10,
                        V11, V12, V13, V14, V15, V16, V17, V18, V19, V20,
                        N00, N01, N02, N03, N04, N05, N06, N07, N08, N09, N10,
                        N11, N12, N13, N14, N15, N16, N17, N18, N19, N20
                    FROM KontrolaTemperatura
                    WHERE RadniNalogID = @RadniNalogId
                    ORDER BY Datum ASC, Smena ASC";

                var parameters = CreateParameters();
                parameters.Add("@RadniNalogId", radniNalogId);

                return (await _db.QueryAsync<KontrolaTemperaturaModel>(sql, parameters)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajTemperaturaAsync za RadniNalogId={Id}", radniNalogId);
                return new List<KontrolaTemperaturaModel>();
            }
        }

        private async Task<List<KontrolaTezineModel>> UcitajTezineAsync(long radniNalogId)
        {
            try
            {
                const string sql = @"
                    SELECT
                        ID, RadniNalogID, Datum, Smena, Komitent, Artikal, Pakovanje,
                        BrojLota, VrstaPakovanja, Vaga, Pregledao,
                        V00, V01, V02, V03, V04, V05, V06, V07, V08, V09, V10,
                        V11, V12, V13, V14, V15, V16, V17, V18, V19, V20,
                        N00, N01, N02, N03, N04, N05, N06, N07, N08, N09, N10,
                        N11, N12, N13, N14, N15, N16, N17, N18, N19, N20
                    FROM KontrolaTezina
                    WHERE RadniNalogID = @RadniNalogId
                    ORDER BY Datum ASC, Smena ASC";

                var parameters = CreateParameters();
                parameters.Add("@RadniNalogId", radniNalogId);

                return (await _db.QueryAsync<KontrolaTezineModel>(sql, parameters)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajTezineAsync za RadniNalogId={Id}", radniNalogId);
                return new List<KontrolaTezineModel>();
            }
        }

        private async Task<List<KontrolaZavrsnaModel>> UcitajZavrsneAsync(long radniNalogId)
        {
            try
            {
                const string sql = @"
                    SELECT
                        ID, Datum, Smena,
                        COALESCE(Komitent, '') AS Komitent,
                        COALESCE(Artikal, '') AS Artikal,
                        COALESCE(Pakovanje, '') AS Pakovanje,
                        COALESCE(Kolicina, 0) AS Kolicina,
                        MestoKontrole, Nalaz,
                        COALESCE(RezultatKontrole, 0) AS RezultatKontrole,
                        Pregledao, PrisutnoLice1, PrisutnoLice2, Vozilo, Napomena
                    FROM KontrolaZavrsna
                    WHERE RadniNalogID = @RadniNalogId
                    ORDER BY Datum ASC";

                var parameters = CreateParameters();
                parameters.Add("@RadniNalogId", radniNalogId);

                return (await _db.QueryAsync<KontrolaZavrsnaModel>(sql, parameters)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajZavrsneAsync za RadniNalogId={Id}", radniNalogId);
                return new List<KontrolaZavrsnaModel>();
            }
        }
    }
}
