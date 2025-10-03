namespace FruitSysWeb.Models;

public class BrziPregledKonfiguracija
{
    public List<int> IzabraniDobavljaci { get; set; } = new List<int>();
    public List<int> IzabraniKupci { get; set; } = new List<int>();
    public List<long> IzabraniArtikli { get; set; } = new List<long>();
    
    // ✨ NOVO: Artikli po vrstama voća
    public Dictionary<string, List<long>> ArtikliPoVrstama { get; set; } = new Dictionary<string, List<long>>();

    // Helper metode
    public bool ImaDobavljace() => IzabraniDobavljaci?.Any() == true;
    public bool ImaKupce() => IzabraniKupci?.Any() == true;
    public bool ImaArtikle() => IzabraniArtikli?.Any() == true;
    public bool ImaArtiklePoVrstama() => ArtikliPoVrstama?.Any(x => x.Value.Any()) == true;
    
    // Helper za proveru da li vrsta ima artikle
    public bool ImaArtikleZaVrstu(string vrsta)
    {
        return ArtikliPoVrstama.ContainsKey(vrsta) && ArtikliPoVrstama[vrsta].Any();
    }
    
    // Helper za dobijanje artikala za vrstu
    public List<long> GetArtikliZaVrstu(string vrsta)
    {
        return ArtikliPoVrstama.ContainsKey(vrsta) ? ArtikliPoVrstama[vrsta] : new List<long>();
    }
}

public class BrziPregledStavka
{
    public int KomitentID { get; set; }
    public string? Naziv { get; set; }
    public decimal VrednostRobe { get; set; }
    public decimal Isplata { get; set; }
    public decimal Stanje => Isplata - VrednostRobe;
    public string StanjeBadgeClass => Stanje < 0 ? "bg-danger" : 
                                      Stanje > 0 ? "bg-success" : 
                                      "bg-secondary";
}

/// <summary>
/// Model za Roba na zalihama brzi pregled
/// </summary>
public class RobaZaliheStavka
{
    public string VrstaProizvoda { get; set; } = string.Empty;
    
    // Nabavka (KL- dokumenti)
    public decimal NabavkaKg { get; set; }
    public decimal NabavnaVrednost { get; set; }
    
    // Prodaja (FK- dokumenti)
    public decimal ProdajaKg { get; set; }
    public decimal ProdajaVrednost { get; set; }
    
    // Lager (trenutno stanje)
    public decimal LagerKg { get; set; }
    public decimal LagerVrednost { get; set; }
}
