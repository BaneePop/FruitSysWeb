using Microsoft.Extensions.Options;

namespace FruitSysWeb.Services.Solar;

/// <summary>
/// Periodično povlači real-time KPI sa FusionSolar-a i upisuje u lokalnu SQLite bazu.
/// Ne radi ništa ako FusionSolar nije konfigurisan (Username/Password/StationDn prazni).
/// </summary>
public class SolarPollingService : BackgroundService
{
    private readonly FusionSolarClient _client;
    private readonly SolarLocalDbService _db;
    private readonly FusionSolarOptions _options;
    private readonly ILogger<SolarPollingService> _logger;

    public SolarPollingService(
        FusionSolarClient client,
        SolarLocalDbService db,
        IOptions<FusionSolarOptions> options,
        ILogger<SolarPollingService> logger)
    {
        _client = client;
        _db = db;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.IsConfigured)
        {
            _logger.LogWarning(
                "FusionSolar polling preskočen — nedostaje konfiguracija (Username/Password/StationDn) u appsettings/user-secrets.");
            return;
        }

        var interval = TimeSpan.FromSeconds(Math.Max(_options.PollIntervalSeconds, 3));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _client.RefreshSessionAsync(stoppingToken);

                var kpi = await _client.GetStationSummaryAsync(stoppingToken);
                var status = await _client.GetStationStatusCountAsync(stoppingToken);
                var messages = await _client.GetMessageCountAsync(stoppingToken);
                var energyFlow = await _client.GetEnergyFlowAsync(stoppingToken);

                if (kpi != null)
                {
                    await _db.InsertKpiAsync(_options.StationDn, kpi, status, messages, energyFlow, stoppingToken);
                    _logger.LogInformation(
                        "Solar KPI upisan: PV {Pv} kW, hala {Load} kW, mreža {Grid} kW, uređaji {Connected}/{Total} connected",
                        kpi.CurrentPower, energyFlow?.LoadPower, energyFlow?.GridPower,
                        status?.Connected, status?.Total);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri FusionSolar polling-u");
            }

            try
            {
                await Task.Delay(interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}