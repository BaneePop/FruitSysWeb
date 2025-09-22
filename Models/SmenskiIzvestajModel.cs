using System;

namespace FruitSysWeb.Models
{
    /// <summary>
    /// Model za smenski izveštaj
    /// Reprezentuje jedan zapis u SmenskiIzvestaj tabeli
    /// </summary>
    public class SmenskiIzvestajModel
    {
        public long ID { get; set; }
        public string Broj { get; set; } = string.Empty;
        public DateTime Datum { get; set; }
        public int Smena { get; set; }
        public int DokumentStatus { get; set; }
        public string Smenovoda { get; set; } = string.Empty;
        
        // Foreign Keys
        public long? PoslovodjaID { get; set; }
        public long? ProizvodniProcesID { get; set; }
        
        // Metadata
        public DateTime Kreirano { get; set; }
        public DateTime Azurirano { get; set; }
        public int Version { get; set; }
        
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
        
        public string SmenaText
        {
            get
            {
                return Smena switch
                {
                    1 => "I (06-14h)",
                    2 => "II (14-22h)",
                    3 => "III (22-06h)",
                    _ => "Nepoznato"
                };
            }
        }
        
        public string SmenaBadgeClass
        {
            get
            {
                return Smena switch
                {
                    1 => "bg-info text-dark",
                    2 => "bg-warning text-dark",
                    3 => "bg-dark text-white",
                    _ => "bg-secondary"
                };
            }
        }
        
        public string DanUNedeljiNaziv
        {
            get
            {
                return Datum.DayOfWeek switch
                {
                    DayOfWeek.Monday => "Ponedeljak",
                    DayOfWeek.Tuesday => "Utorak",
                    DayOfWeek.Wednesday => "Sreda",
                    DayOfWeek.Thursday => "Četvrtak",
                    DayOfWeek.Friday => "Petak",
                    DayOfWeek.Saturday => "Subota",
                    DayOfWeek.Sunday => "Nedelja",
                    _ => "Nepoznato"
                };
            }
        }
        
        // Format helper properties
        public string FormattedDatum => Datum.ToString("dd.MM.yyyy");
        public bool IsWeekend => Datum.DayOfWeek == DayOfWeek.Saturday || Datum.DayOfWeek == DayOfWeek.Sunday;
        public bool IsNightShift => Smena == 3;
    }
}
