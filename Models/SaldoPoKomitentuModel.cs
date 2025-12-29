namespace FruitSysWeb.Models;

/// <summary>
/// Model za pregled salda po komitentima
/// Kombinuje Potražuje (KL- dokumenti) i Duguje (FK- dokumenti) iz vPrometFinansijev9
/// </summary>
public class SaldoPoKomitentuModel
{
    public long KomitentID { get; set; }
    public string Komitent { get; set; } = string.Empty;

    /// <summary>
    /// Ukupno potražuje (šta oni duguju nama) - KL- dokumenti
    /// </summary>
    public decimal Potrazuje { get; set; }

    /// <summary>
    /// Ukupno duguje (šta mi dugujemo njima) - FK-, IS-, UP- dokumenti
    /// </summary>
    public decimal Duguje { get; set; }

    /// <summary>
    /// Stanje = Duguje - Potražuje
    /// Pozitivno = oni nam duguju
    /// Negativno = mi njima dugujemo
    /// </summary>
    public decimal Stanje => Duguje - Potrazuje;

    /// <summary>
    /// Apsolutna vrednost stanja za filtriranje
    /// </summary>
    public decimal ApsolutnoStanje => Math.Abs(Stanje);

    /// <summary>
    /// Bootstrap badge klasa za stanje
    /// </summary>
    public string StanjeBadgeClass => Stanje switch
    {
        > 0 => "bg-success text-white",   // Oni nam duguju (pozitivno)
        < 0 => "bg-danger text-white",    // Mi njima dugujemo (negativno)
        _ => "bg-secondary text-white"    // Nula
    };

    /// <summary>
    /// Ikonica za stanje
    /// </summary>
    public string StanjeIkonica => Stanje switch
    {
        > 0 => "bi-arrow-up-circle-fill",     // Oni nam duguju
        < 0 => "bi-arrow-down-circle-fill",   // Mi njima dugujemo
        _ => "bi-dash-circle-fill"            // Nula
    };
}
