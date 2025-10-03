using FruitSysWeb.Models;
using FruitSysWeb.Services.Models.Requests;

namespace FruitSysWeb.Services.Interfaces
{
    public interface IBrziPregledService
    {
        Task SacuvajKonfiguraciju(BrziPregledKonfiguracija config);
        Task<BrziPregledKonfiguracija> UcitajKonfiguraciju();
        Task<List<BrziPregledStavka>> UcitajBrziPregledDobavljaca(
            List<int> komitentIds, FilterRequest filter);
        Task<List<BrziPregledStavka>> UcitajBrziPregledKupaca(
            List<int> komitentIds, FilterRequest filter);
        
        // ✨ NOVO: Roba na zalihama po vrstama voća
        Task<List<RobaZaliheStavka>> UcitajRobaNaZalihama(
            Dictionary<string, List<long>> artikliPoVrstama, 
            FilterRequest filter);
    }
}
