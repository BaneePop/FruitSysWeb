using System.Globalization;

namespace FruitSysWeb.Models.IzvodDokumenata
{
    public class IzvodOtpremnicaRow
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

    public class IzvodOtpremnicaDetalji
    {
        public long ID { get; set; }
        public string Sifra { get; set; } = string.Empty;
        public DateTime? Datum { get; set; }
        public string Komitent { get; set; } = string.Empty;
        public string KomitentAdresa { get; set; } = string.Empty;
        public string KomitentPib { get; set; } = string.Empty;
        public string KomitentMb { get; set; } = string.Empty;
        public string Vozac { get; set; } = string.Empty;
        public string Vozilo { get; set; } = string.Empty;
        public string BrojPasosa { get; set; } = string.Empty;
        public string Vozar { get; set; } = string.Empty;
        public string VozarPib { get; set; } = string.Empty;
        public string VozarAdresa { get; set; } = string.Empty;
        public string GranicniPrelaz { get; set; } = string.Empty;
        public string BrojPlombi { get; set; } = string.Empty;
        public decimal? Temperatura { get; set; }
        public string Lot { get; set; } = string.Empty;
        public string RadniNalog { get; set; } = string.Empty;
        public string Napomena { get; set; } = string.Empty;
        public string Spedicija { get; set; } = string.Empty;
        // Kvalitet
        public bool? Uzorkovano { get; set; }
        public bool? VizuelnaKontrola { get; set; }
        public string? VizuelnaKontrolaPrimedba { get; set; }
        public bool? StanjeRobePakovanja { get; set; }
        public bool? StanjeVozila { get; set; }
        public bool? KontrolaDeklaracija { get; set; }
        public string? TemperaturaPrimedba { get; set; }
        public decimal BrutoTezina { get; set; }

        public List<IzvodOtpremnicaStavka> Stavke { get; set; } = new();
        public List<IzvodOtpremnicaAmbalaza> Ambalaza { get; set; } = new();

        private static readonly CultureInfo Sr = new("sr-Latn-RS");
        public string DatumFormatted => Datum?.ToString("dd.MM.yyyy", Sr) ?? "-";
        public string BrutoTezinaFormatted => BrutoTezina.ToString("N2", Sr);
    }

    public class IzvodOtpremnicaStavka
    {
        public string Artikal { get; set; } = string.Empty;
        public string JM { get; set; } = string.Empty;
        public decimal Kolicina { get; set; }
        public string? Rok { get; set; }
        private static readonly CultureInfo Sr = new("sr-Latn-RS");
        public string KolicinaFormatted => Kolicina.ToString("N2", Sr);
    }

    public class IzvodOtpremnicaAmbalaza
    {
        public string Artikal { get; set; } = string.Empty;
        public string JM { get; set; } = string.Empty;
        public decimal Kolicina { get; set; }
        public decimal Tezina { get; set; }
        private static readonly CultureInfo Sr = new("sr-Latn-RS");
        public string KolicinaFormatted => Kolicina.ToString("N2", Sr);
        public string TezinaFormatted => Tezina.ToString("N2", Sr);
    }
}
