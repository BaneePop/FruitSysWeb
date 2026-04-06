using FruitSysWeb.Models.IzvodDokumenata;

namespace FruitSysWeb.Services.Interfaces
{
    public interface IIzvodDokumenataService
    {
        // ── Prijemnice ──────────────────────────────────────────────────
        Task<List<IzvodPrijemnicaRow>> UcitajPrijemnice(IzvodDokumenataFilter filter);
        Task<IzvodPrijemnicaDetalji?> UcitajPrijemnicuDetalji(long id);
        Task<List<IzvodPaletniListRow>> UcitajPaletneListovePrijemnice(long prijemnicaId);

        // ── Otpremnice ──────────────────────────────────────────────────
        Task<List<IzvodOtpremnicaRow>> UcitajOtpremnice(IzvodDokumenataFilter filter);
        Task<IzvodOtpremnicaDetalji?> UcitajOtpremnicuDetalji(long id);
        Task<List<IzvodPaletniListRow>> UcitajPaletneListoveOtpremnice(long otpremnicaId);

        // ── Radni Nalozi ─────────────────────────────────────────────────
        Task<List<IzvodRadniNalogRow>> UcitajRadneNaloge(IzvodDokumenataFilter filter);
        Task<IzvodRadniNalogDetalji?> UcitajRadniNalogDetalji(long id);
    }
}
