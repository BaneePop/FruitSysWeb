using System.ComponentModel.DataAnnotations;

namespace FruitSysWeb.Models
{
    public class UgovorModel
    {
        [Display(Name = "ID Ugovora")]
        public long UgovorID { get; set; }

        [Display(Name = "Broj Ugovora")]
        public string BrojUgovora { get; set; } = string.Empty;

        [Display(Name = "Komitent")]
        public string Komitent { get; set; } = string.Empty;

        [Display(Name = "Komitent ID")]
        public long KomitentID { get; set; }

        [Display(Name = "Artikal")]
        public string Artikal { get; set; } = string.Empty;

        [Display(Name = "Artikal ID")]
        public long ArtikalID { get; set; }

        [Display(Name = "Ugovorena Količina")]
        public decimal UgovorenaKolicina { get; set; }

        [Display(Name = "Jedinična Cena (EUR)")]
        public decimal JedinicnaCenaEur { get; set; }

        [Display(Name = "Isporučeno")]
        public decimal Isporuceno { get; set; }

        [Display(Name = "Preostala Količina")]
        public decimal PreostalaKolicina { get; set; }

        [Display(Name = "Status Ugovora")]
        public int DokumentStatus { get; set; }

        [Display(Name = "Datum Ugovora")]
        public DateTime DatumUgovora { get; set; }

        // Computed properties
        [Display(Name = "Ukupna Vrednost (EUR)")]
        public decimal UkupnaVrednostEur => UgovorenaKolicina * JedinicnaCenaEur;

        [Display(Name = "Vrednost Preostalo (EUR)")]
        public decimal VrednostPreostaloEur => PreostalaKolicina * JedinicnaCenaEur;

        [Display(Name = "Procenat Isporuke")]
        public decimal ProcenatIsporuke => UgovorenaKolicina > 0 
            ? Math.Round((Isporuceno / UgovorenaKolicina) * 100, 1) 
            : 0;

        [Display(Name = "Status")]
        public string Status => DokumentStatus switch
        {
            2 => "Aktivan",
            3 => "Zaključen",
            _ => "Nepoznato"
        };

        [Display(Name = "Je Aktivan")]
        public bool JeAktivan => DokumentStatus == 2;

        [Display(Name = "Status Boja")]
        public string StatusBoja
        {
            get
            {
                if (!JeAktivan) return "secondary";
                if (PreostalaKolicina <= 0) return "success";
                return "warning";
            }
        }
    }
}