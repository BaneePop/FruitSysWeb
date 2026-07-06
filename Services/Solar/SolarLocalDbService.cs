using Dapper;
using FruitSysWeb.Models.Solar;
using Microsoft.Data.Sqlite;

namespace FruitSysWeb.Services.Solar;

/// <summary>
/// Lokalna SQLite baza za solarne podatke (Data/solar.db).
/// Odvojena od fruitsysdb_v2 — solar_kpi ne ide u produkcionu MySQL bazu.
/// </summary>
public class SolarLocalDbService
{
    private readonly string _connectionString;
    private readonly ILogger<SolarLocalDbService> _logger;

    public SolarLocalDbService(IWebHostEnvironment env, ILogger<SolarLocalDbService> logger)
    {
        _logger = logger;

        var dataFolder = Path.Combine(env.ContentRootPath, "Data");
        Directory.CreateDirectory(dataFolder);

        var dbPath = Path.Combine(dataFolder, "solar.db");
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = dbPath,
            Cache = SqliteCacheMode.Shared
        }.ToString();

        InitializeSchema();
    }

    private SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var pragma = connection.CreateCommand();
        pragma.CommandText = "PRAGMA journal_mode=WAL;";
        pragma.ExecuteNonQuery();
        return connection;
    }

    private void InitializeSchema()
    {
        using var connection = OpenConnection();
        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS solar_kpi (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                station_dn TEXT NOT NULL,
                timestamp TEXT NOT NULL,
                current_power REAL,
                daily_energy REAL,
                cumulative_energy REAL,
                month_energy REAL,
                year_energy REAL,
                connected_devices INTEGER,
                disconnected_devices INTEGER,
                trouble_devices INTEGER,
                total_devices INTEGER,
                event_count INTEGER,
                notice_count INTEGER,
                daily_buy_energy REAL,
                daily_on_grid_energy REAL,
                daily_use_energy REAL,
                daily_self_use_energy REAL,
                inverter_power REAL,
                load_power REAL,
                grid_power REAL,
                daily_savings_eur REAL,
                created_at TEXT DEFAULT CURRENT_TIMESTAMP
            );
            CREATE INDEX IF NOT EXISTS idx_solar_kpi_station_time ON solar_kpi(station_dn, timestamp);
        ");

        _logger.LogInformation("Solar SQLite šema inicijalizovana ({Path})", connection.DataSource);
    }

    public async Task InsertKpiAsync(
        string stationDn,
        StationSummary kpi,
        StationStatusCount? status,
        MessageCount? messages,
        SolarEnergyFlow? energyFlow,
        CancellationToken ct = default)
    {
        using var connection = OpenConnection();
        await connection.ExecuteAsync(new CommandDefinition(@"
            INSERT INTO solar_kpi (
                station_dn, timestamp, current_power, daily_energy, cumulative_energy, month_energy, year_energy,
                connected_devices, disconnected_devices, trouble_devices, total_devices, event_count, notice_count,
                daily_buy_energy, daily_on_grid_energy, daily_use_energy, daily_self_use_energy, inverter_power,
                load_power, grid_power)
            VALUES (
                @StationDn, @Timestamp, @CurrentPower, @DailyEnergy, @CumulativeEnergy, @MonthEnergy, @YearEnergy,
                @ConnectedDevices, @DisconnectedDevices, @TroubleDevices, @TotalDevices, @EventCount, @NoticeCount,
                @DailyBuyEnergy, @DailyOnGridEnergy, @DailyUseEnergy, @DailySelfUseEnergy, @InverterPower,
                @LoadPower, @GridPower)",
            new
            {
                StationDn = stationDn,
                Timestamp = DateTime.UtcNow.ToString("O"),
                CurrentPower = ParseDecimal(kpi.CurrentPower),
                DailyEnergy = ParseDecimal(kpi.DailyEnergy),
                CumulativeEnergy = ParseDecimal(kpi.CumulativeEnergy),
                MonthEnergy = ParseDecimal(kpi.MonthEnergy),
                YearEnergy = ParseDecimal(kpi.YearEnergy),
                ConnectedDevices = status?.Connected,
                DisconnectedDevices = status?.Disconnected,
                TroubleDevices = status?.Trouble,
                TotalDevices = status?.Total,
                EventCount = messages?.EventCount,
                NoticeCount = messages?.NoticeCount,
                kpi.DailyBuyEnergy,
                kpi.DailyOnGridEnergy,
                kpi.DailyUseEnergy,
                kpi.DailySelfUseEnergy,
                InverterPower = ParseDecimal(kpi.InverterPower),
                energyFlow?.LoadPower,
                energyFlow?.GridPower
            }, cancellationToken: ct));
    }

    public async Task<SolarKpiRecord?> GetLatestKpiAsync(string stationDn, CancellationToken ct = default)
    {
        using var connection = OpenConnection();
        var row = await connection.QueryFirstOrDefaultAsync<dynamic>(new CommandDefinition(@"
            SELECT id, station_dn, timestamp, current_power, daily_energy, cumulative_energy, month_energy, year_energy,
                   connected_devices, disconnected_devices, trouble_devices, total_devices, event_count, notice_count,
                   daily_buy_energy, daily_on_grid_energy, daily_use_energy, daily_self_use_energy, inverter_power,
                   load_power, grid_power, daily_savings_eur
            FROM solar_kpi
            WHERE station_dn = @StationDn
            ORDER BY timestamp DESC
            LIMIT 1", new { StationDn = stationDn }, cancellationToken: ct));

        if (row == null)
        {
            return null;
        }

        return MapRow(row);
    }

    /// <summary>Sve KPI očitane danas (intraday, lokalni kalendar) — za graf snage kroz dan.</summary>
    public Task<List<SolarKpiRecord>> GetTodayHistoryAsync(string stationDn, CancellationToken ct = default) =>
        GetIntradayForDateAsync(stationDn, DateTime.Today, ct);

    /// <summary>Poslednji red za lokalni dan — dnevni kumulativi do tog trenutka (FusionSolar daily_* polja).</summary>
    public Task<SolarKpiRecord?> GetTodayDailySummaryAsync(string stationDn, CancellationToken ct = default) =>
        GetDailySummaryAsync(stationDn, DateTime.Today, ct);

    /// <summary>
    /// Dnevni total za konkretan datum (poslednji red tog dana — FusionSolar polja su već kumulativi u okviru dana,
    /// pa poslednje očitavanje = dnevni total). Za stare datume (pre live pollinga) vraća backfill red sa samo daily_energy.
    /// </summary>
    public async Task<SolarKpiRecord?> GetDailySummaryAsync(string stationDn, DateTime date, CancellationToken ct = default)
    {
        using var connection = OpenConnection();
        var dateStr = date.ToString("yyyy-MM-dd");
        var row = await connection.QueryFirstOrDefaultAsync<dynamic>(new CommandDefinition(@"
            SELECT id, station_dn, timestamp, current_power, daily_energy, cumulative_energy, month_energy, year_energy,
                   connected_devices, disconnected_devices, trouble_devices, total_devices, event_count, notice_count,
                   daily_buy_energy, daily_on_grid_energy, daily_use_energy, daily_self_use_energy, inverter_power,
                   load_power, grid_power, daily_savings_eur
            FROM solar_kpi
            WHERE station_dn = @StationDn AND date(timestamp, 'localtime') = @Date
            ORDER BY timestamp DESC
            LIMIT 1", new { StationDn = stationDn, Date = dateStr }, cancellationToken: ct));

        return row == null ? null : MapRow(row);
    }

    /// <summary>
    /// Dnevni totali za opseg datuma (od-do) — po jedan red za svaki dan (poslednje očitavanje tog dana).
    /// Koristi se za "Istorija" tab (grafikon/tabela po danima unutar izabranog perioda).
    /// </summary>
    public async Task<List<SolarKpiRecord>> GetDailySummariesForRangeAsync(string stationDn, DateTime od, DateTime doDatuma, CancellationToken ct = default)
    {
        using var connection = OpenConnection();
        var odStr = od.ToString("yyyy-MM-dd");
        var doStr = doDatuma.ToString("yyyy-MM-dd");
        var rows = await connection.QueryAsync<dynamic>(new CommandDefinition(@"
            SELECT s.id, s.station_dn, s.timestamp, s.current_power, s.daily_energy, s.cumulative_energy, s.month_energy, s.year_energy,
                   s.connected_devices, s.disconnected_devices, s.trouble_devices, s.total_devices, s.event_count, s.notice_count,
                   s.daily_buy_energy, s.daily_on_grid_energy, s.daily_use_energy, s.daily_self_use_energy, s.inverter_power,
                   s.load_power, s.grid_power, s.daily_savings_eur
            FROM solar_kpi s
            INNER JOIN (
                SELECT date(timestamp) AS d, MAX(timestamp) AS maxts
                FROM solar_kpi
                WHERE station_dn = @StationDn AND date(timestamp) BETWEEN @Od AND @Do
                GROUP BY d
            ) poslednji ON date(s.timestamp) = poslednji.d AND s.timestamp = poslednji.maxts
            WHERE s.station_dn = @StationDn
            ORDER BY s.timestamp ASC", new { StationDn = stationDn, Od = odStr, Do = doStr }, cancellationToken: ct));

        return rows.Select(MapRow).ToList();
    }

    /// <summary>
    /// Intraday očitavanja (kriva kroz dan) za konkretan datum — postoji samo za datume od kad radi live polling.
    /// Isključuje backfill markere (podne-tačka za stare dane), pa za stare datume vraća praznu listu.
    /// </summary>
    public async Task<List<SolarKpiRecord>> GetIntradayForDateAsync(string stationDn, DateTime date, CancellationToken ct = default)
    {
        using var connection = OpenConnection();
        var dateStr = date.ToString("yyyy-MM-dd");
        var rows = await connection.QueryAsync<dynamic>(new CommandDefinition(@"
            SELECT id, station_dn, timestamp, current_power, daily_energy, cumulative_energy, month_energy, year_energy,
                   connected_devices, disconnected_devices, trouble_devices, total_devices, event_count, notice_count,
                   daily_buy_energy, daily_on_grid_energy, daily_use_energy, daily_self_use_energy, inverter_power,
                   load_power, grid_power, daily_savings_eur
            FROM solar_kpi
            WHERE station_dn = @StationDn
              AND date(timestamp, 'localtime') = @Date
              AND timestamp NOT LIKE '%T12:00:00%'
            ORDER BY timestamp ASC", new { StationDn = stationDn, Date = dateStr }, cancellationToken: ct));

        return rows.Select(MapRow).ToList();
    }

    /// <summary>
    /// Upis intraday tačaka iz FusionSolar istorije — deduplikacija po 5-min slotu (zadržava polling vrednosti ako postoje).
    /// </summary>
    public async Task<int> MergeIntradayHistoryAsync(
        string stationDn,
        IReadOnlyList<SolarKpiRecord> points,
        CancellationToken ct = default)
    {
        if (points.Count == 0)
        {
            return 0;
        }

        using var connection = OpenConnection();
        var inserted = 0;

        foreach (var point in points.OrderBy(p => p.Timestamp))
        {
            var tsLocal = point.Timestamp.Kind == DateTimeKind.Utc
                ? point.Timestamp.ToLocalTime()
                : DateTime.SpecifyKind(point.Timestamp, DateTimeKind.Unspecified);
            tsLocal = new DateTime(tsLocal.Year, tsLocal.Month, tsLocal.Day, tsLocal.Hour, tsLocal.Minute, 0, DateTimeKind.Unspecified);
            var slotEnd = tsLocal.AddMinutes(5);

            var exists = await connection.ExecuteScalarAsync<int>(new CommandDefinition(@"
                SELECT COUNT(1) FROM solar_kpi
                WHERE station_dn = @StationDn
                  AND datetime(timestamp, 'localtime') >= @SlotStart
                  AND datetime(timestamp, 'localtime') < @SlotEnd
                  AND timestamp NOT LIKE '%T12:00:00%'",
                new
                {
                    StationDn = stationDn,
                    SlotStart = tsLocal.ToString("yyyy-MM-dd HH:mm:ss"),
                    SlotEnd = slotEnd.ToString("yyyy-MM-dd HH:mm:ss")
                }, cancellationToken: ct));

            if (exists > 0)
            {
                continue;
            }

            var tsUtc = DateTime.SpecifyKind(tsLocal, DateTimeKind.Local);
            tsUtc = tsUtc.ToUniversalTime();
            await connection.ExecuteAsync(new CommandDefinition(@"
                INSERT INTO solar_kpi (
                    station_dn, timestamp, current_power, load_power, grid_power)
                VALUES (
                    @StationDn, @Timestamp, @CurrentPower, @LoadPower, @GridPower)",
                new
                {
                    StationDn = stationDn,
                    Timestamp = tsUtc.ToString("O"),
                    point.CurrentPower,
                    point.LoadPower,
                    point.GridPower
                }, cancellationToken: ct));

            inserted++;
        }

        return inserted;
    }

    /// <summary>Kompletna istorija ukupne proizvodnje (cumulative_energy) — uključuje i backfill iz AppLog-a.</summary>
    public async Task<List<SolarKpiRecord>> GetCumulativeHistoryAsync(string stationDn, CancellationToken ct = default)
    {
        using var connection = OpenConnection();
        var rows = await connection.QueryAsync<dynamic>(new CommandDefinition(@"
            SELECT id, station_dn, timestamp, current_power, daily_energy, cumulative_energy, month_energy, year_energy,
                   connected_devices, disconnected_devices, trouble_devices, total_devices, event_count, notice_count,
                   daily_buy_energy, daily_on_grid_energy, daily_use_energy, daily_self_use_energy, inverter_power,
                   load_power, grid_power, daily_savings_eur
            FROM solar_kpi
            WHERE station_dn = @StationDn AND cumulative_energy IS NOT NULL
            ORDER BY timestamp ASC", new { StationDn = stationDn }, cancellationToken: ct));

        return rows.Select(MapRow).ToList();
    }

    /// <summary>Dnevna proizvodnja (daily_energy) po danima — uključuje backfill iz FusionSolar "Korist PV-a" izveštaja.</summary>
    public async Task<List<SolarKpiRecord>> GetDailyEnergyHistoryAsync(string stationDn, CancellationToken ct = default)
    {
        using var connection = OpenConnection();
        var rows = await connection.QueryAsync<dynamic>(new CommandDefinition(@"
            SELECT id, station_dn, timestamp, current_power, daily_energy, cumulative_energy, month_energy, year_energy,
                   connected_devices, disconnected_devices, trouble_devices, total_devices, event_count, notice_count,
                   daily_buy_energy, daily_on_grid_energy, daily_use_energy, daily_self_use_energy, inverter_power,
                   load_power, grid_power, daily_savings_eur
            FROM solar_kpi
            WHERE station_dn = @StationDn AND daily_energy IS NOT NULL AND timestamp LIKE '____-__-__T12:00:00%'
            ORDER BY timestamp ASC", new { StationDn = stationDn }, cancellationToken: ct));

        return rows.Select(MapRow).ToList();
    }

    /// <summary>Dnevna ušteda (EUR) po danima — iz FusionSolar "Ukupna korist" izveštaja.</summary>
    public async Task<List<SolarKpiRecord>> GetDailySavingsHistoryAsync(string stationDn, CancellationToken ct = default)
    {
        using var connection = OpenConnection();
        var rows = await connection.QueryAsync<dynamic>(new CommandDefinition(@"
            SELECT id, station_dn, timestamp, current_power, daily_energy, cumulative_energy, month_energy, year_energy,
                   connected_devices, disconnected_devices, trouble_devices, total_devices, event_count, notice_count,
                   daily_buy_energy, daily_on_grid_energy, daily_use_energy, daily_self_use_energy, inverter_power,
                   load_power, grid_power, daily_savings_eur
            FROM solar_kpi
            WHERE station_dn = @StationDn AND daily_savings_eur IS NOT NULL AND timestamp LIKE '____-__-__T12:00:00%'
            ORDER BY timestamp ASC", new { StationDn = stationDn }, cancellationToken: ct));

        return rows.Select(MapRow).ToList();
    }

    private static SolarKpiRecord MapRow(dynamic row) => new()
    {
        Id = (int)(long)row.id,
        StationDn = (string)row.station_dn,
        Timestamp = DateTime.Parse((string)row.timestamp),
        CurrentPower = row.current_power == null ? null : (decimal?)(double)row.current_power,
        DailyEnergy = row.daily_energy == null ? null : (decimal?)(double)row.daily_energy,
        CumulativeEnergy = row.cumulative_energy == null ? null : (decimal?)(double)row.cumulative_energy,
        MonthEnergy = row.month_energy == null ? null : (decimal?)(double)row.month_energy,
        YearEnergy = row.year_energy == null ? null : (decimal?)(double)row.year_energy,
        ConnectedDevices = row.connected_devices == null ? null : (int?)(long)row.connected_devices,
        DisconnectedDevices = row.disconnected_devices == null ? null : (int?)(long)row.disconnected_devices,
        TroubleDevices = row.trouble_devices == null ? null : (int?)(long)row.trouble_devices,
        TotalDevices = row.total_devices == null ? null : (int?)(long)row.total_devices,
        EventCount = row.event_count == null ? null : (int?)(long)row.event_count,
        NoticeCount = row.notice_count == null ? null : (int?)(long)row.notice_count,
        DailyBuyEnergy = row.daily_buy_energy == null ? null : (decimal?)(double)row.daily_buy_energy,
        DailyOnGridEnergy = row.daily_on_grid_energy == null ? null : (decimal?)(double)row.daily_on_grid_energy,
        DailyUseEnergy = row.daily_use_energy == null ? null : (decimal?)(double)row.daily_use_energy,
        DailySelfUseEnergy = row.daily_self_use_energy == null ? null : (decimal?)(double)row.daily_self_use_energy,
        InverterPower = row.inverter_power == null ? null : (decimal?)(double)row.inverter_power,
        LoadPower = row.load_power == null ? null : (decimal?)(double)row.load_power,
        GridPower = row.grid_power == null ? null : (decimal?)(double)row.grid_power,
        DailySavingsEur = row.daily_savings_eur == null ? null : (decimal?)(double)row.daily_savings_eur
    };

    private static decimal? ParseDecimal(string? value) =>
        decimal.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var result)
            ? result
            : null;
}
