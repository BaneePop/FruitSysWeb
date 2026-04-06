namespace FruitSysWeb.Models
{
    /// <summary>
    /// Model za čuvanje globalne selekcije artikala na Stanje Kesa - Altiva stranici
    /// </summary>
    public class StanjeKeseSelekcija
    {
        public List<long> SelektovaniArtikli { get; set; } = new List<long>();
    }
}
