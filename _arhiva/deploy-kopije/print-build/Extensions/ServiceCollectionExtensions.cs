using Microsoft.AspNetCore.Components.Web;
using ApexCharts;
using FruitSysWeb.Components.Charts;
using FruitSysWeb.Services;
using FruitSysWeb.Services.Interfaces;
using FruitSysWeb.Services.Implementations;
using FruitSysWeb.Services.Implementations.IzvestajService;
using FruitSysWeb.Services.Implementations.ExportService;
using FruitSysWeb.Services.Core;
using FruitSysWeb.Services.Print;
using Microsoft.AspNetCore.Components;
using DocumentFormat.OpenXml.Spreadsheet;
using Blazor_ApexCharts;

namespace FruitSysWeb.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFruitSysServices(this IServiceCollection services, IConfiguration configuration)
        {
            // ========================================
            // DATABASE & CORE SERVICES
            // ========================================
            services.AddScoped<DatabaseService>();
            services.AddScoped<ITypeMappingService, TypeMappingService>();
            services.AddSingleton<CacheService>();  // Singleton for shared cache
            services.AddSingleton<KesaSelekcijaService>();  // Singleton for global selection

            // ========================================
            // AUTHENTICATION & AUTHORIZATION
            // ========================================
            services.AddHttpContextAccessor();
            services.AddSingleton<IKorisnikAktivnostService, KorisnikAktivnostService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ILocalStorageService, LocalStorageService>();

            // ========================================
            // DASHBOARD & REPORTING SERVICES
            // ========================================
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IBrziPregledService, BrziPregledService>();

            // ========================================
            // BUSINESS LOGIC SERVICES
            // ========================================
            services.AddScoped<IProizvodnjaService, ProizvodnjaService>();
            services.AddScoped<IFinansijeService, FinansijeService>();
            services.AddScoped<IMagacinLagerService, MagacinLagerService>();
            services.AddScoped<IPreradaService, PreradaService>();
            services.AddScoped<IUlazIzlazService, UlazIzlazService>();
            services.AddScoped<IPaletniListService, PaletniListService>();
            services.AddScoped<IUgovorService, UgovorService>();
            services.AddScoped<IPovratnaAmbalazaService, PovratnaAmbalazaService>();

            // ========================================
            // MASTER DATA SERVICES
            // ========================================
            services.AddScoped<IKomitentService, KomitentService>();
            services.AddScoped<IArtikalService, ArtikalService>();
            services.AddScoped<IArtikalKlasifikacijaService, ArtikalKlasifikacijaService>();

            // ========================================
            // EXPORT SERVICES
            // ========================================
            services.AddScoped<IExportService, SimpleExportService>();

            // ========================================
            // FAKTURA SERVICES
            // ========================================
            services.AddScoped<IFakturaService, FakturaService>();
            services.AddScoped<FakturaPdfService>();
            services.AddScoped<FakturaExcelService>();
            services.AddScoped<IKarticaKomitentaService, KarticaKomitentaService>();
            services.AddScoped<IKontrolaService, KontrolaService>();
            services.AddScoped<IIzvodDokumenataService, IzvodDokumenataService>();
            services.AddScoped<PrijemnicaPdfService>();
            services.AddScoped<OtpremnicaPdfService>();
            services.AddScoped<RadniNalogIzvodPdfService>();

            // ========================================
            // PROMENE SERVICES
            // ========================================
            services.AddScoped<IPromenService, PromenService>();
            services.AddScoped<PromeneExcelService>();
            services.AddScoped<PromenePdfService>();

            // ========================================
            // REKLAMACIJE
            // ========================================
            services.AddSingleton<IReklamacijaService, ReklamacijaService>();

            // ========================================
            // SLEDLJIVOST (TRACEABILITY) SERVICES
            // ========================================
            services.AddScoped<ISledljivostService, SledljivostService>();
            services.AddScoped<SledljivostExcelService>();
            services.AddScoped<SledljivostHtmlService>();


            services.AddScoped<OtkupniListPdfService>();
            services.AddScoped<PaletniListPdfService>();

            // PRINT API (desktop štampa dokumenata)
            services.Configure<PrintApiOptions>(configuration.GetSection(PrintApiOptions.SectionName));
            services.AddScoped<IPrintTokenService, PrintTokenService>();
            services.AddScoped<IDokumentPdfStore, DokumentPdfStore>();
            services.AddScoped<IPrintDocumentService, PrintDocumentService>();

            return services;
        }
    }
}
