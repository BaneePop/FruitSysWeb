# Odetta Solar

Samostalna Blazor Server aplikacija za praćenje rada i korišćenja solarne elektrane (Huawei FusionSolar), za kompaniju **ODETTA DOO**, Šabac.

Nastala izdvajanjem iz FruitSysWeb ERP-a — ne sadrži poslovni (MySQL) kod te aplikacije.

## Tech Stack

- **ASP.NET Core 8.0** + **Blazor Server** (SignalR)
- **SQLite** (dva odvojena fajla — vidi ispod) + **Dapper** (micro-ORM)
- **Huawei FusionSolar API** (Playwright login + HTTP) za live podatke
- **Blazor-ApexCharts** + Custom dark theme
- **Serilog** (logging)

## Stranice

| Stranica | URL | Namena |
|----------|-----|--------|
| Proizvodnja uživo | `/solar-hala` | KPI, graf, preporuka za paljenje mašina |
| Dostupni podaci | `/solar-pregled` | Istorija, dnevni/mesečni grafici |

## Baze podataka

- `Data/solar.db` — istorijski i live solar podaci (FusionSolar KPI)
- `Data/app.db` — korisnici (lokalni login, PBKDF2 hash, bez MySQL-a)
- `Data/korisnik-aktivnost.json` — login/logout audit log

## Pokretanje lokalno

```bash
dotnet build
dotnet run
# Dostupno na: http://localhost:5073 (vidi Properties/launchSettings.json)
```

Prvi put — Playwright (za FusionSolar login):
```bash
pwsh bin/Debug/net8.0/playwright.ps1 install chromium
```

## Upravljanje korisnicima

Nema registracije kroz UI — nalozi se dodaju preko CLI alata (pokrenuti iz korena repoa):

```bash
dotnet run --project Tools/UserAdmin -- add <ime>       # kreira novog korisnika
dotnet run --project Tools/UserAdmin -- passwd <ime>    # resetuje lozinku
dotnet run --project Tools/UserAdmin -- disable <ime>   # deaktivira nalog
dotnet run --project Tools/UserAdmin -- list            # lista korisnika
```

## Struktura projekta

```
Odetta Solar/
├── Components/Pages/       # SolarHala.razor, SolarPregled.razor, Login.razor, Home.razor
├── Services/
│   ├── Solar/               # FusionSolarClient, SolarPollingService, SolarLocalDbService...
│   ├── Auth/                # PasswordHasher, AuthLocalDbService (SQLite login)
│   └── Implementations/     # AuthService, KorisnikAktivnostService
├── Models/                  # KorisnikModel, Solar/
├── Extensions/               # ServiceCollectionExtensions.cs — jedino mesto za DI
├── Tools/
│   ├── SolarHistoryProbe/   # CLI alat za proveru FusionSolar API-ja
│   └── UserAdmin/           # CLI alat za korisnike
└── Dokumentacija/            # Solar-Modul.md, Vizuelni-Smernice-Solar.md, API uputstvo
```

## Dokumentacija

- [DOCS.md](DOCS.md) — arhitektura, auth, dodavanje novih funkcionalnosti
- [DEPLOY.md](DEPLOY.md) — deployment na server
- [Dokumentacija/Solar-Modul.md](Dokumentacija/Solar-Modul.md) — tehnički pregled Solar modula
- [Dokumentacija/FusionSolar_API_Uputstvo.md](Dokumentacija/FusionSolar_API_Uputstvo.md) — API reference

## Status

Izdvojeno u samostalnu aplikaciju jul 2026 — uklonjen ceo FruitSysWeb poslovni sloj (MySQL, ~52 stranice, poslovni servisi), auth prebačen na lokalni SQLite. Sledeća faza: vizuelni redizajn (dark "glass" tema).
