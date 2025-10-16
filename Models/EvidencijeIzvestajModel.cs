using System;

namespace FruitSysWeb.Models
{
    /// <summary>
    /// Model za evidencije izveštaj
    /// Kombinuje podatke iz EvidencijaRada, Artikal, ProizvodniProces, Komitent tabela
    /// </summary>
    public class EvidencijeIzvestajModel
    {
        public string BrojEvidencije { get; set; } = string.Empty;
        public string VrstaProizvoda { get; set; } = string.Empty;
        public string ProizvodniProces { get; set; } = string.Empty;
        public string Smenovoda { get; set; } = string.Empty;
        public decimal RadniSati { get; set; }
        public decimal Kolicina { get; set; }
        public decimal Efikasnost { get; set; }
        public string Status { get; set; } = string.Empty;

        // Helper properties za UI
        public string StatusBadgeClass
        {
            get
            {
                return Status switch
                {
                    "Otvoren" => "bg-primary",
                    "Zaključen" => "bg-success",
                    "Storno" => "bg-danger",
                    _ => "bg-secondary"
                };
            }
        }

        public string FormattedRadniSati => RadniSati.ToString("F2");
        public string FormattedKolicina => Kolicina.ToString("F2");
        public string FormattedEfikasnost => Efikasnost > 0 ? $"{Efikasnost:F2}%" : "-";
    }
}
