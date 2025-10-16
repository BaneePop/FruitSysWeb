namespace FruitSysWeb.Models
{
    public class KorisnikModel
    {
        public long ID { get; set; }
        public string Ime { get; set; } = string.Empty;
        public string Lozinka { get; set; } = string.Empty;
        public string Seed { get; set; } = string.Empty;
        public bool Administrator { get; set; }
        public DateTime Kreirano { get; set; }
        public DateTime Azurirano { get; set; }
        public int Version { get; set; }
        public long? RadnikID { get; set; }
        public long GrupaKorisnikaID { get; set; }
        public long? KomitentID { get; set; }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }

    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public KorisnikModel? Korisnik { get; set; }
    }
}