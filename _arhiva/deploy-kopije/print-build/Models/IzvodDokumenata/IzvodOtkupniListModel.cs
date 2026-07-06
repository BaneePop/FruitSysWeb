using System.Globalization;

namespace FruitSysWeb.Models.IzvodDokumenata
{
    public class IzvodOtkupniListRow
    {
        public long ID { get; set; }
        public string Sifra { get; set; } = string.Empty;
        public DateTime? Datum { get; set; }
        public string Komitent { get; set; } = string.Empty;
        public string Artikal { get; set; } = string.Empty;
        public decimal IznosUkupno { get; set; }

        private static readonly CultureInfo Sr = new("sr-Latn-RS");
        public string DatumFormatted => Datum?.ToString("dd.MM.yyyy", Sr) ?? "-";
        public string IznosUkupnoFormatted => IznosUkupno.ToString("N2", Sr);
    }

    public class IzvodOtkupniListDetalji
    {
        public long ID { get; set; }
        public string Sifra { get; set; } = string.Empty;
        public DateTime? Datum { get; set; }
        public DateTime? DatumPrometa { get; set; }
        public string Komitent { get; set; } = string.Empty;
        public string KomitentAdresa { get; set; } = string.Empty;
        public string JMBG { get; set; } = string.Empty;
        public string OtkupnoMesto { get; set; } = string.Empty;
        public decimal IznosOsnovice { get; set; }
        public decimal StopaPDV { get; set; }
        public decimal IznosPDV { get; set; }
        public decimal IznosUkupno { get; set; }
        public string RokIsplate { get; set; } = string.Empty;
        public int? NacinIsplate { get; set; }
        public string RegistarskiBroj { get; set; } = string.Empty;
        public string TekuciRacun { get; set; } = string.Empty;
        public string PoreskiBroj { get; set; } = string.Empty;

        public List<IzvodOtkupniListStavka> Stavke { get; set; } = new();
        public List<IzvodOtkupniListAmbalaza> Ambalaza { get; set; } = new();

        private static readonly CultureInfo Sr = new("sr-Latn-RS");
        public string DatumFormatted => Datum?.ToString("dd.MM.yyyy", Sr) ?? "-";
        public string DatumPrometaFormatted => DatumPrometa?.ToString("dd.MM.yyyy", Sr) ?? DatumFormatted;

        public string NacinIsplateTekst => NacinIsplate switch
        {
            1 => "Gotovina",
            2 => "Ček",
            3 => "Kartica",
            4 => "Bankovni transfer",
            _ => "Bankovni transfer"
        };
    }

    public class IzvodOtkupniListStavka
    {
        public string Artikal { get; set; } = string.Empty;
        public string JM { get; set; } = string.Empty;
        public decimal Kolicina { get; set; }
        public decimal Cena { get; set; }
        public decimal Iznos { get; set; }

        private static readonly CultureInfo Sr = new("sr-Latn-RS");
        public string KolicinaFormatted => Kolicina.ToString("N2", Sr);
        public string CenaFormatted => Cena.ToString("N2", Sr);
        public string IznosFormatted => Iznos.ToString("N2", Sr);
    }

    public class IzvodOtkupniListAmbalaza
    {
        public string Naziv { get; set; } = string.Empty;
        public decimal Kolicina { get; set; }
        public decimal Cena { get; set; }
        public decimal Iznos { get; set; }

        private static readonly CultureInfo Sr = new("sr-Latn-RS");
        public string KolicinaFormatted => Kolicina.ToString("N2", Sr);
        public string CenaFormatted => Cena.ToString("N2", Sr);
        public string IznosFormatted => Iznos.ToString("N2", Sr);
    }
}
