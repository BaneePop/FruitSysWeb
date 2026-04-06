using FruitSysWeb.Models;

namespace FruitSysWeb.Services.Interfaces
{
    public interface IKorisnikAktivnostService
    {
        Task<long> ZabeležiLogin(string korisnikIme, string? ipAdresa);
        Task ZabeležiLogout(long aktivnostId, string korisnikIme);
        Task<List<KorisnikAktivnostModel>> UcitajAktivnosti(KorisnikAktivnostFilter filter);
        Task<List<string>> UcitajSveKorisnike();
    }
}
