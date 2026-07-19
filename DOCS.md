# Odetta Solar — Razvojna Dokumentacija

## Arhitektura

```
Presentation Layer  →  Components/Pages/ (Blazor)
Service Layer       →  Services/Solar/, Services/Auth/, Services/Implementations/
Data Access Layer   →  SolarLocalDbService, AuthLocalDbService (SQLite + Dapper)
Domain Layer        →  Models/
```

Svi servisi se registruju **isključivo** u `Extensions/ServiceCollectionExtensions.cs` (`AddOdettaSolarServices`). Ne dodavati u `Program.cs`.

**Nema MySQL, nema Entity Framework.** Dva odvojena SQLite fajla:
- `Data/solar.db` — solarni podaci (`SolarLocalDbService`)
- `Data/app.db` — korisnici (`AuthLocalDbService`)

Razdvojeni namerno — solar polling piše u `solar.db` na svakih 60s, auth se retko čita/piše; razdvajanje sprečava lock-contention.

## Autentifikacija

- Lokalni login, bez MySQL/eksternih servisa
- Lozinke: PBKDF2-HMAC-SHA256, 210.000 iteracija, so po korisniku (`Services/Auth/PasswordHasher.cs`)
- Sesija: `ProtectedSessionStorage` (browser sessionStorage, enkriptovano Data Protection API-jem) — per-tab
- Nema nivoa pristupa — svaki ulogovan korisnik ima pun pristup (aplikacija ima samo 2 stranice)
- `InactivityHandler` — auto-logout posle 30 min neaktivnosti; izuzeci u `appsettings.json` → `Auth:InactivityExemptUsers`
- Korisnici se dodaju preko `Tools/UserAdmin` CLI alata — nema UI za registraciju

Tok: `Login.razor` → `IAuthService.Login()` → `AuthLocalDbService.GetByImeAsync()` → `PasswordHasher.Verify()` → upis u `ProtectedSessionStorage` → redirect na `/solar-hala`.

## Solar modul

- `FusionSolarClient.cs` — Playwright login na FusionSolar portal + HTTP pozivi ka API-ju
- `SolarPollingService.cs` — background servis, poll na 60s, upisuje u `solar.db`
- `SolarIntradaySyncService.cs` — povlači intraday tačke (backfill)
- `SolarLocalDbService.cs` — sav SQL prema `solar.db`

Feature flag: `FusionSolar:Enabled` u `appsettings.json` (isključen u produkciji dok se ne potvrdi).

## Dodavanje nove stranice

```razor
@page "/nova-stranica"
@using FruitSysWeb.Services.Interfaces
@inject IAuthService AuthService

@code {
    protected override async Task OnInitializedAsync()
    {
        // Stranica je već zaštićena globalno preko AuthorizeView u Pages/App.razor
        // — nije potrebna dodatna provera osim ako stranica treba specifičnu logiku.
    }
}
```

Novi servis — registruj u `Extensions/ServiceCollectionExtensions.cs`, ne u `Program.cs`.

## Konvencije koda

- Privatni field-ovi: `_camelCase`
- Async/await za sve DB operacije, nikad `.Result`/`.Wait()` (deadlock rizik u Blazor Server)
- SQL: uppercase keywords, snake_case kolone (SQLite tabele)
- Komentari u kodu: srpski jezik

## Changelog

**Jul 2026** — Izdvajanje u samostalnu aplikaciju: uklonjen FruitSysWeb poslovni sloj (MySQL, ~52 stranice, poslovni servisi/modeli/kontroleri), auth prebačen sa MySQL+MD5 na SQLite+PBKDF2, uklonjeni nivoi pristupa (ulogovan = pun pristup), dodat `Tools/UserAdmin` CLI, očišćena dokumentacija/duplikati.
