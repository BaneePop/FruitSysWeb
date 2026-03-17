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

        Task<Dictionary<string, int>> UcitajTopRadneProcese(FilterRequest filterRequest);
        Task<Dictionary<string, int>> UcitajTopProizvodneProcese(FilterRequest filterRequest);
        Task<int> UcitajUkupnuAktivnost(FilterRequest filterRequest);

        // Novi izveštaji
        Task<List<RadniNalogIzvestajModel>> UcitajRadniNalogIzvestaj(FilterRequest filterRequest);
        Task<List<EvidencijeIzvestajModel>> UcitajEvidencijeIzvestaj(FilterRequest filterRequest);
        Task<StatistikeModel> UcitajStatistike(FilterRequest filterRequest);

        // Helper metode za dropdown liste
        Task<List<RadniProcesModel>> UcitajRadneProcese();
        Task<List<ProizvodniProcesModel>> UcitajProizvodneProcese();

        // Dodati u postojeći IPreradaService.cs interface:
        Task<List<RadniNalogIzvestajModel>> UcitajRadniNalogIzvestajPoNalogu(string radniNalog);
        Task<decimal> UcitajUkupanTrosakPoRadnomNalogu(string radniNalog);
        Task<decimal> UcitajUkupneRadneSatePoNalogu(string radniNalog);
        Task<decimal> UcitajUkupnuRobuPoNalogu(string radniNalog);
        Task<decimal> UcitajProcenatIskoriscenjaPoNalogu(string radniNalog);
        Task<List<string>> UcitajSveRadneNaloge();
        /* Task<List<RadniProcesModel>> UcitajRadneProcese(); */
        Task<Dictionary<string, decimal>> UcitajStatistikePoRadnomNalogu(FilterRequest filter);

        // Smenski izvestaji metode
        Task<List<string>> UcitajSveSmenskeIzvestaje();
        Task<List<RadniProcesModel>> UcitajRadneProcesePoPorizvodnomProcesu(int proizvodniProcesId);
        Task<List<SmenskiIzvestajModel>> UcitajSmenskeIzvestaje(FilterRequest filter);
        Task<Dictionary<string, decimal>> UcitajStatistikePoSmenskimIzvestajima(FilterRequest filter);

        // Najavljeni utovari za dashboard
        Task<List<NajavljeniUtovarModel>> UcitajNajavljeneUtovare();

        // Prethodna smena izveštaj
        Task<PredhodnaSmenaInfo?> UcitajPredhodnuSmenuInfo();
        Task<List<PredhodnaSmenaModel>> UcitajPredhodnuSmenu();

        // Trošak po kg gotovog proizvoda
        Task<Dictionary<string, Dictionary<string, decimal>>> UcitajTrosakPoKgDirektni(int brojSmena = 20);
        Task<Dictionary<string, Dictionary<string, decimal>>> UcitajTrosakPoKgUkupni(int brojSmena = 20);

        /// <summary>
        /// Cena koštanja po radnom nalogu razložena po elementima:
        /// direktan rad, režija, direktan materijal (sirovina), ambalaza.
        /// Sadrži ukupan trošak i cenu po kg gotovog proizvoda.
        /// </summary>
        Task<List<CenaKostanjaModel>> UcitajCenuKostanjaPoRadnimNalozima(FilterRequest filterRequest);

        /// <summary>
        /// Lista radnih naloga sa osnovnim podacima za izbor pri kreiranju reklamacije.
        /// </summary>
        Task<List<Components.Pages.Reklamacije.RadniNalogZaIzbor>> UcitajRadneNalogeZaIzbor(DateTime? odDatum = null, DateTime? doDatum = null);
    }
}
