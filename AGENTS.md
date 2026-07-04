# AGENTS.md

## Cursor Cloud specific instructions

### Pregled
`FruitSysWeb` je .NET 8 **Blazor Server** web aplikacija (C#) za upravljanje voćarskim biznisom (dashboard, finansije, proizvodnja, lager, izveštaji). Podaci se čitaju preko **Dapper + MySqlConnector** iz **udaljene MySQL baze** definisane u `appsettings.json` (`ConnectionStrings:DefaultConnection`). Nema lokalne baze — aplikacija se oslanja na taj remote server.

### Servisi i komande
Jedan servis (web app). Standardne komande su u `README.md` i `*.sh` skriptama:
- Build: `dotnet build`
- Run (dev): `dotnet run --launch-profile http` → sluša na `http://localhost:5073`
- Lint: projekat nema poseban linter; `dotnet build` prijavljuje warnings (npr. CS0169 nekorišćena polja) i služi kao provera. Nema unit testova u repou.

### Non-obvious napomene
- `.NET 8 SDK (8.0.413)` je zakovan u `global.json`. SDK je već instaliran u snapshot-u (`/usr/share/dotnet`, symlink `/usr/local/bin/dotnet`); update script samo radi `dotnet restore`.
- `build.sh` sadrži hard-kodiran `cd /Users/nikola/FruitSysWeb` (autorova mašina) — ne koristiti ga direktno; pokretati `dotnet` komande iz root-a repoa (`/workspace`).
- `dotnet run` bez profila bira `https` profil (traži dev cert). Koristiti `--launch-profile http` za jednostavan HTTP na portu 5073.
- Aplikacija zahteva mrežni pristup remote MySQL bazi (`185.102.237.236:63388`). Ako baza nije dostupna, dashboard stranice prikazuju "Nema podataka" umesto pada; startup i dalje uspeva.
- ApexCharts servis registracija je namerno zakomentarisana u `Program.cs` (charts rade preko `@rendermode` u .NET 8).
