using FruitSysWeb.Models;
using FruitSysWeb.Services.Models.Requests;

namespace FruitSysWeb.Services.Interfaces
{
    public interface IKontrolaService
    {
        /// <summary>
        /// Učitava sve kontrole za jedan radni nalog (sve tabele).
        /// </summary>
        Task<KontrolaRadniNalogModel> UcitajKontrole(long radniNalogId);

        /// <summary>
        /// Pretraga radnih naloga po filteru za dropdown (datum + komitent).
        /// </summary>
        Task<List<KontrolaRadniNalogHeaderModel>> PretragaRadnihNaloga(FilterRequest filterRequest, string? komitent = null);
    }
}
