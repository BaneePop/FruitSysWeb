namespace FruitSysWeb.Models;

/// <summary>
/// Model za prikaz robe na zalihama - kombinacija nabavke, prodaje i lagera
/// </summary>
public class RobaZalihaModel
{
    // Osnovni podaci
    public string VrstaVoca { get; set; } = string.Empty;
    public int ArtikalID { get; set; }
    public int MagacinID { get; set; }
    
    // Nabavka (od 01.06.2025)
    public decimal NabavkaKolicina { get; set; }  // U kg
    public decimal NabavkaVrednost { get; set; }   // Ukupna vrednost
    
    // Prodaja (od 01.06.2025)
    public decimal ProdajaKolicina { get; set; }   // U kg
    public decimal ProdajaVrednost { get; set; }   // Ukupna vrednost
    
    // Lager (trenutno stanje)
    public decimal LagerKolicina { get; set; }     // U kg
    public decimal LagerVrednost { get; set; }     // Trenutna vrednost
    
    // Prosečne cene
    public decimal ProsecnaNabavnaCena { get; set; }
    public decimal ProsecnaProdajnaCena { get; set; }
    
    // Badge klase za UI
    public string MagacinBadgeClass => GetMagacinBadgeClass(MagacinID);
    
    private string GetMagacinBadgeClass(int magacinId) => magacinId switch
    {
        2 => "bg-danger text-white",      // Sveza Roba
        3 => "bg-info text-white",        // Sirovine
        6 => "bg-success text-white",     // Gotov Proizvod
        _ => "bg-secondary text-white"
    };
}
