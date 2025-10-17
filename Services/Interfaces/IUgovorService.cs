using FruitSysWeb.Models;
using FruitSysWeb.Services.Models.Requests;

namespace FruitSysWeb.Services.Interfaces
{
    public interface IUgovorService
    {
        Task<List<UgovorModel>> UcitajAktivneUgovore();
        Task<List<UgovorModel>> UcitajUgovoreSaFilterima(FilterRequest filterRequest);
        Task<decimal> UcitajUkupnuVrednostAktivnihUgovora();
        Task<List<OtpremnicaDetaljiModel>> UcitajOtpremnicePoUgovoru(long ugovorId);
    }
}