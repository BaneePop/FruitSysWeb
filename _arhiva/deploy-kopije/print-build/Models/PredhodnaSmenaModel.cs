namespace FruitSysWeb.Models
{
    /// <summary>
    /// Model za izveštaj Prethodna Smena - prikazuje radne naloge iz prethodne smene
    /// Grupisan po radnom nalogu i pakovanju
    /// </summary>
    public class PredhodnaSmenaModel
    {
        public string RadniNalog { get; set; } = string.Empty;
        public string VrstaArtikla { get; set; } = string.Empty;
        public string Pakovanje { get; set; } = string.Empty;
        public decimal Kolicina { get; set; }
        public DateTime? DatumSmene { get; set; }
        public string? BrojSmene { get; set; }
        public int? SmenaID { get; set; }
    }

    /// <summary>
    /// Info o prethodnoj smeni
    /// </summary>
    public class PredhodnaSmenaInfo
    {
        public string BrojSmene { get; set; } = string.Empty;
        public DateTime? Datum { get; set; }
        public int? Smena { get; set; }
        public string? Poslovodja { get; set; }
    }
}
