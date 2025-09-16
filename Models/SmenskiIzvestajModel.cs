using System.ComponentModel.DataAnnotations;

namespace FruitSysWeb.Models
{
    public class SmenskiIzvestajModel
    {
        [Display(Name = "ID")]
        public long ID { get; set; }

        [Display(Name = "Broj")]
        public string Broj { get; set; } = string.Empty;

        [Display(Name = "Datum")]
        public DateTime Datum { get; set; }

        [Display(Name = "Smena")]
        public int Smena { get; set; }

        [Display(Name = "Dokument Status")]
        public int DokumentStatus { get; set; }

        [Display(Name = "Kreirano")]
        public DateTime Kreirano { get; set; }

        [Display(Name = "Ažurirano")]
        public DateTime Azurirano { get; set; }

        [Display(Name = "Version")]
        public int Version { get; set; }

        [Display(Name = "Poslovođa ID")]
        public long? PoslovodjaID { get; set; }

        [Display(Name = "Poslovođa")]
        public string? Poslovodja { get; set; }

        // Computed properties
        [Display(Name = "Status")]
        public string Status => DokumentStatus switch
        {
            2 => "Otvoren",
            3 => "Zaključen",
            _ => "Nepoznato"
        };

        [Display(Name = "Naziv Smene")]
        public string NazivSmene => Smena switch
        {
            1 => "Prva smena",
            2 => "Druga smena", 
            3 => "Treća smena",
            _ => $"Smena {Smena}"
        };

        [Display(Name = "Dani od Datuma")]
        public int DaniOdDatuma => (int)(DateTime.Now - Datum).TotalDays;

        [Display(Name = "Je Zaključen")]
        public bool JeZakljucen => DokumentStatus == 3;

        [Display(Name = "Je Otvoren")]
        public bool JeOtvoren => DokumentStatus == 2;

        [Display(Name = "Status Boja")]
        public string StatusBoja => DokumentStatus switch
        {
            2 => "warning",
            3 => "success",
            _ => "secondary"
        };
    }
}
