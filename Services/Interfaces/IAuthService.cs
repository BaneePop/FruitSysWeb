using FruitSysWeb.Models;


namespace FruitSysWeb.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse> Login(LoginRequest request);
        Task Logout();
        Task<KorisnikModel?> GetCurrentUser();
        Task<bool> IsAuthenticated();
        Task<bool> IsAdministrator();

        // Provere pristupa
        Task<bool> ImaPristupStranici(string url);
        Task<string> PocetnaStranica();
    }
}