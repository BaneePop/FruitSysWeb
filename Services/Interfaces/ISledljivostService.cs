using FruitSysWeb.Models.Sledljivost;

namespace FruitSysWeb.Services.Interfaces
{
    public interface ISledljivostService
    {
        Task<SledljivostModel?> UcitajSledljivostPoRadnomNalogu(string sifra);
        Task<SledljivostModel?> UcitajSledljivostPoPaletnomListu(string sifra);
        Task<SledljivostModel?> UcitajSledljivost(string sifra);
        Task<List<DokumentSearchResultModel>> PretraziDokumente(string searchTerm, int limit = 10);
        Task<List<string>> UcitajSifreRadnihNaloga(string searchTerm = "", int limit = 50);
        Task<List<string>> UcitajSifrePaletnihListova(string searchTerm = "", int limit = 50);
        Task<byte[]> GenerisiUpstreamExcel(string sifra);
        Task<byte[]> GenerisiDownstreamExcel(string sifra);
        Task<byte[]> GenerisiUpstreamHtml(string sifra);
        Task<byte[]> GenerisiDownstreamHtml(string sifra);
        Task<PrijemSledljivostModel?> UcitajPrijemSledljivost(string sifra);
        Task<byte[]> GenerisiPrijemExcel(string sifra);
        Task<byte[]> GenerisiPrijemHtml(string sifra);
    }
}
