using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using FruitSysWeb.Models.Solar;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;

namespace FruitSysWeb.Services.Solar;

/// <summary>
/// Klijent za neslužbeni FusionSolar web portal (Huawei).
/// Login stranica enkriptuje šifru u browseru (RSA/SM2 preko javnog ključa sa /unisso/pubkey)
/// pa se ne može replicirati čistim HTTP pozivom — zato koristimo headless Chromium (Playwright)
/// da se stvarno uloguje kroz njihov UI, pa dalje podatke vučemo preko iste sesije (cookies + roarand token).
/// Vidi FusionSolar_API_Uputstvo.md za kontekst i Dokumentacija/ za detalje reversovanog API-ja.
/// </summary>
public class FusionSolarClient : IAsyncDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly FusionSolarOptions _options;
    private readonly ILogger<FusionSolarClient> _logger;
    private readonly SemaphoreSlim _loginLock = new(1, 1);

    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IBrowserContext? _context;
    private string? _roarand;
    private DateTime _lastLoginUtc = DateTime.MinValue;

    private static readonly TimeSpan SessionLifetime = TimeSpan.FromMinutes(3);

    public FusionSolarClient(IOptions<FusionSolarOptions> options, ILogger<FusionSolarClient> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    private bool SessionValid => _context != null && DateTime.UtcNow - _lastLoginUtc < SessionLifetime;

    private async Task EnsureLoggedInAsync(CancellationToken ct)
    {
        if (SessionValid)
        {
            return;
        }

        await _loginLock.WaitAsync(ct);
        try
        {
            if (SessionValid)
            {
                return;
            }

            await LoginAsync(ct);
        }
        finally
        {
            _loginLock.Release();
        }
    }

    private async Task<IBrowser> GetBrowserAsync()
    {
        if (_browser != null)
        {
            return _browser;
        }

        _playwright ??= await Playwright.CreateAsync();

        try
        {
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        }
        catch (PlaywrightException ex) when (ex.Message.Contains("Executable doesn't exist", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Chromium za Playwright nije instaliran, pokušavam automatsku instalaciju (samo prvi put)...");
            var installExitCode = Microsoft.Playwright.Program.Main(new[] { "install", "chromium" });
            if (installExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"Automatska instalacija Chromium-a za Playwright nije uspela (exit code {installExitCode}). " +
                    "Pokreni ručno: dotnet build pa 'pwsh bin/Debug/net8.0/playwright.ps1 install chromium'.");
            }
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        }

        return _browser;
    }

    private async Task LoginAsync(CancellationToken ct)
    {
        if (!_options.IsConfigured)
        {
            throw new InvalidOperationException(
                "FusionSolar nije konfigurisan (Username/Password/StationDn) — proveri appsettings ili user-secrets.");
        }

        var browser = await GetBrowserAsync();

        if (_context != null)
        {
            await _context.CloseAsync();
            _context = null;
        }

        var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();

        try
        {
            var loginUrl = $"{_options.BaseUrl}/uniportal/pvmswebsite/assets/build/cloud.html?app-id=smartpvms&instance-id=smartpvms";
            await page.GotoAsync(loginUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle, Timeout = 30000 });

            await page.FillAsync("#username", _options.Username, new PageFillOptions { Timeout = 15000 });
            await page.FillAsync("#value", _options.Password, new PageFillOptions { Timeout = 15000 });
            await page.ClickAsync("#btn_outerverify", new PageClickOptions { Timeout = 15000 });

            await page.WaitForURLAsync(url => !url.Contains("login.action"), new PageWaitForURLOptions { Timeout = 20000 });
            await page.WaitForTimeoutAsync(2000);
        }
        finally
        {
            await page.CloseAsync();
        }

        var keepAliveResp = await context.APIRequest.GetAsync($"{_options.BaseUrl}/rest/dpcloud/auth/v1/keep-alive");
        if (!keepAliveResp.Ok)
        {
            await context.CloseAsync();
            throw new InvalidOperationException($"FusionSolar login neuspešan — keep-alive vratio {keepAliveResp.Status}.");
        }

        var keepAlive = JsonSerializer.Deserialize<KeepAliveResponse>(await keepAliveResp.TextAsync(), JsonOptions);
        if (string.IsNullOrEmpty(keepAlive?.Payload))
        {
            await context.CloseAsync();
            throw new InvalidOperationException("FusionSolar login neuspešan — keep-alive nije vratio roarand token.");
        }

        _context = context;
        _roarand = keepAlive.Payload;
        _lastLoginUtc = DateTime.UtcNow;
        _logger.LogInformation("FusionSolar login uspešan za korisnika {User}", _options.Username);
    }

    /// <summary>
    /// Osigurava validnu sesiju i osvežava "roarand" token preko keep-alive poziva.
    /// Poziva se jednom po poll ciklusu (ne po svakom pojedinačnom API pozivu).
    /// </summary>
    public async Task RefreshSessionAsync(CancellationToken ct = default)
    {
        await EnsureLoggedInAsync(ct);

        var resp = await _context!.APIRequest.GetAsync($"{_options.BaseUrl}/rest/dpcloud/auth/v1/keep-alive");
        if (!resp.Ok)
        {
            return;
        }

        var keepAlive = JsonSerializer.Deserialize<KeepAliveResponse>(await resp.TextAsync(), JsonOptions);
        if (!string.IsNullOrEmpty(keepAlive?.Payload))
        {
            _roarand = keepAlive.Payload;
        }
    }

    private async Task<string?> GetJsonAsync(string url, CancellationToken ct)
    {
        await EnsureLoggedInAsync(ct);

        var resp = await _context!.APIRequest.GetAsync(url,
            new APIRequestContextOptions { Headers = new Dictionary<string, string> { ["roarand"] = _roarand ?? string.Empty } });

        if (!resp.Ok)
        {
            var body = await resp.TextAsync();
            _logger.LogWarning("FusionSolar GET {Url} vratio {Status}, sesija se resetuje. Body: {Body}",
                url, resp.Status, body.Length > 500 ? body[..500] : body);
            _lastLoginUtc = DateTime.MinValue;
            return null;
        }

        return await resp.TextAsync();
    }

    public async Task<StationSummary?> GetStationSummaryAsync(CancellationToken ct = default)
    {
        await EnsureLoggedInAsync(ct);

        var resp = await _context!.APIRequest.PostAsync(
            $"{_options.BaseUrl}/rest/pvms/web/station/v1/station/station-list",
            new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string> { ["roarand"] = _roarand ?? string.Empty },
                DataObject = new
                {
                    curPage = 1,
                    pageSize = 10,
                    gridConnectedTime = "",
                    queryTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    timeZone = 2,
                    sortId = "createTime",
                    sortDir = "DESC",
                    locale = "en_US"
                }
            });

        if (!resp.Ok)
        {
            _logger.LogWarning("FusionSolar station-list vratio {Status}, sesija se resetuje.", resp.Status);
            _lastLoginUtc = DateTime.MinValue;
            return null;
        }

        var text = await resp.TextAsync();
        var envelope = JsonSerializer.Deserialize<StationListResponse>(text, JsonOptions);
        var station = envelope?.Data?.List?.FirstOrDefault(s => s.Dn == _options.StationDn)
                      ?? envelope?.Data?.List?.FirstOrDefault();

        if (station == null)
        {
            _logger.LogWarning("FusionSolar station-list ne sadrži postrojenje {StationDn}.", _options.StationDn);
        }

        return station;
    }

    public async Task<StationStatusCount?> GetStationStatusCountAsync(CancellationToken ct = default)
    {
        var text = await GetJsonAsync(
            $"{_options.BaseUrl}/rest/pvms/web/station/v1/station/station-status-count?supportMDevice=1", ct);
        if (text == null)
        {
            return null;
        }

        var envelope = JsonSerializer.Deserialize<StationStatusCountResponse>(text, JsonOptions);
        return envelope?.Data;
    }

    public async Task<MessageCount?> GetMessageCountAsync(CancellationToken ct = default)
    {
        var text = await GetJsonAsync($"{_options.BaseUrl}/rest/dp/pvms/message/v1/message-count", ct);
        return text == null ? null : JsonSerializer.Deserialize<MessageCount>(text, JsonOptions);
    }

    // mocId vrednosti u energy-flow grafu (potvrđeno na Odetta postrojenju)
    private const int MocIdPv = 20812;
    private const int MocIdMeter = 20816;
    private const int MocIdGrid = 90001;
    private const int MocIdLoad = 90002;

    /// <summary>
    /// Pravi trenutni tok energije: PV proizvodnja, potrošnja hale (load) i povlačenje/predaja u mrežu.
    /// Za razliku od dailyXxxEnergy polja (dnevni kumulativi), ovo je snapshot "upravo sad".
    /// </summary>
    public async Task<SolarEnergyFlow?> GetEnergyFlowAsync(CancellationToken ct = default)
    {
        var text = await GetJsonAsync(
            $"{_options.BaseUrl}/rest/pvms/web/station/v3/overview/energy-flow?stationDn={Uri.EscapeDataString(_options.StationDn)}", ct);
        if (text == null)
        {
            return null;
        }

        var envelope = JsonSerializer.Deserialize<EnergyFlowResponse>(text, JsonOptions);
        var nodes = envelope?.Data?.Flow?.Nodes;
        var links = envelope?.Data?.Flow?.Links;
        if (nodes == null || links == null)
        {
            return null;
        }

        var pvPower = nodes.FirstOrDefault(n => n.MocId == MocIdPv)?.Value;
        var loadPower = nodes.FirstOrDefault(n => n.MocId == MocIdLoad)?.Value;

        var gridNodeId = nodes.FirstOrDefault(n => n.MocId == MocIdGrid)?.Id;
        var meterNodeId = nodes.FirstOrDefault(n => n.MocId == MocIdMeter)?.Id;

        decimal? gridPower = null;
        if (gridNodeId != null && meterNodeId != null)
        {
            foreach (var link in links)
            {
                var value = ParseKw(link.Description?.Value);
                if (value == null)
                {
                    continue;
                }

                if (link.FromNode == gridNodeId && link.ToNode == meterNodeId)
                {
                    gridPower = value; // povlači se iz mreže
                    break;
                }
                if (link.FromNode == meterNodeId && link.ToNode == gridNodeId)
                {
                    gridPower = -value; // predaje se u mrežu
                    break;
                }
            }
        }

        return new SolarEnergyFlow { PvPower = pvPower, LoadPower = loadPower, GridPower = gridPower };
    }

    /// <summary>
    /// 5-min istorija signala za jedan dan.
    /// FusionSolar web: GET sa deviceDn, signalIds i date (epoch ms početka lokalnog dana) u query stringu.
    /// </summary>
    public async Task<DeviceHistoryResponse?> GetDeviceDayHistoryAsync(
        string deviceDn,
        IEnumerable<int> signalIds,
        DateTime date,
        CancellationToken ct = default)
    {
        var ids = signalIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return null;
        }

        await EnsureLoggedInAsync(ct);

        var dayTimestamp = ToFusionSolarDayTimestamp(date);
        var query = string.Join("&", ids.Select(id => $"signalIds={id}"))
                    + $"&deviceDn={Uri.EscapeDataString(deviceDn)}"
                    + $"&date={dayTimestamp}";
        var url = $"{_options.BaseUrl}/rest/pvms/web/device/v1/device-history-data?showDst=true&{query}";

        var resp = await _context!.APIRequest.GetAsync(url,
            new APIRequestContextOptions { Headers = new Dictionary<string, string> { ["roarand"] = _roarand ?? string.Empty } });

        if (!resp.Ok)
        {
            var body = await resp.TextAsync();
            _logger.LogWarning("FusionSolar device-history {Device} vratio {Status}: {Body}",
                deviceDn, resp.Status, body.Length > 200 ? body[..200] : body);
            return null;
        }

        return ParseDeviceHistoryResponse(await resp.TextAsync(), ids);
    }

    /// <summary>
    /// Epoch ms za FusionSolar device-history date parametar.
    /// API vraća 24h PRE ovog trenutka — za podatke 4.7. prosledi 5.7. 00:00 lokalno.
    /// </summary>
    public static long ToFusionSolarDayTimestamp(DateTime dateForData)
    {
        var tz = TimeZoneInfo.Local;
        var apiAnchor = DateTime.SpecifyKind(dateForData.Date.AddDays(1), DateTimeKind.Unspecified);
        var utc = TimeZoneInfo.ConvertTimeToUtc(apiAnchor, tz);
        return new DateTimeOffset(utc, TimeSpan.Zero).ToUnixTimeMilliseconds();
    }

    private DeviceHistoryResponse? ParseDeviceHistoryResponse(string json, IReadOnlyList<int> signalIds)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        if (!root.TryGetProperty("success", out var successProp) || !successProp.GetBoolean())
        {
            return null;
        }

        if (!root.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var result = new DeviceHistoryResponse { Success = true };
        foreach (var signalId in signalIds)
        {
            if (!data.TryGetProperty(signalId.ToString(), out var signalNode))
            {
                continue;
            }

            if (!signalNode.TryGetProperty("pmDataList", out var list) || list.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            var points = new List<DeviceHistoryPoint>();
            foreach (var item in list.EnumerateArray())
            {
                if (!item.TryGetProperty("startTime", out var startProp))
                {
                    continue;
                }

                var startSeconds = startProp.GetInt64();
                var timestamp = DateTimeOffset.FromUnixTimeSeconds(startSeconds).UtcDateTime;

                if (!TryParseCounterValue(item, out var value))
                {
                    continue;
                }

                points.Add(new DeviceHistoryPoint { Timestamp = timestamp, Value = value });
            }

            result.Signals[signalId] = points.OrderBy(p => p.Timestamp).ToList();
        }

        return result;
    }

    private static bool TryParseCounterValue(JsonElement item, out decimal value)
    {
        value = 0;
        if (!item.TryGetProperty("counterValue", out var valueProp))
        {
            return false;
        }

        if (valueProp.ValueKind == JsonValueKind.String)
        {
            var s = valueProp.GetString();
            if (string.IsNullOrWhiteSpace(s) || s == "-")
            {
                return false;
            }

            return decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
        }

        if (valueProp.ValueKind != JsonValueKind.Number)
        {
            return false;
        }

        if (!valueProp.TryGetDouble(out var d))
        {
            return false;
        }
        // FusionSolar koristi DBL_MAX kao "nema podatka"
        if (double.IsNaN(d) || double.IsInfinity(d) || d > 1e300)
        {
            return false;
        }

        value = (decimal)d;
        return true;
    }

    private static decimal? ParseKw(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var match = Regex.Match(value, @"[\d.]+");
        return match.Success && decimal.TryParse(match.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
            ? result
            : null;
    }

    public async ValueTask DisposeAsync()
    {
        if (_context != null)
        {
            await _context.CloseAsync();
        }
        if (_browser != null)
        {
            await _browser.CloseAsync();
        }
        _playwright?.Dispose();
    }
}
