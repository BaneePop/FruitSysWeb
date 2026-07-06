namespace FruitSysWeb.Models;

/// <summary>
/// Model za brzi pregled robe na zalihama
/// Kombinuje podatke iz vPrometFinansijev9, vwMagacinLager i KalkulacijaArtikalCena
/// </summary>
public class RobaNaZalihamaModel
{
    public long ArtikalID { get; set; }
    public string Artikal { get; set; } = string.Empty;

    // === NABAVKA (KL- dokumenti) ===
    public decimal NabavkaKg { get; set; }
    public decimal NabavkaVrednost { get; set; }

    // === PRODAJA (FK- dokumenti) ===
    public decimal ProdajaKg { get; set; }
    public decimal ProdajaVrednost { get; set; }

    // === LAGER (trenutno stanje) ===
    public decimal LagerKg { get; set; }
    public decimal LagerVrednost { get; set; }
    public decimal BrutoCena { get; set; } // Iz KalkulacijaArtikalCena

    // === KALKULISANA POLJA ===
    public decimal RazlikaKg => NabavkaKg - ProdajaKg;
    public decimal Profit => ProdajaVrednost - NabavkaVrednost;
    public decimal ProcentMarze => NabavkaVrednost > 0
        ? (Profit / NabavkaVrednost) * 100
        : 0;

    // === HELPER PROPERTIES ZA UI ===
    public string RazlikaBadgeClass => RazlikaKg switch
    {
        > 0 => "bg-success",
        < 0 => "bg-danger",
        _ => "bg-secondary"
    };

    public string ProfitBadgeClass => Profit switch
    {
        > 0 => "bg-success",
        < 0 => "bg-danger",
        _ => "bg-secondary"
    };
}
