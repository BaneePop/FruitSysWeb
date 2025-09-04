using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FruitSysWeb.Services;
using MySqlConnector;
using Microsoft.Extensions.Configuration;
using FruitSysWeb.Services.Interfaces;
using FruitSysWeb.Services.Implementations.IzvestajService;
using FruitSysWeb.Services.Implementations.ExportService;




var builder = WebApplication.CreateBuilder(args);



builder.Services.AddScoped<IFinansijeService, FinansijeService>();
builder.Services.AddScoped<IProizvodnjaService, ProizvodnjaService>();
builder.Services.AddScoped<IExportService, ExcelExportService>();



// Dodavanje servisa
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();



builder.Services.AddScoped<IzvestajService>();
builder.Services.AddScoped<ExportService>();


// REGISTRACIJA SERVISA
builder.Services.AddScoped<DatabaseService>();
builder.Services.AddScoped<IzvestajService>();
builder.Services.AddScoped<ArtikalService>();
builder.Services.AddScoped<KomitentService>();
builder.Services.AddScoped<ExportService>();


// Konfiguracija baze podataka - samo connection string
builder.Services.AddTransient<MySqlConnection>(_ =>
    new MySqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));
    

var app = builder.Build();

// Ostali kod ostaje isti...

// Konfiguracija middleware-a
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