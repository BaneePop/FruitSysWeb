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
│       └── ExportService/      # SimpleExportService, SledljivostExcelService, SledljivostHtmlService
├── Models/             # 49 modela + Filters/ + Sledljivost/
├── Constants/          # MagacinTypes, DocumentStatus, SezonaConstants
├── Extensions/         # ServiceCollectionExtensions (svi DI)
└── Database/           # OptimizacijeIndeksi.sql, create_korisnik_aktivnost.sql
```

## Dokumentacija

- [DOCS.md](DOCS.md) - Arhitektura, razvoj novih funkcionalnosti, baza podataka, best practices
- [DEPLOY.md](DEPLOY.md) - Deployment na server, update aplikacije, troubleshooting

## Verzija

**v1.5.0** (mart 2026) — Lager kretanje chart, Preostale Količine po Ugovoru, optimizacije.

### Šta je novo u v1.5.0

**Area chart "Stanje Lagera po Vrsti Voća":**
- Prikazuje kretanje lagera po vrsti voća (Malina, Kupina, Šljiva, Borovnica, Kajsija) od 01.06.2025 do danas
- Selektor intervala: Dnevno / Nedeljno / Mesečno
- Inicijalno stanje izračunato iz `MagacinLager` (trenutno stanje) minus promet od početka sezone
- Identifikacija vrsta voća po `PrvaKlasifikacijaID` (ne po nazivu)
- Količine iz `vPrometRobav6`: `LEFT(Dokument,2)='PR'` = Ulaz, `'OT'` = Izlaz, `DokumentStatus=3`
- Ukupno badge u summaryju (zbir svih vrsta na lageru)
- Chart dodat na stranice: **Prezentacija** i **LagerHome** (na vrhu)

**Tabela "Preostale Količine po Ugovoru" na PoslovanjeHome:**
- Prikazuje aktivne ugovore za isporuku: artikal, preostala količina, prosečna cena EUR, vrednost EUR
- SQL: `UgovorProdaja JOIN UgovorProdajaStavka` minus isporučeno po `Otpremnica` sa `DokumentStatus=3`
- Pozicionirana između kartica i Dnevnih Promena
- Footer sa ukupnim vrednostima, link ka `/ugovori`

**Optimizacije i čišćenje:**
- Nabavka bar chart (Prezentacija) ubrzano — uklonjen JOIN između `vPrometRobav6` i `vPrometFinansijev9`
- Uklonjen "Nabavka po Vrsti Voća" line chart sa Prezentacija stranice
- Uklonjen "Nabavka po Vrsti Voća" i "Prodaja po Vrsti Voća" line chartovi sa LagerHome (kod sačuvan kao `_DISABLED` metode)

---

**v1.4.0** (mart 2026) — Sledljivost HTML/Excel izvoz, reorganizacija Home stranica, Poslovanje meni.

### Šta je novo u v1.4.0

**Sledljivost — novi HTML izvoz:**
- Dugme "HTML Sledljivost" zamenjuje stare PDF i PDF Interaktivni dugmadi
- Generiše standalone `.html` fajl koji se može poslati kupcu mejlom
- Redosled sekcija: Otpremnice (Prodaja) → Evidencije Rada (Proizvodnja) → Prijemnice (Nabavka)
- Klik na accordion karticu (Otpremnica, Evidencija, Prijemnica) razvija stavke i Paletne Listove
- Klik na Paletni List otvara sve veze: Upstream (Prijemnica, LOT), Proizvodnja (Evidencija, Smenski, RN), Downstream (Otpremnica), Povezani PL sa artiklom i komitentom
- Otpremnice prikazuju samo PL koji imaju pakovanje
- Prijemnice prikazuju samo PL sirovine/ambalaze (bez gotove robe)
- Printabilno iz browsera (sve sekcije otvorene u print modu)

**Sledljivost — poboljšan Excel izvoz:**
- Isti redosled kao HTML: Prodaja → Proizvodnja → Nabavka
- Kolona "Veze/Napomene" sadrži sve dokumente vezane za PL (Prijemnica, LOT, Evidencija, RN, Otpremnica, Povezani PL sa artiklom i komitentom)
- Otpremnice — PL samo sa pakovanjem
- Uklonjene kolone Status

**Reorganizacija Home stranica:**
- `Home.razor` — 7 boksova u jednom redu: Prerada, Kvalitet, Finansije, Poslovanje, Lager, Troškovi, Promet Roba
- `FinansijeHome.razor` — 6 boksova, uklonjen link ka Poslovanju
- `PreradaHome.razor` — uklonjen boks Sledljivost
- `KvalitetHome.razor` — dodat boks Paletni List Pregled
- `LagerHome.razor` — dodat boks Stanje Kesa Altiva
- `TroskoviHome.razor` — dodat boks Cena Koštanja, svi boksovi u jednom redu (5 kolona)

**Navigacija — novi Poslovanje meni:**
- Izdvojen iz Finansije u poseban dropdown: Promene, Fakture, Ugovori, Kartice Komitenata
- Nova home stranica `/poslovanje` sa dnevnim prometom za tekući dan

**Promene izveštaj:**
- Popravljen PDF/Excel izvoz (ispravno JS ime funkcije i redosled parametara)
- Prazne tabele se ne prikazuju (ULAZ/IZLAZ/FINANSIJE kartice skrivene ako nema podataka)

---

**v1.3.0** (mart 2026) — Fakture, Poslovanje modul, Paletni List Pregled (Kvalitet tab), zaštita stranica od direktnog URL pristupa.

**v1.2.0** (22. decembar 2025) — Pregled Salda, Stanje Kese Altiva, centralizacija servisa.
