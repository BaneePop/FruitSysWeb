using FruitSysWeb.Models.IzvodDokumenata;

namespace FruitSysWeb.Services.Interfaces
{
    public interface IIzvodDokumenataService
    {
        // Prijemnice
        Task<List<IzvodPrijemnicaRow>> UcitajPrijemnice(IzvodDokumenataFilter filter);
        Task<IzvodPrijemnicaDetalji?> UcitajPrijemnicuDetalji(long id);
        Task<List<IzvodPaletniListRow>> UcitajPaletneListovePrijemnice(long prijemnicaId);

        // Otpremnice
        Task<List<IzvodOtpremnicaRow>> UcitajOtpremnice(IzvodDokumenataFilter filter);
        Task<IzvodOtpremnicaDetalji?> UcitajOtpremnicuDetalji(long id);
        Task<List<IzvodPaletniListRow>> UcitajPaletneListoveOtpremnice(long otpremnicaId);

        // Radni Nalozi
        Task<List<IzvodRadniNalogRow>> UcitajRadneNaloge(IzvodDokumenataFilter filter);
        Task<IzvodRadniNalogDetalji?> UcitajRadniNalogDetalji(long id);

        // Otkupni Listovi
        Task<List<IzvodOtkupniListRow>> UcitajOtkupneListove(IzvodDokumenataFilter filter);
        Task<IzvodOtkupniListDetalji?> UcitajOtkupniListDetalji(long id);

        // Paletni Listovi
        Task<List<IzvodPaletniListRow>> UcitajPaletneListoveNabavke(IzvodDokumenataFilter filter);
        Task<IzvodPaletniListDetalji?> UcitajPaletniListNabavkaDetalji(long id);
        Task<List<IzvodPaletniListRow>> UcitajPaletneListoveGotovaRoba(IzvodDokumenataFilter filter);
        Task<IzvodPaletniListDetalji?> UcitajPaletniListGotovaRobaDetalji(long id);
    }
}
