using ChartJs.Blazor.BarChart;
using ChartJs.Blazor.Common;
using ChartJs.Blazor.PieChart;
using ChartJs.Blazor.LineChart;
using ChartJs.Blazor.Common.Axes;
using ChartJs.Blazor.Common.Axes.Ticks;
using ChartJs.Blazor.Common.Enums;

namespace FruitSysWeb.Utils.Charts
{
    public static class ChartHelper
    {
        // Default colors for charts
        private static readonly string[] DefaultColors = new[]
        {
            "rgba(40, 167, 69, 0.8)",   // Green
            "rgba(220, 53, 69, 0.8)",   // Red
            "rgba(13, 110, 253, 0.8)",  // Blue
            "rgba(255, 193, 7, 0.8)",   // Yellow
            "rgba(108, 117, 125, 0.8)", // Gray
            "rgba(23, 162, 184, 0.8)",  // Cyan
            "rgba(220, 53, 69, 0.8)",   // Red variant
            "rgba(111, 66, 193, 0.8)",  // Purple
            "rgba(255, 108, 0, 0.8)",   // Orange
            "rgba(32, 201, 151, 0.8)"   // Teal
        };

        private static readonly string[] BorderColors = new[]
        {
            "rgba(40, 167, 69, 1)",   
            "rgba(220, 53, 69, 1)",   
            "rgba(13, 110, 253, 1)",  
            "rgba(255, 193, 7, 1)",   
            "rgba(108, 117, 125, 1)", 
            "rgba(23, 162, 184, 1)",  
            "rgba(220, 53, 69, 1)",   
            "rgba(111, 66, 193, 1)",  
            "rgba(255, 108, 0, 1)",   
            "rgba(32, 201, 151, 1)"   
        };

        /// <summary>
        /// Creates a bar chart configuration
        /// </summary>
        public static BarConfig CreateBarChart(string[] labels, decimal[] data, string label, string? color = null)
        {
            var config = new BarConfig
            {
                Options = new BarOptions
                {
                    Responsive = true,
                    MaintainAspectRatio = false,
                    Legend = new Legend
                    {
                        Display = false
                    },
                    Scales = new BarScales
                    {
                        YAxes = new List<CartesianAxis>
                        {
                            new LinearCartesianAxis
                            {
                                Ticks = new LinearCartesianTicks
                                {
                                    BeginAtZero = true
                                }
                            }
                        }
                    }
                }
            };

            var dataset = new BarDataset<decimal>
            {
                Label = label,
                BackgroundColor = color ?? DefaultColors[0],
                BorderColor = color?.Replace("0.8", "1") ?? BorderColors[0],
                BorderWidth = 1
            };

            foreach (var value in data)
            {
                dataset.Add(value);
            }

            // Use for loop instead of AddRange
            foreach (var labelItem in labels)
            {
                config.Data.Labels.Add(labelItem);
            }
            config.Data.Datasets.Add(dataset);

            return config;
        }

        /// <summary>
        /// Creates a pie chart configuration
        /// </summary>
        public static PieConfig CreatePieChart(string[] labels, decimal[] data, string? title = null)
        {
            var config = new PieConfig
            {
                Options = new PieOptions
                {
                    Responsive = true,
                    MaintainAspectRatio = false,
                    Legend = new Legend
                    {
                        Position = Position.Right
                    }
                }
            };

            if (!string.IsNullOrEmpty(title))
            {
                config.Options.Title = new OptionsTitle
                {
                    Display = true,
                    Text = title
                };
            }

            var dataset = new PieDataset<decimal>
            {
                BackgroundColor = DefaultColors.Take(labels.Length).ToArray(),
                BorderColor = BorderColors.Take(labels.Length).ToArray(),
                BorderWidth = 1
            };

            foreach (var value in data)
            {
                dataset.Add(value);
            }

            // Use for loop instead of AddRange
            foreach (var labelItem in labels)
            {
                config.Data.Labels.Add(labelItem);
            }
            config.Data.Datasets.Add(dataset);

            return config;
        }

        /// <summary>
        /// Creates a line chart configuration
        /// </summary>
        public static LineConfig CreateLineChart(string[] labels, decimal[] data, string label, string? color = null)
        {
            var config = new LineConfig
            {
                Options = new LineOptions
                {
                    Responsive = true,
                    MaintainAspectRatio = false,
                    Legend = new Legend
                    {
                        Display = false
                    },
                    Scales = new Scales
                    {
                        YAxes = new List<CartesianAxis>
                        {
                            new LinearCartesianAxis
                            {
                                Ticks = new LinearCartesianTicks
                                {
                                    BeginAtZero = true
                                }
                            }
                        }
                    }
                }
            };

            var dataset = new LineDataset<decimal>
            {
                Label = label,
                BackgroundColor = (color ?? DefaultColors[2]).Replace("0.8", "0.2"),
                BorderColor = color?.Replace("0.8", "1") ?? BorderColors[2],
                BorderWidth = 2
                // Removing Fill property to avoid compilation errors
            };

            foreach (var value in data)
            {
                dataset.Add(value);
            }

            // Use for loop instead of AddRange
            foreach (var labelItem in labels)
            {
                config.Data.Labels.Add(labelItem);
            }
            config.Data.Datasets.Add(dataset);

            return config;
        }

        /// <summary>
        /// Converts Dictionary to arrays for chart consumption
        /// </summary>
        public static (string[] labels, decimal[] values) ConvertDictionaryToArrays(Dictionary<string, decimal> data, int maxItems = 10)
        {
            var items = data.Take(maxItems).ToArray();
            var labels = items.Select(x => x.Key).ToArray();
            var values = items.Select(x => x.Value).ToArray();
            
            return (labels, values);
        }

        /// <summary>
        /// Truncates long labels for better display
        /// </summary>
        public static string[] TruncateLabels(string[] labels, int maxLength = 20)
        {
            return labels.Select(label => 
                label.Length > maxLength ? label.Substring(0, maxLength) + "..." : label
            ).ToArray();
        }

        /// <summary>
        /// Formats numbers for display in charts
        /// </summary>
        public static string FormatNumber(decimal number)
        {
            if (number >= 1000000)
                return $"{number / 1000000:F1}M";
            if (number >= 1000)
                return $"{number / 1000:F1}K";
            return number.ToString("F1");
        }

        /// <summary>
        /// Creates chart with custom colors
        /// </summary>
        public static BarConfig CreateCustomBarChart(Dictionary<string, decimal> data, string title, string? color = null, int maxItems = 5)
        {
            var (labels, values) = ConvertDictionaryToArrays(data, maxItems);
            var truncatedLabels = TruncateLabels(labels, 15);
            
            return CreateBarChart(truncatedLabels, values, title, color);
        }

        /// <summary>
        /// Creates pie chart from dictionary
        /// </summary>
        public static PieConfig CreateCustomPieChart(Dictionary<string, decimal> data, string? title = null, int maxItems = 8)
        {
            var (labels, values) = ConvertDictionaryToArrays(data, maxItems);
            var truncatedLabels = TruncateLabels(labels, 12);
            
            return CreatePieChart(truncatedLabels, values, title);
        }
    }
}
