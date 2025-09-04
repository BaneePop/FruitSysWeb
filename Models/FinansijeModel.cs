using System.ComponentModel.DataAnnotations;
using System.Text;

namespace FruitSysWeb.Models
{
    public class FinansijeModel
    {
        public long ID { get; set; }
        public long KomitentID { get; set; }
        
        [Display(Name = "Komitent")]
        public string? Komitent { get; set; } = string.Empty;

        public bool Otkupljivac { get; set; }
        public bool Proizvodjac { get; set; }
        public bool Dobavljac { get; set; }
        public bool Kupac { get; set; }
        public long DokumentID { get; set; }
        
        [Display(Name = "Datum")]
        public DateTime? Datum { get; set; }

        [Display(Name = "Dokument")]
        public string? Dokument { get; set; } = string.Empty;

        public string? DokumentTip { get; set; }
        public string? DokumentStatus { get; set; }
        public long ArtikalID { get; set; }
        
        [Display(Name = "Artikal")]
        public string? Artikal { get; set; } = string.Empty;

        public long? ArtikalPrvaKlasifikacijaID { get; set; }
        
        [Display(Name = "Količina")]
        public decimal Kolicina { get; set; }

        [Display(Name = "Potražuje")]
        public decimal Potrazuje { get; set; }

        [Display(Name = "Duguje")]
        public decimal Duguje { get; set; }

        [Display(Name = "Saldo")]
        public decimal Saldo { get; set; }

        public string? Roba { get; set; }
        public decimal Provizija { get; set; }
        public decimal Prevoz { get; set; }
        public decimal Marza { get; set; }
        public decimal PCenaPrijem { get; set; }
        public decimal PCenaUkupno { get; set; }
        public decimal Uplata { get; set; }
        public decimal PorezIznos { get; set; }
        public decimal PorezIznos2 { get; set; }
        public decimal UplataPdv { get; set; }
        public decimal NetoIznosOtkup { get; set; }

        // Dodatni podaci iz komitent tabele
        public bool FizickoLice { get; set; }

        // Dodatni podaci iz artikal tabele  
        public int ArtikalTip { get; set; }
        public int? JedinicaMereID { get; set; }
        public bool OtkupniArtikal { get; set; }

        // Calculated properties
        public decimal UkupnoZaduzenje => Duguje + Provizija + Prevoz;
        public decimal NetoSaldo => Saldo - Uplata;
    }
}