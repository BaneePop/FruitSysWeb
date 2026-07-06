using System.Text.Json.Serialization;

namespace FruitSysWeb.Models.Solar;

/// <summary>
/// Odgovor na GET /rest/dpcloud/auth/v1/keep-alive.
/// "Payload" je zapravo "roarand" CSRF token koji mora ići na sve dalje API pozive.
/// </summary>
public class KeepAliveResponse
{
    public int Code { get; set; }
    public string? Message { get; set; }
    public string? Payload { get; set; }
}

public class StationListResponse
{
    public StationListData? Data { get; set; }
    public bool? Success { get; set; }
}

public class StationListData
{
    public int PageCount { get; set; }
    public int Total { get; set; }
    public List<StationSummary> List { get; set; } = new();
}

/// <summary>
/// Jedan red iz POST /rest/pvms/web/station/v1/station/station-list — daje sve ključne KPI odjednom
/// (currentPower/dailyEnergy/cumulativeEnergy...), pa poseban "total-real-kpi" poziv nije ni potreban.
/// </summary>
public class StationSummary
{
    [JsonPropertyName("dn")]
    public string? Dn { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("plantStatus")]
    public string? PlantStatus { get; set; }

    [JsonPropertyName("currentPower")]
    public string? CurrentPower { get; set; }

    [JsonPropertyName("dailyEnergy")]
    public string? DailyEnergy { get; set; }

    [JsonPropertyName("monthEnergy")]
    public string? MonthEnergy { get; set; }

    [JsonPropertyName("yearEnergy")]
    public string? YearEnergy { get; set; }

    [JsonPropertyName("cumulativeEnergy")]
    public string? CumulativeEnergy { get; set; }

    [JsonPropertyName("installedCapacity")]
    public string? InstalledCapacity { get; set; }

    [JsonPropertyName("plantAddress")]
    public string? PlantAddress { get; set; }

    [JsonPropertyName("dailyBuyEnergy")]
    public decimal? DailyBuyEnergy { get; set; }

    [JsonPropertyName("dailyOnGridEnergy")]
    public decimal? DailyOnGridEnergy { get; set; }

    [JsonPropertyName("dailyUseEnergy")]
    public decimal? DailyUseEnergy { get; set; }

    [JsonPropertyName("dailySelfUseEnergy")]
    public decimal? DailySelfUseEnergy { get; set; }

    [JsonPropertyName("inverterPower")]
    public string? InverterPower { get; set; }

    [JsonPropertyName("systemEfficiency")]
    public string? SystemEfficiency { get; set; }
}

public class StationStatusCountResponse
{
    public StationStatusCount? Data { get; set; }
}

/// <summary>GET /rest/pvms/web/station/v1/station/station-status-count?supportMDevice=1</summary>
public class StationStatusCount
{
    public int Trouble { get; set; }
    public int Disconnected { get; set; }
    public int Connected { get; set; }
    public int Total { get; set; }
}

/// <summary>GET /rest/dp/pvms/message/v1/message-count — bez "data" omotača, polja direktno u root-u.</summary>
public class MessageCount
{
    public int NoticeCount { get; set; }
    public int ToDoListCount { get; set; }
    public int EventCount { get; set; }
    public int UpgradeCount { get; set; }
    public int UpgradeStationCount { get; set; }
    public int OpMsgCount { get; set; }
}

/// <summary>GET /rest/pvms/web/station/v3/overview/energy-flow?stationDn=... — pun graf čvorova/veza.</summary>
public class EnergyFlowResponse
{
    public bool Success { get; set; }
    public EnergyFlowData? Data { get; set; }
}

public class EnergyFlowData
{
    public EnergyFlowGraph? Flow { get; set; }
}

public class EnergyFlowGraph
{
    public List<EnergyFlowNode> Nodes { get; set; } = new();
    public List<EnergyFlowLink> Links { get; set; } = new();
}

public class EnergyFlowNode
{
    public int MocId { get; set; }
    public string Id { get; set; } = string.Empty;
    public decimal? Value { get; set; }
}

public class EnergyFlowLink
{
    public string FromNode { get; set; } = string.Empty;
    public string ToNode { get; set; } = string.Empty;
    public EnergyFlowLinkDescription? Description { get; set; }
}

public class EnergyFlowLinkDescription
{
    public string? Value { get; set; }
}

/// <summary>
/// Parsirani tok energije u ovom trenutku: PV (mocId 20812) → Meter (20816) ← Grid (90001), Load (90002).
/// GridPower: pozitivno = povlači se iz mreže, negativno = predaje se u mrežu.
/// </summary>
public class SolarEnergyFlow
{
    public decimal? PvPower { get; set; }
    public decimal? LoadPower { get; set; }
    public decimal? GridPower { get; set; }
}

public class DeviceHistoryPoint
{
    public DateTime Timestamp { get; set; }
    public decimal Value { get; set; }
}

public class DeviceHistoryResponse
{
    public bool Success { get; set; }
    public Dictionary<int, List<DeviceHistoryPoint>> Signals { get; set; } = new();
}

/// <summary>Snimljen KPI red iz solar_kpi tabele (SQLite).</summary>
public class SolarKpiRecord
{
    public int Id { get; set; }
    public string StationDn { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public decimal? CurrentPower { get; set; }
    public decimal? DailyEnergy { get; set; }
    public decimal? CumulativeEnergy { get; set; }
    public decimal? MonthEnergy { get; set; }
    public decimal? YearEnergy { get; set; }
    public int? ConnectedDevices { get; set; }
    public int? DisconnectedDevices { get; set; }
    public int? TroubleDevices { get; set; }
    public int? TotalDevices { get; set; }
    public int? EventCount { get; set; }
    public int? NoticeCount { get; set; }
    public decimal? DailyBuyEnergy { get; set; }
    public decimal? DailyOnGridEnergy { get; set; }
    public decimal? DailyUseEnergy { get; set; }
    public decimal? DailySelfUseEnergy { get; set; }
    public decimal? InverterPower { get; set; }
    public decimal? LoadPower { get; set; }
    public decimal? GridPower { get; set; }
    public decimal? DailySavingsEur { get; set; }
}
