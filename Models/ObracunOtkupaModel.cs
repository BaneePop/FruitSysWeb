namespace FruitSysWeb.Models;

/// <summary>
/// Model za obračun otkupa po dobavljačima
/// </summary>
public class ObracunOtkupaModel
{
    // Osnovni podaci
    public int KomitentID { get; set; }
    public string Dobavljac { get; set; } = string.Empty;

    // Finansijski podaci (od 01.06.2025)
    public decimal UkupnoZaduzenje { get; set; }   // Nabavna vrednost (Din)
    public decimal UkupnoIsplata { get; set; }     // Isplaćeno (Din)
    public decimal Stanje { get; set; }            // Razlika = Zaduženje - Isplata

    // Helper svojstva
    public bool ImaZaduzenje => UkupnoZaduzenje > 0;
    public bool JeIsplaceno => Stanje <= 0;

    // Badge klasa za stanje
    public string StanjeBadgeClass => Stanje switch
    {
        > 0 => "text-danger fw-bold",      // Dug (negativan za dobavljača)
        0 => "text-success fw-bold",       // Isplaćeno
        < 0 => "text-warning fw-bold"      // Preplaćeno
    };

    // Euro konverzija (opciono, kurs 117.5)
    public decimal UkupnoZaduzenjeEur { get; set; }
    public decimal UkupnoIsplataEur { get; set; }
    public decimal StanjeEur { get; set; }
}
