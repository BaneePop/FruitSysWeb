using FruitSysWeb.Models;
using FruitSysWeb.Services.Models.Requests;

namespace FruitSysWeb.Services.Interfaces
{
    public interface IKarticaKomitentaService
    {
        /// <summary>
        /// Učitava sve transakcije komitenta sortirane po datumu, sa tekućim saldom.
        /// </summary>
        Task<List<KarticaStavkaModel>> UcitajKarticu(long komitentId, FilterRequest filterRequest);

        /// <summary>
        /// Sumarne informacije: ukupno potražuje/duguje, završni saldo, broj transakcija.
        /// </summary>
        Task<KarticaKomitentaSumaModel?> UcitajSumu(long komitentId, FilterRequest filterRequest);
    }
}
