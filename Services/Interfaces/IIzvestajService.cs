using FruitSysWeb.Services.Models.Requests;
using FruitSysWeb.Models;
using FruitSysWeb.Components;



namespace FruitSysWeb.Services.Interfaces
{
    public interface IIzvestajService
    {
        Task<List<FinansijeModel>> UcitajFinansijskiIzvestaj(FilterRequest filter);
        Task<List<ProizvodnjaModel>> UcitajIzvestajProizvodnje(FilterRequest filter);
        Task<decimal> UcitajUkupnoSaldo(FilterRequest filter);
    }

    public interface IFinansijeService : IIzvestajService
    {
        Task<Dictionary<string, decimal>> UcitajTopKupce(FilterRequest filter);
        Task<Dictionary<string, decimal>> UcitajTopDobavljace(FilterRequest filter);
        Task<Dictionary<string, decimal>> UcitajRashodePoKategorijama(FilterRequest filter);
    }

    public interface IProizvodnjaService : IIzvestajService
    {
        Task<decimal> UcitajUkupnuProizvodnju(FilterRequest filter);
        Task<int> UcitajBrojAktivnihNaloga(FilterRequest filter);
        Task<Dictionary<string, decimal>> UcitajProizvodnjuPoArtiklima(FilterRequest filter);
    }


}
