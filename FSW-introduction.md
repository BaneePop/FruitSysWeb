# FruitSysWeb - Uvod u Projekat

## Sadržaj
- [O Projektu](#o-projektu)
- [Tehnologije](#tehnologije)
- [Arhitektura](#arhitektura)
- [Biznis Domen](#biznis-domen)
- [Struktura Projekta](#struktura-projekta)
- [Kako Početi](#kako-početi)
- [Razvoj Novih Funkcionalnosti](#razvoj-novih-funkcionalnosti)
- [Baza Podataka](#baza-podataka)
- [Autentifikacija i Autorizacija](#autentifikacija-i-autorizacija)
- [Export Funkcionalnosti](#export-funkcionalnosti)
- [Best Practices](#best-practices)

---

## O Projektu

**FruitSysWeb** je kompletan **ERP sistem** dizajniran za upravljanje operacijama prerade voća kompanije **ODETTA DOO**. Sistem pokriva celokupan poslovni ciklus od nabavke sirovine, preko proizvodnje, do prodaje gotovih proizvoda, sa potpunim finansijskim i skladišnim praćenjem.

### Ključne karakteristike
- ✅ Real-time praćenje proizvodnje i lagera
- ✅ Kompleksna sledljivost (upstream/downstream traceability)
- ✅ Multi-magacinski sistem (11 različitih magacina)
- ✅ Profesionalni izvještaji (PDF/Excel sa ODETTA brendiranjem)
- ✅ Dashboard sa KPI-ovima i grafičkim prikazima
- ✅ Kontrola pristupa bazirana na korisničkim grupama
- ✅ Optimizovane performanse (caching, pagination, indexed queries)

---

## Tehnologije

### Backend
- **ASP.NET Core 8.0** - Najnoviji .NET framework
- **Blazor Server** - Server-side interaktivni UI sa SignalR-om
- **C# 12** - Najnovija verzija C# jezika

### Baza podataka
- **MySQL** (`fruitsysdb_v2`) - Relaciona baza
- **Dapper 2.1.28** - Lightweight micro-ORM (NE koristi Entity Framework)
- **MySqlConnector 2.3.7** - Moderni MySQL driver
- **Database Views** - Ekstenzivna upotreba View-ova za kompleksne upite

### Frontend
- **Blazor Components** - Komponente sa .razor ekstenzijom
- **Bootstrap 5.3.2** - Responsive CSS framework
- **Bootstrap Icons** - Ikonografija
- **Chart.js & ApexCharts** - Interaktivne vizualizacije podataka
- **Custom Dark Theme** - Moderan tamni dizajn

### Export & Reporting
- **QuestPDF 2025.7.1** - PDF generisanje sa ODETTA brendiranjem
- **ClosedXML 0.102.2** - Excel generisanje
- **Srpska lokalizacija** - Datumi i brojevi u srpskom formatu

### Deployment
- **IIS** - Web server (Windows Server/Windows 11)
- **PowerShell Scripts** - Automatizovani deployment
- **Serilog** - Strukturisano logovanje

---

## Arhitektura

Projekat prati **Clean Architecture** pristup sa jasnom separacijom odgovornosti:

```
┌──────────────────────────────────────────────────┐
│  Presentation Layer                              │
│  - Blazor Pages (Components/Pages/)              │
│  - Shared Components (Charts, Filters, Tables)   │
│  - Layout Components (NavMenu, Footer)           │
├──────────────────────────────────────────────────┤
│  Service Layer                                   │
│  - Report Services (IzvestajService/)            │
│  - Export Services (ExportService/)              │
│  - Core Services (AuthService, DashboardService) │
├──────────────────────────────────────────────────┤
│  Data Access Layer                               │
│  - DatabaseService (Dapper wrapper)              │
│  - BaseService (Shared SQL logic)                │
│  - CacheService (Performance optimization)       │
├──────────────────────────────────────────────────┤
│  Domain Layer                                    │
│  - Models (49 data models)                       │
│  - Constants (Business logic constants)          │
│  - Filters (Request/Response models)             │
└──────────────────────────────────────────────────┘
```

### Dependency Injection Pattern

Svi servisi se registruju u `ServiceCollectionExtensions.cs`:

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFruitSysServices(this IServiceCollection services)
    {
        // Core services
        services.AddSingleton<DatabaseService>();
        services.AddSingleton<CacheService>();

        // Scoped services
        services.AddScoped<IProizvodnjaService, ProizvodnjaService>();
        services.AddScoped<IPreradaService, PreradaService>();
        // ... ostali servisi

        return services;
    }
}
```

---

## Biznis Domen

### Glavni Moduli

#### 1. PRERADA (Processing/Manufacturing) 🏭
**Centralni modul za proizvodnju**

- **Radni Nalozi (Work Orders)**: Praćenje proizvodnih naloga od početka do kraja
  - Status: Otvoren/Zaključen
  - Povezivanje sa komitentima, artiklima, i radnicima

- **Smenski Izvestaji (Shift Reports)**: Praćenje rada po smenama
  - Evidencija radnika po smeni
  - Troškovi rada
  - Koeficijenti efikasnosti

- **Proizvodnja**: Utrošak sirovina i proizvodnja gotovih proizvoda

- **Lager Proizvodnje**: Skladište gotovih proizvoda

- **Sledljivost (Traceability)**:
  - **UPSTREAM**: Prijem → Radni Nalog → Proizvodnja → Prodaja
  - **DOWNSTREAM**: Gotov Proizvod → Sirovine koje su korišćene

#### 2. FINANSIJE (Finance) 💰
**Pun pristup samo za administratore**

- **Nabavka (Procurement)**: Otkupni listovi, troškovi nabavke
- **Prodaja (Sales)**: Fakture, prihod od prodaje
- **Obračun Poslovanja**: Profit & Loss izvještaji
- **Ugovori (Contracts)**: Upravljanje ugovorima sa kupcima
- **Finansijski Pregled**: KPI-ovi i finansijske metrike

#### 3. LAGER (Warehouse/Inventory) 📦
**Multi-magacinski sistem**

11 tipova magacina (MagacinID):

| ID | Naziv | Opis |
|----|-------|------|
| 2  | Sveza Roba | Sveže voće i povrće |
| 3  | Sirovine | Zamrznute sirovine (maline, kupine, šljive) |
| 4  | Ambalaza | Ambalaža i pakovanje |
| 5  | Poluproizvodi | Poluproizvodi u fazi obrade |
| 6  | Gotova Roba | Finalni proizvodi |
| 7  | Kalo i Rastur | Otpad/škart (isključen iz kalkulacija) |
| 8  | Usl. Mleko | Servisni magacin - mlečni proizvodi |
| 9  | Repromaterijal | Potrošni materijal (trake, itd.) |
| 10 | Djubriva | Đubriva |
| 11 | Usl. Voce | Servisni magacin - voće/povrće |
| 12 | Usl. Meso | Servisni magacin - meso |

#### 4. TROŠKOVI (Costs) 📊
- **Troškovi Proizvodnje**: Proizvodni troškovi
- **Troškovi Radne Snage**: Troškovi rada (svi korisnici)
- **Troškovi Ambalaze**: Troškovi ambalaže (samo admin)
- **Troškovi Poslovanja**: Operativni troškovi (samo admin)

#### 5. PROMET (Turnover) 🔄
- **Izvestaj Prijem**: Izvještaji o prijemu robe
- **Povratna Ambalaza**: Praćenje povratne ambalaže
- **Paletni List**: Real-time praćenje paleta

---

## Struktura Projekta

```
FruitSysWeb/
│
├── Components/
│   ├── Pages/                      # 36 Blazor stranica
│   │   ├── Home.razor              # Dashboard sa finansijskim graficima (Admin)
│   │   ├── HomeOgraniceni.razor    # Dashboard za limited access
│   │   ├── Login.razor             # Autentifikacija
│   │   │
│   │   ├── PRERADA MODULE
│   │   ├── PreradaHome.razor       # Početna stranica Prerada
│   │   ├── Proizvodnja.razor       # Praćenje proizvodnje
│   │   ├── LagerProizvodnje.razor  # Skladište gotovih proizvoda
│   │   ├── RadniNaloziPregled.razor # Pregled radnih naloga
│   │   ├── SmenskiIzvestaji.razor  # Smenski izvještaji
│   │   ├── Sledljivost.razor       # Upstream/Downstream sledljivost
│   │   │
│   │   ├── FINANSIJE MODULE (Admin only)
│   │   ├── FinansijeHome.razor     # Početna stranica Finansije
│   │   ├── Nabavka.razor           # Nabavka/Procurement
│   │   ├── Prodaja.razor           # Prodaja/Sales
│   │   ├── FinansijskiPregled.razor # Finansijski KPI-ovi
│   │   ├── Ugovori.razor           # Ugovori
│   │   │
│   │   ├── LAGER MODULE
│   │   ├── Lager.razor             # Opšti pregled lagera
│   │   ├── Ambalaza.razor          # Ambalažni lager
│   │   ├── Roba.razor              # Lager sirovina
│   │   │
│   │   └── TROŠKOVI MODULE
│   │       ├── TroskoviHome.razor
│   │       ├── TroskoviProizvodnje.razor
│   │       ├── TroskoviRadneSnage.razor
│   │       ├── TroskoviAmbalaze.razor    # Admin only
│   │       └── TroskoviPoslovanja.razor  # Admin only
│   │
│   ├── Charts/                     # 8 chart komponenti
│   │   ├── ApexBarChart.razor
│   │   ├── ApexPieChart.razor
│   │   ├── ApexMultiLineChart.razor
│   │   ├── DashboardCharts.razor
│   │   └── ChartDataHelper.cs      # Pomoćne funkcije za chart data
│   │
│   ├── Shared/
│   │   ├── Filters/                # Reusable filter komponente
│   │   │   ├── DateFilter.razor
│   │   │   ├── KomitentFilter.razor
│   │   │   ├── ArtikalFilter.razor
│   │   │   └── TextFilter.razor
│   │   │
│   │   └── Tables/                 # Tabela komponente
│   │       ├── DataTable.razor     # Univerzalna data tabela
│   │       └── ExportButtons.razor # Excel/PDF dugmad
│   │
│   └── Layout/
│       ├── MainLayout.razor        # Glavni layout sa NavMenu
│       ├── EmptyLayout.razor       # Prazan layout za login
│       └── NavMenu.razor           # Navigacioni meni
│
├── Services/
│   ├── Core/
│   │   ├── DatabaseService.cs      # Dapper wrapper, CRUD operacije
│   │   ├── BaseService.cs          # Deljene SQL builder metode
│   │   ├── CacheService.cs         # In-memory caching (Singleton)
│   │   └── TypeMappingService.cs   # Type conversion
│   │
│   ├── Implementations/
│   │   ├── IzvestajService/        # 14 report servisa
│   │   │   ├── ProizvodnjaService.cs
│   │   │   ├── PreradaService.cs   # 14+ metoda za prerada izvještaje
│   │   │   ├── FinansijeService.cs
│   │   │   ├── MagacinLagerService.cs
│   │   │   ├── BrziPregledService.cs
│   │   │   ├── SledljivostService.cs
│   │   │   └── ...
│   │   │
│   │   ├── ExportService/          # Export servisi
│   │   │   ├── SimpleExportService.cs
│   │   │   ├── SledljivostPdfService.cs
│   │   │   ├── SledljivostExcelService.cs
│   │   │   └── SledljivostInteraktivniPdfService.cs
│   │   │
│   │   ├── AuthService.cs          # Username-based autentifikacija
│   │   ├── DashboardService.cs     # Dashboard data
│   │   ├── ArtikalService.cs       # Artikal management
│   │   └── KomitentService.cs      # Partner management
│   │
│   └── Interfaces/                 # 20 service interfejsa
│       ├── IProizvodnjaService.cs
│       ├── IPreradaService.cs
│       └── ...
│
├── Models/                         # 49 modela
│   ├── NabavkaModel.cs
│   ├── ProizvodnjaModel.cs
│   ├── RadniNalogModel.cs
│   ├── SmenskiIzvestajModel.cs
│   ├── Filters/                    # Filter request modeli
│   │   └── FilterRequest.cs
│   └── Sledljivost/                # Sledljivost modeli
│       ├── SledljivostUzlaznaModel.cs
│       └── SledljivostSilaznaModel.cs
│
├── Constants/
│   ├── SystemConstants.cs          # Sistemske konstante
│   ├── MagacinTypes.cs             # MagacinID konstante
│   ├── DocumentStatus.cs           # Statusi dokumenata
│   └── SezonaConstants.cs          # Sezonske konstante
│
├── Extensions/
│   └── ServiceCollectionExtensions.cs  # DI registracija
│
├── Database/
│   └── OptimizacijeIndeksa.sql     # Indeksi za performanse
│
├── wwwroot/
│   ├── css/
│   │   └── site.css                # Custom dark theme
│   ├── js/
│   └── lib/                        # Bootstrap, Chart.js
│
├── appsettings.json                # Development konfiguracija
├── appsettings.Production.json     # Production konfiguracija
└── Program.cs                      # Application entry point
```

---

## Kako Početi

### Preduslovi
- **.NET 8.0 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Visual Studio 2022** ili **VS Code** sa C# ekstenzijom
- **MySQL** pristup (connection string u `appsettings.json`)

### Setup koraci

1. **Kloniranje/Otvaranje projekta**
   ```bash
   cd /Users/Bane/FruitSysWeb
   code .
   ```

2. **Provera connection stringa**

   Otvori `appsettings.json` i proveri:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=185.102.237.236;Port=63388;Database=fruitsysdb_v2;..."
     }
   }
   ```

3. **Restore NuGet paketa**
   ```bash
   dotnet restore
   ```

4. **Build projekta**
   ```bash
   dotnet build
   ```

5. **Pokretanje aplikacije**
   ```bash
   dotnet run
   ```

   Aplikacija će biti dostupna na: `https://localhost:5001` ili `http://localhost:5000`

6. **Login**

   Korisničko ime i šifra iz `Korisnik` tabele u bazi.

---

## Razvoj Novih Funkcionalnosti

### Tipičan workflow za dodavanje nove stranice/funkcionalnosti

#### 1. Kreiraj Model

```csharp
// Models/NovaFunkcijaModel.cs
namespace FruitSysWeb.Models
{
    public class NovaFunkcijaModel
    {
        public int Id { get; set; }
        public string Naziv { get; set; }
        public DateTime Datum { get; set; }
        public decimal Iznos { get; set; }
    }
}
```

#### 2. Kreiraj Service Interface

```csharp
// Services/Interfaces/INovaFunkcijaService.cs
namespace FruitSysWeb.Services.Interfaces
{
    public interface INovaFunkcijaService
    {
        Task<List<NovaFunkcijaModel>> GetDataAsync(FilterRequest filter);
        Task<NovaFunkcijaModel> GetByIdAsync(int id);
    }
}
```

#### 3. Implementiraj Service

```csharp
// Services/Implementations/NovaFunkcijaService.cs
using FruitSysWeb.Services.Core;

namespace FruitSysWeb.Services.Implementations
{
    public class NovaFunkcijaService : BaseService, INovaFunkcijaService
    {
        private readonly DatabaseService _db;

        public NovaFunkcijaService(DatabaseService db)
        {
            _db = db;
        }

        public async Task<List<NovaFunkcijaModel>> GetDataAsync(FilterRequest filter)
        {
            var sql = CreateSqlBuilder("SELECT * FROM vNovaFunkcijaView WHERE 1=1");

            // Koristi BaseService metode za filtering
            ApplyDateFilter(sql, filter);
            ApplyKomitentFilter(sql, filter);

            sql.Append(" ORDER BY Datum DESC");

            return await _db.QueryAsync<NovaFunkcijaModel>(sql.ToString(), sql.Parameters);
        }

        public async Task<NovaFunkcijaModel> GetByIdAsync(int id)
        {
            var sql = "SELECT * FROM vNovaFunkcijaView WHERE Id = @Id";
            return await _db.QueryFirstOrDefaultAsync<NovaFunkcijaModel>(sql, new { Id = id });
        }
    }
}
```

#### 4. Registruj Service u DI

```csharp
// Extensions/ServiceCollectionExtensions.cs
services.AddScoped<INovaFunkcijaService, NovaFunkcijaService>();
```

#### 5. Kreiraj Blazor Page

```razor
@* Components/Pages/NovaFunkcija.razor *@
@page "/nova-funkcija"
@using FruitSysWeb.Services.Interfaces
@using FruitSysWeb.Models
@inject INovaFunkcijaService NovaFunkcijaService

<PageTitle>Nova Funkcija - FruitSysWeb</PageTitle>

<div class="container-fluid">
    <div class="row mb-4">
        <div class="col">
            <h1 class="page-title">Nova Funkcija</h1>
        </div>
    </div>

    @* Filteri *@
    <div class="row mb-3">
        <div class="col-md-4">
            <DateFilter @bind-StartDate="filterRequest.StartDate"
                       @bind-EndDate="filterRequest.EndDate"
                       OnFilterChanged="ApplyFilters" />
        </div>
    </div>

    @* Tabela *@
    @if (loading)
    {
        <div class="text-center">
            <div class="spinner-border" role="status">
                <span class="visually-hidden">Učitavanje...</span>
            </div>
        </div>
    }
    else if (data != null && data.Any())
    {
        <DataTable Items="data" TItem="NovaFunkcijaModel">
            <TableHeader>
                <th>ID</th>
                <th>Naziv</th>
                <th>Datum</th>
                <th>Iznos</th>
            </TableHeader>
            <RowTemplate>
                <td>@context.Id</td>
                <td>@context.Naziv</td>
                <td>@context.Datum.ToString("dd.MM.yyyy")</td>
                <td>@context.Iznos.ToString("N2")</td>
            </RowTemplate>
        </DataTable>
    }
    else
    {
        <p class="text-muted">Nema podataka za prikaz.</p>
    }
</div>

@code {
    private List<NovaFunkcijaModel> data = new();
    private FilterRequest filterRequest = new();
    private bool loading = true;

    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }

    private async Task LoadData()
    {
        loading = true;
        try
        {
            data = await NovaFunkcijaService.GetDataAsync(filterRequest);
        }
        finally
        {
            loading = false;
        }
    }

    private async Task ApplyFilters()
    {
        await LoadData();
    }
}
```

#### 6. Dodaj link u NavMenu

```razor
@* Shared/NavMenu.razor *@
<NavLink class="nav-link" href="nova-funkcija">
    <i class="bi bi-star"></i> Nova Funkcija
</NavLink>
```

---

## Baza Podataka

### Pristup podacima kroz DatabaseService

**DatabaseService** (`/Services/DatabaseService.cs`) je Dapper wrapper koji pruža:

#### Osnovne CRUD operacije

```csharp
// Query - vraća listu
var results = await _db.QueryAsync<Model>("SELECT * FROM Table WHERE Id = @Id", new { Id = 1 });

// QueryFirstOrDefault - vraća jedan objekat ili null
var result = await _db.QueryFirstOrDefaultAsync<Model>("SELECT * FROM Table WHERE Id = @Id", new { Id = 1 });

// Execute - izvršava INSERT/UPDATE/DELETE
var rowsAffected = await _db.ExecuteAsync("UPDATE Table SET Name = @Name WHERE Id = @Id", new { Name = "Test", Id = 1 });

// ExecuteScalar - vraća jednu vrednost
var count = await _db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Table");
```

#### Pagination support

```csharp
var (items, totalCount) = await _db.QueryPagedAsync<Model>(
    sql: "SELECT * FROM Table WHERE Active = 1",
    parameters: null,
    pageNumber: 1,
    pageSize: 20
);
```

#### Transaction support

```csharp
// Auto-commit/rollback
await _db.ExecuteInTransactionAsync(async transaction =>
{
    await _db.ExecuteAsync("INSERT INTO Table1...", transaction: transaction);
    await _db.ExecuteAsync("INSERT INTO Table2...", transaction: transaction);
});

// Manuelna kontrola
await using var transaction = await _db.BeginTransactionAsync();
try
{
    await _db.ExecuteAsync("INSERT...", transaction: transaction);
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

### Database Views

Projekat **ekstenzivno koristi MySQL View-ove** za kompleksne upite:

- `vPreradaPregled` - Pregled prerade
- `vPrometFinansijev9` - Finansijske transakcije
- `vEvidencijaRadaPreradaMnozilac` - Koeficijenti efikasnosti rada

**Best Practice**: Kreiraj View za kompleksne JOIN-ove i agregacije, pa koristi u Dapper upitima.

### Optimizacija performansi

Indeksi su definisani u `/Database/OptimizacijeIndeksa.sql`:

```sql
-- Composite index
CREATE INDEX idx_radni_nalog_datum_komitent
ON RadniNalog(Datum, KomitentID, StatusID);

-- Covering index
CREATE INDEX idx_evidencija_rada_covering
ON EvidencijaRada(RadniNalogID, Datum)
INCLUDE (BrojSati, Iznos);
```

---

## Autentifikacija i Autorizacija

### Sistem autentifikacije

**Username-based** autentifikacija (NE ASP.NET Identity):

```csharp
// Services/Implementations/AuthService.cs
public async Task<LoginResponse> Login(string username, string password)
{
    var user = await GetUserByUsernameAsync(username);

    if (user == null)
        return new LoginResponse { Success = false, Message = "Pogrešno korisničko ime." };

    // MD5 hash sa salt-om
    var hashedPassword = HashPassword(password, user.Seed);

    if (user.Password != hashedPassword)
        return new LoginResponse { Success = false, Message = "Pogrešna lozinka." };

    return new LoginResponse
    {
        Success = true,
        Username = user.Username
    };
}
```

### Sistem autorizacije

**Dva nivoa pristupa** definisana u `GrupaKorisnikaHelper.cs`:

#### 1. Limited Access korisnici (6 korisnika)
```csharp
private static readonly HashSet<string> LimitedAccessUsers = new()
{
    "zoran", "jelena", "pedja", "radmila", "masinska", "BaneT"
};
```

**Pristup**:
- ✅ Prerada (Proizvodnja, Radni Nalozi, Smenski Izvestaji)
- ✅ Lager (svi magacini)
- ✅ Troškovi (Proizvodnje, Radne Snage)
- ✅ Promet (Izvestaj Prijem, Povratna Ambalaza)
- ❌ Nabavka, Prodaja, Finansije
- ❌ Troškovi Ambalaze, Troškovi Poslovanja

#### 2. Full Access korisnici (svi ostali)
**Pristup**: Sve funkcionalnosti

### Provera pristupa

```csharp
// U Blazor komponenti
@code {
    [CascadingParameter]
    private Task<AuthenticationState> AuthenticationStateTask { get; set; }

    private string currentUsername;

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthenticationStateTask;
        currentUsername = authState.User.Identity?.Name;

        // Provera pristupa
        if (!GrupaKorisnikaHelper.ImaPristupStranici(currentUsername, "/finansije"))
        {
            NavigationManager.NavigateTo("/unauthorized");
            return;
        }
    }
}
```

### NavMenu conditional rendering

```razor
@* Shared/NavMenu.razor *@
@if (ImaPuniPristup())
{
    <li class="nav-item dropdown">
        <a class="nav-link dropdown-toggle" href="#" data-bs-toggle="dropdown">
            <i class="bi bi-currency-dollar"></i> FINANSIJE
        </a>
        <ul class="dropdown-menu">
            <li><a class="dropdown-item" href="/nabavka">Nabavka</a></li>
            <li><a class="dropdown-item" href="/prodaja">Prodaja</a></li>
        </ul>
    </li>
}

@code {
    private bool ImaPuniPristup()
    {
        return !GrupaKorisnikaHelper.JeLimitedAccess(currentUsername);
    }
}
```

---

## Export Funkcionalnosti

### Excel Export

**SimpleExportService** sa srpskim formatiranjem:

```csharp
public byte[] ExportToExcel<T>(List<T> data, string sheetName, List<string> columnNames = null)
{
    using var workbook = new XLWorkbook();
    var worksheet = workbook.Worksheets.Add(sheetName);

    // Serbian number format
    var numberFormat = "#,##0.00";
    var dateFormat = "dd.MM.yyyy";

    // Load data
    var table = worksheet.Cell(1, 1).InsertTable(data);

    // Format columns
    foreach (var column in worksheet.ColumnsUsed())
    {
        if (column.FirstCell().DataType == XLDataType.Number)
            column.Style.NumberFormat.Format = numberFormat;
        else if (column.FirstCell().DataType == XLDataType.DateTime)
            column.Style.NumberFormat.Format = dateFormat;
    }

    // Auto-fit columns
    worksheet.Columns().AdjustToContents();

    // Return as byte array
    using var stream = new MemoryStream();
    workbook.SaveAs(stream);
    return stream.ToArray();
}
```

### PDF Export

**QuestPDF** sa ODETTA brendiranjem:

```csharp
public byte[] ExportToPdf<T>(List<T> data, string title, List<string> columnNames = null)
{
    return Document.Create(container =>
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(20);

            // Header sa ODETTA logom
            page.Header().Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Image("wwwroot/images/odetta-logo.png").FitWidth();
                    column.Item().Text("ODETTA DOO").FontSize(20).Bold();
                });
            });

            // Content
            page.Content().Column(column =>
            {
                column.Item().Text(title).FontSize(16).Bold();

                // Table sa zebra prugama
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        foreach (var prop in typeof(T).GetProperties())
                            columns.RelativeColumn();
                    });

                    // Header
                    table.Header(header =>
                    {
                        foreach (var columnName in columnNames)
                        {
                            header.Cell().Background("#198754")
                                  .Padding(5).Text(columnName).FontColor("#FFFFFF");
                        }
                    });

                    // Rows with zebra stripes
                    for (int i = 0; i < data.Count; i++)
                    {
                        var backgroundColor = i % 2 == 0 ? "#F8F9FA" : "#FFFFFF";
                        // ... row rendering
                    }
                });
            });

            // Footer
            page.Footer().AlignCenter().Text(text =>
            {
                text.Span($"Strana ");
                text.CurrentPageNumber();
                text.Span(" od ");
                text.TotalPages();
            });
        });
    }).GeneratePdf();
}
```

### Export u Blazor komponentama

```razor
@* Components/Shared/Tables/ExportButtons.razor *@
<div class="btn-group">
    <button class="btn btn-success" @onclick="ExportToExcel">
        <i class="bi bi-file-earmark-excel"></i> Excel
    </button>
    <button class="btn btn-danger" @onclick="ExportToPdf">
        <i class="bi bi-file-earmark-pdf"></i> PDF
    </button>
