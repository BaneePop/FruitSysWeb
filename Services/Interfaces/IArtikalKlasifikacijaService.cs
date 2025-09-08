using FruitSysWeb.Models;

namespace FruitSysWeb.Services.Interfaces
{
    public interface IArtikalKlasifikacijaService
    {
        Task<List<ArtikalKlasifikacija>> UcitajSveKlasifikacije();
        Task<ArtikalKlasifikacija?> UcitajKlasifikaciju(long id);
        Task<List<ArtikalKlasifikacija>> UcitajKlasifikacijePoPretezi(string pretraga);
    }
}
