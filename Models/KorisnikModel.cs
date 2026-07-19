namespace FruitSysWeb.Models
{
    public class KorisnikModel
    {
        public long Id { get; set; }
        public string Ime { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public KorisnikModel? Korisnik { get; set; }
    }
}
