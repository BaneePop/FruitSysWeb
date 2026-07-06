# Solar modul — FruitSysWeb

**Grana:** `feature/solar-modul`  
**Status:** integrisan, izolovan od ostatka aplikacije — **pauzirano** (vidi `Solar-Stanje-Razvoja.md`)

---

## Rute

| Stranica | URL | Namena |
|----------|-----|--------|
| Proizvodnja uživo | `/solar-hala` | Dashboard za mašinsku službu |
| Dostupni podaci | `/solar-pregled` | Grafici i istorija iz SQLite |

Meni: **Energija** → obe stranice.

---

## Arhitektura

- **MySQL (fruitsysdb_v2)** — ne dira se
- **SQLite `Data/solar.db`** — lokalna istorija KPI (819+ redova iz copy-ja, jul 2025 → jul 2026)
- **FusionSolar API** — opciono, preko Playwright login-a
- **Feature flag:** `FusionSolar:Enabled` u config-u

---

## Konfiguracija

### Produkcija (`appsettings.json`)

```json
"FusionSolar": {
  "Enabled": false,
  ...
}
```

Polling isključen — stranice rade iz `solar.db`.

### Lokalni razvoj (`appsettings.Development.json`)

`Enabled: true` + kredencijali za live polling.

### Prvi put — Playwright

```bash
cd /Users/Bane/FruitSysWeb
dotnet build
pwsh bin/Debug/net8.0/playwright.ps1 install chromium
# ili: dotnet exec ... playwright install chromium
dotnet run
```

---

## Fajlovi modula

```
Services/Solar/          — API klijent, polling, SQLite
Models/Solar/            — DTO modeli
Models/SolarPodatakKatalog.cs
Components/Pages/SolarHala.razor
Components/Pages/SolarPregled.razor
wwwroot/css/solar-tokens.css
wwwroot/css/solar.css
wwwroot/images/solar/    — logo, pozadina, screenshot-i
Dokumentacija/FusionSolar_API_Uputstvo.md
```

---

## Dodatni istorijski podaci

Ako fale podaci u grafikonima, Bane može ponovo skinuti export iz FusionSolar-a.  
U sledećoj iteraciji: import skripta u `solar_kpi` (po dogovoru).

---

## Isključivanje modula

1. `FusionSolar:Enabled` → `false`
2. (opciono) ukloniti meni Energija iz `NavMenu.razor`
3. DI blok u `ServiceCollectionExtensions.cs` — komentarisati

Ostatak FruitSysWeb ostaje netaknut.
