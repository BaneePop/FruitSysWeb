using System.ComponentModel.DataAnnotations;

namespace FruitSysWeb.Models
{
    public class OtpremnicaDetaljiModel
    {
        [Display(Name = "ID")]
        public long ID { get; set; }

        [Display(Name = "Broj Otpremnice")]
        public string BrojOtpremnice { get; set; } = string.Empty;

        [Display(Name = "Datum")]
        public DateTime Datum { get; set; }

        [Display(Name = "Dokument Status")]
        public int DokumentStatus { get; set; }

        [Display(Name = "Količina")]
        public decimal Kolicina { get; set; }

        [Display(Name = "Artikal")]
        public string Artikal { get; set; } = string.Empty;

        [Display(Name = "Komitent")]
        public string Komitent { get; set; } = string.Empty;

        [Display(Name = "Status")]
        public string Status => DokumentStatus switch
        {
            2 => "Otvoren",
            3 => "Zaključen",
            4 => "Storno",
            _ => "Nepoznato"
        };
    }
}
