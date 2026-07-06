namespace FruitSysWeb.Models
{
    public class ArtikliStatistikaFilter
    {
        public DateTime OdDatum { get; set; }
        public DateTime DoDatum { get; set; }
    }

    public class ArtikliStatistikaRow
    {
        public int ArtikalID { get; set; }
        public string Artikal { get; set; } = string.Empty;
        public decimal Kolicina { get; set; }
        public decimal ProsecnaCena { get; set; }
        public decimal Vrednost { get; set; }
    }

    public class ArtikliStatistikaGrupa
    {
        public string NazivGrupe { get; set; } = string.Empty;
        public List<ArtikliStatistikaRow> Stavke { get; set; } = new();

        public decimal UkupnoKolicina => Stavke.Sum(s => s.Kolicina);
        public decimal UkupnoVrednost => Stavke.Sum(s => s.Vrednost);
        public decimal ProsecnaCenaUkupno => UkupnoKolicina > 0 ? UkupnoVrednost / UkupnoKolicina : 0;
    }
}
