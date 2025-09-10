using FruitSysWeb.Models;
using FruitSysWeb.Services.Models.Requests;

namespace FruitSysWeb.Services.Interfaces
{
    public interface IMagacinLagerService
    {
        // Postojeće metode
        Task<List<MagacinLagerModel>> UcitajLagerStanje();
        Task<List<MagacinLagerModel>> UcitajLagerStanjeSaFilterima(FilterRequest filterRequest);
        Task<List<MagacinLagerModel>> UcitajLagerStanjeSaFilterima(string filter);
        Task<List<RadniNalogLagerModel>> UcitajRadneNalogeLager();
        Task<List<MagacinLagerModel>> UcitajLagerStanjePoArtiklu(long artikalId);
        Task<List<MagacinLagerModel>> UcitajLagerStanjePoArtiklu(int artikalId);
        Task<List<MagacinLagerModel>> UcitajLagerStanjePoTipu(int artikalTip);
        Task<decimal> UcitajUkupnuVrednostLager();

        // DODATO - nove metode prema dokumentu
        Task<List<string>> UcitajListuPakovanja();
        Task<List<RadniNalogLagerModel>> UcitajLagerProizvodnje();
        Task<List<MagacinLagerModel>> UcitajGotoveRobe();
        Task<List<MagacinLagerModel>> UcitajSirovine();
        Task<List<MagacinLagerModel>> UcitajAmbalaze();
        Task<List<MagacinLagerModel>> UcitajArtikleIspodMinimuma(decimal minKolicina = 10);
        Task<List<MagacinLagerModel>> UcitajArtikleIspodMinimuma();
        Task<Dictionary<string, decimal>> UcitajStatistikeLagera();
        
        // DASHBOARD - nove metode za strukturu lagera
        Task<Dictionary<string, decimal>> UcitajStrukturuSirovina();
        Task<Dictionary<string, decimal>> UcitajStrukturuGotovihProizvoda();
        Task<Dictionary<string, decimal>> UcitajStrukturuKutija();
        Task<Dictionary<string, decimal>> UcitajStrukturuKesa();
        
        // Ostale metode
        Task<List<MagacinLagerModel>> UcitajLagerStanjePoLotu(string lot);
        Task<List<RadniNalogLagerModel>> UcitajOtvoreneRadneNaloge();
        Task<List<RadniNalogLagerModel>> UcitajRadneNalogePoStatusu(int status);
    }
}
