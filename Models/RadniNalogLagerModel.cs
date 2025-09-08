using System.ComponentModel.DataAnnotations;

namespace FruitSysWeb.Models
{
    public class RadniNalogLagerModel
    {
        [Display(Name = "Broj naloga")]
        public string BrojNaloga { get; set; } = string.Empty;
        
        [Display(Name = "Komitent")]
        public string Komitent { get; set; } = string.Empty;
        
        [Display(Name = "Artikal ID")]
        public long ArtikalID { get; set; }
        
        [Display(Name = "Artikal")]
        public string Artikal { get; set; } = string.Empty;
        
        [Display(Name = "Količina")]
        public decimal Kolicina { get; set; }
        
        [Display(Name = "Pakovanje")]
        public string? Pakovanje { get; set; }
        
        [Display(Name = "Broj pakovanja")]
        public int? BrojPakovanja { get; set; }
        
        [Display(Name = "Potrebna količina")]
        public decimal PotrebnaKolicina { get; set; }
        
        [Display(Name = "Dokument status")]
        public int DokumentStatus { get; set; }
        
        // DODATO: Dodatna polja
        [Display(Name = "Datum kreiranja")]
        public DateTime? DatumKreiranja { get; set; }
        
        [Display(Name = "Status naziv")]
        public string StatusNaziv => GetStatusNaziv();
        
        [Display(Name = "Procenat izvršenja")]
        public decimal ProcenatIzvrsenja => PotrebnaKolicina > 0 ? (Kolicina / PotrebnaKolicina * 100) : 0;
        
        [Display(Name = "Ostalo")]
        public decimal Ostalo => Math.Max(0, PotrebnaKolicina - Kolicina);

        private string GetStatusNaziv()
        {
            return DokumentStatus switch
            {
                1 => "Kreiran",
                2 => "Otvoren", // Prema dokumentu
                3 => "Zaključen", // Prema dokumentu
                4 => "Odustano",
                _ => "Nepoznato"
            };
        }
    }
}
