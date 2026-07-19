# Odetta Solar — Claude Code pravila

## Stack (verifikovano iz koda)

- **Framework**: .NET 8.0, Blazor Server (InteractiveServerComponents)
- **ORM**: Dapper — direktni SQL upiti, NEMA Entity Framework
- **Baza**: SQLite, DVA odvojena fajla — `Data/solar.db` (solarni podaci) i `Data/app.db` (korisnici). **Nema MySQL.**
- **Live podaci**: Huawei FusionSolar API (Playwright login + HTTP)
- **Charts**: Blazor-ApexCharts 3.5.0
- **Logging**: Serilog → `Logs/odetta-solar-DATUM.log`

## Pokretanje

```bash
dotnet run
```

Port se čita iz `Properties/launchSettings.json`.
Build provera: `dotnet build` — mora proći bez grešaka pre završetka svakog taska.

## Struktura projekta

```
Odetta Solar/
├── Components/
│   ├── Pages/       — SolarHala.razor, SolarPregled.razor, Login.razor, Home.razor,
│   │                   Error.razor, KorisnikAktivnostLog.razor
│   ├── Layout/      — MainLayout, EmptyLayout, AuthorizeView
│   └── Shared/      — InactivityHandler
├── Shared/          — NavMenu.razor, Footer.razor
├── Services/
│   ├── Solar/       — FusionSolarClient, SolarPollingService, SolarLocalDbService...
│   ├── Auth/        — PasswordHasher (PBKDF2), AuthLocalDbService (SQLite login)
│   ├── Interfaces/  — IAuthService, IKorisnikAktivnostService
│   └── Implementations/ — AuthService, KorisnikAktivnostService
├── Models/          — KorisnikModel, KorisnikAktivnostModel, Solar/
├── Extensions/
│   └── ServiceCollectionExtensions.cs — AddOdettaSolarServices() — JEDINO mesto za DI
├── Tools/
│   ├── SolarHistoryProbe/ — CLI za proveru FusionSolar API-ja
│   └── UserAdmin/         — CLI za dodavanje/upravljanje korisnicima
├── Data/            — solar.db, app.db, korisnik-aktivnost.json (ne commitovati)
├── appsettings.json / .Development.json / .Production.json
└── Program.cs
```

## Pattern: Kako se radi sa bazom (Dapper + SQLite)

Dve odvojene lokalne SQLite baze, svaka sa svojim servisom (`SolarLocalDbService`, `AuthLocalDbService`) — po istom obrascu:

```csharp
public class MojServis
{
    private readonly string _connectionString;

    public MojServis(IWebHostEnvironment env, ILogger<MojServis> logger)
    {
        var dbPath = Path.Combine(env.ContentRootPath, "Data", "moja.db");
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
}
```

**Ne praviti novu MySQL/Dapper konekciju na produkcijsku bazu — te ne postoji, i ne treba je vraćati.**

## Pattern: Dodavanje novog servisa

**JEDINO mesto za DI registraciju** je `Extensions/ServiceCollectionExtensions.cs` → `AddOdettaSolarServices()`.

```csharp
services.AddScoped<INovServis, NovServis>();
```

**Nikad** ne dodavati servise direktno u `Program.cs`.

## Pattern: Nova Blazor stranica

```razor
@page "/nova-stranica"
@using FruitSysWeb.Services.Interfaces
@inject IAuthService AuthService

@code {
    protected override async Task OnInitializedAsync()
    {
        // Stranica je već zaštićena globalno preko AuthorizeView (Pages/App.razor)
        // — svaki ulogovan korisnik ima pun pristup, nema nivoa/rola za proveru.
    }
}
```

Dodati link u `Shared/NavMenu.razor` (svesno drži se na minimum — samo Solar rute).

## Konvencije koda

### Imenovanje
- **Privatni field-ovi**: `_camelCase`
- **Parametri metoda**: camelCase
- **Komentari u kodu**: srpski jezik
- Namespace u kodu je i dalje `FruitSysWeb.*` (nasleđeno iz originalnog projekta — preimenovanje u `OdettaSolar` je otvorena stavka, vidi `PLAN.md`)

### Async/await
- Obavezno za sve DB operacije
- `StateHasChanged` uvek kroz `await InvokeAsync(StateHasChanged)`
- Nikad `.Result` ili `.Wait()` — deadlock rizik u Blazor Server

### Logging (Serilog)
```csharp
_logger.LogInformation("Solar KPI upisan: {Detalji}", detalji);
_logger.LogError(ex, "Greška u {Metoda}", nameof(MetodaAsync));
```

## Autentifikacija

- Lokalni SQLite login (`Data/app.db`), PBKDF2-HMAC-SHA256 (`Services/Auth/PasswordHasher.cs`)
- Nema nivoa pristupa — ulogovan korisnik = pun pristup (aplikacija ima samo 2 zaštićene rute)
- Korisnici se dodaju/menjaju preko `Tools/UserAdmin` CLI, ne preko UI-ja
- Sesija: `ProtectedSessionStorage` (per-tab, browser sessionStorage)

## Pravila kojih se UVEK pridržavaš

1. **Nikad ne vraćati MySQL/DatabaseService** — namerno uklonjen, aplikacija je SQLite-only
2. **Nikad ne dodavati servise u Program.cs** — samo u `ServiceCollectionExtensions.cs`
3. **Nikad ne vraćati nivoe pristupa/role** — namerno uklonjeni, previše su za 4-5 korisnika
4. **Uvek async/await za DB** — nikad `.Result`/`.Wait()`
5. **dotnet build bez grešaka** pre završetka svakog taska
6. **Ne širiti nazad poslovni FruitSysWeb kod** — ovo je namerno mala, samostalna Solar app

## Testovi

Još nema testova. Za sada: `dotnet build` je minimalna verifikacija, plus ručna provera kroz browser (login, `/solar-hala`, `/solar-pregled`).

## Deployment

Još nije deployovano na produkciju — vidi `DEPLOY.md` za skicu kad dođe vreme.
Lokalni razvoj: `dotnet run`, port iz `launchSettings.json`.
