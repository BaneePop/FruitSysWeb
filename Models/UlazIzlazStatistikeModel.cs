using System.ComponentModel.DataAnnotations;

namespace FruitSysWeb.Models
{
    public class UlazIzlazStatistikeModel
    {
        [Display(Name = "Ukupna Prodaja")]
        public decimal UkupnaProdaja { get; set; }

        [Display(Name = "Ukupna Nabavka")]
        public decimal UkupnaNabavka { get; set; }

        [Display(Name = "Ukupni Profit")]
        public decimal UkupniProfit { get; set; }

        [Display(Name = "Broj Dokumenata")]
        public int BrojDokumenata { get; set; }

        [Display(Name = "Broj Faktura")]
        public int BrojFaktura { get; set; }

        [Display(Name = "Broj Otkupnih Listova")]
        public int BrojOtkupnihListova { get; set; }

        [Display(Name = "Broj Prijemnica")]
        public int BrojPrijemnica { get; set; }

        [Display(Name = "Broj Otpremnica")]
        public int BrojOtpremnica { get; set; }

        [Display(Name = "Prosečna Vrednost Po Dokumentu")]
        public decimal ProsecnaVrednostPoKumentu => BrojDokumenata > 0 ?
            (UkupnaProdaja + UkupnaNabavka) / BrojDokumenata : 0;

        [Display(Name = "Profit Margin %")]
        public decimal ProfitMarginProcenat => UkupnaProdaja > 0 ?
            (UkupniProfit / UkupnaProdaja) * 100 : 0;

        [Display(Name = "Ukupna Prodaja Formatirana")]
        public string UkupnaProdajaFormatirana => UkupnaProdaja.ToString("N2") + " RSD";

        [Display(Name = "Ukupna Nabavka Formatirana")]
        public string UkupnaNabavkaFormatirana => UkupnaNabavka.ToString("N2") + " RSD";

        [Display(Name = "Ukupni Profit Formatiran")]
        public string UkupniProfitFormatiran => UkupniProfit.ToString("N2") + " RSD";

        [Display(Name = "Prosečna Vrednost Formatirana")]
        public string ProsecnaVrednostFormatirana => ProsecnaVrednostPoKumentu.ToString("N2") + " RSD";

        // Chart data properties
        [Display(Name = "Podaci za Chart Prodaja Po Mesecima")]
        public Dictionary<string, decimal> ProdajaPoMesecima { get; set; } = new();

        [Display(Name = "Podaci za Chart Nabavka Po Mesecima")]
        public Dictionary<string, decimal> NabavkaPoMesecima { get; set; } = new();

        [Display(Name = "Podaci za Chart Top Komitenti")]
        public Dictionary<string, decimal> TopKomitenti { get; set; } = new();

        [Display(Name = "Podaci za Chart Top Dobavljači")]
        public Dictionary<string, decimal> TopDobavljaci { get; set; } = new();

        // Status counts
        [Display(Name = "Broj Otvorenih Dokumenata")]
        public int BrojOtvorenihDokumenata { get; set; }

        [Display(Name = "Broj Zaključenih Dokumenata")]
        public int BrojZakljucenihDokumenata { get; set; }

        [Display(Name = "Broj Storno Dokumenata")]
        public int BrojStornoDokumenata { get; set; }
    }
}