</div>

@code {
    [Parameter] public List<object> Data { get; set; }
    [Parameter] public string FileName { get; set; }

    [Inject] private SimpleExportService ExportService { get; set; }
    [Inject] private IJSRuntime JS { get; set; }

    private async Task ExportToExcel()
    {
        var bytes = ExportService.ExportToExcel(Data, FileName);
        await JS.InvokeVoidAsync("downloadFile", $"{FileName}.xlsx", Convert.ToBase64String(bytes));
    }

    private async Task ExportToPdf()
    {
        var bytes = ExportService.ExportToPdf(Data, FileName);
        await JS.InvokeVoidAsync("downloadFile", $"{FileName}.pdf", Convert.ToBase64String(bytes));
    }
}
```

---

## Best Practices

### 1. Imenovanje konvencije

#### Servisi
- Interface: `I{Naziv}Service.cs` (npr. `IProizvodnjaService.cs`)
- Implementacija: `{Naziv}Service.cs` (npr. `ProizvodnjaService.cs`)

#### Modeli
- Sufiks: `{Naziv}Model.cs` (npr. `RadniNalogModel.cs`)
- Request: `{Naziv}Request.cs` (npr. `FilterRequest.cs`)
- Response: `{Naziv}Response.cs` (npr. `LoginResponse.cs`)

#### Blazor komponente
- Pages: `{Naziv}.razor` (npr. `Proizvodnja.razor`)
- Shared: `{Naziv}.razor` (npr. `DateFilter.razor`)

### 2. SQL upiti

**Uvek koristi parameterizovane upite**:

```csharp
// ✅ DOBRO - Parametrizovano
var sql = "SELECT * FROM Table WHERE Datum >= @StartDate AND Datum <= @EndDate";
var result = await _db.QueryAsync<Model>(sql, new { StartDate = start, EndDate = end });

