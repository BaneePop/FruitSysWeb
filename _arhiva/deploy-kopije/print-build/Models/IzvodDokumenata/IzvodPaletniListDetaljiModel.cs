using System.Globalization;

namespace FruitSysWeb.Models.IzvodDokumenata
{
    public class IzvodPaletniListDetalji
    {
        public long ID { get; set; }
        public string Sifra { get; set; } = string.Empty;
        public DateTime? Datum { get; set; }
        public string AmbalazaNaziv { get; set; } = string.Empty;
        public decimal BrutoTezina { get; set; }
        public decimal NetoTezina { get; set; }
        public decimal ProsecnaTezina { get; set; }
        public string Komitent { get; set; } = string.Empty;
        public string ArtikalNaziv { get; set; } = string.Empty;
        public int PaletniListTip { get; set; }

        // Nabavka (Tip=1): veza sa prijemnicom
        public string OtpremnicaSifra { get; set; } = string.Empty;
        public string LotNaloga { get; set; } = string.Empty;

        // Gotova roba (Tip=2/3): veza sa radnim nalogom
        public string RadniNalogSifra { get; set; } = string.Empty;
        public List<string> DobijenOd { get; set; } = new();

        public List<IzvodPaletniListStavkaDetalji> Stavke { get; set; } = new();

        private static readonly CultureInfo Sr = new("sr-Latn-RS");
        public string DatumFormatted => Datum?.ToString("dd.MM.yyyy", Sr) ?? "-";
        public string BrutoTezinaFormatted => BrutoTezina.ToString("N2", Sr);
        public string NetoTezinaFormatted => NetoTezina.ToString("N2", Sr);
        public string ProsecnaTezinaFormatted => ProsecnaTezina.ToString("N2", Sr);

        public bool JeNabavka => PaletniListTip == 1;
    }

    public class IzvodPaletniListStavkaDetalji
    {
        public string Artikal { get; set; } = string.Empty;
        public string JM { get; set; } = string.Empty;
        public decimal Kolicina { get; set; }
        public decimal Procenat { get; set; }

        private static readonly CultureInfo Sr = new("sr-Latn-RS");
        public string KolicinaFormatted => Kolicina.ToString("N2", Sr);
        public string ProcenatFormatted => Procenat.ToString("N1", Sr);
    }
}
