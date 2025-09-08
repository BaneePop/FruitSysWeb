using System.ComponentModel.DataAnnotations;

namespace FruitSysWeb.Models
{
    public class MagacinLagerModel
    {
        [Display(Name = "Lager ID")]
        public long MagacinLagerID { get; set; }
        
        [Display(Name = "Artikal ID")]
        public long ArtikalID { get; set; }
        
        [Display(Name = "Tip Artikla")]
        public string? ArtikalTip { get; set; }
        
        [Display(Name = "Artikal")]
        public string? Artikal { get; set; }
        
        [Display(Name = "Količina")]
        public decimal Kolicina { get; set; }
        
        [Display(Name = "Pakovanje")]
        public string? Pakovanje { get; set; }
        
        [Display(Name = "Jedinica Mere")]
        public string? JM { get; set; }

        // Dodatna polja iz vwMagacinLager view-a
        [Display(Name = "Broj Pakovanja")]
        public int BrojPakovanja { get; set; }
        
        [Display(Name = "Tip Pakovanja")]
        public int PakovanjeTip { get; set; }
        
        [Display(Name = "Rok Važenja")]
        public DateTime? RokVazenja { get; set; }
        
        [Display(Name = "Lot")]
        public string? Lot { get; set; }
        
        [Display(Name = "Radni Nalog")]
        public string? RadniNalog { get; set; }
        
        [Display(Name = "Artikal Instanca ID")]
        public long ArtikalInstancaID { get; set; }
        
        [Display(Name = "Pakovanje ID")]
        public long PakovanjeID { get; set; }
        
        [Display(Name = "Ambalaza Tip")]
        public int AmbalazaTip { get; set; }

        // Computed properties
        [Display(Name = "Status Zalihe")]
        public string StatusZalihe => Kolicina switch
        {
            < 10 => "Kritično",
            < 50 => "Nisko",
            < 100 => "Umjereno",
            _ => "Dovoljno"
        };

        [Display(Name = "Dani Do Isteka")]
        public int? DaniDoIsteka => RokVazenja.HasValue && RokVazenja != DateTime.Parse("2099-12-31")
            ? (int)(RokVazenja.Value - DateTime.Now).TotalDays
            : null;

        [Display(Name = "Istice Rok")]
        public bool IsticePoskoro => DaniDoIsteka.HasValue && DaniDoIsteka < 30;

        // Helper metode
        public int GetArtikalTipAsInt()
        {
            if (int.TryParse(ArtikalTip, out int tip))
            {
                return tip;
            }
            return 0;
        }

        public string GetBadgeClass()
        {
            return ArtikalTipHelper.GetBadgeClass(GetArtikalTipAsInt());
        }

        public string GetTipNaziv()
        {
            return ArtikalTipHelper.GetDisplayName(GetArtikalTipAsInt());
        }
    }
}