namespace FruitSysWeb.Utils.Charts
{
    // Jednostavni tipovi umesto ChartJs.Blazor tipova
    public class BarConfig
    {
        public ChartData Data { get; set; } = new ChartData();
        public BarOptions Options { get; set; } = new BarOptions();
    }

    public class PieConfig
    {
        public ChartData Data { get; set; } = new ChartData();
        public PieOptions Options { get; set; } = new PieOptions();
    }

    public class LineConfig
    {
        public ChartData Data { get; set; } = new ChartData();
        public LineOptions Options { get; set; } = new LineOptions();
    }

    public class ChartData
    {
        public List<string> Labels { get; set; } = new List<string>();
        public List<object> Datasets { get; set; } = new List<object>();
    }

    public class BarOptions
    {
        public bool Responsive { get; set; } = true;
        public bool MaintainAspectRatio { get; set; } = false;
        public object? Legend { get; set; }
        public object? Scales { get; set; }
    }

    public class PieOptions
    {
        public bool Responsive { get; set; } = true;
        public bool MaintainAspectRatio { get; set; } = false;
        public object? Title { get; set; }
        public object? Legend { get; set; }
    }

    public class LineOptions
    {
        public bool Responsive { get; set; } = true;
        public bool MaintainAspectRatio { get; set; } = false;
        public object? Legend { get; set; }
        public object? Scales { get; set; }
    }

    public class BarDataset
    {
        public string Label { get; set; } = "";
        public decimal[] Data { get; set; } = Array.Empty<decimal>();
        public string BackgroundColor { get; set; } = "";
        public string BorderColor { get; set; } = "";
        public int BorderWidth { get; set; } = 1;

        public void Clear()
        {
            Data = Array.Empty<decimal>();
        }

        public void Add(decimal value)
        {
            var list = Data.ToList();
            list.Add(value);
            Data = list.ToArray();
        }
    }

    public class PieDataset
    {
        public decimal[] Data { get; set; } = Array.Empty<decimal>();
        public string[] BackgroundColor { get; set; } = Array.Empty<string>();
        public string[] BorderColor { get; set; } = Array.Empty<string>();
        public int BorderWidth { get; set; } = 1;

        public void Clear()
        {
            Data = Array.Empty<decimal>();
        }

        public void Add(decimal value)
        {
            var list = Data.ToList();
            list.Add(value);
            Data = list.ToArray();
        }
    }

    public class LineDataset
    {
        public string Label { get; set; } = "";
        public double[] Data { get; set; } = Array.Empty<double>();
        public string BackgroundColor { get; set; } = "";
        public string BorderColor { get; set; } = "";
        public int BorderWidth { get; set; } = 1;
        public bool Fill { get; set; } = false;
        public double LineTension { get; set; } = 0.1;
    }
}