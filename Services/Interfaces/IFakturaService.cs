using FruitSysWeb.Models;
using FruitSysWeb.Services.Models.Requests;

namespace FruitSysWeb.Services.Interfaces
{
    public interface IFakturaService
    {
        /// <summary>
        /// Učitava listu faktura sa filterima (datum, kupac, status, Ino tip)
        /// </summary>
        Task<List<FakturaModel>> UcitajFakture(FilterRequest filterRequest, bool? samoIno = null);

        /// <summary>
        /// Učitava kompletne detalje jedne fakture (sa stavkama, kupcem, otpremnicom, ugovorom)
        /// </summary>
        Task<FakturaDetaljiModel?> UcitajDetalje(long fakturaId);

        /// <summary>
        /// Učitava samo stavke jedne fakture
        /// </summary>
        Task<List<FakturaStavkaModel>> UcitajStavke(long fakturaId);

        /// <summary>
        /// Učitava listu kupaca (JeKupac = 1) za dropdown
        /// </summary>
        Task<List<KomitentDropdownModel>> UcitajKupce();
    }

    public class KomitentDropdownModel
    {
        public long ID { get; set; }
        public string Naziv { get; set; } = string.Empty;
        public bool Ino { get; set; }
    }
}
