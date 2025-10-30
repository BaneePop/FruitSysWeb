using System.ComponentModel.DataAnnotations;

namespace FruitSysWeb.Models
{
    /// <summary>
    /// Agregat za jedan Komitent + Artikal (za cache)
    /// </summary>
    public class PovratnaAmbalazaAgregat
    {
        public long KomitentID { get; set; }
        public string Komitent { get; set; } = string.Empty;
        public long ArtikalID { get; set; }
        public decimal UkupnoUlaz { get; set; }
        public decimal UkupnoIzlaz { get; set; }

        /// <summary>
        /// Stanje = Ulaz - Izlaz
        /// Ako je pozitivno - ONI NAM DUGUJU (mi smo im dali više)
        /// Ako je negativno - MI NJIMA DUGUJEMO (oni su nam dali više)
        /// </summary>
        public decimal Stanje => UkupnoIzlaz - UkupnoUlaz;

        public decimal Duguje => Stanje > 0 ? Stanje : 0;
        public decimal Potrazuje => Stanje < 0 ? Math.Abs(Stanje) : 0;
    }

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
