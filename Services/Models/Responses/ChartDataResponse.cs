using FruitSysWeb.Models;
using FruitSysWeb.Components;
using FruitSysWeb.Shared;
using FruitSysWeb.Services.Models;
using FruitSysWeb.Services.Interfaces;
using FruitSysWeb.Components.Shared.Charts;



namespace FruitSysWeb.Services.Models.Responses

    {
    public class ChartDataResponse
        {
        public List<string> Labels { get; set; } = new();
        public List<decimal> Values { get; set; } = new();
        public string Title { get; set; } = string.Empty;
     }
}