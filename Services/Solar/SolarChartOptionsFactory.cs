using ApexCharts;
using FruitSysWeb.Models.Solar;

namespace FruitSysWeb.Services.Solar;

/// <summary>
/// Zajedničke ApexCharts opcije za solar stranice — bez hover tooltip-a i bez menjanja podataka pri prelasku mišem.
/// Toolbar: samo zoom in / zoom out.
/// </summary>
public static class SolarChartOptionsFactory
{
    public static ApexChartOptions<SolarKpiRecord> ZaSolarKpi(
        bool darkTheme = false,
        IReadOnlyList<string>? colors = null,
        int strokeWidth = 3,
        int markerSize = 0,
        string? yAxisTitle = null,
        bool smooth = true,
        bool kwFormatter = true) =>
        Build(darkTheme, colors, strokeWidth, markerSize, yAxisTitle, smooth, kwFormatter);

    private static ApexChartOptions<SolarKpiRecord> Build(
        bool darkTheme,
        IReadOnlyList<string>? colors,
        int strokeWidth,
        int markerSize,
        string? yAxisTitle,
        bool smooth,
        bool kwFormatter)
    {
        var options = new ApexChartOptions<SolarKpiRecord>
        {
            Theme = darkTheme ? new Theme { Mode = Mode.Dark } : null,
            Chart = new Chart
            {
                Type = ChartType.Line,
                Background = "transparent",
                ForeColor = darkTheme ? "#e5e7eb" : "#9ca3af",
                Animations = new Animations { Enabled = false },
                Toolbar = new Toolbar
                {
                    Show = true,
                    Tools = new Tools
                    {
                        Download = false,
                        Selection = false,
                        Zoom = false,
                        Zoomin = true,
                        Zoomout = true,
                        Pan = false,
                        Reset = false
                    }
                },
                Zoom = new Zoom { Enabled = true, Type = AxisType.X, AutoScaleYaxis = false }
            },
            Colors = colors?.ToList() ?? new List<string> { "#22c55e" },
            Stroke = new Stroke { Curve = smooth ? Curve.Smooth : Curve.Straight, Width = strokeWidth },
            Markers = new Markers
            {
                Size = markerSize,
                Hover = new MarkersHover { Size = 0, SizeOffset = 0 }
            },
            States = new States
            {
                Normal = new StatesNormal { Filter = new StatesFilter { Type = StatesFilterType.none } },
                Hover = new StatesHover { Filter = new StatesFilter { Type = StatesFilterType.none } },
                Active = new StatesActive
                {
                    AllowMultipleDataPointsSelection = false,
                    Filter = new StatesFilter { Type = StatesFilterType.none }
                }
            },
            Xaxis = new XAxis
            {
                Type = XAxisType.Datetime,
                Labels = new XAxisLabels
                {
                    Style = new AxisLabelStyle { Colors = "#9ca3af" },
                    DatetimeUTC = false
                },
                Crosshairs = new AxisCrosshairs { Show = false },
                Tooltip = new AxisTooltip { Enabled = false }
            },
            Yaxis = new List<YAxis>
            {
                new YAxis
                {
                    Title = yAxisTitle == null
                        ? null
                        : new AxisTitle { Text = yAxisTitle, Style = new AxisTitleStyle { Color = "#9ca3af" } },
                    Labels = new YAxisLabels
                    {
                        Style = new AxisLabelStyle { Colors = "#9ca3af" },
                        Formatter = kwFormatter
                            ? "function(val) { return Math.round(val) + ' kW'; }"
                            : null
                    }
                }
            },
            Grid = new Grid
            {
                Show = true,
                BorderColor = darkTheme ? "#374151" : "rgba(255,255,255,0.08)",
                StrokeDashArray = darkTheme ? 4 : 0
            },
            DataLabels = new DataLabels { Enabled = false },
            Legend = new Legend
            {
                Show = true,
                Position = LegendPosition.Top,
                HorizontalAlign = Align.Center,
                Labels = new LegendLabels { Colors = "#e5e7eb" }
            },
            Tooltip = new ApexCharts.Tooltip { Enabled = false }
        };

        return options;
    }
}
