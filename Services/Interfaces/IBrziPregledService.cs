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

        // ✨ NOVO: Nabavka i prodaja po danima po vrsti voća (za Home.razor charts)
        Task<Dictionary<string, Dictionary<string, decimal>>> UcitajNabavkuPoDanimaPoVociAsync();
        Task<Dictionary<string, Dictionary<string, decimal>>> UcitajProdajuPoDanimaPoVociAsync();

        // ✨ NOVO: Ukupne vrednosti (količina i vrednost) za summary kartice
        Task<Dictionary<string, (decimal Kolicina, decimal Vrednost)>> UcitajUkupneVrednostiNabavkeAsync();
        Task<Dictionary<string, (decimal Kolicina, decimal Vrednost)>> UcitajUkupneVrednostiProdajeAsync();
    }
}
