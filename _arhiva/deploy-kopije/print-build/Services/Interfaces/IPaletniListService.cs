using FruitSysWeb.Models;
using FruitSysWeb.Services.Models.Requests;



namespace FruitSysWeb.Services.Interfaces
{
    public interface IPaletniListService
    {
        // ── Paletni List Pregled (4 taba) ──────────────────────────────────
        Task<List<PaletniListPregledRow>> UcitajNabavkuPaletniListova(PaletniListPregledFilter filter);
        Task<List<PaletniListPregledRow>> UcitajProdajuPaletniListova(PaletniListPregledFilter filter);
        Task<List<PaletniListPregledRow>> UcitajProizvodnjuPaletniListova(PaletniListPregledFilter filter);
        Task<PaletniListPovezanostResult> UcitajPovezanostPaletniListova(string sifraPL);
        Task<List<PaletniListPregledRow>> UcitajPojedinacniDokument(string tip, string sifra);

        // ── Kvalitet tab ────────────────────────────────────────────────────
        Task<List<PaletniListKvalitetRow>> UcitajKvalitetIzvestaj(PaletniListKvalitetFilter filter);
        Task<List<(string ID, string Naziv)>> UcitajArtikleZaKvalitet();
        Task<List<string>> UcitajKomitentePaletniListovaKvalitet();
        Task<List<string>> UcitajPrijemniceZaPeriodKvalitet(DateTime? odDatum, DateTime? doDatum);

        // ── Dropdown liste za filtere ───────────────────────────────────────
        Task<List<string>> UcitajKomitentePaletniListova(int tip);
        Task<List<string>> UcitajArtiklePaletniListova(int tip);
        Task<List<string>> UcitajPrijemniceZaPeriod(DateTime? odDatum, DateTime? doDatum);
        Task<List<string>> UcitajOtpremnicaZaPeriod(DateTime? odDatum, DateTime? doDatum);
        Task<List<string>> UcitajRadneNalogeZaPeriod(DateTime? odDatum, DateTime? doDatum);

        /// <summary>
        /// Učitava sve prijeme za današnji dan (PaletniListTip = 1)
        /// </summary>

        /// <summary>
        /// Učitava prijeme po datumu
        /// </summary>
        Task<List<PaletniListModel>> UcitajPrijemePoDatumu(DateTime datum);

        /// <summary>
        /// Učitava ukupne količine prijema po vrstama voća za današnji dan
        /// </summary>


        /// <summary>
        /// Učitava grupisane podatke za chart - prijem po dobavljačima za svaku vrstu voća
        /// </summary>
        Task<Dictionary<string, List<PrijemPoDobavljacuModel>>> UcitajPrijemPoVocuIDobavljacima();

        /// <summary>
        /// Učitava broj aktivnih prijema za današnji dan
        /// </summary>
        Task<int> UcitajBrojAktivnihPrijema();


        Task<decimal> UcitajUkupnuTezinoZaDan();

        Task<int> UcitajBrojAktivnihPrijemaPeriod(FilterRequest filterRequest);

        Task<decimal> UcitajUkupnuTezinuPeriod(FilterRequest filterRequest);
        Task<List<PrijemStatistikaModel>> UcitajStatistikuPrijemaPoVocuPeriod(FilterRequest filterRequest);
        Task<Dictionary<string, List<PrijemPoDobavljacuModel>>> UcitajPrijemPoVocuIDobavljacimaPeriod(FilterRequest filterRequest);

        Task<List<PaletniListModel>> UcitajDanasnjePrijeme();
        Task<List<PaletniListModel>> UcitajPrijemeZaPeriod(FilterRequest filterRequest);
        Task<List<PrijemStatistikaModel>> UcitajStatistikuPrijemaPoVocu();
    }
}
        
    
