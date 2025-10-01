namespace FruitSysWeb.Models;

/// <summary>
/// Model za kompletan obračun - sva roba + troškovi
/// </summary>
public class ObracunSvaRobaModel
{
    // Nabavka (od 01.06.2025)
    public decimal NabavnaVrednostVoca { get; set; }        // Sirovine (MagacinID = 3)
    public decimal NabavnaVrednostAmbalaze { get; set; }    // Ambalaža (MagacinID = 4)
    
    // Troškovi prerade (od 01.06.2025)
    public decimal UkupanTrosakPrerade { get; set; }        // Iz EvidencijaRada
    
    // Prodaja (od 01.06.2025)
    public decimal UkupnaProdaja { get; set; }              // Gotov Proizvod (MagacinID = 6)
    
    // Roba na zalihama (trenutno stanje)
    public decimal VrednostRobeNaZalihama { get; set; }     // Po prosečnoj ceni zadnjih 3 kalkulacije
    
    // Kalkulisana svojstva
    public decimal UkupnaNabavka => NabavnaVrednostVoca + NabavnaVrednostAmbalaze;
    public decimal UkupniTroskovi => UkupnaNabavka + UkupanTrosakPrerade;
    public decimal Profit => UkupnaProdaja - UkupniTroskovi;
    public decimal ProfitnaStopaProcenat => UkupnaProdaja > 0 
        ? (Profit / UkupnaProdaja) * 100 
        : 0;
    
    // Profit po kg (prosečan)
    public decimal ProfitPoKg { get; set; }
    
    // Euro konverzija (opciono, sa kursom 117.5)
    public decimal UkupnaProdajaEur => UkupnaProdaja / 117.5m;
    public decimal ProfitEur => Profit / 117.5m;
    public decimal ProfitPoKgEur => ProfitPoKg / 117.5m;
}
