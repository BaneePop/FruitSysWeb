using System.Globalization;

namespace FruitSysWeb.Models
{
    public class OtvoreniRadniNalogModel
    {
        public long ID { get; set; }
        public string Sifra { get; set; } = string.Empty;
        public string Komitent { get; set; } = string.Empty;
        public string Artikal { get; set; } = string.Empty;
        public string Pakovanje { get; set; } = string.Empty;
        public DateTime? DatumPocetka { get; set; }
        public decimal Kolicina { get; set; }
        public string? LotNaloga { get; set; }
        public int BrojPakovanja { get; set; }
        public int BrojEvidencija { get; set; }
        // Lager podaci
        public decimal KolicinaULageru { get; set; }
        public decimal FaliDoZavrsetka => Math.Max(0, Kolicina - KolicinaULageru);

        private static readonly CultureInfo SrFormat = new CultureInfo("sr-Latn-RS");
        public string DatumPocetkaFormatted => DatumPocetka?.ToString("dd.MM.yyyy", SrFormat) ?? "-";
        public string KolicinaFormatted => Kolicina.ToString("N2", SrFormat);
        public string KolicinaULageruFormatted => KolicinaULageru.ToString("N2", SrFormat);
        public string FaliDoZavrsetkaFormatted => FaliDoZavrsetka.ToString("N2", SrFormat);
        public int BrojDana => DatumPocetka.HasValue
            ? (int)(DateTime.Today - DatumPocetka.Value.Date).TotalDays
            : 0;
    }
}
