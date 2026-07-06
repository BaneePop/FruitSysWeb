using System.Globalization;

namespace FruitSysWeb.Models
{
    /// <summary>
    /// Finansijsko stanje jednog komitenta — agregirano po tipu
    /// Koristi se za stranicu /finansijsko-stanje (tabovi: Dobavljaci, Kupci, Proizvodjaci, Otkupljivaci)
    /// </summary>
    public class FinansijskoStanjeModel
    {
        private static readonly CultureInfo SrFormat = new CultureInfo("sr-Latn-RS");

        public long KomitentID { get; set; }
        public string Naziv { get; set; } = string.Empty;

        /// <summary>
        /// Potrazuje: ono sto oni duguju nama (FK- fakture, IS- isplate koje primamo)
        /// </summary>
        public decimal Potrazuje { get; set; }

        /// <summary>
        /// Duguje: ono sto mi dugujemo njima (KL- kalkulacije, OL- otkupni listovi, UP- uplate njima)
        /// </summary>
        public decimal Duguje { get; set; }

        /// <summary>
        /// Saldo = Potrazuje - Duguje
        /// Pozitivno = oni nam duguju
        /// Negativno = mi njima dugujemo
        /// </summary>
        public decimal Saldo => Potrazuje - Duguje;

        /// <summary>
        /// Datum zadnje finansijske promene (MAX datum iz view-a)
        /// </summary>
        public DateTime? DatumZadnjePromene { get; set; }

        // ─── Formatiranje ───
        public string PotrazujeFormatted => Potrazuje.ToString("N2", SrFormat);
        public string DugujeFormatted => Duguje.ToString("N2", SrFormat);
        public string SaldoFormatted => Saldo.ToString("N2", SrFormat);
        public string DatumFormatted => DatumZadnjePromene?.ToString("dd.MM.yyyy", SrFormat) ?? "-";

        // Koliko dana je proslo od zadnje promene
        public int DanaCekanja => DatumZadnjePromene.HasValue
            ? (DateTime.Now.Date - DatumZadnjePromene.Value.Date).Days
            : 0;

        public string KasnjenjeBadgeClass => DanaCekanja switch
        {
            <= 30 => "bg-success",
            <= 60 => "bg-warning text-dark",
            _ => "bg-danger"
        };

        public string SaldoTextClass => Saldo switch
        {
            > 0 => "text-success fw-bold",
            < 0 => "text-danger fw-bold",
            _ => "text-secondary"
        };

        public string SaldoIkonica => Saldo switch
        {
            > 0 => "bi-arrow-up-circle-fill text-success",
            < 0 => "bi-arrow-down-circle-fill text-danger",
            _ => "bi-dash-circle-fill text-secondary"
        };
    }
}
