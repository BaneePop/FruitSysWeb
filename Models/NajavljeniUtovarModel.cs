using System;
using System.ComponentModel.DataAnnotations;

namespace FruitSysWeb.Models
{
    public class NajavljeniUtovarModel
    {
        // ID kolone (sakrivene u UI)
        public long ID { get; set; }
        public long? KomitentID { get; set; }
        public long? ArtikalInstancaID { get; set; }
        public long? UgovorProdajaID { get; set; }

        // Prikazne kolone
        [Display(Name = "Radni nalog")]
        public string RadniNalog { get; set; } = string.Empty;

        [Display(Name = "Datum utovara")]
        public DateTime? DatumIsporuke { get; set; }

        [Display(Name = "Kupac")]
        public string Kupac { get; set; } = string.Empty;

        [Display(Name = "Artikal")]
        public string Artikal { get; set; } = string.Empty;

        [Display(Name = "Potrebna količina")]
        public decimal Kolicina { get; set; }

        [Display(Name = "Lot")]
        public string? LotNaloga { get; set; }

        [Display(Name = "Ugovor prodaja")]
        public string? BrojUgovora { get; set; }

        [Display(Name = "Status")]
        public int DokumentStatus { get; set; }

        // Kalkulisana svojstva
        public int DanaDoUtovara => DatumIsporuke.HasValue
            ? (DatumIsporuke.Value.Date - DateTime.Today).Days
            : 0;

        public string StatusUrgentnosti => DanaDoUtovara switch
        {
            <= 3 => "Hitno",
            <= 7 => "Uskoro",
            _ => "Planirano"
        };

        public string UrgentnostBadgeClass => DanaDoUtovara switch
        {
            <= 3 => "bg-danger",
            <= 7 => "bg-warning text-dark",
            _ => "bg-primary"
        };

        // Helper za UI
        public string StatusBadgeClass => DokumentStatus == 2 ? "bg-primary" : "bg-secondary";

        // Formatted properties
        public string FormattedDatumIsporuke => DatumIsporuke?.ToString("dd.MM.yyyy") ?? "";
        public string FormattedKolicina => Kolicina.ToString("N2") + " kg";
    }
}
