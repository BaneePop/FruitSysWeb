using System.ComponentModel.DataAnnotations;

namespace FruitSysWeb.Models
{
    public class FakturaModel
    {
        [Display(Name = "ID")]
        public long ID { get; set; }

        [Display(Name = "Šifra")]
        public string Sifra { get; set; } = string.Empty;

        [Display(Name = "Datum")]
        public DateTime Datum { get; set; }

        [Display(Name = "Neto")]
        public decimal Neto { get; set; }

        [Display(Name = "Bruto")]
        public decimal Bruto { get; set; }

        [Display(Name = "Porez")]
        public decimal Porez { get; set; }

        [Display(Name = "Neto EUR")]
        public decimal? NetoEur { get; set; }

        [Display(Name = "Bruto EUR")]
        public decimal? BrutoEur { get; set; }

        [Display(Name = "Porez EUR")]
        public decimal? PorezEur { get; set; }

        [Display(Name = "Kreirano")]
        public DateTime Kreirano { get; set; }

        [Display(Name = "Ažurirano")]
        public DateTime Azurirano { get; set; }

        [Display(Name = "Version")]
        public int Version { get; set; }

        [Display(Name = "Komitent ID")]
        public long? KomitentID { get; set; }

        [Display(Name = "Komitent")]
        public string? Komitent { get; set; }

        [Display(Name = "Otpremnica ID")]
        public long? OtpremnicaID { get; set; }

        [Display(Name = "Ugovor ID")]
        public long? UgovorID { get; set; }

        [Display(Name = "Kurs EUR")]
        public decimal? KursEur { get; set; }

        [Display(Name = "Dokument Status")]
        public int DokumentStatus { get; set; }

        [Display(Name = "Aktivno")]
        public bool Aktivno { get; set; }

        // Computed properties
        [Display(Name = "Status")]
        public string Status => DokumentStatus switch
        {
            2 => "Otvoren",
            3 => "Zaključen",
            4 => "Storno",
            _ => "Nepoznato"
        };

        [Display(Name = "Ukupno RSD")]
        public decimal UkupnoRsd => Bruto;

        [Display(Name = "Ukupno EUR")]
        public decimal UkupnoEur => BrutoEur ?? 0;

        [Display(Name = "Stopa Poreza")]
        public decimal StopaPoreza => Neto > 0 ? (Porez / Neto) * 100 : 0;

        [Display(Name = "Dani od Datuma")]
        public int DaniOdDatuma => (int)(DateTime.Now - Datum).TotalDays;

        [Display(Name = "Je Zaključen")]
        public bool JeZakljucen => DokumentStatus == 3;

        [Display(Name = "Je Storno")]
        public bool JeStorno => DokumentStatus == 4;

        [Display(Name = "Status Boja")]
        public string StatusBoja => DokumentStatus switch
        {
            2 => "warning",
            3 => "success",
            4 => "danger",
            _ => "secondary"
        };

        [Display(Name = "Valuta")]
        public string Valuta => KursEur.HasValue ? "EUR" : "RSD";
    }
}
