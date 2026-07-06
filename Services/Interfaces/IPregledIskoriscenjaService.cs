using FruitSysWeb.Models;

namespace FruitSysWeb.Services.Interfaces
{
    public interface IPregledIskoriscenjaService
    {
        Task<PLSledljivostModel?> UcitajSledljivostPL(string sifra);
        Task<List<PLIskoriscenjeListaModel>> UcitajListuIskoriscenja(PLIskoriscenjeFilter filter);
        Task<List<(long ID, string Naziv)>> UcitajArtikle(long? prvaKlasifikacijaID = null);
        Task<List<(long ID, string Naziv)>> UcitajKomitente();
        Task<List<(long ID, string Naziv)>> UcitajVrsteArtikala();
        Task<List<PLStatistikaModel>> UcitajStatistikuPL(PLIskoriscenjeFilter filter);
    }
}
