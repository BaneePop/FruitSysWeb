namespace FruitSysWeb.Models
{
    /// <summary>
    /// Model za stavke ulaza (nabavka) u izveštaju Promene
    /// </summary>
    public class PromenUlazModel
    {
        public long DokumentID { get; set; }
        public string Dokument { get; set; } = string.Empty;
        public DateTime Datum { get; set; }
        public string VrstaProizvoda { get; set; } = string.Empty;
        public string Dobavljac { get; set; } = string.Empty;
        public decimal Kolicina { get; set; }
        public decimal Cena { get; set; }
        public decimal Vrednost { get; set; }
        public int MagacinID { get; set; }
    }

    /// <summary>
    /// Model za stavke izlaza (prodaja) u izveštaju Promene
    /// </summary>
    public class PromenIzlazModel
    {
        public long DokumentID { get; set; }
        public string Dokument { get; set; } = string.Empty;
        public DateTime Datum { get; set; }
        public string VrstaProizvoda { get; set; } = string.Empty;
        public string Kupac { get; set; } = string.Empty;
        public decimal Kolicina { get; set; }
        public decimal Cena { get; set; }
        public decimal Vrednost { get; set; }
        public int MagacinID { get; set; }
    }

    /// <summary>
    /// Model za finansijske transakcije (uplate/isplate) u izveštaju Promene
    /// </summary>
    public class PromenFinansijeModel
    {
        public long DokumentID { get; set; }
        public string Dokument { get; set; } = string.Empty;
        public DateTime Datum { get; set; }
        public string TipPrometa { get; set; } = string.Empty;
        public string Komitent { get; set; } = string.Empty;
        public decimal Uplata { get; set; }
        public decimal Isplata { get; set; }
    }

    /// <summary>
    /// Filter za izveštaj Promene
    /// </summary>
    public class PromenFilter
    {
        public DateTime OdDatum { get; set; }
        public DateTime DoDatum { get; set; }
    }

    /// <summary>
    /// Kompletni podaci za jedan period u izveštaju Promene
    /// </summary>
    public class PromenPeriodData
    {
        public List<PromenUlazModel> UlazSirovineProizvodi { get; set; } = new();
        public List<PromenUlazModel> UlazAmbalaza { get; set; } = new();
        public List<PromenUlazModel> UlazRepromaterijal { get; set; } = new();
        public List<PromenIzlazModel> IzlazGotovaRobaSirovine { get; set; } = new();
        public List<PromenIzlazModel> IzlazAmbalaza { get; set; } = new();
        public List<PromenIzlazModel> IzlazRepromaterijal { get; set; } = new();
        public List<PromenFinansijeModel> Finansije { get; set; } = new();
    }
}
