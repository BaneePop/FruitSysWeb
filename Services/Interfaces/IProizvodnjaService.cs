// IProizvodnjaService.cs - BEZ PROMENA, već je dobro definisan
using FruitSysWeb.Models;
using FruitSysWeb.Services.Models.Requests;

namespace FruitSysWeb.Services.Interfaces
{
    public interface IProizvodnjaService
    {
        Task<List<ProizvodnjaModel>> UcitajIzvestajProizvodnje(FilterRequest filterRequest);
        Task<Dictionary<string, decimal>> UcitajProizvodnjuPoArtiklima(FilterRequest filterRequest);
        Task<List<ProizvodnjaModel>> UcitajSveRadneNaloge();
        
        // Dodatne metode potrebne za Home.razor
        Task<decimal> UcitajUkupnuProizvodnju(FilterRequest filterRequest);
        Task<int> UcitajBrojAktivnihNaloga(FilterRequest filterRequest);
        
        // Metode po specifičnim kriterijumima
        Task<List<ProizvodnjaModel>> UcitajRadneNalogePoDatumu(DateTime odDatum, DateTime doDatum);
        Task<List<ProizvodnjaModel>> UcitajRadneNalogePoKomitentu(long komitentId);
        Task<List<ProizvodnjaModel>> UcitajRadneNalogePoArtiklu(long artikalId);
        Task<List<ProizvodnjaModel>> UcitajRadneNalogePoTipu(int tipArtikla);
        
        // Analize i statistike
        Task<Dictionary<string, decimal>> UcitajProizvodnjuPoKomitentima(FilterRequest filterRequest);
        Task<Dictionary<string, decimal>> UcitajProizvodnjuPoMesecima(FilterRequest filterRequest);
        Task<Dictionary<int, decimal>> UcitajProizvodnjuPoTipovima(FilterRequest filterRequest);
        Task<List<ProizvodnjaModel>> UcitajNajproduktivnijeNaloge(FilterRequest filterRequest, int brojNaloga = 10);
        
        // Prerada specifične metode
        Task<List<PreradaPregledModel>> UcitajPreraduPregled(FilterRequest filterRequest);
        Task<Dictionary<string, decimal>> UcitajAnalizuPreradePoArtiklima(FilterRequest filterRequest);
    }
}