using System.Globalization;

namespace FruitSysWeb.Models
{
    /// <summary>
    /// Header informacije o radnom nalogu za izveštaj kontrole
    /// </summary>
    public class KontrolaRadniNalogHeaderModel
    {
        public long RadniNalogID { get; set; }
        public string RadniNalogSifra { get; set; } = string.Empty;
        public string Komitent { get; set; } = string.Empty;
        public string Artikal { get; set; } = string.Empty;
        public string Pakovanje { get; set; } = string.Empty;
        public string? LotNaloga { get; set; }
        public DateTime? DatumPocetka { get; set; }
        public DateTime? DatumZavrsetka { get; set; }

        private static readonly CultureInfo SrFormat = new CultureInfo("sr-Latn-RS");
        public string DatumPocetkaFormatted => DatumPocetka?.ToString("dd.MM.yyyy", SrFormat) ?? "-";
        public string DatumZavrsetkaFormatted => DatumZavrsetka?.ToString("dd.MM.yyyy", SrFormat) ?? "-";
    }

    /// <summary>
    /// Jedna kontrola u procesu proizvodnje (svaki zapis = jedan red u tabeli).
    /// Kolona Nalaz se parsira u Dictionary u servisu.
    /// </summary>
    public class KontrolaProizvodnjaModel
    {
        private static readonly CultureInfo SrFormat = new CultureInfo("sr-Latn-RS");

        public long ID { get; set; }
        public DateTime Datum { get; set; }
        public int Smena { get; set; }
        public string Komitent { get; set; } = string.Empty;
        public string Artikal { get; set; } = string.Empty;
        public string Pakovanje { get; set; } = string.Empty;
        public decimal Kolicina { get; set; }
        public string? MestoKontrole { get; set; }
        public string? Nalaz { get; set; }
        public int RezultatKontrole { get; set; }
        public string? Pregledao { get; set; }
        public string? PrisutnoLice1 { get; set; }
        public string? PrisutnoLice2 { get; set; }
        public string? Napomena { get; set; }

        public string DatumFormatted => Datum.ToString("dd.MM.yyyy HH:mm", SrFormat);
        public string RezultatBadge => RezultatKontrole == 1 ? "bg-success" : "bg-danger";
        public string RezultatTekst => RezultatKontrole == 1 ? "OK" : "NOK";

        /// <summary>
        /// Parsira Nalaz (slobodan tekst "Ključ: vrednost\n...") u uredjen recnik.
        /// </summary>
        public Dictionary<string, string> ParsedNalaz => ParseNalaz(Nalaz);

        private static Dictionary<string, string> ParseNalaz(string? nalaz)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(nalaz)) return result;

