# Solar modul — Stanje razvoja

**Datum:** 4. jul 2026.  
**Projekat:** FruitSysWeb  
**Grana:** `feature/solar-modul`  
**Status:** ⏸ Pauzirano — funkcionalno radi, vizuelno doterano dovoljno za sada

---

## Cilj modula

Odvojen „sandbox“ modul za praćenje solarne elektrane (Huawei FusionSolar). Služi i kao **vizuelni lab** za budući redesign cele aplikacije. **Ne dira MySQL** produkcijsku bazu.

---

## Rute

| Stranica | URL | Namena |
|----------|-----|--------|
| Proizvodnja uživo | `/solar-hala` | Dashboard za mašinsku službu — KPI, graf, preporuka za paljenje mašina |
| Dostupni podaci | `/solar-pregled` | Istorija, dnevni/mesečni grafici, kumulativi iz SQLite |

Meni: **Energija** → obe stranice.

---

## Arhitektura

| Sloj | Tehnologija |
|------|-------------|
| Produkcijska baza | MySQL `fruitsysdb_v2` — **ne koristi se** |
| Lokalna istorija | SQLite `Data/solar.db` |
| Live podaci | FusionSolar API (Playwright login + HTTP) |
| Grafici | Blazor-ApexCharts 3.5.0 |
| Feature flag | `FusionSolar:Enabled` u `appsettings.json` |

### Fajlovi

```
Services/Solar/
  FusionSolarClient.cs       — API klijent, Playwright login
  FusionSolarOptions.cs      — konfiguracija
  SolarPollingService.cs     — background polling (60s)
  SolarIntradaySyncService.cs — sync intraday tačaka iz API-ja
  SolarLocalDbService.cs     — SQLite CRUD
  SolarChartOptionsFactory.cs — ApexCharts tema (kW, tamna tema)

Models/Solar/FusionSolarModels.cs
Models/SolarPodatakKatalog.cs — DN invertera, naziv postrojenja

Components/Pages/SolarHala.razor
Components/Pages/SolarPregled.razor

wwwroot/css/solar-tokens.css
wwwroot/css/solar.css
wwwroot/images/solar/

Dokumentacija/FusionSolar_API_Uputstvo.md
Dokumentacija/Solar-Modul.md
Dokumentacija/Vizuelni-Smernice-Solar.md
Tools/SolarHistoryProbe/     — CLI alat za proveru API-ja
```

---

## Šta je urađeno

### Faza 0 — Priprema ✅
- Grana `feature/solar-modul` (odvojeno od `feature/centralize-types`)
- Prenos koda iz `FruitSysWeb copy` (Claude Code, jul 2026)
- Inventar: kod, slike, dokumentacija

### Faza 1 — Integracija ✅
- DI registracija u `ServiceCollectionExtensions.cs`
- Meni **Energija** u `NavMenu.razor`
- `solar.db` prenet (jul 2025 → jul 2026)
- `FusionSolar:Enabled` — `false` u produkciji, `true` u Development
- Paketi: Microsoft.Data.Sqlite, Microsoft.Playwright
- Stranice rade iz lokalne baze čak i kad polling nije uključen

### Faza 2 — Vizuelno + podaci ✅ (delimično)

**SolarPregled (`/solar-pregled`):**
- KPI kartice (trenutno + dnevni kumulativi)
- Grafici: intraday snaga, dnevna proizvodnja, dnevna ušteda
- Istorijski podaci iz SQLite + FusionSolar export polja (`daily_*`)

**SolarHala (`/solar-hala`):**
- Intraday graf (Sunce / Hala / Mreža) — ispravljen upit za lokalni dan
- `SolarIntradaySyncService` — povlačenje tačaka iz API-ja u `solar.db`
- Preporučeni period za paljenje mašina (pokrivenost ≥ 70%)
- Layout preuređen (jul 2026):
  - 5 kompaktnih KPI kartica u jednom redu
  - Chart ~70% + kartica „Preporuka za mašine“ desno
  - 3 mini stat kartice ispod (najbolji period, max solar, max mreža)
  - Detalji (inverteri, mreža, alarmi, dnevni rezime) u sklopivom `<details>`
- CSS izvučen u `solar.css` / `solar-tokens.css`
- Uklonjen konflikt `.solar-header` sa `shared-components.css`

**Podaci u bazi (4. jul 2026.):**
- `solar_kpi`: **1341 redova** (2025-07-12 → 2026-07-04)
- Intraday tačke za tekući dan dok je polling aktivan

---

## Šta nije završeno / poznata ograničenja

| Oblast | Status |
|--------|--------|
| Layout `/solar-hala` na svim rezolucijama | 🟡 Bane nije finalno potvrdio posle poslednjeg CSS fix-a |
| Live polling na produkciji | ⬜ `Enabled: false` — namerno |
| Import skripta za Excel export iz FusionSolar-a | ⬜ Planirano po potrebi |
| `playwright install chromium` na serveru | ⬜ Bane jednom lokalno; produkcija nije podešena |
| Primena Solar dizajna na celu app | ⬜ Faza 5 — nije počela |
| Git commit Solar modula | ⬜ Nije commitovano (grana lokalna) |

---

## Konfiguracija

### Produkcija (`appsettings.json`)
```json
"FusionSolar": {
  "Enabled": false,
  "BaseUrl": "https://uni005eu5.fusionsolar.huawei.com",
  "StationDn": "NE=182162915",
  "PollIntervalSeconds": 60
}
```

### Lokalni razvoj (`appsettings.Development.json`)
- `Enabled: true` + kredencijali (User Secrets preporučeno)

### Prvi put — Playwright
```bash
cd /Users/Bane/FruitSysWeb
dotnet build
pwsh bin/Debug/net8.0/playwright.ps1 install chromium
dotnet run
```

---

## Kako isključiti modul (bez rizika)

1. `FusionSolar:Enabled` → `false`
2. (opciono) ukloniti meni **Energija** iz `NavMenu.razor`
3. (opciono) komentarisati Solar DI blok u `ServiceCollectionExtensions.cs`

Ostatak FruitSysWeb ostaje netaknut.

---

## Šta sledi (kad se vrati na Solar)

1. Bane potvrđuje layout na `/solar-hala` (hard refresh)
2. Provera live polling-a lokalno
3. (Opciono) import dodatnih istorijskih Excel fajlova u `solar.db`
4. Finalizacija design kit-a → `Vizuelni-Smernice-Solar.md` → primena na celu app (Faza 5)

---

## Povezani dokumenti

- `Dokumentacija/Solar-Modul.md` — tehnički pregled
- `Dokumentacija/Vizuelni-Smernice-Solar.md` — boje, kartice, pravila migracije
- `Dokumentacija/FusionSolar_API_Uputstvo.md` — API reference