// ❌ LOŠE - SQL Injection ranjivost
var sql = $"SELECT * FROM Table WHERE Datum >= '{start}' AND Datum <= '{end}'";
```

**Koristi BaseService metode**:

```csharp
public class MojService : BaseService
{
    public async Task<List<Model>> GetData(FilterRequest filter)
    {
        var sql = CreateSqlBuilder("SELECT * FROM View WHERE 1=1");

        ApplyDateFilter(sql, filter);           // Dodaje datum filter
        ApplyKomitentFilter(sql, filter);       // Dodaje komitent filter
        ApplyArtikalFilter(sql, filter);        // Dodaje artikal filter

        sql.Append(" ORDER BY Datum DESC");

        return await _db.QueryAsync<Model>(sql.ToString(), sql.Parameters);
    }
}
```

### 3. Performance optimizacije

#### Koristi CacheService za često pristupane podatke

```csharp
// Inject CacheService
[Inject] private CacheService CacheService { get; set; }

// Cache artikle (retko se menjaju)
var artikli = CacheService.GetOrCreate("artikli", () =>
{
    return ArtikalService.GetAllAsync();
}, TimeSpan.FromHours(1));
```

#### Koristi pagination za velike dataset-ove

```csharp
var (items, totalCount) = await _db.QueryPagedAsync<Model>(
    sql: "SELECT * FROM LargeTable",
    parameters: null,
    pageNumber: currentPage,
    pageSize: 50
);
```

### 4. Error handling

**Uvek wrap-uj database operacije u try-catch**:

```csharp
@code {
    private async Task LoadData()
    {
        loading = true;
        errorMessage = null;

        try
        {
            data = await Service.GetDataAsync(filter);
        }
        catch (Exception ex)
        {
            errorMessage = $"Greška pri učitavanju podataka: {ex.Message}";
            Logger.LogError(ex, "Error loading data");
        }
        finally
        {
            loading = false;
        }
    }
}
```

### 5. Responsive design

**Uvek koristi Bootstrap grid za responsive layout**:

```razor
<div class="row">
    <div class="col-12 col-md-6 col-lg-4">
        @* Kolona - 100% na mobilnom, 50% na tablet, 33% na desktop *@
    </div>
