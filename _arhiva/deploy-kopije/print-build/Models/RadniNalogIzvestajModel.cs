using System;
using System.ComponentModel.DataAnnotations;

namespace FruitSysWeb.Models
{
    // Helper model za radne procese
    /* public class RadniProcesModel
    {
        public long ID { get; set; }
        public string Naziv { get; set; } = string.Empty;
        public long? RezijaID { get; set; }
        public DateTime Kreirano { get; set; }
        public DateTime Azurirano { get; set; }
        public int Version { get; set; }
    } */

    // Helper model za proizvodne procese  
    /* public class ProizvodniProcesModel
    {
        public long ID { get; set; }
        public string Naziv { get; set; } = string.Empty;
        public DateTime Kreirano { get; set; }
        public DateTime Azurirano { get; set; }
        public int Version { get; set; }
    }
 */
    public class RadniNalogIzvestajModel
    {
        // Osnovni identifikatori
        public long ID { get; set; }
        public long RadniNalogID { get; set; }
        public long SmenskiIzvestajID { get; set; }
        public long RadniProcesID { get; set; }
        public long? KomitentID { get; set; }

        // Osnovne informacije
        [Display(Name = "Šifra")]
        public string Sifra { get; set; } = string.Empty;

        [Display(Name = "Radni nalog")]
        public string RadniNalog { get; set; } = string.Empty;

        [Display(Name = "Datum")]
        public DateTime? Datum { get; set; }

        [Display(Name = "Status")]
        public int DokumentStatus { get; set; }

        [Display(Name = "Radni proces")]
        public string RadniProces { get; set; } = string.Empty;

        [Display(Name = "Vrsta artikla")]
        public string VrstaArtikla { get; set; } = string.Empty;

        [Display(Name = "Komitent")]
        public string Komitent { get; set; } = string.Empty;

        // Brojčani podaci
        [Display(Name = "Br. radnika")]
        public int BrojRadnika { get; set; }

        [Display(Name = "Radni sati")]
        public decimal BrojRadnihSati { get; set; }

        [Display(Name = "Smena")]
        public int Smena { get; set; }

        [Display(Name = "Broj izveštaja")]
        public string BrojIzvestaja { get; set; } = string.Empty;

        // Finansijski podaci
        [Display(Name = "Cena kostanja - direktan rad")]
        public decimal CenaKostanjaDirektanRad { get; set; }

        [Display(Name = "Cena sata po reziji")]
        public decimal CenaSataPoReziji { get; set; }

        [Display(Name = "Trošak po RN")]
        public decimal TrosakPoRadnomNalogu { get; set; }

        // Podaci o robi
        [Display(Name = "Roba (kg)")]
        public decimal KolicinaRoba { get; set; }

        [Display(Name = "Procenat iskorišćenja")]
        public decimal ProcenatIskoriscenja { get; set; }

        // Kalkulisani podaci
        [Display(Name = "Trošak/sat")]
        public decimal TrosakPoSatu => BrojRadnihSati > 0 ? TrosakPoRadnomNalogu / BrojRadnihSati : 0;

        [Display(Name = "Trošak/radnik")]
        public decimal TrosakPoRadniku => BrojRadnika > 0 ? TrosakPoRadnomNalogu / BrojRadnika : 0;

        [Display(Name = "Sati/radnik")]
        public decimal SatiPoRadniku => BrojRadnika > 0 ? BrojRadnihSati / BrojRadnika : 0;

        [Display(Name = "Produktivnost")]
        public decimal Produktivnost => BrojRadnihSati > 0 ? KolicinaRoba / BrojRadnihSati : 0;

        // Helper properties za UI
        public string StatusBadgeClass => GetStatusBadgeClass(DokumentStatus);
        public string SmenaBadgeClass => GetSmenaBadgeClass(Smena);
        public string ProductivnostBadgeClass => GetProductivnostBadgeClass(Produktivnost);

        // Helper methods
        private static string GetStatusBadgeClass(int status) => status switch
        {
            2 => "bg-primary", // Otvoren
            3 => "bg-success", // Zaključen
            4 => "bg-danger",  // Storno
            _ => "bg-secondary"
        };

        private static string GetSmenaBadgeClass(int smena) => smena switch
        {
            1 => "bg-info",
            2 => "bg-warning text-dark",
            3 => "bg-dark",
            _ => "bg-secondary"
        };

        private static string GetProductivnostBadgeClass(decimal produktivnost) => produktivnost switch
        {
            > 20 => "bg-success",
            > 10 => "bg-warning text-dark",
            > 5 => "bg-info",
            _ => "bg-danger"
        };

        // Formatted properties za export
        public string FormattedDatum => Datum?.ToString("dd.MM.yyyy") ?? "";
        public string FormattedTrošak => TrosakPoRadnomNalogu.ToString("N2") + " RSD";
        public string FormattedSati => BrojRadnihSati.ToString("N2") + " h";
        public string FormattedRoba => KolicinaRoba.ToString("N2") + " kg";
        public string FormattedProcenat => ProcenatIskoriscenja.ToString("N1") + "%";
        public string FormattedTrosakPoSatu => TrosakPoSatu.ToString("N2") + " RSD/h";
        public string FormattedProduktivnost => Produktivnost.ToString("N2") + " kg/h";
    }


}
