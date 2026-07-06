using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace FruitSysWeb.Models
{
    public class FakturaStavkaModel
    {
        [Display(Name = "ID")]
        public long ID { get; set; }

        [Display(Name = "Faktura ID")]
        public long FakturaID { get; set; }

        [Display(Name = "Artikal Instanca ID")]
        public long ArtikalInstancaID { get; set; }

        [Display(Name = "Artikal")]
        public string ArtikalNaziv { get; set; } = string.Empty;

        [Display(Name = "Lot")]
        public string? Lot { get; set; }

        [Display(Name = "Pakovanje")]
        public string? Pakovanje { get; set; }

        [Display(Name = "Kom/Kutija")]
        public int BrojJPuGP { get; set; }

        [Display(Name = "Količina (kg)")]
        public decimal Kolicina { get; set; }

        [Display(Name = "Br. Pakovanja")]
        public int BrojPakovanja { get; set; }

        [Display(Name = "Jed. Cena RSD")]
        public decimal JedinicnaCena { get; set; }

        [Display(Name = "Jed. Cena EUR")]
        public decimal JedinicnaCenaEur { get; set; }

        [Display(Name = "Neto Iznos RSD")]
        public decimal NetoIznos { get; set; }

        [Display(Name = "Neto Iznos EUR")]
        public decimal NetoIznosEur { get; set; }

        [Display(Name = "PDV %")]
        public decimal PorezStopa { get; set; }

        [Display(Name = "PDV Iznos RSD")]
        public decimal PorezIznos { get; set; }

        [Display(Name = "Bruto Iznos RSD")]
        public decimal BrutoIznos { get; set; }

        [Display(Name = "Bruto Iznos EUR")]
        public decimal BrutoIznosEur { get; set; }

        [Display(Name = "Kurs EUR")]
        public decimal KursEur { get; set; }

        // Computed
        private static readonly CultureInfo SrFormat = new CultureInfo("sr-Latn-RS");

        public string KolicinaFormatted => Kolicina.ToString("N2", SrFormat);
        public string JedinicnaCenaFormatted => JedinicnaCena.ToString("N2", SrFormat);
        public string JedinicnaCenaEurFormatted => JedinicnaCenaEur.ToString("N2", SrFormat);
        public string NetoIznosFormatted => NetoIznos.ToString("N2", SrFormat);
        public string NetoIznosEurFormatted => NetoIznosEur.ToString("N2", SrFormat);
        public string PorezIznosFormatted => PorezIznos.ToString("N2", SrFormat);
        public string BrutoIznosFormatted => BrutoIznos.ToString("N2", SrFormat);
        public string BrutoIznosEurFormatted => BrutoIznosEur.ToString("N2", SrFormat);
        public string PorezStopaFormatted => PorezStopa.ToString("N0", SrFormat) + "%";
    }
}
