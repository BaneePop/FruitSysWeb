# FruitSysWeb — Claude Code pravila

## Stack (verifikovano iz koda)

- **Framework**: .NET 8.0.413, Blazor Server (InteractiveServerComponents)
- **ORM**: Dapper — direktni SQL upiti, NEMA Entity Framework Core
- **DB konektor**: MySqlConnector 2.3.7 (ne MySql.Data)
- **Baza**: MySQL 5.1.54 na `185.102.237.236:63388`, baza `fruitsysdb_v2`
- **PDF**: QuestPDF 2025.7.1
- **Excel**: ClosedXML 0.102.2
- **Charts**: Blazor-ApexCharts 3.5.0
- **Logging**: Serilog → `Logs/fruitsys-DATUM.log`
- **Barkod**: ZXing.Net 0.16.11

## Pokretanje

```bash
dotnet run
```

Port se čita iz `Properties/launchSettings.json` — nikad ne pretpostavljaj port.
Build provera: `dotnet build` — mora proći bez grešaka pre završetka svakog taska.

## Struktura projekta

```
FruitSysWeb/
├── Components/
│   ├── Pages/       — SVE Blazor stranice (.razor) — 50+ modula
│   ├── Layout/      — MainLayout, NavMenu i sl.
│   ├── Shared/      — Deljene komponente
│   └── Charts/      — ApexCharts komponente
├── Services/
│   ├── Interfaces/  — I*Service.cs interfejsi (25+)
│   ├── Implementations/ — Konkretne implementacije
│   └── Core/        — DatabaseService, CacheService, KesaSelekcijaService
├── Models/          — Data modeli
├── Extensions/
│   └── ServiceCollectionExtensions.cs — AddFruitSysServices() — JEDINO mesto za DI
├── Data/            — Dodatni data sloj
├── Database/        — SQL skripte, šema
├── Utils/           — Pomoćne klase
├── wwwroot/         — Statički fajlovi
├── Logs/            — Serilog logovi (ne commitovati)
├── appsettings.json             — Produkcija (connection string)
├── appsettings.Development.json — Razvoj
└── Program.cs       — App startup i konfiguracija
```

## MySQL 5.1.54 — STROGA OGRANIČENJA

**Ne podržava — nikad ne koristiti:**
- `WITH` klauzule (CTE)
- Window funkcije: `ROW_NUMBER()`, `RANK()`, `DENSE_RANK()`, `LAG()`, `LEAD()`, `NTILE()`
- `JSON_*` funkcije
- `UPSERT` sa složenim izrazima

**Alternativa za CTE** — koristi privremenu tabelu:
```sql
CREATE TEMPORARY TABLE tmp_rezultat AS
SELECT komitent_id, SUM(vrednost) AS ukupno
FROM stavke
WHERE datum >= '2024-01-01'
GROUP BY komitent_id;

SELECT k.naziv, t.ukupno
FROM tmp_rezultat t
JOIN komitenti k ON k.id = t.komitent_id
ORDER BY t.ukupno DESC;

DROP TEMPORARY TABLE IF EXISTS tmp_rezultat;
```

**Pre svakog SQL upita** — proveri da nema zabranjenih konstrukcija.

## Pattern: Kako se radi sa bazom (Dapper)

DatabaseService je centralni servis za sve DB operacije:

```csharp
// Uvek injektuj DatabaseService, nikad direktno konekciju
public class MojService : IMojService
{
    private readonly DatabaseService _db;
    private readonly ILogger<MojService> _logger;

    public MojService(DatabaseService db, ILogger<MojService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IEnumerable<MojModel>> GetPodatkeAsync(int godina)
    {
        const string sql = @"
            SELECT id, naziv, vrednost
            FROM moja_tabela
            WHERE YEAR(datum) = @Godina
            ORDER BY naziv";

        return await _db.QueryAsync<MojModel>(sql, new { Godina = godina });
    }
}
```

## Pattern: Dodavanje novog servisa

**JEDINO mesto za DI registraciju** je `Extensions/ServiceCollectionExtensions.cs`:

