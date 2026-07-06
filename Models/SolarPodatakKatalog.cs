namespace FruitSysWeb.Models;

public record SolarPodatakStavka(
    string Kategorija,
    string Izvor,
    string Ucestalost,
    IReadOnlyList<string> Polja,
    IReadOnlyList<string> Grafikoni,
    IReadOnlyList<string> Izvestaji,
    string Status);

public static class SolarPodatakKatalog
{
    public const string Postrojenje = "Odetta";
    public const string StationDn = "NE=182162915";
    public const string LoggerSn = "102536559502";
    public const string MeterDn = "NE=182162917";

    /// <summary>3× SUN2000 invertera (pod PowerMeter-om).</summary>
    public static IReadOnlyList<string> InverterDns { get; } = new[]
    {
        "NE=183539038",
        "NE=183918730",
        "NE=249692600"
    };

    /// <summary>Signal ID za 5-min istoriju — aktivna snaga invertera (kW).</summary>
    public const int SignalInverterPower = 30014;

    /// <summary>Signal ID za 5-min istoriju — aktivna snaga na meteru / potrošnja (kW).</summary>
    public const int SignalMeterLoadPower = 30034;

    public static IReadOnlyList<SolarPodatakStavka> Sve => new[]
    {
        new SolarPodatakStavka(
            "Real-time KPI postrojenja",
            "GET /station/total-real-kpi",
            "Svakih 5 min",
            ["Trenutna snaga (kW)", "Proizvedeno danas (kWh)", "Ukupno od instalacije (kWh)", "Prihod danas"],
            ["Gauge — trenutna snaga", "KPI kartice (4 broja)", "Line — snaga kroz dan (iz istorije)"],
            ["Dnevni pregled proizvodnje", "Mesečni sažetak kWh i prihoda"],
            "Planirano"),

        new SolarPodatakStavka(
            "KPI grafikon (dan / mesec / godina)",
            "GET /station-kpi-data, /home-station-kpi-chart",
            "Na zahtev + dnevni cache",
            ["Vremenska serija kWh po satu", "Agregat po danu/mesecu/godini", "timeDim: 2=dan, 3=mesec, 4=godina, 5=lifetime"],
            ["Area/Line — proizvodnja po satu", "Bar — mesečna proizvodnja", "Bar — godišnja proizvodnja"],
            ["Izveštaj proizvodnje po periodu", "Uporedba meseci / godina"],
            "Planirano"),

        new SolarPodatakStavka(
            "Društveni doprinos (CO₂, uglje, stabla)",
            "GET /station/social-contribution",
            "Dnevno",
            ["CO₂ redukcija ukupno i po godini", "Ekvivalent sadnje stabala", "Ušteda standardnog uglja"],
            ["KPI kartice", "Pie — udeo po kategoriji doprinosа"],
            ["Godišnji ekološki izveštaj"],
            "Planirano"),

        new SolarPodatakStavka(
            "Lista uređaja",
            "GET /device-list",
            "Svakih 15 min",
            ["deviceDn", "Naziv", "mocId (tip)", "Status (connected/disconnected)"],
            ["Tabela uređaja sa status badge-om", "Donut — broj po tipu uređaja"],
            ["Inventar opreme", "Pregled offline uređaja"],
            "Planirano"),

        new SolarPodatakStavka(
            "Real-time signali po uređaju",
            "GET /device-signals, /get-device-signals",
            "Svakih 5 min (po inverteru)",
            ["Snaga (kW)", "Dnevna energija (kWh)", "Ukupna energija", "Efikasnost (%)", "Temperatura (°C)", "Napon/struja/frekvencija mreže"],
            ["Line — snaga po inverteru", "Bar — dnevna energija po inverteru", "Heatmap — temperatura"],
            ["Poređenje 3 invertera", "Dnevni radni list po inverteru"],
            "Planirano"),

        new SolarPodatakStavka(
            "Istorijski signali",
            "GET /device-history-data",
            "Na zahtev (izbor perioda)",
            ["Vrednost signala po vremenu", "signalId", "startTime / endTime"],
            ["Line — bilo koji signal u periodu", "Multi-line — više invertera"],
            ["Istorijski izveštaj po danu/mesecu", "Export CSV/PDF"],
            "Planirano"),

        new SolarPodatakStavka(
            "Energetski tok (PV → mreža / potrošnja)",
            "GET /energy-flow, /energy-balance",
            "Svakih 5 min",
            ["Smer toka energije", "PV proizvodnja", "Potrošnja", "Uvoz/izvoz u mrežu"],
            ["Sankey / flow dijagram", "KPI — trenutni balans"],
            ["Dnevni energetski bilans"],
            "Planirano"),

        new SolarPodatakStavka(
            "Status i alarmi",
            "GET /station-status-count",
            "Svakih 5 min",
            ["Connected / Disconnected / Trouble", "Ukupan broj uređaja"],
            ["Status panel (zeleno/žuto/crveno)", "Pie — stanje uređaja"],
            ["Alarm lista", "Log offline perioda"],
            "Planirano"),

        new SolarPodatakStavka(
            "Postrojenje (meta)",
            "POST /station-list",
            "Jednom + pri promeni",
            ["Naziv (Odetta)", "Adresa", "plantStatus", "stationDn"],
            ["—"],
            ["—"],
            "Planirano")
    };

    public static IReadOnlyList<(int MocId, string Tip)> TipoviUredjaja => new[]
    {
        (20812, "Postrojenje"),
        (20816, "PowerMeter"),
        (20821, "SmartLogger"),
        (20822, "Inverter (SUN2000)"),
        (90001, "Mreža (Grid)"),
        (90002, "Potrošnja (Load)")
    };

    public static IReadOnlyList<(int SignalId, string Naziv, string Jedinica)> SignaliInvertera => new[]
    {
        (10025, "Aktivna snaga", "kW"),
        (10029, "Dnevna energija", "kWh"),
        (10032, "Ukupna energija", "kWh"),
        (10034, "Efikasnost", "%"),
        (10037, "Temperatura invertera", "°C"),
        (11007, "Napon mreže", "V"),
        (11008, "Struja mreže", "A"),
        (11015, "Frekvencija", "Hz")
    };
}
