# FruitSysWeb - ODETTA DOO

ERP sistem za upravljanje operacijama prerade voća. Blazor Server aplikacija razvijena za kompaniju **ODETTA DOO**, Šabac.

## Tech Stack

- **ASP.NET Core 8.0** + **Blazor Server** (SignalR)
- **MySQL** (`fruitsysdb_v2`) + **Dapper** (micro-ORM, bez Entity Framework)
- **QuestPDF** (PDF export) + **ClosedXML** (Excel export)
- **Bootstrap 5.3** + **ApexCharts** + Custom dark theme
- **Serilog** (logging) + **IIS** (Windows Server deployment)

## Moduli

| Modul | Stranice | Pristup |
|-------|----------|---------|
| **Prerada** | Proizvodnja, Radni Nalozi, Smenski Izvestaji, Sledljivost, Kontrola RN | Svi |
| **Kvalitet** | Sledljivost, Kontrole RN, Reklamacije, Paletni List Pregled | Svi |
| **Lager** | Lager, Ambalaza, Roba, Lager Proizvodnje, Stanje Kesa Altiva | Svi |
| **Troškovi** | Troškovi Proizvodnje, Radne Snage, Cena Koštanja | Svi |
| **Troškovi** | Troškovi Ambalaze, Troškovi Poslovanja | Samo admin |
| **Finansije** | Nabavka, Prodaja, Finansijski Pregled, Pregled Salda, Finansijsko Stanje | Samo admin |
| **Poslovanje** | Promene, Fakture, Ugovori, Kartice Komitenata | Samo admin |
| **Promet** | Izvestaj Prijem, Povratna Ambalaza, Paletni List Pregled | Svi |

## Korisnici i Pristup

**Ograničeni korisnici** (nema pristup Finansijama i Poslovanju):
`zoran`, `jelena`, `pedja`, `radmila`, `masinska`, `BaneT`

**Admin korisnici**: Pun pristup svim modulima.

Sve stranice sa ograničenim pristupom imaju server-side redirect proveru — direktan URL pristup nije moguć bez ovlašćenja.

## Pokretanje lokalno

```bash
cd /Users/Bane/FruitSysWeb
dotnet restore
dotnet run
# Dostupno na: https://localhost:5001
```

```bash
# Live reload tokom razvoja
dotnet watch run
```

## Struktura projekta

```
FruitSysWeb/
├── Components/
│   ├── Pages/          # 36+ Blazor stranica
│   ├── Charts/         # ApexCharts komponente + ChartDataHelper
│   ├── Shared/         # Filteri (DateFilter, KomitentFilter...), DataTable, ExportButtons
│   └── Layout/         # MainLayout, NavMenu, EmptyLayout
├── Services/
│   ├── Core/           # DatabaseService, BaseService, CacheService, TypeMappingService
│   ├── Interfaces/     # 20 service interfejsa
│   └── Implementations/
│       ├── IzvestajService/    # 13 report servisa (ProizvodnjaService, FinansijeService...)
│       └── ExportService/      # SimpleExportService, SledljivostPdfService...
├── Models/             # 49 modela + Filters/ + Sledljivost/
├── Constants/          # MagacinTypes, DocumentStatus, SezonaConstants
├── Extensions/         # ServiceCollectionExtensions (svi DI)
└── Database/           # OptimizacijeIndeksa.sql
```

## Dokumentacija

- [DOCS.md](DOCS.md) - Arhitektura, razvoj novih funkcionalnosti, baza podataka, best practices
- [DEPLOY.md](DEPLOY.md) - Deployment na server, update aplikacije, troubleshooting

## Verzija

**v1.3.0** (mart 2026) — Fakture, Poslovanje modul, Paletni List Pregled (Kvalitet tab), zaštita stranica od direktnog URL pristupa.

**v1.2.0** (22. decembar 2025) — Pregled Salda, Stanje Kase Altiva, centralizacija servisa.
