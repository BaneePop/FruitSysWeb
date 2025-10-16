using Microsoft.AspNetCore.Components.Web;
using ApexCharts;
using FruitSysWeb.Components.Charts;
using FruitSysWeb.Services;
using FruitSysWeb.Services.Interfaces;
using FruitSysWeb.Services.Implementations.IzvestajService;
using FruitSysWeb.Services.Implementations.ExportService;
using FruitSysWeb.Services.Core; // DODATO: Core services
using Microsoft.AspNetCore.Components;
using DocumentFormat.OpenXml.Spreadsheet;
using Blazor_ApexCharts;

namespace FruitSysWeb.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFruitSysServices(this IServiceCollection services)
        {
            // Database
            services.AddScoped<DatabaseService>();

            // NOVO: Core services - centralizovani mapiranje i helpers
            services.AddScoped<ITypeMappingService, TypeMappingService>();

            // Core services
            services.AddScoped<IProizvodnjaService, ProizvodnjaService>();
            services.AddScoped<IFinansijeService, FinansijeService>();
            services.AddScoped<IMagacinLagerService, MagacinLagerService>();
            services.AddScoped<IKomitentService, KomitentService>();
            services.AddScoped<IArtikalService, ArtikalService>();
            services.AddScoped<IArtikalKlasifikacijaService, ArtikalKlasifikacijaService>();
            services.AddScoped<IExportService, SimpleExportService>();

            // NOVO: Prerada i Ulaz-Izlaz servisi
            services.AddScoped<IPreradaService, PreradaService>();
            services.AddScoped<IUlazIzlazService, UlazIzlazService>();
            
            // NOVO: PaletniList servis za real-time prijem
            services.AddScoped<IPaletniListService, PaletniListService>();

            return services;
        }
    }
}
