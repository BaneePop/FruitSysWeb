using System.ComponentModel.DataAnnotations;

namespace FruitSysWeb.Models
{
    public class Artikal
    {
        [Display(Name = "ID")]
        public long Id { get; set; }
        
        [Display(Name = "Naziv")]
        public string Naziv { get; set; } = string.Empty;
        
        [Display(Name = "Tip")]
        public int Tip { get; set; }
        
        [Display(Name = "Tip naziv")]
        public string TipNaziv => GetTipNaziv(Tip);
        
        [Display(Name = "Jedinica mere ID")]
        public int? JedinicaMereID { get; set; }
        
        [Display(Name = "Datum kreiranja")]
        public DateTime? Kreirano { get; set; }

        private static string GetTipNaziv(int tip)
        {
            return tip switch
            {
                1 => "Sirovina",
                2 => "Ambalaza",
                3 => "Potrosni materijal",
                4 => "Gotova roba",
                5 => "Oprema",
                _ => $"Tip {tip}"
            };
        }
    }
}