            foreach (var line in nalaz.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                var idx = line.IndexOf(':');
                if (idx > 0)
                {
                    var key = line[..idx].Trim();
                    var value = line[(idx + 1)..].Trim();
                    if (!string.IsNullOrEmpty(key))
                        result[key] = value;
                }
            }
            return result;
        }
    }

    /// <summary>
    /// Jedan red metal detektor izveštaja (smena sa satnim pregledom).
    /// H00-H23 = 1 ako je provereno taj sat, NULL ako nije.
    /// </summary>
    public class KontrolaMetalDetektorModel
    {
        private static readonly CultureInfo SrFormat = new CultureInfo("sr-Latn-RS");

        public long ID { get; set; }
        public DateTime Datum { get; set; }
        public int Smena { get; set; }
        public string? Artikal { get; set; }
        public string? Pakovanje { get; set; }
        public decimal? Kolicina { get; set; }
        public string? Napomena { get; set; }

        // Sat po sat — 1 = provereno, NULL = nije
        public int? H00 { get; set; }
        public int? H01 { get; set; }
        public int? H02 { get; set; }
        public int? H03 { get; set; }
        public int? H04 { get; set; }
        public int? H05 { get; set; }
        public int? H06 { get; set; }
        public int? H07 { get; set; }
        public int? H08 { get; set; }
        public int? H09 { get; set; }
        public int? H10 { get; set; }
        public int? H11 { get; set; }
        public int? H12 { get; set; }
        public int? H13 { get; set; }
        public int? H14 { get; set; }
        public int? H15 { get; set; }
        public int? H16 { get; set; }
        public int? H17 { get; set; }
        public int? H18 { get; set; }
        public int? H19 { get; set; }
        public int? H20 { get; set; }
        public int? H21 { get; set; }
        public int? H22 { get; set; }
        public int? H23 { get; set; }

        public string DatumFormatted => Datum.ToString("dd.MM.yyyy", SrFormat);
        public string KolicinaFormatted => Kolicina.HasValue ? Kolicina.Value.ToString("N0", SrFormat) : "-";

        public bool SatProveren(int sat) => sat switch
        {
            0 => H00 == 1, 1 => H01 == 1, 2 => H02 == 1, 3 => H03 == 1,
            4 => H04 == 1, 5 => H05 == 1, 6 => H06 == 1, 7 => H07 == 1,
            8 => H08 == 1, 9 => H09 == 1, 10 => H10 == 1, 11 => H11 == 1,
            12 => H12 == 1, 13 => H13 == 1, 14 => H14 == 1, 15 => H15 == 1,
            16 => H16 == 1, 17 => H17 == 1, 18 => H18 == 1, 19 => H19 == 1,
            20 => H20 == 1, 21 => H21 == 1, 22 => H22 == 1, 23 => H23 == 1,
            _ => false
        };

        public int BrojProverenihSati =>
            new[] { H00, H01, H02, H03, H04, H05, H06, H07, H08, H09, H10, H11,
                    H12, H13, H14, H15, H16, H17, H18, H19, H20, H21, H22, H23 }
            .Count(h => h == 1);
    }

    /// <summary>
    /// Temperatura kontrola — do 20 merenja (V00-V20) sa napomenama (N00-N20).
    /// Prikazujemo kao listu redova u C#.
    /// </summary>
    public class KontrolaTemperaturaModel
    {
        private static readonly CultureInfo SrFormat = new CultureInfo("sr-Latn-RS");

        public long ID { get; set; }
        public DateTime Datum { get; set; }
        public int Smena { get; set; }
        public string? Komitent { get; set; }
        public string? Artikal { get; set; }
        public string? Pakovanje { get; set; }
        public string? BrojLota { get; set; }
        public string? VrstaPakovanja { get; set; }
        public string? Pregledao { get; set; }

        // Vrednosti V00-V20
        public string? V00 { get; set; } public string? V01 { get; set; } public string? V02 { get; set; }
        public string? V03 { get; set; } public string? V04 { get; set; } public string? V05 { get; set; }
        public string? V06 { get; set; } public string? V07 { get; set; } public string? V08 { get; set; }
        public string? V09 { get; set; } public string? V10 { get; set; } public string? V11 { get; set; }
        public string? V12 { get; set; } public string? V13 { get; set; } public string? V14 { get; set; }
        public string? V15 { get; set; } public string? V16 { get; set; } public string? V17 { get; set; }
        public string? V18 { get; set; } public string? V19 { get; set; } public string? V20 { get; set; }

        // Napomene N00-N20
        public string? N00 { get; set; } public string? N01 { get; set; } public string? N02 { get; set; }
        public string? N03 { get; set; } public string? N04 { get; set; } public string? N05 { get; set; }
        public string? N06 { get; set; } public string? N07 { get; set; } public string? N08 { get; set; }
        public string? N09 { get; set; } public string? N10 { get; set; } public string? N11 { get; set; }
        public string? N12 { get; set; } public string? N13 { get; set; } public string? N14 { get; set; }
        public string? N15 { get; set; } public string? N16 { get; set; } public string? N17 { get; set; }
        public string? N18 { get; set; } public string? N19 { get; set; } public string? N20 { get; set; }

        public string DatumFormatted => Datum.ToString("dd.MM.yyyy", SrFormat);

        /// <summary>Kreira listu merenja (redni broj, vrednost, napomena) samo za popunjene redove.</summary>
        public List<(int Rbr, string Vrednost, string Napomena)> GetMerenja()
        {
            var vrednosti = new[] { V00, V01, V02, V03, V04, V05, V06, V07, V08, V09,
                                    V10, V11, V12, V13, V14, V15, V16, V17, V18, V19, V20 };
            var napomene = new[] { N00, N01, N02, N03, N04, N05, N06, N07, N08, N09,
                                   N10, N11, N12, N13, N14, N15, N16, N17, N18, N19, N20 };
            var result = new List<(int, string, string)>();
            for (int i = 0; i < vrednosti.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(vrednosti[i]))
                    result.Add((i + 1, vrednosti[i]!, napomene[i] ?? ""));
            }
            return result;
        }
    }

    /// <summary>
    /// Kontrola težine — do 20 merenja (V00-V20) sa napomenama (N00-N20).
    /// </summary>
    public class KontrolaTezineModel
    {
        private static readonly CultureInfo SrFormat = new CultureInfo("sr-Latn-RS");

        public long ID { get; set; }
        public DateTime Datum { get; set; }
        public int Smena { get; set; }
        public string? Komitent { get; set; }
        public string? Artikal { get; set; }
        public string? Pakovanje { get; set; }
        public string? BrojLota { get; set; }
        public string? VrstaPakovanja { get; set; }
        public string? Vaga { get; set; }
        public string? Pregledao { get; set; }

        public string? V00 { get; set; } public string? V01 { get; set; } public string? V02 { get; set; }
        public string? V03 { get; set; } public string? V04 { get; set; } public string? V05 { get; set; }
        public string? V06 { get; set; } public string? V07 { get; set; } public string? V08 { get; set; }
        public string? V09 { get; set; } public string? V10 { get; set; } public string? V11 { get; set; }
        public string? V12 { get; set; } public string? V13 { get; set; } public string? V14 { get; set; }
        public string? V15 { get; set; } public string? V16 { get; set; } public string? V17 { get; set; }
        public string? V18 { get; set; } public string? V19 { get; set; } public string? V20 { get; set; }

        public string? N00 { get; set; } public string? N01 { get; set; } public string? N02 { get; set; }
        public string? N03 { get; set; } public string? N04 { get; set; } public string? N05 { get; set; }
        public string? N06 { get; set; } public string? N07 { get; set; } public string? N08 { get; set; }
        public string? N09 { get; set; } public string? N10 { get; set; } public string? N11 { get; set; }
        public string? N12 { get; set; } public string? N13 { get; set; } public string? N14 { get; set; }
        public string? N15 { get; set; } public string? N16 { get; set; } public string? N17 { get; set; }
        public string? N18 { get; set; } public string? N19 { get; set; } public string? N20 { get; set; }

        public string DatumFormatted => Datum.ToString("dd.MM.yyyy", SrFormat);

        public List<(int Rbr, string Vrednost, string Napomena)> GetMerenja()
        {
            var vrednosti = new[] { V00, V01, V02, V03, V04, V05, V06, V07, V08, V09,
                                    V10, V11, V12, V13, V14, V15, V16, V17, V18, V19, V20 };
            var napomene = new[] { N00, N01, N02, N03, N04, N05, N06, N07, N08, N09,
                                   N10, N11, N12, N13, N14, N15, N16, N17, N18, N19, N20 };
            var result = new List<(int, string, string)>();
            for (int i = 0; i < vrednosti.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(vrednosti[i]))
                    result.Add((i + 1, vrednosti[i]!, napomene[i] ?? ""));
            }
            return result;
        }
    }

    /// <summary>
    /// Završna kontrola — ista struktura kao KontrolaProizvodnja plus Vozilo.
    /// </summary>
    public class KontrolaZavrsnaModel
    {
        private static readonly CultureInfo SrFormat = new CultureInfo("sr-Latn-RS");

        public long ID { get; set; }
        public DateTime Datum { get; set; }
        public int Smena { get; set; }
        public string Komitent { get; set; } = string.Empty;
        public string Artikal { get; set; } = string.Empty;
        public string Pakovanje { get; set; } = string.Empty;
        public decimal Kolicina { get; set; }
        public string? MestoKontrole { get; set; }
        public string? Nalaz { get; set; }
        public int RezultatKontrole { get; set; }
        public string? Pregledao { get; set; }
        public string? PrisutnoLice1 { get; set; }
        public string? PrisutnoLice2 { get; set; }
        public string? Vozilo { get; set; }
        public string? Napomena { get; set; }

        public string DatumFormatted => Datum.ToString("dd.MM.yyyy HH:mm", SrFormat);
        public string RezultatBadge => RezultatKontrole == 1 ? "bg-success" : "bg-danger";
        public string RezultatTekst => RezultatKontrole == 1 ? "OK" : "NOK";

        public Dictionary<string, string> ParsedNalaz => ParseNalaz(Nalaz);

        private static Dictionary<string, string> ParseNalaz(string? nalaz)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(nalaz)) return result;
            foreach (var line in nalaz.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                var idx = line.IndexOf(':');
                if (idx > 0)
                {
                    var key = line[..idx].Trim();
                    var value = line[(idx + 1)..].Trim();
                    if (!string.IsNullOrEmpty(key))
                        result[key] = value;
                }
            }
            return result;
        }
    }

    /// <summary>
    /// Agregirani model svih kontrola za jedan radni nalog.
    /// </summary>
    public class KontrolaRadniNalogModel
    {
        public KontrolaRadniNalogHeaderModel? Header { get; set; }
        public List<KontrolaProizvodnjaModel> KontroleProzvodnje { get; set; } = new();
        public List<KontrolaMetalDetektorModel> MetalDetektor { get; set; } = new();
        public List<KontrolaTemperaturaModel> Temperature { get; set; } = new();
        public List<KontrolaTezineModel> Tezine { get; set; } = new();
        public List<KontrolaZavrsnaModel> ZavrsneKontrole { get; set; } = new();

        public bool ImaKontroleProizvodnje => KontroleProzvodnje.Count > 0;
        public bool ImaMetalDetektor => MetalDetektor.Count > 0;
        public bool ImaTemperature => Temperature.Count > 0;
        public bool ImaTezine => Tezine.Count > 0;
        public bool ImaZavrsneKontrole => ZavrsneKontrole.Count > 0;
        public bool ImaPodataka => ImaKontroleProizvodnje || ImaMetalDetektor || ImaTemperature || ImaTezine || ImaZavrsneKontrole;

        /// <summary>
        /// Sve ključeve nalaza iz svih kontrola proizvodnje (union skup kolona za tabelu).
        /// </summary>
        public List<string> SviKljeveviNalazaProizvodnje =>
            KontroleProzvodnje
                .SelectMany(k => k.ParsedNalaz.Keys)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

        public List<string> SviKljeveviNalazaZavrsnih =>
            ZavrsneKontrole
                .SelectMany(k => k.ParsedNalaz.Keys)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
    }
}
