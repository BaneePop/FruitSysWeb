using FruitSysWeb.Models;
using FruitSysWeb.Models.Solar;
using Microsoft.Extensions.Options;

namespace FruitSysWeb.Services.Solar;

/// <summary>
/// Povlači 5-min intraday istoriju iz FusionSolar API-ja i upisuje u solar_kpi (merge sa polling podacima).
/// </summary>
public class SolarIntradaySyncService
{
    private readonly FusionSolarClient _client;
    private readonly SolarLocalDbService _db;
    private readonly FusionSolarOptions _options;
    private readonly ILogger<SolarIntradaySyncService> _logger;
    private readonly SemaphoreSlim _syncLock = new(1, 1);

    public SolarIntradaySyncService(
        FusionSolarClient client,
        SolarLocalDbService db,
        IOptions<FusionSolarOptions> options,
        ILogger<SolarIntradaySyncService> logger)
    {
        _client = client;
        _db = db;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Sinhronizuje intraday za dan ako u bazi nema punog dana (manje od ~100 tačaka sa load_power).
    /// </summary>
    public async Task<int> SyncTodayIfNeededAsync(CancellationToken ct = default)
    {
        if (!_options.IsConfigured)
        {
            return 0;
        }

        var stationDn = string.IsNullOrWhiteSpace(_options.StationDn)
            ? SolarPodatakKatalog.StationDn
            : _options.StationDn;

        var existing = await _db.GetIntradayForDateAsync(stationDn, DateTime.Today, ct);
        if (!NeedsIntradaySync(existing))
        {
            return 0;
        }

        if (!await _syncLock.WaitAsync(0, ct))
        {
            return 0;
        }

        try
        {
            return await SyncDayAsync(stationDn, DateTime.Today, ct);
        }
        finally
        {
            _syncLock.Release();
        }
    }

    public async Task<int> SyncDayAsync(string stationDn, DateTime date, CancellationToken ct = default)
    {
        if (!_options.IsConfigured)
        {
            return 0;
        }

        await _client.RefreshSessionAsync(ct);

        var loadHistory = await _client.GetDeviceDayHistoryAsync(
            SolarPodatakKatalog.MeterDn,
            new[] { SolarPodatakKatalog.SignalMeterLoadPower },
            date,
            ct);

        var solarByTime = new Dictionary<DateTime, decimal>();
        foreach (var inverterDn in SolarPodatakKatalog.InverterDns)
        {
            try
            {
                var invHistory = await _client.GetDeviceDayHistoryAsync(
                    inverterDn,
                    new[] { SolarPodatakKatalog.SignalInverterPower },
                    date,
                    ct);

                if (invHistory?.Signals.TryGetValue(SolarPodatakKatalog.SignalInverterPower, out var points) != true)
                {
                    continue;
                }

                foreach (var point in points)
                {
                    var key = RoundToFiveMinutes(point.Timestamp);
                    solarByTime[key] = solarByTime.GetValueOrDefault(key) + point.Value;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "FusionSolar inverter istorija preskočena za {Device}", inverterDn);
            }
        }

        if (loadHistory?.Signals.TryGetValue(SolarPodatakKatalog.SignalMeterLoadPower, out var loadPoints) != true
            || loadPoints.Count == 0)
        {
            if (solarByTime.Count == 0)
            {
                _logger.LogWarning("FusionSolar intraday sync nema podataka za {Date:yyyy-MM-dd}", date);
                return 0;
            }

            loadPoints = solarByTime.Select(kv => new DeviceHistoryPoint
            {
                Timestamp = kv.Key,
                Value = kv.Value
            }).ToList();
        }

        var records = new List<SolarKpiRecord>();
        foreach (var load in loadPoints)
        {
            var ts = RoundToFiveMinutes(load.Timestamp);
            var solar = solarByTime.GetValueOrDefault(ts);
            var grid = Math.Max(load.Value - solar, 0);

            records.Add(new SolarKpiRecord
            {
                StationDn = stationDn,
                Timestamp = DateTime.SpecifyKind(ts, DateTimeKind.Local),
                CurrentPower = solar,
                LoadPower = load.Value,
                GridPower = grid
            });
        }

        var merged = await _db.MergeIntradayHistoryAsync(stationDn, records, ct);
        _logger.LogInformation(
            "FusionSolar intraday sync za {Date:yyyy-MM-dd}: {Count} tačaka upisano/merge",
            date, merged);

        return merged;
    }

    /// <summary>
    /// Sinhronizuj ako nema dovoljno tačaka ili prva tačka nije blizu ponoći (samo polling od otvaranja app-a).
    /// </summary>
    private static bool NeedsIntradaySync(IReadOnlyList<SolarKpiRecord> existing)
    {
        if (existing.Count == 0)
        {
            return true;
        }

        var earliest = existing.Min(r => r.Timestamp.ToLocalTime());
        if (earliest.TimeOfDay > TimeSpan.FromMinutes(30))
        {
            return true;
        }

        var expectedByNow = (int)(DateTime.Now.TimeOfDay.TotalMinutes / 5) + 1;
        var threshold = Math.Min(100, Math.Max(48, (int)(expectedByNow * 0.6)));
        return existing.Count < threshold;
    }

    private static DateTime RoundToFiveMinutes(DateTime timestamp)
    {
        var local = timestamp.Kind == DateTimeKind.Utc ? timestamp.ToLocalTime() : timestamp;
        var minute = local.Minute - local.Minute % 5;
        return new DateTime(local.Year, local.Month, local.Day, local.Hour, minute, 0, DateTimeKind.Local);
    }
}
