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

        /// <summary>
        /// Lista isporučenih (DokumentStatus=3) radnih naloga za Pregled Isporučenih RN.
        /// </summary>
        Task<List<UcitajRadneNalogeModel>> UcitajIsporuceneNaloge(FilterRequest filterRequest, int? komitentId = null, string? radniNalog = null);

        /// <summary>
        /// Lista otvorenih (DokumentStatus=2, Stalni=0) radnih naloga sa lager podacima.
        /// </summary>
        Task<List<OtvoreniRadniNalogModel>> UcitajOtvoreneNaloge();
    }
}
