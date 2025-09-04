using System.ComponentModel.DataAnnotations;

namespace FruitSysWeb.Models
{
    public class ProizvodnjaModel
    {
        [Display(Name = "Datum")]
        public DateTime? Datum { get; set; }
        
        [Display(Name = "Radni Nalog")]
        public string RadniNalog { get; set; } = string.Empty;
        
        [Display(Name = "Artikal")]
        public string Artikal { get; set; } = string.Empty;
        
        [Display(Name = "Količina")]
        public decimal Kolicina { get; set; }
        
        [Display(Name = "Komitent")]
        public string Komitent { get; set; } = string.Empty;
        
        [Display(Name = "Klasifikacija")]
        public string Klasifikacija { get; set; } = string.Empty;
        
        [Display(Name = "Tip Artikla")]
        public int TipArtikla { get; set; }
        
        public int RadniNalogID { get; set; }
    }
}