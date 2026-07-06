using FruitSysWeb.Models;
using FruitSysWeb.Services.Models.Requests;

namespace FruitSysWeb.Services.Interfaces
{
    public interface IPovratnaAmbalazaService
    {
        /// <summary>
        /// Učitava SVE podatke o PVC ambalaži jednim upitom
        /// </summary>
        /// <param name="filterRequest">Datum filteri</param>
        /// <returns>Agregirani podaci po komitentima i artiklima</returns>
        Task<List<PovratnaAmbalazaAgregat>> UcitajSveAgregiraneAsync(FilterRequest filterRequest);

        /// <summary>
        /// Učitava stanje na lageru za određeni tip PVC ambalaže
        /// </summary>
        /// <param name="tipAmbalazeFilter">Filter naziva (npr. "PVC 4/1", "PVC 6/1")</param>
        /// <param name="filterRequest">Dodatni filteri (datum)</param>
        /// <returns>Ukupno stanje (Ulaz - Izlaz)</returns>
        Task<decimal> UcitajStanjeNaLageruAsync(string tipAmbalazeFilter, FilterRequest filterRequest);

        /// <summary>
        /// Učitava detaljni izveštaj za Tab 2
        /// </summary>
        /// <param name="filterRequest">Filteri (datum, komitent, artikal)</param>
        /// <returns>Lista svih stavki prometa povratne ambalaže</returns>
        Task<List<PovratnaAmbalazaModel>> UcitajDetaljniIzvestajAsync(FilterRequest filterRequest);
    }
}
