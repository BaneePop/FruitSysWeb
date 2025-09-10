using FruitSysWeb.Models;
using FruitSysWeb.Services.Models.Requests;

namespace FruitSysWeb.Services.Interfaces
{
    public interface IProizvodnjaService
    {
        // Osnovne metode
        Task<List<ProizvodnjaModel>> UcitajIzvestajProizvodnje(FilterRequest filterRequest);
        Task<decimal> UcitajUkupnuProizvodnju(FilterRequest filterRequest);
        Task<int> UcitajBrojAktivnihNaloga(FilterRequest filterRequest);

        // DODATO - nova metoda prema dokumentu
        Task<List<string>> UcitajListuRadnihNaloga(FilterRequest filterRequest);

        // Analitičke metode
        Task<Dictionary<string, decimal>> UcitajProizvodnjuPoArtiklima(FilterRequest filterRequest);
        Task<Dictionary<string, decimal>> UcitajProizvodnjuPoKomitentima(FilterRequest filterRequest);
        Task<Dictionary<int, decimal>> UcitajProizvodnjuPoTipovima(FilterRequest filterRequest);
        Task<List<ProizvodnjaModel>> UcitajNajproduktivnijeNaloge(FilterRequest filterRequest, int brojNaloga = 10);

        // Ostale metode
        Task<List<ProizvodnjaModel>> UcitajSveRadneNaloge();
        Task<List<ProizvodnjaModel>> UcitajRadneNalogePoDatumu(DateTime odDatum, DateTime doDatum);
        Task<List<ProizvodnjaModel>> UcitajRadneNalogePoKomitentu(long komitentId);
        Task<List<ProizvodnjaModel>> UcitajRadneNalogePoArtiklu(long artikalId);
        Task<List<ProizvodnjaModel>> UcitajRadneNalogePoTipu(int tipArtikla);
        Task<Dictionary<string, decimal>> UcitajProizvodnjuPoMesecima(FilterRequest filterRequest);
        Task<List<PreradaPregledModel>> UcitajPreraduPregled(FilterRequest filterRequest);
        Task<Dictionary<string, decimal>> UcitajAnalizuPreradePoArtiklima(FilterRequest filterRequest);
    }
}