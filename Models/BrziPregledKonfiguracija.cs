namespace FruitSysWeb.Models;

public class BrziPregledKonfiguracija
{
    public List<int> IzabraniDobavljaci { get; set; } = new List<int>();
    public List<int> IzabraniKupci { get; set; } = new List<int>();
    public List<long> IzabraniArtikli { get; set; } = new List<long>();

    // Helper metode
    public bool ImaDobavljace() => IzabraniDobavljaci?.Any() == true;
    public bool ImaKupce() => IzabraniKupci?.Any() == true;
    public bool ImaArtikle() => IzabraniArtikli?.Any() == true;
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
