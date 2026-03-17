namespace FruitSysWeb.Models
{
    /// <summary>
    /// Filter za PaletniList pregled izveštaj (4 taba: Nabavka, Prodaja, Proizvodnja, Povezanost)
    /// </summary>
    public class PaletniListPregledFilter
    {
        public DateTime? OdDatum { get; set; }
        public DateTime? DoDatum { get; set; }
        public string? SifraPL { get; set; }        // Šifra paletnog lista
        public string? Komitent { get; set; }
        public string? TipArtikla { get; set; }      // Za Nabavka tab (MagacinID)
        public string? Artikal { get; set; }
        public string? Prijemnica { get; set; }     // Za Nabavka tab
        public string? Otpremnica { get; set; }     // Za Prodaja tab
        public string? RadniNalog { get; set; }     // Za Proizvodnja tab
        public string? EvidencijaRada { get; set; } // Za Proizvodnja tab
    }

    /// <summary>
    /// Red u izveštaju - zajednički za sve tipove
    /// </summary>
    public class PaletniListPregledRow
    {
        public long ID { get; set; }
        public string Sifra { get; set; } = "";
        public DateTime Datum { get; set; }
        public int PaletniListTip { get; set; } // 1=Nabavka, 2=Prodaja, 3=Proizvodnja

        // Nabavka
        public string? PrijemnicaSifra { get; set; }

        // Prodaja
        public string? OtpremnicaSifra { get; set; }

        // Proizvodnja
        public string? RadniNalogSifra { get; set; }
        public string? EvidencijaRadaSifra { get; set; }

        // Zajednicko
        public string Komitent { get; set; } = "";
        public string Artikal { get; set; } = "";
        public decimal Kolicina { get; set; }
        public decimal BrojAmbalaze { get; set; }
        public string Ambalaza { get; set; } = "";

        // Computed
        private static readonly System.Globalization.CultureInfo _sr = new("sr-Latn-RS");
        public string DatumFormatted => Datum.ToString("dd.MM.yyyy", _sr);
        public string TipNaziv => PaletniListTip switch
        {
            1 => "Nabavka",
            2 => "Prodaja",
            3 => "Proizvodnja",
            _ => "Nepoznato"
        };
    }

    /// <summary>
    /// Model za tab Povezanost - veze između paletnih listova
    /// </summary>
    public class PaletniListVezaModel
    {
        public long PaletniListID { get; set; }
        public string PaletniListSifra { get; set; } = "";
        public int PaletniListTip { get; set; }
        public decimal Kolicina { get; set; }
        public string Komitent { get; set; } = "";
        public string Artikal { get; set; } = "";
        public DateTime Datum { get; set; }

        // Dokumenti vezani za ovaj PL
        public string? PrijemnicaSifra { get; set; }
        public string? OtpremnicaSifra { get; set; }
        public string? RadniNalogSifra { get; set; }
        public string? EvidencijaRadaSifra { get; set; }

        private static readonly System.Globalization.CultureInfo _sr = new("sr-Latn-RS");
        public string DatumFormatted => Datum.ToString("dd.MM.yyyy", _sr);
        public string TipNaziv => PaletniListTip switch
        {
            1 => "Nabavka",
            2 => "Proizvodnja",
            3 => "Prodaja",
            _ => "Nepoznato"
        };
        public string TipBadgeClass => PaletniListTip switch
        {
            1 => "bg-success",
            2 => "bg-warning text-dark",
            3 => "bg-primary",
            _ => "bg-secondary"
        };
    }

    /// <summary>
    /// Filter za Kvalitet tab
    /// </summary>
    public class PaletniListKvalitetFilter
    {
        public DateTime? OdDatum { get; set; }
        public DateTime? DoDatum { get; set; }
        public string? Komitent { get; set; }
        public string? ArtikalID { get; set; }   // obavezno — ID artikla (Sveza Roba/Sirovine)
        public string? Prijemnica { get; set; }
    }

    /// <summary>
    /// Red u Kvalitet izveštaju — grupisano po Prijemnica/Komitent/Artikal
    /// </summary>
    public class PaletniListKvalitetRow
    {
        public string PrijemnicaSifra { get; set; } = "";
        public string Komitent { get; set; } = "";
        public string Artikal { get; set; } = "";
        public decimal KlasaI { get; set; }       // SUM Kolicina gde ArtikalID=3
        public decimal KlasaII { get; set; }      // SUM Kolicina gde ArtikalID=4
        public decimal KlasaIII { get; set; }     // SUM Kolicina gde ArtikalID=5
        public decimal Ukupno => KlasaI + KlasaII + KlasaIII;
        public decimal ProcenatKlaseI => Ukupno > 0 ? Math.Round(KlasaI / Ukupno * 100, 1) : 0;

        private static readonly System.Globalization.CultureInfo _sr = new("sr-Latn-RS");
        public string KlasaIStr => KlasaI.ToString("N2", _sr);
        public string KlasaIIStr => KlasaII.ToString("N2", _sr);
        public string KlasaIIIStr => KlasaIII.ToString("N2", _sr);
        public string UkupnoStr => Ukupno.ToString("N2", _sr);
        public string ProcenatStr => ProcenatKlaseI.ToString("N1", _sr) + " %";
    }

    /// <summary>
    /// Rezultat pretrage za Povezanost tab
    /// </summary>
    public class PaletniListPovezanostResult
    {
        public PaletniListVezaModel? Izvorni { get; set; }
        public List<PaletniListVezaModel> Povezani { get; set; } = new();
    }
}
