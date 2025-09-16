using System.ComponentModel.DataAnnotations;

namespace FruitSysWeb.Models
{
    public class RadniNalogModel
    {
        [Display(Name = "ID")]
        public long ID { get; set; }

        [Display(Name = "Broj Naloga")]
        public string BrojNaloga { get; set; } = string.Empty;

        [Display(Name = "Datum Početka")]
        public DateTime? DatumPocetka { get; set; }

        [Display(Name = "Datum Završetka")]
        public DateTime? DatumZavrsetka { get; set; }

        [Display(Name = "Komitent ID")]
        public long? KomitentID { get; set; }

        [Display(Name = "Komitent")]
        public string? Komitent { get; set; }

        [Display(Name = "Artikal ID")]
        public long? ArtikalID { get; set; }

        [Display(Name = "Artikal")]
        public string? Artikal { get; set; }

        [Display(Name = "Količina")]
        public decimal Kolicina { get; set; }

        [Display(Name = "Status Dokumenta")]
        public int DokumentStatus { get; set; }

        [Display(Name = "Aktivno")]
        public bool Aktivno { get; set; }

        [Display(Name = "Opis")]
        public string? Opis { get; set; }

        [Display(Name = "Tip Artikla")]
        public int? TipArtikla { get; set; }

        // Computed properties
        [Display(Name = "Status")]
        public string Status => DokumentStatus switch
        {
            2 => "Otvoren",
            3 => "Zaključen",
            _ => "Nepoznato"
        };

        [Display(Name = "Je Otvoren")]
        public bool JeOtvoren => DokumentStatus == 2;

        [Display(Name = "Trajanje (dani)")]
        public int? TrajanjeDani => DatumPocetka.HasValue && DatumZavrsetka.HasValue
            ? (int)(DatumZavrsetka.Value - DatumPocetka.Value).TotalDays
            : null;

        [Display(Name = "Status Izvršavanja")]
        public string StatusIzvrsavanja
        {
            get
            {
                if (!Aktivno) return "Neaktivan";
                if (DokumentStatus == 3) return "Završen";
                if (DatumZavrsetka.HasValue && DatumZavrsetka < DateTime.Now) return "Zakasnio";
                return "U toku";
            }
        }
    }
}

   /*  public class RadniNalogLagerModel
    {
        [Display(Name = "Broj Naloga")]
        public string BrojNaloga { get; set; } = string.Empty;
        
        [Display(Name = "Komitent")]
        public string Komitent { get; set; } = string.Empty;
        
        [Display(Name = "Artikal ID")]
        public long ArtikalID { get; set; }
        
        [Display(Name = "Artikal")]
        public string Artikal { get; set; } = string.Empty;
        
        [Display(Name = "Količina")]
        public decimal Kolicina { get; set; }
        
        [Display(Name = "Pakovanje")]
        public string? Pakovanje { get; set; }
        
        [Display(Name = "Broj Pakovanja")]
        public int BrojPakovanja { get; set; }
        
        [Display(Name = "Potrebna Količina")]
        public decimal PotrebnaKolicina { get; set; }
        
        [Display(Name = "Status Dokumenta")]
        public int DokumentStatus { get; set; }

        // Computed properties
        [Display(Name = "Nedostajuća Količina")]
        public decimal NedostajucaKolicina => Math.Max(0, PotrebnaKolicina - Kolicina);
        
        [Display(Name = "Je Otvoren")]
        public bool JeOtvoren => DokumentStatus == 2; // 2 = Otvoren prema dokumentu
        
        [Display(Name = "Status")]
        public string Status => DokumentStatus switch
        {
            2 => "Otvoren",
            3 => "Zaključen", 
            _ => "Nepoznato"
        };

        [Display(Name = "Procenat Završenosti")]
        public decimal ProcenatZavrsenosti => PotrebnaKolicina > 0 
            ? Math.Min(100, (Kolicina / PotrebnaKolicina) * 100) 
            : 0;

        [Display(Name = "Je Kompletiran")]
        public bool JeKompletiran => Kolicina >= PotrebnaKolicina;

        [Display(Name = "Status Boja")]
        public string StatusBoja => JeKompletiran ? "success" : NedostajucaKolicina > 0 ? "warning" : "info";
    }
} */