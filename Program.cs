using FruitSysWeb.Services;
using FruitSysWeb.Services.Interfaces;
using FruitSysWeb.Services.Implementations.IzvestajService;
using FruitSysWeb.Services.Implementations.ExportService;
using FruitSysWeb.Models;
using ApexCharts;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// DODANO: ApexCharts.NET servisi - možda nije potrebno u .NET 8 sa @rendermode
// builder.Services.AddApexCharts();

// POSTOJEĆI servisi
builder.Services.AddScoped<DatabaseService>();

// AŽURIRANE registracije postojećih servisa
builder.Services.AddScoped<IProizvodnjaService, FruitSysWeb.Services.Implementations.IzvestajService.ProizvodnjaService>();
builder.Services.AddScoped<IFinansijeService, FruitSysWeb.Services.Implementations.IzvestajService.FinansijeService>();
builder.Services.AddScoped<IMagacinLagerService, FruitSysWeb.Services.Implementations.IzvestajService.MagacinLagerService>();
builder.Services.AddScoped<IExportService, FruitSysWeb.Services.Implementations.ExportService.SimpleExportService>();
builder.Services.AddScoped<IKomitentService, KomitentService>();
builder.Services.AddScoped<IArtikalService, ArtikalService>();

// DODANO: Registracija ArtikalKlasifikacijaService
builder.Services.AddScoped<IArtikalKlasifikacijaService, ArtikalKlasifikacijaService>();

// ISPRAVLJENA registracija DashboardService - bez HttpClient
builder.Services.AddScoped<IDashboardService, DashboardService>();

// DODANO: Konfigurisanje baze podataka ako koristiš EF Core
// builder.Services.AddDbContext<ApplicationDbContext>(options =>
//     options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
//         ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));

// DODATO: Logging konfigurisanje
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Services.AddRazorComponents().AddInteractiveServerComponents()
    .AddCircuitOptions(options => {
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

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// DODANO: Session middleware ako je potreban
// app.UseSession();

// DODANO: CORS middleware ako je potreban
// app.UseCors();

app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

// DODANO: Test servisa na startup (opciono)
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbService = scope.ServiceProvider.GetRequiredService<DatabaseService>();
        Console.WriteLine("Database service registered successfully");
        
        var exportService = scope.ServiceProvider.GetRequiredService<IExportService>();
        if (exportService is FruitSysWeb.Services.Implementations.ExportService.SimpleExportService simpleExportService)
        {
            var pdfTest = simpleExportService.TestPdfGeneration();
            Console.WriteLine($"PDF generation test: {(pdfTest ? "PASSED" : "FAILED")}");
        }
        
        var dashboardService = scope.ServiceProvider.GetRequiredService<IDashboardService>();
        Console.WriteLine("Dashboard service registered successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Service registration test failed: {ex.Message}");
    }
}

app.Run();
