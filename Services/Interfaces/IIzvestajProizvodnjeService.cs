using FruitSysWeb.Models;

namespace FruitSysWeb.Services.Interfaces
{
    public interface IIzvestajProizvodnjeService
    {
        Task<IzvestajProizvodnjeModel> UcitajIzvestaj(DateTime datumOd, DateTime datumDo);
    }
}
