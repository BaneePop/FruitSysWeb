namespace FruitSysWeb.Utils.Charts.Models
{
    public class ChartConfig
    {
        public string Type { get; set; } = "";
        public ChartData Data { get; set; } = new();
        public ChartOptions Options { get; set; } = new();
    }

    public class ChartData
    {
        public List<string> Labels { get; set; } = new();
        public List<Dataset> Datasets { get; set; } = new();
    }

    public class Dataset
    {
        public string Label { get; set; } = "";
        public List<decimal> Data { get; set; } = new();
        public string BackgroundColor { get; set; } = "";
        public string BorderColor { get; set; } = "";
        public int BorderWidth { get; set; } = 1;
        public bool Fill { get; set; } = false;
        public double Tension { get; set; } = 0.1;
    }

    public class ChartOptions
    {
        public bool Responsive { get; set; } = true;
        public bool MaintainAspectRatio { get; set; } = false;
        public ChartPlugins Plugins { get; set; } = new();
        public ChartScales? Scales { get; set; }
    }

    public class ChartPlugins
    {
        public ChartLegend Legend { get; set; } = new();
        public ChartTitle? Title { get; set; }
    }

    public class ChartLegend
    {
        public bool Display { get; set; } = false;
        public string Position { get; set; } = "top";
    }

    public class ChartTitle
    {
        public bool Display { get; set; } = false;
        public string Text { get; set; } = "";
    }

    public class ChartScales
    {
        public ChartAxis? Y { get; set; }
        public ChartAxis? X { get; set; }
    }

    public class ChartAxis
    {
        public bool BeginAtZero { get; set; } = true;
    }

    // Tipovi za kompatibilnost sa postojećim kodom
    public class BarConfig : ChartConfig
    {
        public BarConfig()
        {
            Type = "bar";
            Options.Scales = new ChartScales
            {
                Y = new ChartAxis { BeginAtZero = true }
            };
        }
    }

    public class PieConfig : ChartConfig
    {
        public PieConfig()
        {
            Type = "pie";
            Options.Plugins.Legend.Display = true;
            Options.Plugins.Legend.Position = "right";
        }
    }

    public class LineConfig : ChartConfig
    {
        public LineConfig()
        {
            Type = "line";
            Options.Scales = new ChartScales
            {
                Y = new ChartAxis { BeginAtZero = true }
            };
        }
    }
}