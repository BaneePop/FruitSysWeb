using System.ComponentModel.DataAnnotations;

namespace FruitSysWeb.Models
{
    public class PovratnaAmbalazaModel
    {
        [Display(Name = "Datum")]
        public DateTime Datum { get; set; }

        [Display(Name = "Komitent ID")]
        public long KomitentID { get; set; }

        [Display(Name = "Komitent")]
        public string Komitent { get; set; } = string.Empty;

        [Display(Name = "Dokument")]
        public string Dokument { get; set; } = string.Empty;

        [Display(Name = "Artikal ID")]
        public long ArtikalID { get; set; }

        [Display(Name = "Artikal")]
        public string Artikal { get; set; } = string.Empty;

        [Display(Name = "Ulaz")]
        public decimal Ulaz { get; set; }

        [Display(Name = "Izlaz")]
        public decimal Izlaz { get; set; }

        [Display(Name = "Stanje")]
        public decimal Stanje => Ulaz - Izlaz;

        [Display(Name = "Duguje")]
        public decimal Duguje => Izlaz > Ulaz ? Izlaz - Ulaz : 0;

        [Display(Name = "Potražuje")]
        public decimal Potrazuje => Ulaz > Izlaz ? Ulaz - Izlaz : 0;
    }
}
