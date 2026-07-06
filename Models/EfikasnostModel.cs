namespace FruitSysWeb.Models
{
    // Jedan red u tabeli — kumulativ po poslovođi za odabrani period
    public class EfikasnostPoslovodjeModel
    {
        public long PoslovodjaID { get; set; }
        public string Poslovodja { get; set; } = "";
        public int BrojSmena { get; set; }
        public decimal UkupnoRadnikaSati { get; set; }
        public int BrojEvidencija { get; set; }
        public int BrojRadnihNaloga { get; set; }
        public decimal GotovProizvod { get; set; }

        public int ProsecnoRadnikaSatiPoSmeni => BrojSmena > 0 ? (int)Math.Round(UkupnoRadnikaSati / BrojSmena) : 0;
        // Prosečan broj radnika = prosečni rad.·sati po smeni / 8
        public int ProsecnoBrojRadnikaPoSmeni => (int)Math.Round(ProsecnoRadnikaSatiPoSmeni / 8m);
        // Gotov proizvod po radnom satu = ukupno GP / ukupno rad.·sati
        public int GotovProizvodPoRadnomSatu => UkupnoRadnikaSati > 0 ? (int)Math.Round(GotovProizvod / UkupnoRadnikaSati) : 0;
        // Gotov proizvod po radniku = ukupno GP / ukupan broj radnika-smena (UkupnoRadnikaSati / 8)
        public int GotovProizvodPoRadniku => UkupnoRadnikaSati > 0 ? (int)Math.Round(GotovProizvod / (UkupnoRadnikaSati / 8m)) : 0;
    }

    // Detalji smena za odabranog poslovođu — drill-down
    public class EfikasnostSmenaDetaljiModel
    {
        public long SmenskiIzvestajID { get; set; }
        public string SmenskiBroj { get; set; } = "";
        public DateTime Datum { get; set; }
        public int Smena { get; set; }
        public int BrojEvidencija { get; set; }
        public int BrojRadnihNaloga { get; set; }
        public decimal UkupnoRadnikaSati { get; set; }
        public decimal UkupnoBrojRadnika { get; set; }
    }

    // Filter za oba pregleda
    public class EfikasnostFilter
    {
        public DateTime? OdDatum { get; set; }
        public DateTime? DoDatum { get; set; }
        public long? PoslovodjaID { get; set; }
        public int? Smena { get; set; }          // null=sve, 1=dan, 2=noć
    }
}
