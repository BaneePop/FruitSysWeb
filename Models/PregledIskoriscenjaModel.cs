namespace FruitSysWeb.Models
{
    // TAB 1 - Sledljivost: rezultat pretrage po šifri PL
    public class PLSledljivostModel
    {
        public long PaletniListID { get; set; }
        public string Sifra { get; set; } = "";
        public string Artikal { get; set; } = "";
        public string? Komitent { get; set; }
        public decimal Tezina { get; set; }
        public decimal BrutoTezina { get; set; }
        public int PaletniListTip { get; set; }
        public DateTime DatumKreiranja { get; set; }
        public string? LotDobavljaca { get; set; }

        // EvidencijaRada u kojoj je PL utroseni
        public List<PLEvidencijaRadaModel> UtroseniU { get; set; } = new();

        // PL iz PaletniListoviPracenje gde je ovaj PL = TPaletniListID (sirovine/ambalaža)
        // → prikazujemo PL iz kolone PaletniListID (PL proizvodnje koji je koristio ovaj PL)
        public List<PLPovezaniModel> PovezaniProizvodni { get; set; } = new();
    }

    public class PLEvidencijaRadaModel
    {
        public long EvidencijaRadaID { get; set; }
        public string EvidencijaSifra { get; set; } = "";
        public DateTime EvidencijaDatum { get; set; }
        public string RadniNalogSifra { get; set; } = "";
        public long RadniNalogID { get; set; }
        public string SmenskiIzvestajBroj { get; set; } = "";
        public long SmenskiIzvestajID { get; set; }
        public DateTime SmenskiDatum { get; set; }
        public int Smena { get; set; }
        public string PoslovodjaNaziv { get; set; } = "";
        public string RadniProcesNaziv { get; set; } = "";
    }

    public class PLPovezaniModel
    {
        public long PaletniListID { get; set; }
        public string Sifra { get; set; } = "";
        public string Artikal { get; set; } = "";
        public string? Komitent { get; set; }
        public decimal Kolicina { get; set; }
        public string? RadniNalogSifra { get; set; }
        public DateTime DatumKreiranja { get; set; }
    }

    // TAB 2 - Lista iskorišćenosti PL sa filterima (samo Nabavka)
    public class PLIskoriscenjeListaModel
    {
        public long ID { get; set; }
        public string Sifra { get; set; } = "";
        public string Artikal { get; set; } = "";
        public long ArtikalID { get; set; }
        public string? Komitent { get; set; }
        public decimal Tezina { get; set; }
        public decimal BrutoTezina { get; set; }
        public DateTime DatumKreiranja { get; set; }
        public string? LotDobavljaca { get; set; }
        public string? VrstaArtikla { get; set; }   // naziv iz ArtikalKlasifikacija
        public int BrojEvidencijaUtroseno { get; set; }
        public bool JeIskoriscen => BrojEvidencijaUtroseno > 0;
    }

    // Filter za TAB 2
    public class PLIskoriscenjeFilter
    {
        public DateTime? OdDatum { get; set; }
        public DateTime? DoDatum { get; set; }
        public long? ArtikalID { get; set; }
        public long? PrvaKlasifikacijaID { get; set; }   // vrsta artikla
        public long? KomitentID { get; set; }
        public bool? SamoNeiskorisceni { get; set; }
    }

    // TAB 3 - PL Statistika: PL korišćeni u više naloga/evidencija
    public class PLStatistikaModel
    {
        public long ID { get; set; }
        public string Sifra { get; set; } = "";
        public string Artikal { get; set; } = "";
        public long ArtikalID { get; set; }
        public string? Komitent { get; set; }
        public decimal Tezina { get; set; }
        public DateTime DatumKreiranja { get; set; }
        public string? VrstaArtikla { get; set; }
        public int BrojRadnihNaloga { get; set; }
        public int BrojEvidencija { get; set; }
        // true = Ambalaža (PrvaKlasifikacijaID=25), false = Sirovine
        public bool JeAmbalaža => VrstaArtiklaID == 25;
        public long VrstaArtiklaID { get; set; }
    }
}
