using System.ComponentModel.DataAnnotations;

namespace FruitSysWeb.Models
{
    public class PreradaPregledModel
    {
        [Display(Name = "Datum")]
        public DateTime? Datum { get; set; }
        
        [Display(Name = "Radni nalog")]
        public string RadniNalog { get; set; } = string.Empty;
        
        [Display(Name = "Artikal")]
        public string Artikal { get; set; } = string.Empty;
        
        [Display(Name = "Količina gotov proizvod")]
        public decimal? KolicinaGotovProizvod { get; set; }
        
        [Display(Name = "Količina sirovina")]
        public decimal? KolicinaSirovina { get; set; }
        
        [Display(Name = "Komitent")]
        public string Komitent { get; set; } = string.Empty;
        
        [Display(Name = "Ukupna količina")]
        public decimal UkupnaKolicina => (KolicinaGotovProizvod ?? 0) + (KolicinaSirovina ?? 0);
    }
}