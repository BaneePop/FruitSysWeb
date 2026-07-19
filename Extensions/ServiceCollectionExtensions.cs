using FruitSysWeb.Services.Auth;
using FruitSysWeb.Services.Interfaces;
using FruitSysWeb.Services.Implementations;
using FruitSysWeb.Services.Solar;

namespace FruitSysWeb.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOdettaSolarServices(this IServiceCollection services, IConfiguration configuration)
        {
            // ========================================
            // AUTHENTICATION (lokalni SQLite — Data/app.db)
            // ========================================
            services.AddHttpContextAccessor();
            services.AddSingleton<IKorisnikAktivnostService, KorisnikAktivnostService>();
            services.AddSingleton<AuthLocalDbService>();
            services.AddScoped<IAuthService, AuthService>();

            // ========================================
            // SOLAR (FusionSolar — Data/solar.db)
            // ========================================
            services.Configure<FusionSolarOptions>(configuration.GetSection(FusionSolarOptions.SectionName));
            services.AddSingleton<FusionSolarClient>();
            services.AddSingleton<SolarLocalDbService>();
            services.AddSingleton<SolarIntradaySyncService>();
            services.AddHostedService<SolarPollingService>();

            return services;
        }
    }
}
