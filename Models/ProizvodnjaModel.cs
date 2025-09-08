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
        
        [Display(Name = "Radni Nalog ID")]
        public long RadniNalogID { get; set; }  // Promenjeno sa int na long
        
        // Dodajte dodatne properties za kompletnost
        [Display(Name = "Artikal ID")]
        public long? ArtikalID { get; set; }
        
        [Display(Name = "Komitent ID")]
        public long? KomitentID { get; set; }
        
        [Display(Name = "Status")]
        public string? Status { get; set; }
        
        [Display(Name = "Broj Pakovanja")]
        public int? BrojPakovanja { get; set; }
        
        [Display(Name = "Lot Naloga")]
        public string? LotNaloga { get; set; }
        
        [Display(Name = "Opis")]
        public string? Opis { get; set; }
        
        [Display(Name = "Datum Isporuke")]
        public DateTime? DatumIsporuke { get; set; }
        
        [Display(Name = "Jedinica Mere")]
        public string? JedinicaMere { get; set; }
        
        // Calculated properties
        [Display(Name = "Dani do Isporuke")]
        public int? DaniDoIsporuke => DatumIsporuke.HasValue ? 
            (int)(DatumIsporuke.Value - DateTime.Now).TotalDays : null;
        
        [Display(Name = "Status Naloga")]
        public string StatusNaloga => DatumIsporuke.HasValue && DatumIsporuke < DateTime.Now ? 
            "Zakasnio" : "U toku";
    }
}