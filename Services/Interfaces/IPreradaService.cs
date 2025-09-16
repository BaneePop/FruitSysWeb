using FruitSysWeb.Models;
using FruitSysWeb.Services.Models.Requests;

namespace FruitSysWeb.Services.Interfaces
{
    public interface IPreradaService
    {
        // EvidencijaRada metode
        Task<List<EvidencijaRadaModel>> UcitajSveEvidencijeRada();
        Task<EvidencijaRadaModel?> UcitajEvidencijuRadaPoId(long id);
        Task<List<EvidencijaRadaModel>> UcitajEvidencijeRadaPoReziji(long rezijaId);
        Task<List<EvidencijaRadaModel>> UcitajEvidencijeRadaPoNazivu(string naziv);

        // RadniProces metode
        Task<List<RadniProcesModel>> UcitajSveRadneProcese();
        Task<RadniProcesModel?> UcitajRadniProcesPoId(long id);
        Task<List<RadniProcesModel>> UcitajRadneProcesePoNazivu(string naziv);
        Task<List<RadniProcesModel>> UcitajNoveRadneProcese(int dana = 30);

        // ProizvodniProces metode
        Task<List<ProizvodniProcesModel>> UcitajSveProizvodneProcese();
        Task<ProizvodniProcesModel?> UcitajProizvodniProcesPoId(long id);
        Task<List<ProizvodniProcesModel>> UcitajProizvodneProcesePoKategoriji(string kategorija);
        Task<List<ProizvodniProcesModel>> UcitajProizvodneProcesePoNazivu(string naziv);

        // SmenskiIzvestaj metode
        Task<List<SmenskiIzvestajModel>> UcitajSveSmenskeIzvestaje(FilterRequest filterRequest);
        Task<SmenskiIzvestajModel?> UcitajSmenskiIzvestajPoId(long id);
        Task<List<SmenskiIzvestajModel>> UcitajSmenskeIzvestajePoDatumu(DateTime odDatum, DateTime doDatum);
        Task<List<SmenskiIzvestajModel>> UcitajSmenskeIzvestajePoSmeni(int smena);
        Task<List<SmenskiIzvestajModel>> UcitajSmenskeIzvestajePoPoslovodji(long poslovodjaId);
        Task<List<SmenskiIzvestajModel>> UcitajOtvoreneSmenskeIzvestaje();
        Task<List<SmenskiIzvestajModel>> UcitajZakljuceneSmenskeIzvestaje();

        // Analitičke metode
        Task<Dictionary<string, int>> UcitajStatistikuPoSmenama(FilterRequest filterRequest);
        Task<Dictionary<string, int>> UcitajStatistikuPoPoslovodjama(FilterRequest filterRequest);
        Task<Dictionary<string, int>> UcitajStatistikuPoStatusima(FilterRequest filterRequest);
        Task<int> UcitajUkupanBrojSmenskihIzvestaja(FilterRequest filterRequest);
        Task<int> UcitajBrojOtvorenihSmenskihIzvestaja();
        Task<int> UcitajBrojZakljucenihSmenskihIzvestaja();

        // Dashboard metode
        Task<Dictionary<string, decimal>> UcitajTopRadneProcese(FilterRequest filterRequest);
        Task<Dictionary<string, decimal>> UcitajTopProizvodneProcese(FilterRequest filterRequest);
        Task<decimal> UcitajUkupnuAktivnost(FilterRequest filterRequest);
    }
}
