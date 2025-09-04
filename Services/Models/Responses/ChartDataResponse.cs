namespace FruitSysWeb.Services.Models.Responses
{
    public class ChartDataResponse
    {
        public List<string> Labels { get; set; } = new();
        public List<decimal> Values { get; set; } = new();
        public string Title { get; set; } = string.Empty;
    }
}