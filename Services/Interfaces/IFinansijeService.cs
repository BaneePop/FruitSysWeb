// IFinansijeService.cs - AŽURIRANI INTERFEJS
using FruitSysWeb.Models;
using FruitSysWeb.Services.Models.Requests;

namespace FruitSysWeb.Services.Interfaces
{
    public interface IFinansijeService
    {
        // Osnovne metode
        Task<List<FinansijeModel>> UcitajFinansijskiIzvestaj(FilterRequest filterRequest);
        Task<decimal> UcitajUkupnoSaldo(FilterRequest filterRequest);
        
        // Top partneri
        Task<Dictionary<string, decimal>> UcitajTopKupce(FilterRequest filterRequest);
        Task<Dictionary<string, decimal>> UcitajTopDobavljace(FilterRequest filterRequest);
        
        // DODATO: Metode koje nedostaju u interfejsu
        Task<List<FinansijeModel>> UcitajSaldoPoKomitentima(FilterRequest filterRequest);
        Task<List<FinansijeModel>> UcitajPrometePoArtiklima(FilterRequest filterRequest);
        Task<decimal> UcitajUkupanPromet(FilterRequest filterRequest);
        Task<decimal> UcitajUkupnuZaradu(FilterRequest filterRequest);
        
        // Specifični filteri
        Task<List<FinansijeModel>> UcitajFinansijePoKomitentu(long komitentId, FilterRequest filterRequest);
        Task<List<FinansijeModel>> UcitajFinansijePoArtiklu(long artikalId, FilterRequest filterRequest);
        
        // Dodatne analize
        Task<decimal> UcitajUkupnuZaduzenju(FilterRequest filterRequest);
        Task<decimal> UcitajUkupnoPotrazenost(FilterRequest filterRequest);
        Task<Dictionary<string, decimal>> UcitajSaldoPoMesecima(FilterRequest filterRequest);
    }
}


