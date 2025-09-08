using System.ComponentModel.DataAnnotations;

namespace FruitSysWeb.Models
{
    public class Komitent
    {
        [Display(Name = "ID")]
        public long Id { get; set; }
        
        [Display(Name = "Naziv")]
        public string Naziv { get; set; } = string.Empty;
        
        [Display(Name = "PIB")]
        public string? PoreskiBroj { get; set; }
        
        [Display(Name = "Matični broj")]
        public string? MaticniBroj { get; set; }
        
        [Display(Name = "Adresa")]
        public string? Adresa { get; set; }
        
        [Display(Name = "Mesto")]
        public string? Mesto { get; set; }
        
        [Display(Name = "Telefon")]
        public string? Telefon { get; set; }
        
        [Display(Name = "Kupac")]
        public bool JeKupac { get; set; }
        
        [Display(Name = "Dobavljač")]
        public bool JeDobavljac { get; set; }
        
        [Display(Name = "Proizvođač")]
        public bool JeProizvodjac { get; set; }
        
        [Display(Name = "Otkupljivač")]
        public bool JeOtkupljivac { get; set; }
        
        [Display(Name = "Fizičko lice")]
        public bool FizickoLice { get; set; }
        
        [Display(Name = "Aktivno")]
        public bool Aktivno { get; set; }
        
        [Display(Name = "Datum kreiranja")]
        public DateTime? Kreirano { get; set; }
    }
}
