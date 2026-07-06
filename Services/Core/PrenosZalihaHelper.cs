namespace FruitSysWeb.Services.Core;

/// <summary>
/// Prenos Zaliha = stanje na dan pre sezone (OdDatum − 1).
/// Ukupno po vrsti voća: TrenutnoStanje(MagacinLager) − NetoPromet(OdSezona → danas) — isto kao LagerHome.
/// Po artiklu/redu: proporcionalna raspodela ukupnog po trenutnom udelu u vrsti voća.
/// </summary>
public static class PrenosZalihaHelper
{
    public const string FilterIskljuciUslugu =
        "AND a.Naziv NOT LIKE '%-ALTIVA%' AND a.Naziv NOT LIKE '%FRIKOS%'";

    public const string FilterSamoUsluga =
        "AND (a.Naziv LIKE '%-ALTIVA%' OR a.Naziv LIKE '%FRIKOS%')";

    public static readonly int[] PraceneKlasifikacije = { 6, 10, 11, 15, 28, 34 };

    public static readonly int[] PraceneKlasifikacijeSaBorovnicom = { 6, 10, 11, 15, 28, 34, 39 };

    public static string KlasifikacijeInClause(IReadOnlyList<int> klasifikacije) =>
        string.Join(", ", klasifikacije);

    private static string UslugaFilter(bool? samoUsluga) => samoUsluga switch
    {
        true => FilterSamoUsluga,
        false => FilterIskljuciUslugu,
        _ => string.Empty
    };

    /// <summary>Ukupno stanje po vrsti voća (MagacinLager) — kao LagerHome grafikon.</summary>
    public static string SqlStanjePoVrsti(
        IReadOnlyList<int> klasifikacije,
        bool samoSirovine,
        bool? samoUsluga,
        out string sql)
    {
        string magacinFilter = samoSirovine ? "AND a.MagacinID IN (2, 3)" : "";
        string uslugaFilter = UslugaFilter(samoUsluga);

        sql = $@"
            SELECT
                a.PrvaKlasifikacijaID AS KlasId,
                SUM(ml.Kolicina) AS Kolicina
            FROM MagacinLager ml
            JOIN ArtikalInstanca ai ON ml.ArtikalInstancaID = ai.ID
            JOIN Artikal a ON ai.ArtikalID = a.ID
            WHERE a.PrvaKlasifikacijaID IN ({KlasifikacijeInClause(klasifikacije)})
              {magacinFilter}
              {uslugaFilter}
            GROUP BY a.PrvaKlasifikacijaID";

        return sql;
    }

    public static string SqlPrometPoVrsti(
        IReadOnlyList<int> klasifikacije,
        bool samoSirovine,
        bool? samoUsluga,
        out string sql)
    {
        string magacinFilter = samoSirovine ? "AND a.MagacinID IN (2, 3)" : "";
        string uslugaFilter = UslugaFilter(samoUsluga);

        sql = $@"
            SELECT
                fm.ArtikalPrvaKlasifikacijaID AS KlasId,
                SUM(CASE WHEN LEFT(fm.Dokument, 2) = 'PR' THEN COALESCE(fm.Ulaz, 0) ELSE 0 END) AS Ulaz,
                SUM(CASE WHEN LEFT(fm.Dokument, 2) = 'OT' THEN COALESCE(fm.Izlaz, 0) ELSE 0 END) AS Izlaz
            FROM vPrometRobav6 fm
            INNER JOIN Artikal a ON a.ID = fm.ArtikalID
            WHERE fm.DokumentStatus = 3
              AND fm.ArtikalPrvaKlasifikacijaID IN ({KlasifikacijeInClause(klasifikacije)})
              AND (fm.Ulaz > 0 OR fm.Izlaz > 0)
              AND fm.Datum >= @OdSezona
              AND fm.Datum <= @Danas
              {magacinFilter}
              {uslugaFilter}
            GROUP BY fm.ArtikalPrvaKlasifikacijaID";

        return sql;
    }

    public static Dictionary<int, decimal> RekonstruisiPoVrsti(
        IEnumerable<dynamic> lagerRows,
        IEnumerable<dynamic> prometRows,
        IReadOnlyList<int> klasifikacije)
    {
        var trenutno = lagerRows.ToDictionary(r => (int)r.KlasId, r => (decimal)r.Kolicina);
        var neto = prometRows.ToDictionary(
            r => (int)r.KlasId,
            r => (decimal)r.Ulaz - (decimal)r.Izlaz);

        var rezultat = new Dictionary<int, decimal>();
        foreach (var klasId in klasifikacije)
        {
            decimal t = trenutno.TryGetValue(klasId, out var tv) ? tv : 0m;
            decimal n = neto.TryGetValue(klasId, out var nv) ? nv : 0m;
            rezultat[klasId] = Math.Max(0m, t - n);
        }

        return rezultat;
    }

    /// <summary>
    /// Raspodela ukupnog stanja vrste voća na dan pre sezone po redovima (magacin + proizvodnja).
    /// Zbir redova za istu vrstu = ukupnoPoVrsti[klasId].
    /// </summary>
    public static decimal RaspodeliRed(
        decimal trenutnaKolicinaReda,
        decimal ukupnoPoVrstiNaDan,
        decimal trenutniZbirVrste)
    {
        if (ukupnoPoVrstiNaDan <= 0m || trenutniZbirVrste <= 0m || trenutnaKolicinaReda <= 0m)
            return 0m;

        return ukupnoPoVrstiNaDan * (trenutnaKolicinaReda / trenutniZbirVrste);
    }

    /// <summary>
    /// Prenos Zaliha po vrsti = zbir redova minus označeni ArtikalID-evi (Sekcija 5).
    /// </summary>
    public static Dictionary<int, decimal> SaberiPrenosIzRedova(
        IEnumerable<(int KlasId, int ArtikalID, decimal Kg)> redovi,
        IReadOnlyList<int> klasifikacije,
        IReadOnlyCollection<int>? iskljuceniArtikli)
    {
        var excluded = iskljuceniArtikli?.ToHashSet() ?? new HashSet<int>();
        var ukupno = klasifikacije.ToDictionary(id => id, _ => 0m);

        foreach (var (klasId, artId, kg) in redovi)
        {
            if (!ukupno.ContainsKey(klasId) || excluded.Contains(artId))
                continue;

            ukupno[klasId] += kg;
        }

        foreach (var klasId in klasifikacije)
            ukupno[klasId] = Math.Max(0m, ukupno[klasId]);

        return ukupno;
    }
}
