using FruitSysWeb.Models;

namespace FruitSysWeb.Services.Interfaces
{
    public interface IEfikasnostService
    {
        Task<List<EfikasnostPoslovodjeModel>> UcitajEfikasnostPoslovodja(EfikasnostFilter filter);
        Task<List<EfikasnostSmenaDetaljiModel>> UcitajDetaljeSmena(long poslovodjaID, EfikasnostFilter filter);
        Task<List<(long ID, string ImePrezime)>> UcitajPoslovodje();
    }
}
