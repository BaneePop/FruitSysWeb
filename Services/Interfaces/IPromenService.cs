using FruitSysWeb.Models;

namespace FruitSysWeb.Services.Interfaces
{
    public interface IPromenService
    {
        Task<PromenPeriodData> UcitajPromene(PromenFilter filter);
    }
}
