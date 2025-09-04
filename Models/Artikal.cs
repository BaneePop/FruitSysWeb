namespace FruitSysWeb.Models


{
    public class Artikal
    {
        public int Id { get; set; }
        public string? Naziv { get; set; }
        public string? Sifra { get; set; }
        public decimal Cena { get; set; }
        public long Tip { get; set; } 
        public int? PrvaKlasifikacijaID { get; set; }
        public int? JedinicaMereID { get; set; }
        public bool OtkupniArtikal { get; set; }
    }
}