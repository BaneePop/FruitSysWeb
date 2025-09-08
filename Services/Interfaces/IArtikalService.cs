using FruitSysWeb.Models;

namespace FruitSysWeb.Services.Interfaces
{
    public interface IArtikalService
    {
        Task<List<Artikal>> UcitajSveArtikle();
        Task<Artikal?> UcitajArtikal(long id);
        Task<List<Artikal>> UcitajArtiklePoTipu(int tip);
        Task<List<Artikal>> UcitajArtiklePoPretezi(string pretraga);
        // NOVO: Metoda za kaskadno filtriranje
        Task<List<Artikal>> UcitajArtiklePoPretragaITipu(string pretraga = "", int? tip = null);
    }
}