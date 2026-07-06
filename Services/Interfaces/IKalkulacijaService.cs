using FruitSysWeb.Models;

namespace FruitSysWeb.Services.Interfaces
{
    public interface IKalkulacijaService
    {
        Task<KalkulacijaStanjeRobeResult> UcitajStanjeRobe(KalkulacijaPeriod period, List<int> iskluceniArtikli);

        Task<List<KalkulacijaLagerPrenosStavka>> UcitajLagerZaPrenosKonfiguraciju(KalkulacijaPeriod period);

        Task<KalkulacijaObracunOtkupaResult> UcitajObracunOtkupa(
            KalkulacijaPeriod period,
            List<int> izabraniDobavljaci,
            Dictionary<int, decimal> pocetnoStanjePoDobaveljacu);

        Task<List<(int ID, string Naziv)>> UcitajDobavljace();

        Task<KalkulacijaObracunPosloResult> UcitajObracunPoslovanja(
            KalkulacijaPeriod period,
            Dictionary<string, decimal> ceneNabavke,
            Dictionary<string, decimal> cenePrerade,
            List<int>? iskluceniArtikli = null);

        Task<List<KalkulacijaArtikalNaCeniRed>> UcitajArtikleZaCeneNabavke(KalkulacijaPeriod period, List<int> iskluceniArtikli);

        Task<Dictionary<int, List<(int ID, string Naziv)>>> UcitajArtikleZaFormulu();

        Task<KalkulacijaVrednostZalihaResult> UcitajVrednostZaliha(
            Dictionary<string, List<FruitSysWeb.Services.Core.FormulaIskoriscenjaStavka>> formula,
            Dictionary<string, decimal> prodajneCene);

        Task<KalkulacijaUslugaResult> UcitajUslugu(KalkulacijaPeriod period);
    }
}
