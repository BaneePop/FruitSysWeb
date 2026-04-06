

namespace FruitSysWeb.Components.Charts
{
    public class ChartDataPoint
    {
        public string Label { get; set; } = "";
        public decimal Value { get; set; }
        public string? Color { get; set; }
        public object? ExtraData { get; set; }

        public ChartDataPoint() { }

        public ChartDataPoint(string label, decimal value)
        {
            Label = label;
            Value = value;
        }

        public ChartDataPoint(string label, decimal value, string color)
        {
            Label = label;
            Value = value;
            Color = color;
        }
    }

    /// <summary>
    /// Predstavlja jednu seriju podataka za multi-series chart
    /// </summary>
    public class ChartSeriesData
    {
        public string Name { get; set; } = "";
        public List<ChartDataPoint> Data { get; set; } = new();
        public string? Color { get; set; }

        // Dodatni podaci za summary prikaz
        public decimal TotalKolicina { get; set; }
        public decimal TotalVrednost { get; set; }

        public ChartSeriesData() { }

        public ChartSeriesData(string name, List<ChartDataPoint> data)
        {
            Name = name;
            Data = data;
        }

        public ChartSeriesData(string name, List<ChartDataPoint> data, string color)
        {
            Name = name;
            Data = data;
            Color = color;
        }
    }

    public static class ChartDataHelper
    {
        /// <summary>
        /// Konvertuje Dictionary u ChartDataPoint listu
        /// </summary>
        public static List<ChartDataPoint> FromDictionary(Dictionary<string, decimal> data, int maxItems = 10)
        {
            if (data == null || !data.Any())
                return new List<ChartDataPoint>();

            return data
                .OrderByDescending(x => x.Value)
                .Take(maxItems)
                .Select(x => new ChartDataPoint(x.Key, x.Value))
                .ToList();
        }

        /// <summary>
        /// Skraćuje dugačke labele
        /// </summary>
        public static List<ChartDataPoint> TruncateLabels(List<ChartDataPoint> data, int maxLength = 20)
        {
            return data.Select(x => new ChartDataPoint
            {
                Label = x.Label.Length > maxLength ? x.Label.Substring(0, maxLength) + "..." : x.Label,
                Value = x.Value,
                Color = x.Color,
                ExtraData = x.ExtraData
            }).ToList();
        }

        /// <summary>
        /// Formatira brojeve za prikaz
        /// </summary>
        public static string FormatValue(decimal value, string unit = "")
        {
            string formattedValue = value >= 1000
                ? $"{value / 1000:F1}k"
                : value.ToString("F1");

            return string.IsNullOrEmpty(unit) ? formattedValue : $"{formattedValue} {unit}";
        }

        /// <summary>
        /// Dodaje default boje za Pie Chart
        /// </summary>
        public static List<ChartDataPoint> WithColors(List<ChartDataPoint> data, List<string>? colors = null)
        {
            var defaultColors = new List<string>
            {
                "#28a745", "#dc3545", "#007bff", "#ffc107", "#6f42c1",
                "#20c997", "#fd7e14", "#e83e8c", "#6c757d", "#17a2b8"
            };

            var colorsToUse = colors ?? defaultColors;

            for (int i = 0; i < data.Count; i++)
            {
                data[i].Color = colorsToUse[i % colorsToUse.Count];
            }

            return data;
        }

        /// <summary>
        /// Grupiše manje vrednosti u "Ostalo"
        /// </summary>
        public static List<ChartDataPoint> GroupSmallValues(List<ChartDataPoint> data, int maxItems = 8, string otherLabel = "Ostalo")
        {
            if (data.Count <= maxItems)
                return data;

            var topItems = data.Take(maxItems - 1).ToList();
            var remainingItems = data.Skip(maxItems - 1);
            var otherSum = remainingItems.Sum(x => x.Value);

            if (otherSum > 0)
            {
                topItems.Add(new ChartDataPoint(otherLabel, otherSum));
            }

            return topItems;
        }

        /// <summary>
        /// Određuje boju prema vrsti voća u nazivu
        /// </summary>
        public static string GetFruitColor(string label)
        {
            var labelLower = label.ToLower();

            if (labelLower.Contains("malina")) return "#DC143C"; // Crimson - crvena
            if (labelLower.Contains("kupina")) return "#1C1C1C"; // Tamna grafitna crna

            // Šljiva - uključuje sve sorte šljiva
            if (labelLower.Contains("šljiva") || labelLower.Contains("sljiva") ||
                labelLower.Contains("stenley") || labelLower.Contains("stanley") ||
                labelLower.Contains("čačanka") || labelLower.Contains("cacanka") ||
                labelLower.Contains("požegača") || labelLower.Contains("pozegaca") ||
                labelLower.Contains("pžegača") || labelLower.Contains("pzegaca"))
            {
                return "#0000FF"; // Plava
            }

            if (labelLower.Contains("višnja") || labelLower.Contains("visnja")) return "#8B0000"; // DarkRed - bordo
            if (labelLower.Contains("borovnica")) return "#800000"; // Maroon - bordo
            if (labelLower.Contains("kajsija")) return "#FF8C00"; // DarkOrange - narandžasta

            return "#28a745"; // Default zelena
        }

        /// <summary>
        /// Dodaje boje prema vrsti voća u nazivu
        /// </summary>
        public static List<ChartDataPoint> WithFruitColors(List<ChartDataPoint> data)
        {
            foreach (var item in data)
            {
                item.Color = GetFruitColor(item.Label);
            }
            return data;
        }

