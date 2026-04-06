# FruitSysWeb - Razvojna Dokumentacija

## Sadržaj

- [Arhitektura](#arhitektura)
- [Baza Podataka](#baza-podataka)
- [Servisi](#servisi)
- [Autentifikacija i Autorizacija](#autentifikacija-i-autorizacija)
- [Dodavanje Novih Funkcionalnosti](#dodavanje-novih-funkcionalnosti)
- [Export Funkcionalnosti](#export-funkcionalnosti)
- [Best Practices](#best-practices)
- [Changelog](#changelog)

---

## Arhitektura

```
Presentation Layer  →  Components/Pages/ (Blazor)
Service Layer       →  Services/Implementations/
Data Access Layer   →  Services/Core/DatabaseService (Dapper)
Domain Layer        →  Models/, Constants/
```

Svi servisi se registruju **isključivo** u `Extensions/ServiceCollectionExtensions.cs`. Ne dodavati u `Program.cs`.

### Dependency Injection

```csharp
// Extensions/ServiceCollectionExtensions.cs
public static IServiceCollection AddFruitSysServices(this IServiceCollection services)
{
    // DATABASE & CORE
    services.AddScoped<DatabaseService>();
    services.AddSingleton<CacheService>();

    // BUSINESS LOGIC
    services.AddScoped<IFinansijeService, FinansijeService>();
    services.AddScoped<IPreradaService, PreradaService>();
    // ...

    return services;
}
```

---

## Baza Podataka

- **MySQL** server: `185.102.237.236:63388`, baza: `fruitsysdb_v2`
- Pristup isključivo kroz `DatabaseService` (Dapper wrapper)
- Ekstenzivna upotreba **MySQL View-ova** za kompleksne upite

### DatabaseService metode

```csharp
// Lista rezultata
var results = await _db.QueryAsync<Model>("SELECT * FROM vView WHERE ID = @Id", new { Id = 1 });

// Jedan objekat ili null
var item = await _db.QueryFirstOrDefaultAsync<Model>(sql, parameters);

// INSERT/UPDATE/DELETE
var rows = await _db.ExecuteAsync("UPDATE Table SET Name = @Name WHERE Id = @Id", new { Name = "x", Id = 1 });

// Jedna vrednost
var count = await _db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Table");

// Paginacija
var (items, total) = await _db.QueryPagedAsync<Model>(sql, parameters, pageNumber: 1, pageSize: 20);

// Transakcija
await _db.ExecuteInTransactionAsync(async tx => {
    await _db.ExecuteAsync("INSERT INTO A...", transaction: tx);
    await _db.ExecuteAsync("INSERT INTO B...", transaction: tx);
});
```

### Ključni View-ovi

| View | Opis |
|------|------|
| `vPrometFinansijev9` | Sve finansijske transakcije |
| `vPreradaPregled` | Pregled prerade |
| `vwMagacinLager` | Stanje magacina |
| `vEvidencijaRadaPreradaMnozilac` | Koeficijenti efikasnosti rada |

### Tipovi dokumenata

| Prefiks | Značenje |
|---------|----------|
| `KL-` | Otkupni list (nabavka od dobavljača) |
| `FK-` | Faktura kupcu (prodaja) |
| `IS-` | Isplata dobavljaču |
| `UP-` | Uplata kupca |

### Magacini (MagacinID)

| ID | Naziv |
|----|-------|
| 2 | Sveza Roba |
| 3 | Sirovine (zamrznute) |
| 4 | Ambalaza |
| 5 | Poluproizvodi |
| 6 | Gotova Roba |
| 7 | Kalo i Rastur (isključen iz kalkulacija) |
| 8-12 | Uslužni magacini (Mleko, Voce, Meso...) |

---

## Servisi

### Pregled servisa

| Servis | Interfejs | Opis |
|--------|-----------|------|
| `FinansijeService` | `IFinansijeService` | Nabavka, prodaja, saldo, stanje kese |
| `ProizvodnjaService` | `IProizvodnjaService` | Radni nalozi, smenski izveštaji |
| `PreradaService` | `IPreradaService` | Prerada, evidencija |
| `MagacinLagerService` | `IMagacinLagerService` | Stanje magacina |
| `BrziPregledService` | `IBrziPregledService` | Dashboard brzi pregled, grafici |
| `SledljivostService` | `ISledljivostService` | Upstream/downstream traceability |
| `PaletniListService` | `IPaletniListService` | Real-time praćenje paleta |
| `KomitentService` | `IKomitentService` | Master podaci komitenata |
| `ArtikalService` | `IArtikalService` | Master podaci artikala |
| `KesaSelekcijaService` | — (Singleton) | User preferences za Stanje Kase |
| `SimpleExportService` | `IExportService` | Excel i PDF export |
| `FakturaService` | `IFakturaService` | Učitavanje faktura, detalja, stavki, kupaca |
| `FakturaPdfService` | — | QuestPDF generator faktura/profaktura (SR/EN) |
| `FakturaExcelService` | — | ClosedXML export liste i detalja faktura |
| `KarticaKomitentaService` | `IKarticaKomitentaService` | Kartica komitenta (stavke + kumulativni saldo) |
| `KontrolaService` | `IKontrolaService` | Kontrole radnog naloga (sve tabele) |

### BaseService

Servisi koji extend-uju `BaseService` dobijaju helper metode za SQL filtering:

```csharp
public class MojService : BaseService, IMojService
{
    public async Task<List<Model>> GetData(FilterRequest filter)
    {
        var sql = CreateSqlBuilder("SELECT * FROM vView WHERE 1=1");
        ApplyDateFilter(sql, filter);       // Dodaje OdDatum/DoDatum
        ApplyKomitentFilter(sql, filter);   // Dodaje KomitentID
        ApplyArtikalFilter(sql, filter);    // Dodaje ArtikalID
        sql.Append(" ORDER BY Datum DESC");
        return await _db.QueryAsync<Model>(sql.ToString(), sql.Parameters);
    }
}
```

### KesaSelekcijaService (Singleton)

Čuva user preferences u `Data/stanje-kese-selekcija.json`. Folder mora imati Write permissions za IIS_IUSRS na serveru.

---

## Autentifikacija i Autorizacija

**Username-based** autentifikacija (ne ASP.NET Identity). MD5 hash sa salt-om u `AuthService.cs`.

### Dva nivoa pristupa (GrupaKorisnikaHelper.cs)

```csharp
// Ograničeni korisnici
private static readonly HashSet<string> LimitedAccessUsers = new()
{
    "zoran", "jelena", "pedja", "radmila", "masinska", "BaneT"
};
```

**Ograničeni korisnici NEMAJU pristup**: Finansije (Nabavka, Prodaja, Finansijski Pregled, Ugovori, Pregled Salda, Stanje Kase), Troškovi Ambalaze, Troškovi Poslovanja.

### Provera u komponentama

```razor
@code {
    [CascadingParameter]
    private Task<AuthenticationState> AuthenticationStateTask { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthenticationStateTask;
        var username = authState.User.Identity?.Name;

        if (!GrupaKorisnikaHelper.ImaPristupStranici(username, "/finansije"))
        {
            NavigationManager.NavigateTo("/unauthorized");
            return;
        }
    }
}
```

---

## Dodavanje Novih Funkcionalnosti

### Workflow (6 koraka)

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

#### 2. Kreiraj Interfejs

```csharp
// Services/Interfaces/INovaFunkcijaService.cs
public interface INovaFunkcijaService
{
    Task<List<NovaFunkcijaModel>> GetDataAsync(FilterRequest filter);
}
```

#### 3. Implementiraj Servis

```csharp
// Services/Implementations/IzvestajService/NovaFunkcijaService.cs
public class NovaFunkcijaService : BaseService, INovaFunkcijaService
{
    private readonly DatabaseService _db;

    public NovaFunkcijaService(DatabaseService db) => _db = db;

    public async Task<List<NovaFunkcijaModel>> GetDataAsync(FilterRequest filter)
    {
        var sql = CreateSqlBuilder("SELECT * FROM vNovaFunkcijaView WHERE 1=1");
        ApplyDateFilter(sql, filter);
        ApplyKomitentFilter(sql, filter);
        sql.Append(" ORDER BY Datum DESC");
        return await _db.QueryAsync<NovaFunkcijaModel>(sql.ToString(), sql.Parameters);
    }
}
```

#### 4. Registruj u DI

```csharp
// Extensions/ServiceCollectionExtensions.cs
services.AddScoped<INovaFunkcijaService, NovaFunkcijaService>();
```

#### 5. Kreiraj Blazor Page

```razor
@page "/nova-funkcija"
@inject INovaFunkcijaService NovaFunkcijaService

<div class="container-fluid">
    <h1 class="page-title">Nova Funkcija</h1>

    <DateFilter @bind-StartDate="filter.OdDatum" @bind-EndDate="filter.DoDatum"
                OnFilterChanged="LoadData" />

    @if (loading)
    {
        <div class="spinner-border" role="status"></div>
    }
    else
    {
        <DataTable Items="data" TItem="NovaFunkcijaModel">
            <TableHeader>
                <th>Datum</th><th>Naziv</th><th>Iznos</th>
            </TableHeader>
            <RowTemplate>
                <td>@context.Datum.ToString("dd.MM.yyyy")</td>
                <td>@context.Naziv</td>
                <td>@context.Iznos.ToString("N2")</td>
            </RowTemplate>
        </DataTable>
    }
</div>

@code {
    private List<NovaFunkcijaModel> data = new();
    private FilterRequest filter = new();
    private bool loading = true;

    protected override async Task OnInitializedAsync() => await LoadData();

    private async Task LoadData()
    {
        loading = true;
        try { data = await NovaFunkcijaService.GetDataAsync(filter); }
        finally { loading = false; }
    }
}
```

#### 6. Dodaj u NavMenu

```razor
<NavLink class="nav-link" href="nova-funkcija">
    <i class="bi bi-star"></i> Nova Funkcija
</NavLink>
```

---

## Export Funkcionalnosti

### Faktura PDF/Excel Export (v1.3.0+)

```csharp
// Inject u Blazor komponentu
@inject FakturaPdfService FakturaPdfService
@inject FakturaExcelService FakturaExcelService
@inject IFakturaService FakturaService

// Generisanje PDF fakture
var model = await FakturaService.UcitajDetalje(fakturaId);
var bytes = FakturaPdfService.GenerisiFakturu(model, naEngleskom: false);
// ili profaktura:
var bytes = FakturaPdfService.GenerisiProfakturu(model, naEngleskom: true);

// Generisanje Excel (lista svih faktura)
var bytes = FakturaExcelService.GenerisiListuFaktura(fakture);

// Generisanje Excel (detalji jedne fakture, 2 sheeta)
var bytes = FakturaExcelService.GenerisiDetalje(model);

// Download u browseru
await JS.InvokeVoidAsync("downloadFile", "faktura.pdf", Convert.ToBase64String(bytes));
```

**Domaće vs INO faktura** — automatski se određuje po `Kupac.Ino`:
- `Ino = false` → RSD kolone, PDV red u totalima, srpski bankovni računi
- `Ino = true` → EUR kolone, bez PDV reda, SWIFT/IBAN instrukcije

**SR/EN toggle** — prosleđuje se kao `naEngleskom: bool` parametar. Nezavisno od Ino flaga — korisnik može ručno birati jezik.

---

### SimpleExportService (opšti export)

Export se vrši kroz `SimpleExportService` (`IExportService`).

### Excel Export

```csharp
var bytes = ExportService.ExportToExcel(data, "NazivSheet");
await JS.InvokeVoidAsync("downloadFile", "naziv.xlsx", Convert.ToBase64String(bytes));
```

### PDF Export (QuestPDF sa ODETTA brendiranjem)

```csharp
var bytes = ExportService.ExportToPdf(data, "Naslov Izveštaja");
await JS.InvokeVoidAsync("downloadFile", "naziv.pdf", Convert.ToBase64String(bytes));
```

PDF automatski sadrži: ODETTA logo, kompanijske podatke u header-u, zebra pruge, srpski format brojeva, paginaciju u footer-u.

### Export dugmad u UI

```razor
@inject IExportService ExportService
@inject IJSRuntime JS

<button class="btn btn-success" @onclick="ExportExcel">
    <i class="bi bi-file-earmark-excel"></i> Excel
</button>
<button class="btn btn-danger" @onclick="ExportPdf">
    <i class="bi bi-file-earmark-pdf"></i> PDF
</button>
```

---

## Best Practices

### SQL - Uvek parameterizovano

```csharp
// DOBRO
var sql = "SELECT * FROM Table WHERE Datum >= @Start AND Datum <= @End";
await _db.QueryAsync<Model>(sql, new { Start = start, End = end });

// LOŠE - SQL Injection!
var sql = $"SELECT * FROM Table WHERE Datum >= '{start}'";
```

### Formatiranje

```csharp
// Datum: dd.MM.yyyy
item.Datum.ToString("dd.MM.yyyy")

// Broj sa srpskim formatom: 1.000,56
item.Iznos.ToString("N2", new CultureInfo("sr-Latn-RS"))
```

### Performance

```csharp
// CacheService za retko menjane podatke
var artikli = CacheService.GetOrCreate("artikli",
    () => ArtikalService.GetAllAsync(), TimeSpan.FromHours(1));

// Paginacija za velike dataset-ove
var (items, total) = await _db.QueryPagedAsync<Model>(sql, null, pageNumber, pageSize: 50);
```

### Error handling u Blazor komponentama

```csharp
private async Task LoadData()
{
    loading = true;
    try
    {
        data = await Service.GetDataAsync(filter);
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "Error loading data");
        errorMessage = $"Greška: {ex.Message}";
    }
    finally
    {
        loading = false;
    }
}
```

---

## Changelog

### v1.6.0 (8. mart 2026)

**Novo:**
- `Prezentacija.razor` — Nova stranica `/prezentacija` u **Promet Roba** sekciji
  - Gornji red: **Nabavka po Vrsti Voća** (column chart) | **Prodaja Gotovih Proizvoda** (column chart)
  - Ispod: **Nabavka po Vrsti Voća** — multi-series line chart kroz vreme (isti kao PrometHome)
  - Filter bar: datum od/do (default 01.06.2025 – danas), dugme Prikaži
  - Podaci: `BrziPregledService.UcitajUkupneVrednostiNabavkeAsync`, `UcitajProdajuGotovihProizvodaAsync`, `UcitajNabavkuPoDanimaPoVociAsync`
  - NavMenu: dodat link u Promet Roba dropdown (ikona `bi-bar-chart-line`)

**`ApexPieChart.razor` refaktoring:**
- Uklonjen `@try/@catch` blok koji je gušio render greške
- Uklonjen `ForceTableView` fallback parametar
- Dodat `@key` atribut (isti pattern kao `ApexBarChart`)
- Uklonjena `Responsive` lista sa nested `ApexChartOptions` (uzrokovala silent exception)
- `YValue` cast na `decimal?` za kompatibilnost sa Pie serijom
- Legenda uklonjena (`Show = false`) — zamenjena tabelom ispod charta
- Summary sekcija: tabela sa obojenom tačkom, naziv, količina, % po vrsti

**`ApexMultiSeriesLineChart.razor`:**
- Dodat `TickAmount = 12` na X osi — maksimalno 12 labela (sprečava gužvu za duže periode)

**`BrziPregledService.cs`:**
- Dodata metoda `UcitajProdajuGotovihProizvodaAsync` — FK- dokumenti, MagacinID=6, grupisano po `Artikal.Naziv`
- `UcitajUkupneVrednostiNabavkeAsync`, `UcitajUkupneVrednostiProdajeAsync`, `UcitajNabavkuPoDanimaPoVociAsync`, `UcitajProdajuPoDanimaPoVociAsync` — dodat `doDatum` parametar

**`DatabaseService.cs`:**
- `QueryAsync` i `QueryFirstOrDefaultAsync` — dodat `commandTimeout = 120` sekundi (bio 30s default) radi sprečavanja timeout grešaka na složenim upitima

---

### v1.5.0 (7. mart 2026)

**Novo:**
- `CenaKostanjaPoNalogu.razor` — Nova stranica `/cena-kostanja` premještena u **Troškovi** sekciju (bila u Prerada)
  - Izveštaj cene koštanja po radnom nalogu razložen po elementima troška
  - Filter bar: datum od/do (default 7 dana), komitent dropdown, radni nalog dropdown (padajuća lista, isti pattern kao RadniNaloziPregled)
  - Tabela: RN, Artikal/Pakovanje, Komitent, Količina kg/kom, Dir. rad RSD/kg, Režija RSD/kg, Materijal RSD/kg, Amb. primarna RSD/kg, Amb. sekundarna RSD/kg, Bez sirovine RSD/kg, Sa sirovinom RSD/kg
  - Expandable red — klik otvara detalje: Struktura troška (progress bars), Cena po kg (tabela), Info (radnici, sati, produktivnost)
  - Summary kartice po elementu troška (ukupno RSD + RSD/kg)
  - tfoot: prosek svih kolona po kg

**Izmene na CenaKostanjaPoNalogu:**
- Uklonjena kolona "Udeli %" iz tabele (colspan 13 → 12)
- Uklonjena kolona "Period" (ranije uklonjena)
- Uklonjena legenda ispod filtera
- Hover efekt (table-hover) uklonjen jer je izazivao beli tekst na beloj pozadini u expanded redu
- Expandovani red koristi `class="table-light"` bez hover CSS varijabli
- Radni nalog filter: iz `<input type="text">` u `<select>` padajuću listu koja poziva `PreradaService.UcitajSveRadneNaloge()`
- Broj radnika: računato kao `(int)Math.Round(BrojRadnihSati / 8)` umesto vrednosti iz baze
- Default datum filter: 7 dana unazad (umesto početak godine)
- "Sa sirovinom" kolona: prikazuje se tamno plavim fw-bold (bez badge-a)

**SQL izmene u PreradaService.UcitajCenuKostanjaPoRadnimNalozima:**
- `KolicinaGotovog` → direktno iz `rn.Kolicina` (umesto SUM iz vPreradaPregled_v2)
- `BrojPakovanja` → direktno iz `rn.BrojPakovanja`
- `TrosakDirektanRad` → direktno iz `rn.DirektanRadObracunato`
- `DatumPocetka` → `DATE(rn.DatumPocetka)` direktno
- Filter: `rn.DokumentStatus = 3` (samo zaključeni nalozi)
- `HAVING rn.Kolicina > 0`
- Materijal: `RpArtikalTip IN (1,3)` — tip 3 (povrat sirovine) oduzima se od tipa 1 (ulaz sirovine)
- Ambalaza: bez `RpArtikalTip` filtera, samo `MagacinID = 4` + `AmbalazaTip` razlikuje primarnu (≠3) od sekundarne (=3)
- CTE (`WITH ... AS`) zamenjeno inline korelisanim podupitima (MySQL server prestar za CTE sintaksu)
- `ORDER BY Version DESC LIMIT 1` za `KalkulacijaArtikalCena` (najnovija cena)

**NavMenu:**
- "Cena Koštanja" uklonjena iz Prerada dropdown-a
- "Cena Koštanja" dodana u Troškovi dropdown (posle "Troškovi radne snage", ikona `bi-coin`)
- "Cena Koštanja" dodana u sidebar Troškovi sekciju

---

### v1.4.0 (7. mart 2026)

**Novo:**
- `KontrolaRadniNalog.razor` — Nova stranica `/kontrola-radni-nalog` u Prerada sekciji
  - Filter bar: datum od/do, komitent dropdown, radni nalog dropdown
  - Lista radnih naloga koji imaju kontrole (max 200, ORDER BY DatumPocetka DESC)
  - Klik "Kontrole" — učitava sve kontrolne tabele paralelno za izabrani radni nalog
  - 5 sekcija: Kontrole u procesu, Metal detektor, Temperatura, Težina, Završna kontrola
- `KontrolaService.cs` — Servis koji učitava sve kontrolne tabele (implementira `IKontrolaService`)
  - `UcitajKontrole(radniNalogId)` — 6 SQL upita paralelno (`Task.WhenAll`)
  - `PretragaRadnihNaloga(filterRequest, komitent)` — pretraga za dropdown
- `IKontrolaService.cs` — Interfejs sa 2 metode
- `FinansijskoStanje.razor` — Nova stranica `/finansijsko-stanje` u Finansije sekciji
  - 4 tabova: Dobavljači / Kupci / Proizvođači / Otkupljivači
  - Svi tabovi se učitavaju paralelno (`Task.WhenAll`) pri kliku Pretraži
  - Tabela: Naziv, Potražuje (RSD), Duguje (RSD), Saldo (RSD), Zadnja promena, Čekanje (badge)
  - Filter: datum od/do, minimalni saldo (default 10.000 RSD)
  - Saldo > 0 (zeleno) = oni nam duguju; Saldo < 0 (crveno) = mi njima dugujemo
  - Badge "Čekanje": zeleno ≤30d, žuto ≤60d, crveno >60d od zadnje promene
  - Klik na naziv otvara Karticu komitenta (`/kartica-komitenta?komitentId=...`)

**Modeli:**
- `KontrolaRadniNalogModel.cs` — 6 klasa:
  - `KontrolaRadniNalogHeaderModel` — header (Komitent, Artikal, Pakovanje, LotNaloga, DatumPocetka, DatumZavrsetka)
  - `KontrolaProizvodnjaModel` — `ParsedNalaz` (Dictionary iz "Key: Value\n..." teksta), `RezultatBadge`
  - `KontrolaMetalDetektorModel` — H00-H23 (int? — 1=checked), `SatProveren(int sat)`, `BrojProverenihSati`
  - `KontrolaTemperaturaModel` — V00-V20 (string? temperature), N00-N20 (string? napomene), `GetMerenja()`
  - `KontrolaTezineModel` — isto kao Temperatura + `Vaga` polje
  - `KontrolaZavrsnaModel` — kao KontrolaProizvodnja + `Vozilo` polje
  - `KontrolaRadniNalogModel` (agregat) — Header + 5 listi + `SviKljuccevi...` za dinamičke kolone tabele
- `FinansijskoStanjeModel.cs` — Finansijsko stanje po komitent tipu
  - Polja: `KomitentID`, `Naziv`, `Potrazuje`, `Duguje`, `Saldo` (computed), `DatumZadnjePromene`
  - Computed: `DanaCekanja`, `KasnjenjeBadgeClass`, `SaldoTextClass`

**Servisi (prošireni):**
- `IFinansijeService` + `FinansijeService` — dodata metoda:
  - `UcitajFinansijskoStanje(filterRequest, tipKomitenta, minimumStanje)` — grupiše po komitent, filtrira po `k.JeKupac/JeDobavljac/JeProizvodjac/JeOtkupljivac`, `HAVING ABS(Saldo) > minimum`

**Tabele u bazi koje se koriste za Kontrole:**

| Tabela | Opis |
|--------|------|
| `KontrolaProizvodnja` | Kontrole u toku procesa — `Nalaz` je slobodan tekst "Key: Value\n..." |
| `KontrolaMetalDetektor` | H00-H23 (int? — 1=provereno) + Kolicina/Napomena |
| `KontrolaTemperatura` | V00-V20 (string? temperaturne vrednosti) + N00-N20 (napomene po merenju) |
| `KontrolaTezina` | Isto kao Temperatura + `Vaga` kolona |
| `KontrolaZavrsna` | Završna kontrola pre otpreme — `Nalaz` tekstualno + `Vozilo` |

**NavMenu:**
- "Kontrole RN" dodat u Prerada sekciju (ikona `bi-clipboard2-check`)
- "Finansijsko Stanje" dodat u Finansije sekciju (ikona `bi-graph-up-arrow`, samo Full Access)

---

### v1.3.0 (2. mart 2026)

**Novo:**
- `Fakture.razor` — Nova stranica `/fakture` za pregled i export faktura
  - Filter bar: datum od/do, kupac dropdown, status, tip (domaći/ino)
  - Toggle SR/EN dugme za jezik dokumenta
  - Expandable redovi — klik na fakturu otvara inline stavke
  - Summary kartice: broj faktura, ukupno RSD, ukupno EUR, INO count
  - Per-row dugmad: PDF Faktura, PDF Profaktura, Excel
- `FakturaService.cs` — Servis za fakture (implementira `IFakturaService`)
  - `UcitajFakture(filter, samoIno)` — lista sa JOIN Komitent
  - `UcitajDetalje(id)` — kompletan model: Faktura + Kupac + Otpremnica + Ugovor + Stavke
  - `UcitajStavke(id)` — stavke iz `FakturaStavka` JOIN `ArtikalInstanca/Artikal/Pakovanje`
  - `UcitajKupce()` — dropdown lista kupaca
  - Otpremnica SQL JOIN-uje `RadniNalog` za: `RadniNalogSifra`, `LotNaloga`, `RadniNalogBrojPakovanja`, `BrutoTezina`
- `FakturaPdfService.cs` — QuestPDF generator za fakture/profakture (SR/EN)
- `FakturaExcelService.cs` — ClosedXML generator (lista + detalji po fakturi)
- `IFakturaService.cs` — interfejs sa 4 metode

**Modeli:**
- `FakturaStavkaModel.cs` — stavke fakture iz `FakturaStavka` tabele
- `FakturaDetaljiModel.cs` — agregat: `KupacDetaljiModel`, `OtpremnicaInfoModel`, `UgovorInfoModel`, + `List<FakturaStavkaModel>`
- `OtpremnicaInfoModel` polja: `ID, Sifra, Datum, Vozilo, Vozac, RadniNalogSifra, LotNaloga, RadniNalogBrojPakovanja, BrutoTezina`

**PDF format faktura:**
- Beli header (bez pozadine), veći logo (100px), crni tekst
- Naslov dokumenta crn (18pt bold), desno od firme
- Transport sekcija (siva, između kupca i tabele): Vozilo/LKW NR, LOT, Radni nalog/Work Order, Broj pakovanja, Bruto težina/GROSS
- LOT povlači se iz `RadniNalog.LotNaloga` (ne iz stavki)
- Domaće fakture (Ino=false): RSD kolone, PDV red, "Slovima:", AIK Banka + Halkbank računi
- Inostrane fakture (Ino=true): EUR kolone, bez PDV reda, SWIFT/IBAN instrukcije, iznos u dinarima, PDV oslobođenje čl. 24
- Kolone tabele: `#`, Artikal, Pakovanje (šira, max 2 reda), Količina, Cena RSD/Price EUR, Iznos RSD/Amount EUR
- Svi iznosi i količine formatovani na 2 decimale (N2)

**SWIFT podaci (inostrana plaćanja):**
- Correspondent: SOGEFRPP – SOCIETE GENERALE, F-92978 PARIS FRANCE
- Acc. With Institution 57A: AIKBRS22 – AIK BANKA AD, BEOGRAD, BULEVAR MIHAILA PUPINA 115D, 11070 NOVI BEOGRAD
- Beneficiary IBAN 59: RS35105057012001081134 — ODETTA DOO, KRALJA DRAGUTINA 5, 7/31, ŠABAC

**Domaći bankovni računi ODETTA DOO:**
- AIK Banka AD Beograd: `105-14819-95`
- Halkbank AD Beograd: `155-30407-66`

**NavMenu:** Link "Fakture" dodat u Finansije sekciju (samo Full Access korisnici)

### v1.2.0 (22. decembar 2025)

**Novo:**
- `PregledSalda.razor` — Pregled salda svih komitenata (Potražuje/Duguje/Stanje), export, filteri
- `StanjeKaseAltiva.razor` — Stanje kese po proizvodima sa čuvanjem user selections
- `KesaSelekcijaService.cs` — Singleton za user preferences (JSON persistence u `Data/`)

**Refactoring:**
- `FinansijskiPregledService` uklonjen, svi metodi mergeni u `FinansijeService`
- `RobaZalihaModel` obrisan (nekorišćen)
- Svi servisi centralizovani u `ServiceCollectionExtensions.cs`

**Breaking change:** Svaki kod koji je koristio `IFinansijskiPregledService` mora da koristi `IFinansijeService`.

### v1.1.0 (24. oktobar 2025)

- PDF export sa ODETTA brendiranjem (logo, adresa, zebra pruge)
- Excel/PDF export samo vidljivih kolona (bez ID-eva)
- Srpski format brojeva i datuma
- Tooltips sa nazivima artikala u grafikonima
- Username-based RBAC sa dedicated home stranicom za ograničene korisnike

### v1.0.0 — Inicijalni release
