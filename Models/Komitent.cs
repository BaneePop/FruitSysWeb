using System.ComponentModel.DataAnnotations;

namespace FruitSysWeb.Models
{
    public class Komitent
    {
        [Display(Name = "ID")]
        public long Id { get; set; }
        
        [Display(Name = "Šifra")]
        public string Sifra { get; set; } = string.Empty;
        
        [Display(Name = "Naziv")]
        public string Naziv { get; set; } = string.Empty;
        
        [Display(Name = "Fizičko Lice")]
        public bool FizickoLice { get; set; }
        
        [Display(Name = "Poreski Broj")]
        public string? PoreskiBroj { get; set; }
        
        [Display(Name = "Matični Broj")]
        public string? MaticniBroj { get; set; }
        
        [Display(Name = "Ime")]
        public string? Ime { get; set; }
        
        [Display(Name = "Prezime")]
        public string? Prezime { get; set; }
        
        [Display(Name = "Srednje Ime")]
        public string? SIme { get; set; }
        
        [Display(Name = "Broj Lične Karte")]
        public string? BrLk { get; set; }
        
        [Display(Name = "Je Dobavljač")]
        public bool JeDobavljac { get; set; }
        
        [Display(Name = "Je Kupac")]
        public bool JeKupac { get; set; }
        
        [Display(Name = "Je Proizvođač")]
        public bool JeProizvodjac { get; set; }
        
        [Display(Name = "Je Otkupljivač")]
        public bool JeOtkupljivac { get; set; }
        
        [Display(Name = "Je Prevoznik")]
        public bool JePrevoznik { get; set; }
        
        [Display(Name = "Inostrani")]
        public bool Ino { get; set; }
        
        [Display(Name = "Adresa")]
        public string Adresa { get; set; } = string.Empty;
        
        [Display(Name = "Poštanski Broj")]
        public string PostanskiBroj { get; set; } = string.Empty;
        
        [Display(Name = "Mesto")]
        public string Mesto { get; set; } = string.Empty;
        
        [Display(Name = "Država")]
        public string Drzava { get; set; } = string.Empty;
        
        [Display(Name = "Telefon")]
        public string? Telefon { get; set; }
        
        [Display(Name = "Kreirano")]
        public DateTime Kreirano { get; set; }
        
        [Display(Name = "Ažurirano")]
        public DateTime Azurirano { get; set; }
        
        [Display(Name = "Version")]
        public int Version { get; set; }
        
        [Display(Name = "Poreski Status")]
        public int PoreskiStatus { get; set; }
        
        [Display(Name = "Aktivno")]
        public bool Aktivno { get; set; }
        
        [Display(Name = "Broj Računa")]
        public string? BrojRacuna { get; set; }
        
        [Display(Name = "Broj Registra Poljoprivrednog Gazdinstva")]
        public string? BrojRegPoljGazdinstva { get; set; }
    }
}