        /// <summary>
        /// Mapira PrvaKlasifikacijaID na vrstu voća.
        /// 6=Malina, 10=Kupina, 11=Višnja, 15=Šljiva, 25=Ambalaža, 27=Repromaterijal, 28=Kajsija, 34=Jagoda, 39=Borovnica
        /// </summary>
        public static string MapKlasifikacijaToFruitType(int klasifikacijaId)
        {
            return klasifikacijaId switch
            {
                6  => "Malina",
                10 => "Kupina",
                11 => "Višnja",
                15 => "Šljiva",
                25 => "Ambalaža",
                27 => "Repromaterijal",
                28 => "Kajsija",
                34 => "Jagoda",
                39 => "Borovnica",
                _  => "Ostalo"
            };
        }

        /// <summary>
        /// Mapira naziv artikla na glavnu vrstu voća (fallback za stari kod — preferuj MapKlasifikacijaToFruitType)
        /// </summary>
        public static string MapToFruitType(string artikalNaziv)
        {
            var nazivLower = artikalNaziv.ToLower();

            if (nazivLower.Contains("malina")) return "Malina";
            if (nazivLower.Contains("kupina")) return "Kupina";

            // Šljiva - sve sorte
            if (nazivLower.Contains("šljiva") || nazivLower.Contains("sljiva") ||
                nazivLower.Contains("stenley") || nazivLower.Contains("stanley") ||
                nazivLower.Contains("čačanka") || nazivLower.Contains("cacanka") ||
                nazivLower.Contains("požegača") || nazivLower.Contains("pozegaca") ||
                nazivLower.Contains("pžegača") || nazivLower.Contains("pzegaca"))
            {
                return "Šljiva";
            }

            if (nazivLower.Contains("višnja") || nazivLower.Contains("visnja")) return "Višnja";
            if (nazivLower.Contains("borovnica")) return "Borovnica";
            if (nazivLower.Contains("kajsija")) return "Kajsija";
            if (nazivLower.Contains("jagoda")) return "Jagoda";

            return "Ostalo";
        }

        /// <summary>
        /// Mock podatci za testiranje
        /// </summary>
        public static List<ChartDataPoint> GetMockData(string prefix = "Item", int count = 5)
        {
            var random = new Random();
            var data = new List<ChartDataPoint>();

            for (int i = 1; i <= count; i++)
            {
                data.Add(new ChartDataPoint(
                    $"{prefix} {i}",
                    random.Next(100, 1000)
                ));
            }

            return data.OrderByDescending(x => x.Value).ToList();
        }

        /// <summary>
        /// Validira da li podaci sadrže validne vrednosti
        /// </summary>
        public static bool IsValidData(List<ChartDataPoint>? data)
        {
            return data != null &&
                   data.Any() &&
                   data.All(x => !string.IsNullOrEmpty(x.Label) && x.Value >= 0);
        }

        /// <summary>
        /// Konvertuje Dictionary<Datum, Dictionary<VrstaProizvoda, Vrednost>> u MultiSeriesDataPoint listu
        /// </summary>
        public static List<MultiSeriesDataPoint> ConvertToMultiSeries(Dictionary<string, Dictionary<string, decimal>> data)
        {
            if (data == null || !data.Any())
                return new List<MultiSeriesDataPoint>();

            var result = new List<MultiSeriesDataPoint>();

            // Prvo sakupi sve unikalne serije (vrste proizvoda)
            var sveVrste = data.Values
                .SelectMany(d => d.Keys)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            // Za svaki datum
            foreach (var datum in data.Keys.OrderBy(x => x))
            {
                var point = new MultiSeriesDataPoint
                {
                    Label = datum,
                    Series = new Dictionary<string, decimal>()
                };

                // Za svaku vrstu proizvoda, dodaj vrednost (ili 0 ako ne postoji)
                foreach (var vrsta in sveVrste)
                {
                    point.Series[vrsta] = data[datum].ContainsKey(vrsta) ? data[datum][vrsta] : 0;
                }

                result.Add(point);
            }

            return result;
        }

        /// <summary>
        /// Konvertuje Dictionary<Datum, Dictionary<VrstaProizvoda, Vrednost>> u ChartSeriesData listu
        /// </summary>
        public static List<ChartSeriesData> ConvertToChartSeriesData(Dictionary<string, Dictionary<string, decimal>> data)
        {
            if (data == null || !data.Any())
                return new List<ChartSeriesData>();

            // Prvo sakupi sve unikalne datume i sortiraj ih
            var sviDatumi = data.Keys.OrderBy(x => x).ToList();

            // Zatim sakupi sve unikalne serije (vrste proizvoda)
            var sveVrste = data.Values
                .SelectMany(d => d.Keys)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            // Kreiraj jednu ChartSeriesData za svaku vrstu proizvoda
            var result = new List<ChartSeriesData>();

            foreach (var vrsta in sveVrste)
            {
                var series = new ChartSeriesData
                {
                    Name = vrsta,
                    Color = GetFruitColor(vrsta), // Boja serije na nivou series
                    Data = sviDatumi.Select(datum => new ChartDataPoint
                    {
                        Label = datum,
                        Value = data[datum].ContainsKey(vrsta) ? data[datum][vrsta] : 0
                    }).ToList(),
                    TotalKolicina = sviDatumi.Sum(datum => data[datum].ContainsKey(vrsta) ? data[datum][vrsta] : 0),
                    TotalVrednost = sviDatumi.Sum(datum => data[datum].ContainsKey(vrsta) ? data[datum][vrsta] : 0)
                };

                result.Add(series);
            }

            return result;
        }
    }

    /// <summary>
    /// Predstavlja jednu tačku podataka za multi-series line chart
    /// </summary>
    public class MultiSeriesDataPoint
    {
        public string Label { get; set; } = "";
        public Dictionary<string, decimal> Series { get; set; } = new();
    }
}
