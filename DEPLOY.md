# Odetta Solar — Deployment

Aplikacija još nije deployovana na produkcioni server (radi se lokalno). Ovaj dokument je skica za kad se to bude radilo — detalje (IP, IIS site ime) popuniti kad se prvi put deployuje.

## Build

```bash
dotnet publish FruitSysWeb.csproj -c Release -o ./publish
```

## Šta preneti na server

- Ceo `./publish` sadržaj
- **NE prepisivati**: `appsettings.Production.json` (kredencijali), `Data/` (baze), `Logs/`

## Preduslovi na serveru

- Windows Server sa IIS ili Windows Service, ili bilo koji drugi .NET 8 hosting
- **.NET 8.0 Hosting Bundle** (za IIS)
- IIS komponenta **WebSockets** (obavezno za Blazor Server/SignalR)
- **Playwright Chromium** — `pwsh bin/playwright.ps1 install chromium` (za FusionSolar login)
- Write pristup na `Data/` folder (SQLite baze) i `Logs/`

## Post-deployment provera

- Login stranica se učitava
- Login radi (test nalog kreiran preko `Tools/UserAdmin`)
- `/solar-hala` i `/solar-pregled` prikazuju podatke
- Logovi (`Logs/odetta-solar-*.log`) nemaju `[ERR]`
- `Data/app.db` i `Data/solar.db` postoje i imaju write pristup

## Troubleshooting

### "502.5 - Process Failure"

.NET 8.0 Hosting Bundle nije instaliran na serveru.

### Blazor SignalR ne radi

```powershell
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebSockets -All -NoRestart
iisreset
```

### FusionSolar login ne radi na serveru

Playwright Chromium nije instaliran, ili `FusionSolar:Enabled` je `false` u `appsettings.Production.json`.
