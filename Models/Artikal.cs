using System.ComponentModel.DataAnnotations;

namespace FruitSysWeb.Models
{
    public class Artikal
    {
        [Display(Name = "ID")]
        public long Id { get; set; }
        
        [Display(Name = "Šifra")]
        public string Sifra { get; set; } = string.Empty;
        
        [Display(Name = "Naziv")]
        public string Naziv { get; set; } = string.Empty;
        
        [Display(Name = "Težina")]
        public decimal Tezina { get; set; }
        
        [Display(Name = "Visina")]
        public decimal Visina { get; set; }
        
        [Display(Name = "Širina")]
        public decimal Sirina { get; set; }
        
        [Display(Name = "Dužina")]
        public decimal Duzina { get; set; }
        
        [Display(Name = "Nema Lager")]
        public bool NemaLager { get; set; }
        
        [Display(Name = "Tip")]
        public int Tip { get; set; }
        
        [Display(Name = "Aktivan")]
        public bool Aktivan { get; set; }
        
        [Display(Name = "Težina Pune")]
        public decimal TezinaPune { get; set; }
        
        [Display(Name = "Prizemna Ambalaza")]
        public bool PrijemnaAmbalaza { get; set; }
        
        [Display(Name = "Jedinična Ambalaza")]
        public bool JedinicnaAmbalaza { get; set; }
        
        [Display(Name = "Grupna Ambalaza")]
        public bool GrupnaAmbalaza { get; set; }
        
        [Display(Name = "Ambalaza Tip")]
        public int AmbalazaTip { get; set; }
        
        [Display(Name = "Povratna Ambalaza")]
        public bool PovratnaAmbalaza { get; set; }
        
        [Display(Name = "Kreirano")]
        public DateTime Kreirano { get; set; }
        
        [Display(Name = "Ažurirano")]
        public DateTime Azurirano { get; set; }
        
        [Display(Name = "Poreska Tarifa ID")]
        public long PoreskaTarifaID { get; set; }
        
        [Display(Name = "Jedinica Mere ID")]
        public long JedinicaMereID { get; set; }
        
        [Display(Name = "Prva Klasifikacija ID")]
        public long? PrvaKlasifikacijaID { get; set; }
        
        [Display(Name = "Druga Klasifikacija ID")]
        public long? DrugaKlasifikacijaID { get; set; }
        
        [Display(Name = "Magacin ID")]
        public long MagacinID { get; set; }
        
        [Display(Name = "Lokacija ID")]
        public long LokacijaID { get; set; }
        
        [Display(Name = "Version")]
        public int Version { get; set; }
        
        [Display(Name = "Aktivno")]
        public bool Aktivno { get; set; }
        
        [Display(Name = "Otkupni Artikal")]
        public bool OtkupniArtikal { get; set; }
    }
}