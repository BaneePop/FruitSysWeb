using System.ComponentModel.DataAnnotations;

namespace FruitSysWeb.Models
{
    public class RadniProcesModel
    {
        [Display(Name = "ID")]
        public long ID { get; set; }

        [Display(Name = "Naziv")]
        public string Naziv { get; set; } = string.Empty;

        [Display(Name = "Kreirano")]
        public DateTime Kreirano { get; set; }

        [Display(Name = "Ažurirano")]
        public DateTime Azurirano { get; set; }

        [Display(Name = "Version")]
        public int Version { get; set; }

        // Computed properties
        [Display(Name = "Status")]
        public string Status => "Aktivan";

        [Display(Name = "Dani od Kreiranja")]
        public int DaniOdKreiranja => (int)(DateTime.Now - Kreirano).TotalDays;

        [Display(Name = "Poslednja Izmena")]
        public string PoslednjaIzmena => (DateTime.Now - Azurirano).TotalDays switch
        {
            < 1 => "Danas",
            < 7 => $"Pre {Math.Ceiling((DateTime.Now - Azurirano).TotalDays)} dana",
            _ => "Staro"
        };

        [Display(Name = "Je Nov")]
        public bool JeNov => (DateTime.Now - Kreirano).TotalDays < 30;
    }
}
