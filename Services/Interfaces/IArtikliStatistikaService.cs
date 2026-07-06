using FruitSysWeb.Models;

namespace FruitSysWeb.Services.Interfaces
{
    public interface IArtikliStatistikaService
    {
        Task<List<ArtikliStatistikaGrupa>> UcitajNabavkuVoce(ArtikliStatistikaFilter filter);
        Task<List<ArtikliStatistikaGrupa>> UcitajNabavkuAmbalaza(ArtikliStatistikaFilter filter);
        Task<List<ArtikliStatistikaGrupa>> UcitajProdajuVoce(ArtikliStatistikaFilter filter);
    }
}
