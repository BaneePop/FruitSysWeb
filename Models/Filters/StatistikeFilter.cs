namespace FruitSysWeb.Models.Filters
{
    public class StatistikeFilter
    {
        public DateTime? OdDatum { get; set; }
        public DateTime? DoDatum { get; set; }
        public string TipIzvestaja { get; set; } = "proizvodnja";

        public string OdDatumValue => OdDatum?.ToString("yyyy-MM-dd") ?? "";
        public string DoDatumValue => DoDatum?.ToString("yyyy-MM-dd") ?? "";
    }
}
