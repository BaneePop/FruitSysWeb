using FruitSysWeb.Models;
using FruitSysWeb.Services.Models.Requests;



namespace FruitSysWeb.Services.Interfaces
{
    public interface IPaletniListService
    {
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
        
    
