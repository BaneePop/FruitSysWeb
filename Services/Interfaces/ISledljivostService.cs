using FruitSysWeb.Models.Sledljivost;

namespace FruitSysWeb.Services.Interfaces
{
    /// <summary>
    /// Servis za sledljivost robe (upstream i downstream)
    /// </summary>
    public interface ISledljivostService
    {
        /// <summary>
        /// Učitava kompletnu sledljivost za Radni Nalog po šifri
        /// </summary>
        Task<SledljivostModel?> UcitajSledljivostPoRadnomNalogu(string sifra);

        /// <summary>
        /// Učitava kompletnu sledljivost za Paletni List po šifri
        /// </summary>
        Task<SledljivostModel?> UcitajSledljivostPoPaletnomListu(string sifra);

        /// <summary>
        /// Univerzalna pretraga - detektuje tip dokumenta i poziva odgovarajuću metodu
        /// </summary>
        Task<SledljivostModel?> UcitajSledljivost(string sifra);

        /// <summary>
        /// Autocomplete pretraga za Radne Naloge i Paletne Listove
        /// </summary>
        Task<List<DokumentSearchResultModel>> PretraziDokumente(string searchTerm, int limit = 10);

        /// <summary>
        /// Učitava sve šifre Radnih Naloga za autocomplete (cached)
        /// </summary>
        Task<List<string>> UcitajSifreRadnihNaloga(string searchTerm = "", int limit = 50);

        /// <summary>
        /// Učitava sve šifre Paletnih Listova za autocomplete (cached)
        /// </summary>
        Task<List<string>> UcitajSifrePaletnihListova(string searchTerm = "", int limit = 50);

        /// <summary>
        /// Generiše UPSTREAM PDF izveštaj (Radni Nalog → Nabavka → Proizvodnja → Prodaja)
        /// </summary>
        Task<byte[]> GenerisiUpstreamPdf(string sifra);

        /// <summary>
        /// Generiše DOWNSTREAM PDF izveštaj (Paletni List → Gde je prodat → Kako je proizveden → Odakle dolazi)
        /// </summary>
        Task<byte[]> GenerisiDownstreamPdf(string sifra);

        /// <summary>
        /// Generiše UPSTREAM Excel izveštaj sa hipervezama (Radni Nalog → Nabavka → Proizvodnja → Prodaja)
        /// </summary>
        Task<byte[]> GenerisiUpstreamExcel(string sifra);

        /// <summary>
        /// Generiše DOWNSTREAM Excel izveštaj sa hipervezama (Paletni List → Gde je prodat → Kako je proizveden → Odakle dolazi)
        /// </summary>
        Task<byte[]> GenerisiDownstreamExcel(string sifra);
    }
}
