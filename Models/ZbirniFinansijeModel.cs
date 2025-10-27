namespace FruitSysWeb.Models
{
    public class ZbirniFinansijeModel
    {
        public DateTime Datum { get; set; }
        public string Komitent { get; set; } = string.Empty;
        public long? KomitentId { get; set; }
        public string Artikal { get; set; } = string.Empty;
        public int BrojDokumenata { get; set; }
        public decimal UkupnaKolicina { get; set; }
        public decimal UkupnoPotrazuje { get; set; }
        public decimal UkupnoDuguje { get; set; }
        public decimal Saldo => UkupnoPotrazuje - UkupnoDuguje;
    }
}