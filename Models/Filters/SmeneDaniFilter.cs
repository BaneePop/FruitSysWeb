namespace FruitSysWeb.Models.Filters
{
    public class SmeneDaniFilter
    {
        public DateTime? OdDatum { get; set; }
        public DateTime? DoDatum { get; set; }
        public int? Smena { get; set; }
        public string VrstaGotovogProizvoda { get; set; } = "";
        public long? RadniProcesId { get; set; }

        public string OdDatumValue => OdDatum?.ToString("yyyy-MM-dd") ?? "";
        public string DoDatumValue => DoDatum?.ToString("yyyy-MM-dd") ?? "";
    }
}
