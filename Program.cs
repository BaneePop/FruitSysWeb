using FruitSysWeb.Services;
using FruitSysWeb.Services.Interfaces;
using FruitSysWeb.Services.Implementations.IzvestajService;
using FruitSysWeb.Services.Implementations;
using FruitSysWeb.Models;
using FruitSysWeb.Extensions; // DODATO: Extension methods
using ApexCharts;
using FruitSysWeb.Services.Core;
using FruitSysWeb.Components.Layout;
using Serilog;
using Microsoft.AspNetCore.HttpOverrides;

// Configure Serilog BEFORE creating the builder
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "Logs/fruitsys-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        fileSizeLimitBytes: 10485760, // 10 MB
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting FruitSysWeb application");

    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog to the builder
    builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// DODANO: ApexCharts.NET servisi - možda nije potrebno u .NET 8 sa @rendermode
// builder.Services.AddApexCharts();

// REFACTORED: Koristimo extension metodu za sve FruitSys servise
builder.Services.AddFruitSysServices();

// OSTALI servisi ostaju isti

// ✅ REFACTORED: Svi servisi su sada u ServiceCollectionExtensions.cs
// Ne treba dodavati servise ovde - sve je u AddFruitSysServices() extension metodi




// DODANO: Konfigurisanje baze podataka ako koristiš EF Core
// builder.Services.AddDbContext<ApplicationDbContext>(options =>
//     options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
//         ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));

// Serilog is already configured via builder.Host.UseSerilog() above
// No need for manual logging configuration here
builder.Services.AddRazorComponents().AddInteractiveServerComponents()
    .AddCircuitOptions(options =>
    {
        options.DetailedErrors = true;
        options.DisconnectedCircuitMaxRetained = 100;
        options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3);
        options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(1);
    });
// DODANO: CORS ako je potreban
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// DODANO: HTTP Client za vanjske servise (ali NE za DashboardService)
builder.Services.AddHttpClient();

// DODANO: Memory cache
builder.Services.AddMemoryCache();

// DODANO: Session ako je potreban
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure forwarded headers (za rad iza IIS reverse proxy-ja)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();

app.UseRouting();

// DODANO: Session middleware ako je potreban
// app.UseSession();

// DODANO: CORS middleware ako je potreban
// app.UseCors();

app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

    // DODATO: Test servisa na startup (opciono)
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var dbService = scope.ServiceProvider.GetRequiredService<DatabaseService>();
            Log.Information("Database service registered successfully");

            // DEBUG: Učitaj sve grupe korisnika iz baze
            var grupe = await dbService.QueryAsync<dynamic>("SELECT ID, Naziv FROM GrupaKorisnika ORDER BY ID");
            Log.Information("=== GRUPE KORISNIKA IZ BAZE ===");
            foreach (var grupa in grupe)
            {
                Log.Information($"ID: {grupa.ID}, Naziv: {grupa.Naziv}");
            }
            Log.Information("=== KRAJ GRUPA ===");

            var exportService = scope.ServiceProvider.GetRequiredService<IExportService>();
            if (exportService is FruitSysWeb.Services.Implementations.ExportService.SimpleExportService simpleExportService)
            {
                var pdfTest = simpleExportService.TestPdfGeneration();
                Log.Information("PDF generation test: {TestResult}", pdfTest ? "PASSED" : "FAILED");
            }

            var dashboardService = scope.ServiceProvider.GetRequiredService<IDashboardService>();
            Log.Information("Dashboard service registered successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Service registration test failed");
        }
    }

    app.Run();
    Log.Information("FruitSysWeb application stopped cleanly");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
