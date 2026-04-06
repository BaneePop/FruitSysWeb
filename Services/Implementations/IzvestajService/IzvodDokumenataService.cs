using Dapper;
using FruitSysWeb.Models;
using FruitSysWeb.Models.IzvodDokumenata;
using FruitSysWeb.Services.Core;
using FruitSysWeb.Services.Interfaces;
using System.Text;

namespace FruitSysWeb.Services.Implementations.IzvestajService
{
    public class IzvodDokumenataService : BaseService, IIzvodDokumenataService
    {
        private readonly DatabaseService _db;
        private readonly ILogger<IzvodDokumenataService> _logger;

        public IzvodDokumenataService(DatabaseService db, ILogger<IzvodDokumenataService> logger)
        {
            _db = db;
            _logger = logger;
        }

        // ══════════════════════════════════════════════════════════════════
        // PRIJEMNICE
        // ══════════════════════════════════════════════════════════════════

        public async Task<List<IzvodPrijemnicaRow>> UcitajPrijemnice(IzvodDokumenataFilter filter)
        {
            try
            {
                var sql = new StringBuilder(@"
                    SELECT
                        p.ID,
                        p.Sifra,
                        p.Datum,
                        COALESCE(k.Naziv, '') AS Komitent,
                        COALESCE(a.Naziv, '') AS Artikal,
                        COALESCE(p.Kolicina, 0) AS Kolicina
                    FROM Prijemnica p
                    LEFT JOIN Komitent k ON p.KomitentID = k.ID
                    LEFT JOIN PrijemnicaStavka ps ON ps.PrijemnicaID = p.ID
                    LEFT JOIN ArtikalInstanca ai ON ps.ArtikalInstancaID = ai.ID
                    LEFT JOIN Artikal a ON ai.ArtikalID = a.ID
                    WHERE p.Aktivno = 1
                ");

                var parameters = new DynamicParameters();

                ApplyIzvodFilter(sql, parameters, filter, "p.Datum", "k", "a");

                sql.Append(" GROUP BY p.ID, p.Sifra, p.Datum, k.Naziv, a.Naziv, p.Kolicina");
                sql.Append(" ORDER BY p.Datum DESC, p.Sifra DESC LIMIT 500");

                return (await _db.QueryAsync<IzvodPrijemnicaRow>(sql.ToString(), parameters)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajPrijemnice");
                return new();
            }
        }

        public async Task<IzvodPrijemnicaDetalji?> UcitajPrijemnicuDetalji(long id)
        {
            try
            {
                const string sqlHeader = @"
                    SELECT
                        p.ID,
                        p.Sifra,
                        p.Datum,
                        COALESCE(k.Naziv, '') AS Komitent,
                        COALESCE(k.Adresa, '') AS KomitentAdresa,
                        COALESCE(p.Vozilo, '') AS Vozac,
                        COALESCE(p.Vozilo, '') AS Vozilo,
                        COALESCE(p.Otpremnica, '') AS OtpremnicaDobavljaca,
                        COALESCE(p.Napomena, '') AS Napomena,
                        CAST(p.Uzorkovano AS SIGNED) AS Uzorkovano,
                        p.Temperatura,
                        p.TemperaturaPrimedba,
                        CAST(p.VizuelnaKontrola AS SIGNED) AS VizuelnaKontrola,
                        p.VizuelnaKontrolaPrimedba,
                        CAST(p.StanjeRobePakovanja AS SIGNED) AS StanjeRobePakovanja,
                        p.StanjeRobePakovanjaPrimedba,
                        CAST(p.Primiti AS SIGNED) AS Primiti,
                        CAST(p.PrimitiUzSelekciju AS SIGNED) AS PrimitiUzSelekciju,
                        CAST(p.Reklamirati AS SIGNED) AS Reklamirati,
                        CAST(p.Vratiti AS SIGNED) AS Vratiti,
                        p.UslovnoPrimitiZa
                    FROM Prijemnica p
                    LEFT JOIN Komitent k ON p.KomitentID = k.ID
                    WHERE p.ID = @Id";

                var header = await _db.QueryFirstOrDefaultAsync<IzvodPrijemnicaDetalji>(sqlHeader, new { Id = id });
                if (header == null) return null;

                const string sqlStavke = @"
                    SELECT
                        COALESCE(a.Naziv, '') AS Artikal,
                        CASE WHEN ai.JedinicaMereID = 2 THEN 'kom' ELSE 'kg' END AS JM,
                        COALESCE(ps.Kolicina, 0) AS Kolicina,
                        0 AS Procenat,
                        NULL AS Rok
                    FROM PrijemnicaStavka ps
                    LEFT JOIN ArtikalInstanca ai ON ps.ArtikalInstancaID = ai.ID
                    LEFT JOIN Artikal a ON ai.ArtikalID = a.ID
                    WHERE ps.PrijemnicaID = @Id
                    ORDER BY ps.ID";

                header.Stavke = (await _db.QueryAsync<IzvodPrijemnicaStavka>(sqlStavke, new { Id = id })).ToList();

                // Specifikacija — iz PaletniListStavka per klasa (ako postoji) ili kopija stavki
                const string sqlSpec = @"
                    SELECT
                        COALESCE(a.Naziv, '') AS Artikal,
                        'kg' AS JM,
                        COALESCE(SUM(pls.Kolicina), 0) AS Kolicina,
                        COALESCE(SUM(pls.Procenat), 0) AS Procenat,
                        NULL AS Rok
                    FROM PaletniList pl
                    LEFT JOIN PrijemnicaStavka prs ON pl.PrijemnicaStavkaID = prs.ID
                    LEFT JOIN PaletniListStavka pls ON pls.PaletniListID = pl.ID
                    LEFT JOIN Artikal a ON pls.ArtikalID = a.ID
                    WHERE prs.PrijemnicaID = @Id AND pls.ID IS NOT NULL
                    GROUP BY a.Naziv
                    ORDER BY a.Naziv";

                var spec = (await _db.QueryAsync<IzvodPrijemnicaStavka>(sqlSpec, new { Id = id })).ToList();
                // Ako nema stavki u PaletniListStavka, koristimo stavke prijemnice kao specifikaciju
                header.Specifikacija = spec.Count > 0 ? spec : header.Stavke
                    .Select(s => new IzvodPrijemnicaStavka
                    {
                        Artikal = s.Artikal,
                        JM = s.JM,
                        Kolicina = s.Kolicina,
                        Procenat = header.Stavke.Sum(x => x.Kolicina) > 0
                            ? Math.Round(s.Kolicina / header.Stavke.Sum(x => x.Kolicina) * 100, 1)
                            : 0
                    }).ToList();

                return header;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajPrijemnicuDetalji za ID={Id}", id);
                return null;
            }
        }

        public async Task<List<IzvodPaletniListRow>> UcitajPaletneListovePrijemnice(long prijemnicaId)
        {
            try
            {
                const string sql = @"
                    SELECT
                        pl.ID,
                        pl.Sifra,
                        COALESCE(pl.Tezina, 0) AS Kolicina,
                        COALESCE(k.Naziv, '') AS Komitent
                    FROM PaletniList pl
                    LEFT JOIN PrijemnicaStavka prs ON pl.PrijemnicaStavkaID = prs.ID
                    LEFT JOIN Komitent k ON pl.KomitentID = k.ID
                    WHERE prs.PrijemnicaID = @PrijemnicaId
                    ORDER BY pl.Sifra";

                return (await _db.QueryAsync<IzvodPaletniListRow>(sql, new { PrijemnicaId = prijemnicaId })).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajPaletneListovePrijemnice za PrijemnicaId={Id}", prijemnicaId);
                return new();
            }
        }

        // ══════════════════════════════════════════════════════════════════
        // OTPREMNICE
        // ══════════════════════════════════════════════════════════════════

        public async Task<List<IzvodOtpremnicaRow>> UcitajOtpremnice(IzvodDokumenataFilter filter)
        {
            try
            {
                var sql = new StringBuilder(@"
                    SELECT
                        o.ID,
                        o.Sifra,
                        o.Datum,
                        COALESCE(k.Naziv, '') AS Komitent,
                        COALESCE(a.Naziv, '') AS Artikal,
                        COALESCE(o.Kolicina, 0) AS Kolicina
                    FROM Otpremnica o
                    LEFT JOIN Komitent k ON o.KomitentID = k.ID
                    LEFT JOIN OtpremnicaStavka os ON os.OtpremnicaID = o.ID
                    LEFT JOIN ArtikalInstanca ai ON os.ArtikalInstancaID = ai.ID
                    LEFT JOIN Artikal a ON ai.ArtikalID = a.ID
                    WHERE o.Aktivno = 1
                ");

                var parameters = new DynamicParameters();
                ApplyIzvodFilter(sql, parameters, filter, "o.Datum", "k", "a");

                sql.Append(" GROUP BY o.ID, o.Sifra, o.Datum, k.Naziv, a.Naziv, o.Kolicina");
                sql.Append(" ORDER BY o.Datum DESC, o.Sifra DESC LIMIT 500");

                return (await _db.QueryAsync<IzvodOtpremnicaRow>(sql.ToString(), parameters)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajOtpremnice");
                return new();
            }
        }

        public async Task<IzvodOtpremnicaDetalji?> UcitajOtpremnicuDetalji(long id)
        {
            try
            {
                const string sqlHeader = @"
                    SELECT
                        o.ID,
                        o.Sifra,
                        o.Datum,
                        COALESCE(k.Naziv, '') AS Komitent,
                        COALESCE(CONCAT(k.Adresa, ', ', k.Mesto), '') AS KomitentAdresa,
                        COALESCE(k.PoreskiBroj, '') AS KomitentPib,
                        COALESCE(k.MaticniBroj, '') AS KomitentMb,
                        COALESCE(o.Vozac, '') AS Vozac,
                        COALESCE(o.Vozilo, '') AS Vozilo,
                        COALESCE(o.VozacBrojPasosa, '') AS BrojPasosa,
                        COALESCE(o.Vozar, '') AS Vozar,
                        COALESCE(o.VozarPib, '') AS VozarPib,
                        COALESCE(o.VozarAdresa, '') AS VozarAdresa,
                        COALESCE(o.GranicniPrelaz, '') AS GranicniPrelaz,
                        COALESCE(CAST(o.BrojPlombi AS CHAR), '') AS BrojPlombi,
                        o.Temperatura,
                        o.TemperaturaPrimedba,
                        COALESCE(rn.LotNaloga, '') AS Lot,
                        COALESCE(rn.Sifra, '') AS RadniNalog,
                        COALESCE(o.Napomena, '') AS Napomena,
                        '' AS Spedicija,
                        CAST(o.Uzorkovano AS SIGNED) AS Uzorkovano,
                        CAST(o.VizuelnaKontrola AS SIGNED) AS VizuelnaKontrola,
                        o.VizuelnaKontrolaPrimedba,
                        CAST(o.StanjeRobePakovanja AS SIGNED) AS StanjeRobePakovanja,
                        CAST(o.StanjeVozila AS SIGNED) AS StanjeVozila,
                        CAST(o.KontrolaDeklaracija AS SIGNED) AS KontrolaDeklaracija,
                        COALESCE(o.Kolicina, 0) AS BrutoTezina
                    FROM Otpremnica o
                    LEFT JOIN Komitent k ON o.KomitentID = k.ID
                    LEFT JOIN RadniNalog rn ON o.RadniNalogID = rn.ID
                    WHERE o.ID = @Id";

                var header = await _db.QueryFirstOrDefaultAsync<IzvodOtpremnicaDetalji>(sqlHeader, new { Id = id });
                if (header == null) return null;

                const string sqlStavke = @"
                    SELECT
                        COALESCE(a.Naziv, '') AS Artikal,
                        CASE WHEN ai.JedinicaMereID = 2 THEN 'kom' ELSE 'kg' END AS JM,
                        COALESCE(os.Kolicina, 0) AS Kolicina,
                        NULL AS Rok
                    FROM OtpremnicaStavka os
                    LEFT JOIN ArtikalInstanca ai ON os.ArtikalInstancaID = ai.ID
                    LEFT JOIN Artikal a ON ai.ArtikalID = a.ID
                    WHERE os.OtpremnicaID = @Id
                    ORDER BY os.ID";

                header.Stavke = (await _db.QueryAsync<IzvodOtpremnicaStavka>(sqlStavke, new { Id = id })).ToList();

                // Ambalaza — iz PaletniList za ovu otpremnicu
                const string sqlAmbalaza = @"
                    SELECT
                        COALESCE(pak.Naziv, '') AS Artikal,
                        'kom' AS JM,
                        COALESCE(COUNT(pl.ID), 0) AS Kolicina,
                        COALESCE(SUM(pl.BrutoTezina), 0) AS Tezina
                    FROM PaletniList pl
                    LEFT JOIN OtpremnicaStavka ots ON pl.OtpremnicaStavkaID = ots.ID
                    LEFT JOIN Pakovanje pak ON pl.PakovanjeID = pak.ID
                    WHERE ots.OtpremnicaID = @Id AND pak.ID IS NOT NULL
                    GROUP BY pak.Naziv
                    ORDER BY pak.Naziv";

                header.Ambalaza = (await _db.QueryAsync<IzvodOtpremnicaAmbalaza>(sqlAmbalaza, new { Id = id })).ToList();

                return header;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajOtpremnicuDetalji za ID={Id}", id);
                return null;
            }
        }

        public async Task<List<IzvodPaletniListRow>> UcitajPaletneListoveOtpremnice(long otpremnicaId)
        {
            try
            {
                // Paletni listovi se vezuju za Otpremnicu preko RadniNalogID
                // Otpremnica ima RadniNalogID → PaletniList ima RadniNalogID
                const string sql = @"
                    SELECT
                        pl.ID,
                        pl.Sifra,
                        COALESCE(pl.Tezina, 0) AS Kolicina,
                        COALESCE(k.Naziv, '') AS Komitent
                    FROM PaletniList pl
                    INNER JOIN Otpremnica o ON o.RadniNalogID = pl.RadniNalogID
                        AND o.RadniNalogID IS NOT NULL
                    LEFT JOIN Komitent k ON pl.KomitentID = k.ID
                    WHERE o.ID = @OtpremnicaId
                    ORDER BY pl.Sifra";

                return (await _db.QueryAsync<IzvodPaletniListRow>(sql, new { OtpremnicaId = otpremnicaId })).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajPaletneListoveOtpremnice za OtpremnicaId={Id}", otpremnicaId);
                return new();
            }
        }

        // ══════════════════════════════════════════════════════════════════
        // RADNI NALOZI
        // ══════════════════════════════════════════════════════════════════

        public async Task<List<IzvodRadniNalogRow>> UcitajRadneNaloge(IzvodDokumenataFilter filter)
        {
            try
            {
                var sql = new StringBuilder(@"
                    SELECT
                        rn.ID,
                        rn.Sifra,
                        rn.DatumPocetka,
                        COALESCE(k.Naziv, '') AS Komitent,
                        COALESCE(a.Naziv, '') AS Artikal,
                        COALESCE(rn.Kolicina, 0) AS Kolicina
                    FROM RadniNalog rn
                    LEFT JOIN Komitent k ON rn.KomitentID = k.ID
                    LEFT JOIN ArtikalInstanca ai ON rn.ArtikalInstancaID = ai.ID
                    LEFT JOIN Artikal a ON ai.ArtikalID = a.ID
                    WHERE rn.Aktivno = 1 AND rn.Sifra LIKE 'RN-%'
                ");

                var parameters = new DynamicParameters();

                if (filter.OdDatum.HasValue)
                {
                    sql.Append(" AND rn.DatumPocetka >= @OdDatum");
                    parameters.Add("@OdDatum", filter.OdDatum.Value.Date);
                }
                if (filter.DoDatum.HasValue)
                {
                    sql.Append(" AND rn.DatumPocetka <= @DoDatum");
                    parameters.Add("@DoDatum", filter.DoDatum.Value.Date.AddDays(1).AddSeconds(-1));
                }
                if (filter.KomitentId.HasValue && filter.KomitentId > 0)
                {
                    sql.Append(" AND rn.KomitentID = @KomitentId");
                    parameters.Add("@KomitentId", filter.KomitentId.Value);
                }
                if (!string.IsNullOrWhiteSpace(filter.RadniNalog))
                {
                    sql.Append(" AND rn.Sifra LIKE @RadniNalog");
                    parameters.Add("@RadniNalog", $"%{filter.RadniNalog.Trim()}%");
                }
                if (!string.IsNullOrWhiteSpace(filter.TipArtikla))
                {
                    sql.Append(" AND a.MagacinID = @TipArtikla");
                    parameters.Add("@TipArtikla", filter.TipArtikla);
                }

                sql.Append(" ORDER BY rn.DatumPocetka DESC LIMIT 500");

                return (await _db.QueryAsync<IzvodRadniNalogRow>(sql.ToString(), parameters)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajRadneNaloge");
                return new();
            }
        }

        public async Task<IzvodRadniNalogDetalji?> UcitajRadniNalogDetalji(long id)
        {
            try
            {
                const string sqlHeader = @"
                    SELECT
                        rn.ID,
                        rn.Sifra,
                        COALESCE(k.Naziv, '') AS Komitent,
                        COALESCE(a.Naziv, '') AS Artikal,
                        COALESCE(p.Naziv, '') AS Pakovanje,
                        rn.Kolicina,
                        rn.LotNaloga,
                        rn.BrojPakovanja,
                        rn.DatumPocetka,
                        rn.DatumIsporuke,
                        rn.Opis,
                        u.BrojUgovora AS Ugovor
                    FROM RadniNalog rn
                    LEFT JOIN Komitent k ON rn.KomitentID = k.ID
                    LEFT JOIN ArtikalInstanca ai ON rn.ArtikalInstancaID = ai.ID
                    LEFT JOIN Artikal a ON ai.ArtikalID = a.ID
                    LEFT JOIN Pakovanje p ON ai.PakovanjeID = p.ID
                    LEFT JOIN UgovorProdaja u ON rn.UgovorProdajaID = u.ID
                    WHERE rn.ID = @Id";

                var header = await _db.QueryFirstOrDefaultAsync<IzvodRadniNalogDetalji>(sqlHeader, new { Id = id });
                if (header == null) return null;

                // Učitaj sve kontrole paralelno
                var taskProizvodnja = UcitajKontroleProzvodnje(id);
                var taskTemperatura = UcitajKontroleTempereture(id);
                var taskTezine = UcitajKontroleTezine(id);
                var taskZavrsne = UcitajKontrolazavrsne(id);

                await Task.WhenAll(taskProizvodnja, taskTemperatura, taskTezine, taskZavrsne);

                header.KontroleProzvodnje = await taskProizvodnja;
                header.Temperature = await taskTemperatura;
                header.Tezine = await taskTezine;
                header.ZavrsneKontrole = await taskZavrsne;

                return header;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajRadniNalogDetalji za ID={Id}", id);
                return null;
            }
        }

        // ══════════════════════════════════════════════════════════════════
        // HELPER METODE
        // ══════════════════════════════════════════════════════════════════

        private void ApplyIzvodFilter(StringBuilder sql, DynamicParameters parameters,
            IzvodDokumenataFilter filter, string datumKolona, string komitentAlias, string artikalAlias)
        {
            if (filter.OdDatum.HasValue)
            {
                sql.Append($" AND {datumKolona} >= @OdDatum");
                parameters.Add("@OdDatum", filter.OdDatum.Value.Date);
            }
            if (filter.DoDatum.HasValue)
            {
                sql.Append($" AND {datumKolona} <= @DoDatum");
                parameters.Add("@DoDatum", filter.DoDatum.Value.Date.AddDays(1).AddSeconds(-1));
            }
            if (filter.KomitentId.HasValue && filter.KomitentId > 0)
            {
                sql.Append($" AND {komitentAlias}.ID = @KomitentId");
                parameters.Add("@KomitentId", filter.KomitentId.Value);
            }
            if (!string.IsNullOrWhiteSpace(filter.KomitentTip))
            {
                var tipFilter = filter.KomitentTip.ToLower() switch
                {
                    "kupac" => $" AND {komitentAlias}.JeKupac = 1",
                    "dobavljac" => $" AND {komitentAlias}.JeDobavljac = 1",
                    "proizvodjac" => $" AND {komitentAlias}.JeProizvodjac = 1",
                    "otkupljivac" => $" AND {komitentAlias}.JeOtkupljivac = 1",
                    _ => string.Empty
                };
                sql.Append(tipFilter);
            }
            if (filter.ArtikalId.HasValue && filter.ArtikalId > 0)
            {
                sql.Append($" AND {artikalAlias}.ID = @ArtikalId");
                parameters.Add("@ArtikalId", filter.ArtikalId.Value);
            }
            if (!string.IsNullOrWhiteSpace(filter.TipArtikla))
            {
                sql.Append($" AND {artikalAlias}.MagacinID = @TipArtikla");
                parameters.Add("@TipArtikla", filter.TipArtikla);
            }
        }

        private async Task<List<KontrolaProizvodnjaModel>> UcitajKontroleProzvodnje(long radniNalogId)
        {
            const string sql = @"
                SELECT ID, Datum, Smena,
                    COALESCE(Komitent,'') AS Komitent, COALESCE(Artikal,'') AS Artikal,
                    COALESCE(Pakovanje,'') AS Pakovanje, COALESCE(Kolicina,0) AS Kolicina,
                    MestoKontrole, Nalaz, COALESCE(RezultatKontrole,0) AS RezultatKontrole,
                    Pregledao, PrisutnoLice1, PrisutnoLice2, Napomena
                FROM KontrolaProizvodnja WHERE RadniNalogID = @Id ORDER BY Datum ASC";
            return (await _db.QueryAsync<KontrolaProizvodnjaModel>(sql, new { Id = radniNalogId })).ToList();
        }

        private async Task<List<KontrolaTemperaturaModel>> UcitajKontroleTempereture(long radniNalogId)
        {
            const string sql = @"
                SELECT ID, RadniNalogID, Datum, Smena, Komitent, Artikal, Pakovanje,
                    BrojLota, VrstaPakovanja, Pregledao,
                    V00, V01, V02, V03, V04, V05, V06, V07, V08, V09, V10,
                    V11, V12, V13, V14, V15, V16, V17, V18, V19, V20,
                    N00, N01, N02, N03, N04, N05, N06, N07, N08, N09, N10,
                    N11, N12, N13, N14, N15, N16, N17, N18, N19, N20
                FROM KontrolaTemperatura WHERE RadniNalogID = @Id ORDER BY Datum ASC";
            return (await _db.QueryAsync<KontrolaTemperaturaModel>(sql, new { Id = radniNalogId })).ToList();
        }

        private async Task<List<KontrolaTezineModel>> UcitajKontroleTezine(long radniNalogId)
        {
            const string sql = @"
                SELECT ID, RadniNalogID, Datum, Smena, Komitent, Artikal, Pakovanje,
                    BrojLota, VrstaPakovanja, Vaga, Pregledao,
                    V00, V01, V02, V03, V04, V05, V06, V07, V08, V09, V10,
                    V11, V12, V13, V14, V15, V16, V17, V18, V19, V20,
                    N00, N01, N02, N03, N04, N05, N06, N07, N08, N09, N10,
                    N11, N12, N13, N14, N15, N16, N17, N18, N19, N20
                FROM KontrolaTezina WHERE RadniNalogID = @Id ORDER BY Datum ASC";
            return (await _db.QueryAsync<KontrolaTezineModel>(sql, new { Id = radniNalogId })).ToList();
        }

        private async Task<List<KontrolaZavrsnaModel>> UcitajKontrolazavrsne(long radniNalogId)
        {
            const string sql = @"
                SELECT ID, Datum, Smena,
                    COALESCE(Komitent,'') AS Komitent, COALESCE(Artikal,'') AS Artikal,
                    COALESCE(Pakovanje,'') AS Pakovanje, COALESCE(Kolicina,0) AS Kolicina,
                    MestoKontrole, Nalaz, COALESCE(RezultatKontrole,0) AS RezultatKontrole,
                    Pregledao, PrisutnoLice1, PrisutnoLice2, Vozilo, Napomena
                FROM KontrolaZavrsna WHERE RadniNalogID = @Id ORDER BY Datum ASC";
            return (await _db.QueryAsync<KontrolaZavrsnaModel>(sql, new { Id = radniNalogId })).ToList();
        }
    }
}
