# FruitSysWeb

Blazor Server (.NET 8) aplikacija za upravljanje voćarskim/prehrambenim biznisom (Dashboard, Finansije, Proizvodnja, Lager, Izveštaji). Podaci se čitaju iz udaljene MySQL baze preko Dapper-a (`MySqlConnector`).

## Cursor Cloud specific instructions

### Servisi
Jedan servis: Blazor Server web app.

- Run (dev): `dotnet run --launch-profile http` → sluša na `http://localhost:5073` (HTTPS profil je `https://localhost:7067`).
- Build: `dotnet build`
- Lint / analiza: nema zaseban linter; koristi `dotnet build` (compiler warnings) ili `dotnet format`.
- Testovi: u repozitorijumu ne postoji test projekat.

### Baza podataka
- Connection string je već upisan u `appsettings.json` i pokazuje na **udaljeni** MySQL server (`fruitsysdb_v2`). Ne treba lokalni MySQL — aplikacija se povezuje direktno preko mreže.
- DB nalog je read-only, pa su moduli namenjeni pregledu/izveštavanju; ne očekuj upis podataka.

### Gotchas
- `dotnet` je instaliran u `$HOME/.dotnet` i dodat u PATH preko `~/.bashrc`. U neinteraktivnim shell-ovima pozovi `$HOME/.dotnet/dotnet` ako `dotnet` nije na PATH-u.
- Aplikacija pri startu radi self-check servisa i ispisuje `Database service registered successfully` / `PDF generation test: PASSED` u konzolu — to je normalno.
- `AddApexCharts()` je namerno zakomentarisan u `Program.cs`; ne odkomentarisati bez razloga.
