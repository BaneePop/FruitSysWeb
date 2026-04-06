using FruitSysWeb.Models;
using FruitSysWeb.Services.Models.Requests;

namespace FruitSysWeb.Services.Interfaces
{
    public interface IUlazIzlazService
    {
        // Faktura metode
        Task<List<FakturaModel>> UcitajSveFakture(FilterRequest filterRequest);
        Task<FakturaModel?> UcitajFakturuPoId(long id);
        Task<List<FakturaModel>> UcitajFakturePoKomitentu(long komitentId);
        Task<List<FakturaModel>> UcitajFakturePoDatumu(DateTime odDatum, DateTime doDatum);
        Task<List<FakturaModel>> UcitajFakturePoStatusu(int status);
        Task<List<FakturaModel>> UcitajOtvoreneFakture();
        Task<List<FakturaModel>> UcitajZakljuceneFakture();
        Task<List<FakturaModel>> UcitajStornoFakture();

        // OtkupniList metode
        Task<List<OtkupniListModel>> UcitajSveOtkupneListove(FilterRequest filterRequest);
        Task<OtkupniListModel?> UcitajOtkupniListPoId(long id);
        Task<List<OtkupniListModel>> UcitajOtkupneListovePoKomitentu(long komitentId);
        Task<List<OtkupniListModel>> UcitajOtkupneListovePoDatumu(DateTime odDatum, DateTime doDatum);
        Task<List<OtkupniListModel>> UcitajOtkupneListovePoStatusu(int status);
        Task<List<OtkupniListModel>> UcitajOtkupneListovePoOtkupnomMestu(long otkupnoMestoId);
        Task<List<OtkupniListModel>> UcitajIsplaceneOtkupneListove();
        Task<List<OtkupniListModel>> UcitajNeisplaceneOtkupneListove();

        // Prijemnica metode
        Task<List<PrijemnicaModel>> UcitajSvePrijemnice(FilterRequest filterRequest);
        Task<PrijemnicaModel?> UcitajPrijemnicuPoId(long id);
        Task<List<PrijemnicaModel>> UcitajPrijemnicePoKomitentu(long komitentId);
        Task<List<PrijemnicaModel>> UcitajPrijemnicePoDatumu(DateTime odDatum, DateTime doDatum);
        Task<List<PrijemnicaModel>> UcitajPrijemnicePoStatusu(int status);
        Task<List<PrijemnicaModel>> UcitajPrijemnicePoMagacinu(long magacinId);
        Task<List<PrijemnicaModel>> UcitajPrijemnicePoTipuPrijema(int tipPrijema);
        Task<List<PrijemnicaModel>> UcitajPrijemniceZaKontrolu();
        Task<List<PrijemnicaModel>> UcitajReklamiranePrijemnice();

        // Otpremnica metode
        Task<List<OtpremnicaModel>> UcitajSveOtpremnice(FilterRequest filterRequest);
        Task<OtpremnicaModel?> UcitajOtpremnicuPoId(long id);
        Task<List<OtpremnicaModel>> UcitajOtpremnicePoKomitentu(long komitentId);
        Task<List<OtpremnicaModel>> UcitajOtpremnicePoDatumu(DateTime odDatum, DateTime doDatum);
        Task<List<OtpremnicaModel>> UcitajOtpremnicePoStatusu(int status);
        Task<List<OtpremnicaModel>> UcitajOtpremnicePoMagacinu(long magacinId);
        Task<List<OtpremnicaModel>> UcitajOtpremnicePoTipu(int tipOtpreme);
        Task<List<OtpremnicaModel>> UcitajIzvozneOtpremnice();
        Task<List<OtpremnicaModel>> UcitajTranzitneOtpremnice();

        // Analitičke metode
        Task<Dictionary<string, decimal>> UcitajStatistikuPoKomitentima(FilterRequest filterRequest);
        Task<Dictionary<string, decimal>> UcitajStatistikuPoMagacinima(FilterRequest filterRequest);
        Task<Dictionary<string, decimal>> UcitajStatistikuPoStatusima(FilterRequest filterRequest);
        Task<Dictionary<string, decimal>> UcitajStatistikuPoMesecima(FilterRequest filterRequest);
        Task<decimal> UcitajUkupnuVrednostFaktura(FilterRequest filterRequest);
        Task<decimal> UcitajUkupnuVrednostOtkupnihListova(FilterRequest filterRequest);
        Task<int> UcitajUkupanBrojDokumenata(FilterRequest filterRequest);

        // Dashboard metode
        Task<Dictionary<string, decimal>> UcitajTopKomitentePoVrednosti(FilterRequest filterRequest);
        Task<Dictionary<string, decimal>> UcitajTopMagacinePoKolicini(FilterRequest filterRequest);
        Task<decimal> UcitajUkupnuKolicinu(FilterRequest filterRequest);
        Task<decimal> UcitajUkupnuVrednost(FilterRequest filterRequest);
        Task<int> UcitajBrojOtvorenihDokumenata();
        Task<int> UcitajBrojZakljucenihDokumenata();
        Task<int> UcitajBrojStornoDokumenata();
    }
}
