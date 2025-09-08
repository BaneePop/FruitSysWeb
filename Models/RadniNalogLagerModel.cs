using System.ComponentModel.DataAnnotations;

namespace FruitSysWeb.Models
{
    public class RadniNalogLagerModel
    {
        [Display(Name = "Broj Naloga")]
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
        
        [Display(Name = "Broj Pakovanja")]
        public int BrojPakovanja { get; set; }
        
        [Display(Name = "Potrebna Količina")]
        public decimal PotrebnaKolicina { get; set; }
        
        [Display(Name = "Status Dokumenta")]
        public int DokumentStatus { get; set; }

        // Computed properties
        [Display(Name = "Nedostajuća Količina")]
        public decimal NedostajucaKolicina => Math.Max(0, PotrebnaKolicina - Kolicina);
        
        [Display(Name = "Je Otvoren")]
        public bool JeOtvoren => DokumentStatus == 2; // 2 = Otvoren prema dokumentu
        
        [Display(Name = "Status")]
        public string Status => DokumentStatus switch
        {
            2 => "Otvoren",
            3 => "Zaključen", 
            _ => "Nepoznato"
        };

        [Display(Name = "Procenat Završenosti")]
        public decimal ProcenatZavrsenosti => PotrebnaKolicina > 0 
            ? Math.Min(100, (Kolicina / PotrebnaKolicina) * 100) 
            : 0;

        [Display(Name = "Je Kompletiran")]
        public bool JeKompletiran => Kolicina >= PotrebnaKolicina;

        [Display(Name = "Status Boja")]
        public string StatusBoja => JeKompletiran ? "success" : NedostajucaKolicina > 0 ? "warning" : "info";
    }
}