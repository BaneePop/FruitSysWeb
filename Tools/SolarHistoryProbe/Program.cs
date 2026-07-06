using FruitSysWeb.Models;
using FruitSysWeb.Services.Solar;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

var services = new ServiceCollection();
services.AddLogging(b => b.AddConsole().SetMinimumLevel(LogLevel.Information));
services.AddSingleton<IWebHostEnvironment>(new WebHostEnvStub("/Users/Bane/FruitSysWeb"));
services.Configure<FusionSolarOptions>(o =>
{
    o.Enabled = true;
    o.BaseUrl = "https://uni005eu5.fusionsolar.huawei.com";
    o.Username = "bane@odetta.rs";
    o.Password = "Rs12sr34";
    o.StationDn = "NE=182162915";
});
services.AddSingleton<FusionSolarClient>();
services.AddSingleton<SolarLocalDbService>();
services.AddSingleton<SolarIntradaySyncService>();

var sp = services.BuildServiceProvider();
var sync = sp.GetRequiredService<SolarIntradaySyncService>();
var db = sp.GetRequiredService<SolarLocalDbService>();
var client = sp.GetRequiredService<FusionSolarClient>();

var today = DateTime.Today;
Console.WriteLine($"Today: {today:yyyy-MM-dd}, timestamp={FusionSolarClient.ToFusionSolarDayTimestamp(today)}");

var before = await db.GetIntradayForDateAsync(SolarPodatakKatalog.StationDn, today);
Console.WriteLine($"Pre sync: {before.Count} tačaka");

var meter = await client.GetDeviceDayHistoryAsync(
    SolarPodatakKatalog.MeterDn,
    new[] { SolarPodatakKatalog.SignalMeterLoadPower },
    today);
Console.WriteLine($"Meter API: {(meter?.Signals.TryGetValue(30034, out var mp) == true ? mp.Count + " tačaka" : "FAIL")}");

var n = await sync.SyncDayAsync(SolarPodatakKatalog.StationDn, today);
Console.WriteLine($"Sync upisao: {n}");

var after = await db.GetIntradayForDateAsync(SolarPodatakKatalog.StationDn, today);
Console.WriteLine($"Posle sync: {after.Count} tačaka, od {after.Min(r=>r.Timestamp).ToLocalTime():HH:mm} do {after.Max(r=>r.Timestamp).ToLocalTime():HH:mm}");

await client.DisposeAsync();

file class WebHostEnvStub(string root) : IWebHostEnvironment
{
    public string WebRootPath { get; set; } = root;
    public IFileProvider WebRootFileProvider { get; set; } = null!;
    public string EnvironmentName { get; set; } = "Development";
    public string ApplicationName { get; set; } = "Test";
    public string ContentRootPath { get; set; } = root;
    public IFileProvider ContentRootFileProvider { get; set; } = null!;
}
