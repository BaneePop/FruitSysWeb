using System;

namespace FruitSysWeb.Models
{
    public class RadniNalogModel
    {
        public long Sifra { get; set; }
        public long KomitentID { get; set; }
    
        public DateTime? DatumIsporuke { get; set; }
        public DateTime? DatumPocetka { get; set; }

        public long LotNaloga { get; set; }
        public int DokumentStatus { get; set; }
        public decimal DirektanRadUkupno { get; set; }
        public decimal DirektanRadObracunato { get; set; }
    
        public decimal Kolicina { get; set; }
    
        
    }
}