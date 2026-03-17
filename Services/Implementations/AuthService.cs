using System.Security.Cryptography;
using System.Text;
using FruitSysWeb.Models;
using FruitSysWeb.Services.Core;
using FruitSysWeb.Services.Interfaces;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FruitSysWeb.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly DatabaseService _databaseService;
        private readonly ProtectedSessionStorage _sessionStorage;
        private readonly ILogger<AuthService> _logger;
        private readonly IKorisnikAktivnostService _aktivnostService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string USER_KEY = "current_user";
        private const string AKTIVNOST_KEY = "aktivnost_id";

        public AuthService(DatabaseService databaseService, ProtectedSessionStorage sessionStorage,
            ILogger<AuthService> logger, IKorisnikAktivnostService aktivnostService,
            IHttpContextAccessor httpContextAccessor)
        {
            _databaseService = databaseService;
            _sessionStorage = sessionStorage;
            _logger = logger;
            _aktivnostService = aktivnostService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<LoginResponse> Login(LoginRequest request)
        {
            try
            {
                var sql = @"
            SELECT
                k.ID, k.Ime, k.Lozinka, k.Seed, k.Administrator, k.Kreirano, k.Azurirano,
                k.Version, k.RadnikID, k.GrupaKorisnikaID, k.KomitentID,
                gk.Naziv as GrupaNaziv
            FROM Korisnik k
            LEFT JOIN GrupaKorisnika gk ON k.GrupaKorisnikaID = gk.ID
            WHERE k.Ime = @Username
            LIMIT 1
        ";

                var korisnik = await _databaseService.QueryFirstOrDefaultAsync<KorisnikModel>(
                    sql,
                    new { Username = request.Username }
                );

                if (korisnik == null)
                {
                    return new LoginResponse { Success = false, Message = "Korisničko ime ne postoji" };
                }

                // Debug logging
                _logger.LogInformation($"🔍 Login - Korisnik: {korisnik.Ime}, GrupaKorisnikaID: {korisnik.GrupaKorisnikaID}, GrupaNaziv: {korisnik.GrupaNaziv}");

                var hashedPassword = HashPassword(request.Password, korisnik.Seed);
                

                if (hashedPassword.ToLower() != korisnik.Lozinka.ToLower())

                {
                    return new LoginResponse { Success = false, Message = "Pogrešna lozinka" };
                }

                await _sessionStorage.SetAsync(USER_KEY, korisnik);

                // Zabeleži login (IP adresa + vreme)
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
                // Zabeleži logout pre brisanja sesije
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

        public async Task<bool> IsAdministrator()
        {
            var user = await GetCurrentUser();
            return user?.Administrator ?? false;
        }

        public async Task<bool> ImaPristupStranici(string url)
        {
            var user = await GetCurrentUser();
            if (user == null) return false;

            return user.ImaPristupStranici(url);
        }

        public async Task<string> PocetnaStranica()
        {
            var user = await GetCurrentUser();
            if (user == null)
            {
                _logger.LogWarning("❌ PocetnaStranica - Nema korisnika, redirect na /login");
                return "/login";
            }

            var stranica = GrupaKorisnikaHelper.PocetnaStranica(user.Ime);
            _logger.LogInformation($"✅ PocetnaStranica - Korisnik: {user.Ime}, Početna: {stranica}");
            return stranica;
        }

        // TEST SVE VARIJANTE
        // The following code block is not needed and has been removed to fix ambiguity errors.




        private string HashPassword(string password, string seed)
        {
            using (var md5 = MD5.Create())
            {
                var combinedBytes = Encoding.UTF8.GetBytes(password + seed);
                var hashBytes = md5.ComputeHash(combinedBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToUpper(); // .ToUpper() umesto .ToLower()
            }
        }
    }
}