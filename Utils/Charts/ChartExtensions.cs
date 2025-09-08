using ChartJs.Blazor.BarChart;
using ChartJs.Blazor.PieChart;
using ChartJs.Blazor.Common;
using FruitSysWeb.Components.Shared.Charts;


namespace FruitSysWeb.Utils.Charts
{
    public static class ChartExtensions
    {
        /// <summary>
        /// Updates bar chart data with new values
        /// </summary>
        public static void UpdateData(this BarConfig config, Dictionary<string, decimal> newData, int maxItems = 5)
        {
            if (config?.Data?.Datasets == null || !config.Data.Datasets.Any()) 
                return;

            var (labels, values) = ChartHelper.ConvertDictionaryToArrays(newData, maxItems);
            var truncatedLabels = ChartHelper.TruncateLabels(labels, 15);

            // Clear existing data
            config.Data.Labels.Clear();
            var dataset = config.Data.Datasets.First() as BarDataset<decimal>;
            dataset?.Clear();

            // Add new data using for loop
            foreach (var label in truncatedLabels)
            {
                config.Data.Labels.Add(label);
            }
            
            if (dataset != null)
            {
                foreach (var value in values)
                {
                    dataset.Add(value);
                }
            }
        }

        /// <summary>
        /// Updates pie chart data with new values
        /// </summary>
        public static void UpdateData(this PieConfig config, Dictionary<string, decimal> newData, int maxItems = 8)
        {
            if (config?.Data?.Datasets == null || !config.Data.Datasets.Any()) 
                return;

            var (labels, values) = ChartHelper.ConvertDictionaryToArrays(newData, maxItems);
            var truncatedLabels = ChartHelper.TruncateLabels(labels, 12);

            // Clear existing data
            config.Data.Labels.Clear();
            var dataset = config.Data.Datasets.First() as PieDataset<decimal>;
            dataset?.Clear();

            // Add new data using for loop
            foreach (var label in truncatedLabels)
            {
                config.Data.Labels.Add(label);
            }
            
            if (dataset != null)
            {
                foreach (var value in values)
                {
                    dataset.Add(value);
                }
            }
        }

        /// <summary>
        /// Checks if chart has any data
        /// </summary>
        public static bool HasData(this BarConfig config)
        {
            return config?.Data?.Datasets?.Any() == true && 
                   config.Data.Datasets.First() is BarDataset<decimal> dataset && 
                   dataset.Data.Any();
        }

        /// <summary>
        /// Checks if chart has any data
        /// </summary>
        public static bool HasData(this PieConfig config)
        {
            return config?.Data?.Datasets?.Any() == true && 
                   config.Data.Datasets.First() is PieDataset<decimal> dataset && 
                   dataset.Data.Any();
        }

        /// <summary>
        /// Gets total value from chart data
        /// </summary>
        public static decimal GetTotalValue(this BarConfig config)
        {
            if (!config.HasData()) return 0;
            
            var dataset = config.Data.Datasets.First() as BarDataset<decimal>;
            return dataset?.Data.Sum() ?? 0;
        }

        /// <summary>
        /// Gets total value from chart data
        /// </summary>
        public static decimal GetTotalValue(this PieConfig config)
        {
            if (!config.HasData()) return 0;
            
            var dataset = config.Data.Datasets.First() as PieDataset<decimal>;
            return dataset?.Data.Sum() ?? 0;
        }
    }
}
