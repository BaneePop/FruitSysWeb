using System;

namespace FruitSysWeb.Models
{
    /// <summary>
    /// Model za evidenciju rada
    /// Reprezentuje jedan zapis u EvidencijaRada tabeli
    /// </summary>
    public class EvidencijaRadaModel
    {
        public long ID { get; set; }
        public string Sifra { get; set; } = string.Empty;
        public DateTime Datum { get; set; }
        public string Naziv { get; set; } = string.Empty;
        public int DokumentStatus { get; set; }
        public decimal BrojRadnihSati { get; set; }
        public int BrojRadnika { get; set; }
        public decimal CenaKostanjaDirektanRad { get; set; }
        public bool RezijskiProces { get; set; }
        public decimal CenaSataPoReziji { get; set; }

        // Foreign Keys
        public long RadniNalogID { get; set; }
        public long SmenskiIzvestajID { get; set; }
        public long RadniProcesID { get; set; }
        public long? RezijaID { get; set; }

        // Metadata
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
                    2 => "bg-primary",
                    3 => "bg-success",
                    4 => "bg-danger",
                    _ => "bg-secondary"
                };
            }
        }

        // Kalkulisana svojstva
        public decimal TrosakPoSatu
        {
            get
            {
                return BrojRadnihSati > 0 ? CenaKostanjaDirektanRad / BrojRadnihSati : 0;
            }
        }

        public decimal TrosakPoRadniku
        {
            get
            {
                return BrojRadnika > 0 ? CenaKostanjaDirektanRad / BrojRadnika : 0;
            }
        }

        // Format helper properties
        public string FormattedDatum => Datum.ToString("dd.MM.yyyy");
        public string FormattedBrojRadnihSati => BrojRadnihSati.ToString("F2");
        public string FormattedCenaKostanjaDirektanRad => CenaKostanjaDirektanRad.ToString("C2");
        public string FormattedTrosakPoSatu => TrosakPoSatu.ToString("C2");
    }
}