```csharp
// U AddFruitSysServices() — dodaj u odgovarajuću sekciju
services.AddScoped<INovServis, NovServis>();
```

**Nikad** ne dodavati servise direktno u `Program.cs`.

## Pattern: Nova Blazor stranica

```razor
@page "/nova-stranica"
@using FruitSysWeb.Services.Interfaces
@inject IAuthService AuthService
@inject INovServis NovServis

@* UI ovde *@

@code {
    private List<MojModel> _stavke = new();
    private bool _loading = true;

    protected override async Task OnInitializedAsync()
    {
        if (!await AuthService.IsAuthenticatedAsync())
        {
            return;
        }

        try
        {
            _stavke = (await NovServis.GetPodatkeAsync()).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Greška pri učitavanju podataka");
        }
        finally
        {
            _loading = false;
            await InvokeAsync(StateHasChanged);
        }
    }
}
```

## Konvencije koda

### Imenovanje
- **Klase/interfejsi**: PascalCase srpski nazivi su OK (`ProizvodnjaService`, `KomitentService`)
- **Privatni field-ovi**: `_camelCase` sa podvlakom (`_loading`, `_stavke`)
- **Parametri metoda**: camelCase
- **Komentari u kodu**: srpski jezik

### Async/await
- Obavezno za sve DB operacije (koriste `DatabaseService.QueryAsync`)
- `StateHasChanged` uvek kroz `await InvokeAsync(StateHasChanged)`
- Nikad `.Result` ili `.Wait()` — deadlock rizik u Blazor Server

### Logging (Serilog)
```csharp
_logger.LogInformation("Učitano {Broj} stavki za {Godina}", stavke.Count, godina);
_logger.LogError(ex, "Greška u {Metoda}", nameof(GetPodatkeAsync));
```

### SQL stil
```sql
-- Uppercase keywords, snake_case tabele i kolone
SELECT r.naziv, SUM(s.kolicina) AS ukupna_kolicina
FROM radni_nalozi r
JOIN stavke_naloga s ON s.nalog_id = r.id
WHERE r.datum_kreiranja >= @DatumOd
  AND r.status = 'A'
GROUP BY r.naziv
ORDER BY ukupna_kolicina DESC
LIMIT 100;
```

## Domenska znanja

### IQF linije i voće
- **Proizvodi**: maline, kupine, šljive, trešnje, kajsije, borovnice
- **Godišnji kapacitet**: ~4.000.000 kg
- **Tržište**: EU izvoz (zamrznuto voće, BRC/GlobalGAP sertifikat)

### HACCP redosled kontrolnih tačaka
1. Prijem sirovine → 2. Pranje → 3. Sortiranje → 4. IQF zamrzavanje → 5. Pakovanje → 6. Hladno skladištenje

### OEE formula
`OEE = Availability × Performance × Quality`

### Moduli u aplikaciji (Components/Pages)
Produkcija, Finansije, Lager, Fakture, Ugovori, Sledljivost, Reklamacije,
Kalkulacije, Efikasnost, Nabavka, Prodaja, Prerada, Izveštaji, i drugi.

## Pravila kojih se UVEK pridržavaš

1. **Nikad ne menjati migracije** — dodaj novu ako treba promeniti šemu
2. **Nikad ne dodavati servise u Program.cs** — samo u ServiceCollectionExtensions.cs
3. **Nikad ne koristiti MySql.Data** — samo MySqlConnector
4. **Nikad ne pisati SQL sa CTE/Window funkcijama** — MySQL 5.1.54
5. **Uvek async/await za DB** — nikad `.Result`/`.Wait()`
6. **dotnet build bez grešaka** pre završetka svakog taska

## Testovi

Još nema testova. Planirani framework: **xUnit** u `/FruitSysWeb.Tests/`.
Za sada: `dotnet build` je minimalna verifikacija.

## Deployment

Produkcija je na Windows Server, dostupna na `https://fruitsys.rs`.
Lokalni razvoj: `dotnet run`, port iz `launchSettings.json`.
Publish: `dotnet publish` (vidi `DEPLOY.md` za detalje).
