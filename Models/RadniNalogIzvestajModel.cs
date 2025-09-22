using System;

namespace FruitSysWeb.Models
{
    /// <summary>
    /// Model za izveštaj radnih naloga po evidenciji rada
    /// Kombinuje podatke iz EvidencijaRada, RadniNalog, RadniProces, SmenskiIzvestaj tabela
    /// </summary>
    public class RadniNalogIzvestajModel
    {
        // Osnovni identifikatori - neće se prikazivati u UI
        public long ID { get; set; }
        public long RadniNalogID { get; set; }
        public long SmenskiIzvestajID { get; set; }
        public long RadniProcesID { get; set; }
        public long? RezijaID { get; set; }
        public long? KomitentID { get; set; }

        // Prikaz kolone - glavne kolone za prikaz
        public string SifraEvidencije { get; set; } = string.Empty;
        public string RadniNalog { get; set; } = string.Empty;
        public DateTime Datum { get; set; }
        public int DokumentStatus { get; set; }
        
        // Proces i komitent info
        public string RadniProces { get; set; } = string.Empty;
        public string? Komitent { get; set; }
        
        // Radni podaci - brojevi za kalkulaciju
        public int BrojRadnika { get; set; }
        public decimal BrojRadnihSati { get; set; }
        public bool RezijskiProces { get; set; }
        public decimal CenaSataPoReziji { get; set; }
        
        // Troškovi - kalkulišu se na osnovu radnih sati i cene
        public decimal CenaKostanjaDirektanRad { get; set; }
        public decimal TrosakPoRadnomNalogu { get; set; }
        public decimal TrosakPoSatu
        {
            get
            {
                return BrojRadnihSati > 0 ? TrosakPoRadnomNalogu / BrojRadnihSati : 0;
            }
        }
        
        // Procenat iskorišćenja - iz vPreradaSaProcentima view-a
        public decimal ProcenatIskoriscenja { get; set; }
        
        // Klasifikacija i artikal info - dodatne informacije
        public string? ArtikalPrvaKlasifikacija { get; set; }
        public string? Artikal { get; set; }
        public decimal? Kolicina { get; set; }
        
        // Smena info - iz SmenskiIzvestaj
        public int? Smena { get; set; }
        public string? SmenskiIzvestajBroj { get; set; }
        
        // Metadata - sistemske kolone
        public DateTime Kreirano { get; set; }
        public DateTime Azurirano { get; set; }
        public int Version { get; set; }
        public bool Obrisan { get; set; }
        
        // Helper properties za UI
        public string StatusText
        {
            get
            {
                return DokumentStatus switch
                {
                    2 => "Otvoren",
                    3 => "Zaključen",
                    4 => "Storno",
                    _ => "Nepoznato"
                };
            }
        }
        
        public string StatusBadgeClass
        {
            get
            {
                return DokumentStatus switch
                {
                    2 => "bg-primary",     // Otvoren - plavo
                    3 => "bg-success",     // Zaključen - zeleno  
                    4 => "bg-danger",      // Storno - crveno
                    _ => "bg-secondary"
                };
            }
        }
        
        public string SmenaBadgeClass
        {
            get
            {
                return Smena switch
                {
                    1 => "bg-info",        // Prva smena - svetlo plavo
                    2 => "bg-warning",     // Druga smena - žuto
                    3 => "bg-dark",        // Treća smena - tamno
                    _ => "bg-secondary"
                };
            }
        }
        
        // Format helper za export
        public string FormattedTrosakPoRadnomNalogu => TrosakPoRadnomNalogu.ToString("C2");
        public string FormattedBrojRadnihSati => BrojRadnihSati.ToString("F2");
        public string FormattedProcenatIskoriscenja => ProcenatIskoriscenja > 0 ? $"{ProcenatIskoriscenja:F2}%" : "-";
        public string FormattedDatum => Datum.ToString("dd.MM.yyyy");
    }
}