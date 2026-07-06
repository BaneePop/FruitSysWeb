using System.Globalization;

namespace FruitSysWeb.Models.IzvodDokumenata
{
    public class IzvodPrijemnicaRow
    {
        public long ID { get; set; }
        public string Sifra { get; set; } = string.Empty;
        public DateTime? Datum { get; set; }
        public string Komitent { get; set; } = string.Empty;
        public string Artikal { get; set; } = string.Empty;
        public decimal Kolicina { get; set; }

        private static readonly CultureInfo Sr = new("sr-Latn-RS");
        public string DatumFormatted => Datum?.ToString("dd.MM.yyyy", Sr) ?? "-";
        public string KolicinaFormatted => Kolicina.ToString("N2", Sr);
    }

    public class IzvodPrijemnicaDetalji
    {
        // Header podaci
        public long ID { get; set; }
        public string Sifra { get; set; } = string.Empty;
        public DateTime? Datum { get; set; }
        public string Komitent { get; set; } = string.Empty;
        public string KomitentAdresa { get; set; } = string.Empty;
        public string Vozac { get; set; } = string.Empty;
        public string Vozilo { get; set; } = string.Empty;
        public string OtpremnicaDobavljaca { get; set; } = string.Empty;
        public string OtkupnoMesto { get; set; } = string.Empty;
        public string Napomena { get; set; } = string.Empty;
        // Kvalitet
        public bool? Uzorkovano { get; set; }
        public decimal? Temperatura { get; set; }
        public string? TemperaturaPrimedba { get; set; }
        public bool? VizuelnaKontrola { get; set; }
        public string? VizuelnaKontrolaPrimedba { get; set; }
        public bool? StanjeRobePakovanja { get; set; }
        public string? StanjeRobePakovanjaPrimedba { get; set; }
        public bool? Primiti { get; set; }
        public bool? PrimitiUzSelekciju { get; set; }
        public bool? Reklamirati { get; set; }
        public bool? Vratiti { get; set; }
        public string? UslovnoPrimitiZa { get; set; }

        // Stavke
        public List<IzvodPrijemnicaStavka> Stavke { get; set; } = new();
        public List<IzvodPrijemnicaStavka> Specifikacija { get; set; } = new();

        private static readonly CultureInfo Sr = new("sr-Latn-RS");
        public string DatumFormatted => Datum?.ToString("dd.MM.yyyy", Sr) ?? "-";
    }

    public class IzvodPrijemnicaStavka
    {
        public string Artikal { get; set; } = string.Empty;
        public string JM { get; set; } = string.Empty;
        public decimal Kolicina { get; set; }
        public decimal Procenat { get; set; }
        public string? Rok { get; set; }
        private static readonly CultureInfo Sr = new("sr-Latn-RS");
        public string KolicinaFormatted => Kolicina.ToString("N2", Sr);
        public string ProcenatFormatted => Procenat.ToString("N1", Sr);
    }

    public class IzvodPaletniListRow
    {
        public long ID { get; set; }
        public string Sifra { get; set; } = string.Empty;
        public decimal Kolicina { get; set; }
        public string Komitent { get; set; } = string.Empty;
        private static readonly CultureInfo Sr = new("sr-Latn-RS");
        public string KolicinaFormatted => Kolicina.ToString("N2", Sr);
    }
}
