using FruitSysWeb.Models;

namespace FruitSysWeb.Services.Print
{
    public interface IPrintTokenService
    {
        Task<long?> ValidateTokenAsync(string? token, CancellationToken ct = default);
        Task<KorisnikModel?> UcitajKorisnikaAsync(long korisnikId, CancellationToken ct = default);
        bool ImaPravoNaStampu(KorisnikModel korisnik);
    }
}
