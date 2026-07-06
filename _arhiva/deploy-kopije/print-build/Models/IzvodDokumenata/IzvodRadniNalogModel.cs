using System.Globalization;

namespace FruitSysWeb.Models.IzvodDokumenata
{
    public class IzvodRadniNalogRow
    {
        public long ID { get; set; }
        public string Sifra { get; set; } = string.Empty;
        public DateTime? DatumPocetka { get; set; }
        public string Komitent { get; set; } = string.Empty;
        public string Artikal { get; set; } = string.Empty;
        public decimal Kolicina { get; set; }

        private static readonly CultureInfo Sr = new("sr-Latn-RS");
        public string DatumFormatted => DatumPocetka?.ToString("dd.MM.yyyy", Sr) ?? "-";
        public string KolicinaFormatted => Kolicina.ToString("N2", Sr);
    }

    /// <summary>
    /// Kompletan model za PDF izvoz Radnog Naloga sa svim kontrolama.
    /// </summary>
    public class IzvodRadniNalogDetalji
    {
        // Header
        public long ID { get; set; }
        public string Sifra { get; set; } = string.Empty;
        public string Komitent { get; set; } = string.Empty;
        public string Artikal { get; set; } = string.Empty;
        public string Pakovanje { get; set; } = string.Empty;
        public decimal Kolicina { get; set; }
        public string? LotNaloga { get; set; }
        public int BrojPakovanja { get; set; }
        public DateTime? DatumPocetka { get; set; }
        public DateTime? DatumIsporuke { get; set; }
        public string? Opis { get; set; }
        public string? Ugovor { get; set; }

        // Kontrole
        public List<KontrolaProizvodnjaModel> KontroleProzvodnje { get; set; } = new();
        public List<KontrolaTemperaturaModel> Temperature { get; set; } = new();
        public List<KontrolaTezineModel> Tezine { get; set; } = new();
        public List<KontrolaZavrsnaModel> ZavrsneKontrole { get; set; } = new();

        private static readonly CultureInfo Sr = new("sr-Latn-RS");
        public string DatumPocetkaFormatted => DatumPocetka?.ToString("dd.MM.yyyy", Sr) ?? "-";
        public string DatumIsporukeFormatted => DatumIsporuke?.ToString("dd.MM.yyyy", Sr) ?? "-";
        public string KolicinaFormatted => Kolicina.ToString("N2", Sr);
    }

    public class IzvodDokumenataFilter
    {
        public DateTime? OdDatum { get; set; }
        public DateTime? DoDatum { get; set; }
        public int? KomitentId { get; set; }
        public string? KomitentTip { get; set; }
        public int? ArtikalId { get; set; }
        public string? TipArtikla { get; set; }
        public string? RadniNalog { get; set; }
    }
}
