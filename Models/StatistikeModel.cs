using System;

namespace FruitSysWeb.Models
{
    /// <summary>
    /// Model za statistike prerada modula
    /// Agregira podatke iz EvidencijaRada, RadniNalog, vPreradaSaProcentima
    /// </summary>
    public class StatistikeModel
    {
        public decimal UkupnaProizvodnja { get; set; }
        public decimal UkupniRadniSati { get; set; }
        public decimal UkupniTrosakRada { get; set; }
        public decimal ProsecnaEfikasnost { get; set; }
        public int BrojEvidencija { get; set; }
        public int BrojRadnihNaloga { get; set; }

        // Kalkulisana svojstva
        public decimal ProsecniTrosakPoSatu
        {
            get
            {
                return UkupniRadniSati > 0 ? UkupniTrosakRada / UkupniRadniSati : 0;
            }
        }

        public decimal ProsecnaProizvodnjaPoDanu
        {
            get
            {
                return BrojEvidencija > 0 ? UkupnaProizvodnja / BrojEvidencija : 0;
            }
        }

        // Format helper properties
        public string FormattedUkupnaProizvodnja => UkupnaProizvodnja.ToString("F2");
        public string FormattedUkupniRadniSati => UkupniRadniSati.ToString("F2");
        public string FormattedUkupniTrosakRada => UkupniTrosakRada.ToString("C2");
        public string FormattedProsecnaEfikasnost => ProsecnaEfikasnost > 0 ? $"{ProsecnaEfikasnost:F2}%" : "-";
        public string FormattedProsecniTrosakPoSatu => ProsecniTrosakPoSatu.ToString("C2");
    }
}
