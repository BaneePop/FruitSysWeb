using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FruitSysWeb.Services;
using MySqlConnector;
using Microsoft.Extensions.Configuration;
using FruitSysWeb.Services.Interfaces;
using FruitSysWeb.Services.Implementations.IzvestajService;
using FruitSysWeb.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

// Dodavanje osnovnih servisa
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// CORE SERVIS
builder.Services.AddScoped<DatabaseService>();

// GLAVNI BIZNIS SERVISI - SAMO JEDAN EXPORT SERVIS
builder.Services.AddScoped<IExportService, SimpleExportService>();

// GLAVNI BIZNIS SERVISI - INTERFACE I IMPLEMENTACIJA  
builder.Services.AddScoped<IFinansijeService, FinansijeService>();
builder.Services.AddScoped<IMagacinLagerService, MagacinLagerService>();
builder.Services.AddScoped<IProizvodnjaService, ProizvodnjaService>();
builder.Services.AddScoped<IExportService, SimpleExportService>();


// POMOĆNI SERVISI (bez interface-a)
builder.Services.AddScoped<ArtikalService>();
builder.Services.AddScoped<KomitentService>();
builder.Services.AddScoped<SimpleExportService>();


// UKLONITE OVE DUPLIKATE:
// builder.Services.AddScoped<ExportService>(); // ❌ DUPLIKAT
// builder.Services.AddScoped<IzvestajService>(); // ❌ MOŽDA NIJE POTREBAN

// MySQL konekcija
builder.Services.AddTransient<MySqlConnection>(_ =>
    new MySqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Middleware konfiguracija
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();