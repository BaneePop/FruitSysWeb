using System.Globalization;

namespace FruitSysWeb.Models
{
    public class UcitajRadneNalogeModel
    {
        public long ID { get; set; }
        public string Sifra { get; set; } = string.Empty;
        public string Komitent { get; set; } = string.Empty;
        public string Artikal { get; set; } = string.Empty;
        public string Pakovanje { get; set; } = string.Empty;
        public DateTime? DatumPocetka { get; set; }
        public DateTime? DatumIsporuke { get; set; }
        public decimal Kolicina { get; set; }
        public string? LotNaloga { get; set; }
        public int BrojPakovanja { get; set; }
        public int BrojEvidencija { get; set; }
        public string? OtpremnicaSifra { get; set; }

        private static readonly CultureInfo SrFormat = new CultureInfo("sr-Latn-RS");
        public string DatumPocetkaFormatted => DatumPocetka?.ToString("dd.MM.yyyy", SrFormat) ?? "-";
        public string DatumIsporukeFormatted => DatumIsporuke?.ToString("dd.MM.yyyy", SrFormat) ?? "-";
        public string KolicinaFormatted => Kolicina.ToString("N2", SrFormat);
        public int? BrojDana => (DatumPocetka.HasValue && DatumIsporuke.HasValue)
            ? (int)(DatumIsporuke.Value.Date - DatumPocetka.Value.Date).TotalDays
            : null;
    }
}
