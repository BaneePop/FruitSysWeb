using FruitSysWeb.Models;
using FruitSysWeb.Services.Models.Requests;

namespace FruitSysWeb.Services.Interfaces
{
    public interface IKomitentService
    {
        Task<List<Komitent>> UcitajSveKomitente();
        Task<Komitent?> UcitajKomitenta(long id);
        Task<List<Komitent>> UcitajKomitentePoPretezi(string pretraga);
        Task<List<Komitent>> UcitajKomitentePoTipu(string tip);
        // NOVO: Metoda za kaskadno filtriranje
        Task<List<Komitent>> UcitajKomitentePoPretragaITipu(string pretraga = "", string? tip = null);
        // NOVO: Metoda za komitente koji postoje u RadniNalog tabeli
        Task<List<Komitent>> UcitajKomitenteKojiImajuRadneNaloge();
    }
}
