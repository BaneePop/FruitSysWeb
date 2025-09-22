namespace FruitSysWeb.Models.Filters
{
    public class RadniNalogFilter
    {
        public DateTime? OdDatum { get; set; }
        public DateTime? DoDatum { get; set; }
        public string BrojRadnogNaloga { get; set; } = "";
        public string VrstaProizvoda { get; set; } = "";
        public long? RadniProcesId { get; set; }
        
        public string OdDatumValue => OdDatum?.ToString("yyyy-MM-dd") ?? "";
        public string DoDatumValue => DoDatum?.ToString("yyyy-MM-dd") ?? "";
    }
}
