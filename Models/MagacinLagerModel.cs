using System.ComponentModel.DataAnnotations;

namespace FruitSysWeb.Models
{
    public class MagacinLagerModel
    {
        [Display(Name = "Artikal ID")]
        public long ArtikalID { get; set; }
        
        [Display(Name = "Tip artikla")]
        public string? ArtikalTip { get; set; }
        
        // DODATO: Tip kao string property jer view vraća string
        [Display(Name = "Tip")]
        public string? Tip { get; set; }
        
        [Display(Name = "Artikal")]
        public string Artikal { get; set; } = string.Empty;
        
        [Display(Name = "Količina")]
        public decimal Kolicina { get; set; }
        
        [Display(Name = "Pakovanje")]
        public string? Pakovanje { get; set; }
        
        [Display(Name = "Jedinica mere")]
        public string? JM { get; set; }
        
        // DODATO: Dodatna polja iz view-a ako postoje
        [Display(Name = "Lot")]
        public string? Lot { get; set; }
        
        [Display(Name = "Rok važenja")]
        public DateTime? RokVazenja { get; set; }
        
        [Display(Name = "Cena")]
        public decimal? Cena { get; set; }
        
        [Display(Name = "Vrednost")]
        public decimal Vrednost => Kolicina * (Cena ?? 0);
        
        [Display(Name = "Status")]
        public string Status => GetStatusOpis();
        
        [Display(Name = "Tip naziv")]
        public string TipNaziv => GetTipNaziv();

        private string GetStatusOpis()
        {
            if (Kolicina <= 0) return "Nema na stanju";
            if (Kolicina < 10) return "Ispod minimuma";
            if (Kolicina < 20) return "Ograničeno";
            return "Dostupno";
        }

        private string GetTipNaziv()
        {
            if (int.TryParse(Tip ?? ArtikalTip, out int tipInt))
            {
                return tipInt switch
                {
                    1 => "Sirovina",
                    2 => "Ambalaza", 
                    3 => "Potrosni materijal",
                    4 => "Gotova roba",
                    5 => "Oprema",
                    _ => $"Tip {tipInt}"
                };
            }
            return "Nepoznato";
        }
    }
}