</div>
```

### 6. Datum i broj formatiranje

**Srpski format**:

```csharp
// Datum: dd.MM.yyyy
@item.Datum.ToString("dd.MM.yyyy")

// Broj: 1.000.987,56
@item.Iznos.ToString("N2", new CultureInfo("sr-Latn-RS"))

// U Blazor komponenti
@using System.Globalization

// Ili koristi helper metode iz TypeMappingService
```

### 7. Dependency Injection

**Uvek inject-uj servise kroz DI, NE kreiraj instance ručno**:

```csharp
// ✅ DOBRO
[Inject] private IProizvodnjaService ProizvodnjaService { get; set; }

// ❌ LOŠE
var service = new ProizvodnjaService(new DatabaseService(...));
```

### 8. Logging

**Koristi Serilog za logging**:

```csharp
[Inject] private ILogger<MojComponent> Logger { get; set; }

Logger.LogInformation("Data loaded successfully: {Count} items", data.Count);
Logger.LogWarning("No data found for filter: {@Filter}", filter);
Logger.LogError(ex, "Error occurred while processing data");
```

---

## Česta pitanja (FAQ)

### Kako dodati novi magacin?

1. Dodaj u bazu: `INSERT INTO Magacin (MagacinID, Naziv) VALUES (13, 'Novi Magacin')`
2. Dodaj konstantu u `Constants/MagacinTypes.cs`:
   ```csharp
   public const int NoviMagacin = 13;
   ```
3. Ažuriraj filtere ako je potrebno

### Kako kreirati novi izvještaj?

1. Kreiraj View u MySQL bazi (ako je potreban)
2. Kreiraj Model za rezultat
3. Kreiraj Service koji extend-uje `BaseService`
4. Implementiraj metode za dohvatanje podataka
5. Kreiraj Blazor page koja prikazuje podatke
6. Dodaj export funkcionalnost (Excel/PDF)

### Kako debugovati SQL upite?

1. Dodaj breakpoint u Service metodi
2. Pregledi `sql.ToString()` i `sql.Parameters`
3. Kopiraj SQL u MySQL Workbench i testiraj
4. Proveri rezultat

### Kako testirati pristup kao Limited user?

1. Promeni username u session storage ili
2. Dodaj privremeno svoj username u `LimitedAccessUsers` HashSet u `GrupaKorisnikaHelper.cs`

### Kako deploy-ovati izmene?

1. Build projekta: `dotnet publish -c Release`
2. Kopiraj fajlove iz `bin/Release/net8.0/publish/` na server
3. Restartuj IIS App Pool
4. Ili koristi PowerShell skriptu: `/FruitSysWeb-Deployment-Package-v1.1.0/update-app.ps1`

---

## Dodatni resursi

### Dokumentacija tehnologija
- [Blazor Docs](https://learn.microsoft.com/en-us/aspnet/core/blazor/)
- [Dapper Docs](https://github.com/DapperLib/Dapper)
- [QuestPDF Docs](https://www.questpdf.com/documentation/)
- [ClosedXML Docs](https://github.com/ClosedXML/ClosedXML)
- [Bootstrap 5 Docs](https://getbootstrap.com/docs/5.3/)

### Deployment dokumentacija
- `/FruitSysWeb-Deployment-Package-v1.1.0/START-HERE.txt`
- `/FruitSysWeb-Deployment-Package-v1.1.0/BRZI-START.md`
- `/FruitSysWeb-Deployment-Package-v1.1.0/DEPLOYMENT-UPUTSTVO-v1.1.0.md`

### Database optimizacije
- `/Database/OptimizacijeIndeksa.sql`

---

## Kontakt i podrška

Za pitanja i podršku:
- **Email**: kontakt@odetta.rs
- **GitHub Issues**: [Link ako postoji]

---

**Verzija**: 1.0.0
**Poslednje ažuriranje**: 06.12.2025
**Autor**: FruitSysWeb Development Team
