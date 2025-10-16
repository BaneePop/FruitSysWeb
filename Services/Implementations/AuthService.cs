using System.Security.Cryptography;
using System.Text;
using FruitSysWeb.Models;
using FruitSysWeb.Services.Core;
using FruitSysWeb.Services.Interfaces;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace FruitSysWeb.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly DatabaseService _databaseService;
        private readonly ProtectedSessionStorage _sessionStorage;
        private const string USER_KEY = "current_user";

        public AuthService(DatabaseService databaseService, ProtectedSessionStorage sessionStorage)
        {
            _databaseService = databaseService;
            _sessionStorage = sessionStorage;
        }

        public async Task<LoginResponse> Login(LoginRequest request)
        {
            try
            {
                var sql = @"
            SELECT 
                ID, Ime, Lozinka, Seed, Administrator, Kreirano, Azurirano, 
                Version, RadnikID, GrupaKorisnikaID, KomitentID
            FROM Korisnik k
            WHERE Ime = @Username
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

                var hashedPassword = HashPassword(request.Password, korisnik.Seed);
                

                if (hashedPassword.ToLower() != korisnik.Lozinka.ToLower())

                {
                    return new LoginResponse { Success = false, Message = "Pogrešna lozinka" };
                }

                await _sessionStorage.SetAsync(USER_KEY, korisnik);
                return new LoginResponse { Success = true, Message = "Uspešno ste se prijavili", Korisnik = korisnik };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri logovanju: {ex.Message}");
                return new LoginResponse { Success = false, Message = "Greška pri logovanju" };
            }
        }

        public async Task Logout()
        {
            try
            {
                await _sessionStorage.DeleteAsync(USER_KEY);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri odjavljivanju: {ex.Message}");
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