using System.ComponentModel.DataAnnotations;

namespace FruitSysWeb.Models
{
    public enum ArtikalTip
    {
        [Display(Name = "Sirovina")]
        Sirovina = 1,
        
        [Display(Name = "Ambalaza")]
        Ambalaza = 2,
        
        [Display(Name = "Potrosni materijal")]
        PotrosniMaterijal = 3,
        
        [Display(Name = "Gotova roba")]
        GotovaRoba = 4,
        
        [Display(Name = "Oprema")]
        Oprema = 5,
        
        [Display(Name = "Ostali tip")]
        OstaliTip = 7
    }

    public enum DokumentStatus
    {
        [Display(Name = "Otvoren")]
        Otvoren = 2,
        
        [Display(Name = "Zaključen")]
        Zakljucen = 3
    }

    public enum KomitentTip
    {
        [Display(Name = "Kupac")]
        Kupac = 1,
        
        [Display(Name = "Dobavljač")]
        Dobavljac = 2,
        
        [Display(Name = "Proizvođač")]
        Proizvodjac = 3,
        
        [Display(Name = "Otkupljivač")]
        Otkupljivac = 4
    }

    public static class ArtikalTipHelper
    {
        public static string GetDisplayName(int tipId)
        {
            return tipId switch
            {
                1 => "Sirovina",
                2 => "Ambalaza", 
                3 => "Potrosni materijal",
                4 => "Gotova roba",
                5 => "Oprema",
                7 => "Ostali tip",
                _ => $"Tip {tipId}"
            };
        }

        public static string GetBadgeClass(int tipId)
        {
            return tipId switch
            {
                1 => "bg-primary",           // Sirovina - plava
                2 => "bg-warning text-dark", // Ambalaza - žuta
                3 => "bg-secondary",         // Potrosni materijal - siva
                4 => "bg-success",           // Gotova roba - zelena
                5 => "bg-danger",            // Oprema - crvena
                7 => "bg-info",              // Ostali tip - svetlo plava
                _ => "bg-light text-dark"
            };
        }

        public static List<(int Value, string Text)> GetAllTypes()
        {
            return new List<(int, string)>
            {
                (1, "Sirovina"),
                (2, "Ambalaza"),
                (3, "Potrosni materijal"),
                (4, "Gotova roba"),
                (5, "Oprema"),
                (7, "Ostali tip")
            };
        }

        public static ArtikalTip? GetEnumValue(int tipId)
        {
            return tipId switch
            {
                1 => ArtikalTip.Sirovina,
                2 => ArtikalTip.Ambalaza,
                3 => ArtikalTip.PotrosniMaterijal,
                4 => ArtikalTip.GotovaRoba,
                5 => ArtikalTip.Oprema,
                7 => ArtikalTip.OstaliTip,
                _ => null
            };
        }
    }

    public static class DokumentStatusHelper
    {
        public static string GetDisplayName(int statusId)
        {
            return statusId switch
            {
                2 => "Otvoren",
                3 => "Zaključen",
                _ => "Nepoznato"
            };
        }

        public static string GetBadgeClass(int statusId)
        {
            return statusId switch
            {
                2 => "bg-warning text-dark", // Otvoren - žuta
                3 => "bg-success",           // Zaključen - zelena
                _ => "bg-secondary"          // Nepoznato - siva
            };
        }
    }

    public static class KomitentTipHelper
    {
        public static string GetDisplayName(string tipNaziv)
        {
            return tipNaziv?.ToLower() switch
            {
                "kupac" => "Kupac",
                "dobavljac" => "Dobavljač",
                "proizvodjac" => "Proizvođač",
                "otkupljivac" => "Otkupljivač",
                _ => "Ostalo"
            };
        }

        public static string GetBadgeClass(string tipNaziv)
        {
            return tipNaziv?.ToLower() switch
            {
                "kupac" => "bg-success",      // Kupac - zelena
                "dobavljac" => "bg-warning text-dark", // Dobavljač - žuta
                "proizvodjac" => "bg-info",   // Proizvođač - svetlo plava
                "otkupljivac" => "bg-secondary", // Otkupljivač - siva
                _ => "bg-light text-dark"
            };
        }

        public static List<(string Value, string Text)> GetAllTypes()
        {
            return new List<(string, string)>
            {
                ("kupac", "Kupac"),
                ("dobavljac", "Dobavljač"),
                ("proizvodjac", "Proizvođač"),
                ("otkupljivac", "Otkupljivač")
            };
        }
    }
}