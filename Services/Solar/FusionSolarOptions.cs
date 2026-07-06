namespace FruitSysWeb.Services.Solar;

public class FusionSolarOptions
{
    public const string SectionName = "FusionSolar";

    public string BaseUrl { get; set; } = "https://uni005eu5.fusionsolar.huawei.com";
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string StationDn { get; set; } = string.Empty;
    public int PollIntervalSeconds { get; set; } = 60;

    /// <summary>Uključuje polling i FusionSolar API. Kada je false, stranice koriste samo lokalni solar.db.</summary>
    public bool Enabled { get; set; }

    public bool IsConfigured =>
        Enabled &&
        !string.IsNullOrWhiteSpace(Username) &&
        !string.IsNullOrWhiteSpace(Password) &&
        !string.IsNullOrWhiteSpace(StationDn);
}