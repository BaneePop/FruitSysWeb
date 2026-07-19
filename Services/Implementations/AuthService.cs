using FruitSysWeb.Models;
using FruitSysWeb.Services.Auth;
using FruitSysWeb.Services.Interfaces;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace FruitSysWeb.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly AuthLocalDbService _authDb;
        private readonly ProtectedSessionStorage _sessionStorage;
        private readonly ILogger<AuthService> _logger;
        private readonly IKorisnikAktivnostService _aktivnostService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string USER_KEY = "current_user";
        private const string AKTIVNOST_KEY = "aktivnost_id";

        public AuthService(AuthLocalDbService authDb, ProtectedSessionStorage sessionStorage,
            ILogger<AuthService> logger, IKorisnikAktivnostService aktivnostService,
            IHttpContextAccessor httpContextAccessor)
        {
            _authDb = authDb;
            _sessionStorage = sessionStorage;
            _logger = logger;
            _aktivnostService = aktivnostService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<LoginResponse> Login(LoginRequest request)
        {
            try
            {
                var red = await _authDb.GetByImeAsync(request.Username);
                if (red == null || !red.Aktivan)
                {
                    return new LoginResponse { Success = false, Message = "Korisničko ime ne postoji" };
                }

                if (!PasswordHasher.Verify(request.Password, red.LozinkaHash, red.LozinkaSalt, red.Iteracije))
                {
                    return new LoginResponse { Success = false, Message = "Pogrešna lozinka" };
                }

                var korisnik = new KorisnikModel { Id = red.Id, Ime = red.Ime };
                await _sessionStorage.SetAsync(USER_KEY, korisnik);

                var ipAdresa = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                var aktivnostId = await _aktivnostService.ZabeležiLogin(korisnik.Ime, ipAdresa);
                await _sessionStorage.SetAsync(AKTIVNOST_KEY, aktivnostId);

                return new LoginResponse { Success = true, Message = "Uspešno ste se prijavili", Korisnik = korisnik };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri logovanju");
                return new LoginResponse { Success = false, Message = "Greška pri logovanju" };
            }
        }

        public async Task Logout()
        {
            try
            {
                var aktivnostResult = await _sessionStorage.GetAsync<long>(AKTIVNOST_KEY);
                var korisnikResult = await _sessionStorage.GetAsync<KorisnikModel>(USER_KEY);
                if (aktivnostResult.Success && korisnikResult.Success && korisnikResult.Value != null)
                {
                    await _aktivnostService.ZabeležiLogout(aktivnostResult.Value, korisnikResult.Value.Ime);
                }

                await _sessionStorage.DeleteAsync(USER_KEY);
                await _sessionStorage.DeleteAsync(AKTIVNOST_KEY);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri odjavljivanju");
            }
        }

        public async Task<KorisnikModel?> GetCurrentUser()
        {
            try
            {
                var result = await _sessionStorage.GetAsync<KorisnikModel>(USER_KEY);
                return result.Success ? result.Value : null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> IsAuthenticated()
        {
            var user = await GetCurrentUser();
            return user != null;
        }
    }
}
