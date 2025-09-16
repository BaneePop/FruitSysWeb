using System.ComponentModel.DataAnnotations;

namespace FruitSysWeb.Models
{
    public class ArtikalKlasifikacija
    {
        [Display(Name = "ID")]
        public long Id { get; set; }
        
        [Display(Name = "Naziv")]
        public string Naziv { get; set; } = string.Empty;
        
        [Display(Name = "Opis")]
        public string? Opis { get; set; }
        
        [Display(Name = "Aktivno")]
        public bool Aktivno { get; set; }
        
        [Display(Name = "Datum kreiranja")]
        public DateTime? Kreirano { get; set; }
    }
}
