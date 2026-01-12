# FruitSysWeb - Change Log

## Verzija 1.2.0 - 22. decembar 2025

### 🎯 Nove Funkcionalnosti

#### 1. Pregled Salda Po Komitentu
- **Fajl:** `Components/Pages/PregledSalda.razor`
- **Opis:** Kompletna stranica za pregled salda svih komitenata (kupci i dobavljači)
- **Funkcionalnosti:**
  - Prikaz dugovanja i potraživanja po komitentu
  - Sortiranje po imenu, dugovanje, potraživanju
  - Export to Excel i PDF
  - Responsive dizajn
- **Model:** `Models/SaldoPoKomitentuModel.cs`
- **Servis:** `IFinansijeService.UcitajSaldoPoKomitentu()`
- **Pristup:** FinansijeHome → "Pregled Salda" dugme

#### 2. Stanje Kase Altiva
- **Fajl:** `Components/Pages/StanjeKaseAltiva.razor`
- **Opis:** Pregled stanja kese sa mogućnošću selekcije proizvoda
- **Funkcionalnosti:**
  - Multi-select proizvoda sa checkbox-ima
  - **Persistencija user izbora** (čuva se između sessions)
  - Real-time filtriranje podataka
  - Export to Excel i PDF
  - Responsive table sa fixed header
- **Model:** `Models/StanjeKeseSelekcija.cs`
- **Servis:** `Services/Core/KesaSelekcijaService.cs` (Singleton)
- **Data:** `Data/stanje-kese-selekcija.json` - čuva user preferences
- **Pristup:** FinansijeHome → "Stanje Kase Altiva" dugme

#### 3. KesaSelekcijaService - User Preferences System
- **Fajl:** `Services/Core/KesaSelekcijaService.cs`
- **Opis:** Singleton servis za čuvanje user selections
- **Funkcionalnosti:**
  - `GetSelections(username)` - Učitaj saved selections
  - `SaveSelections(username, selections)` - Sačuvaj selections
  - JSON file persistence: `Data/stanje-kese-selekcija.json`
  - Thread-safe operacije
- **DI Registration:** Singleton u `ServiceCollectionExtensions.cs`

---

### 🔄 Refactoring i Optimizacije

#### Centralizacija Tipova i Servisa

**1. FinansijskiPregledService → FinansijeService**
- **Obrisano:** `Services/Implementations/IzvestajService/FinansijskiPregledService.cs`
- **Obrisano:** `Services/Interfaces/IFinansijskiPregledService.cs`
- **Razlog:** Duplicirani funkcionalnost sa FinansijeService
- **Izmene:** Svi metodi prebačeni u `FinansijeService.cs`

**2. Novi Metodi u IFinansijeService**
```csharp
// Dodati metodi (prebačeni iz IFinansijskiPregledService):
Task<List<SaldoPoKomitentuModel>> UcitajSaldoPoKomitentu();
Task<List<StanjeKeseModel>> UcitajStanjeKese();
// ... ostali metodi
```

**3. Obrisani Nekorišćeni Modeli**
- **Obrisano:** `Models/RobaZalihaModel.cs`
- **Razlog:** Nekorišćen model, nema referenci u projektu

---

### 📝 Izmenjene Stranice (Update za Refactoring)

Sve stranice koje su koristile stari `IFinansijskiPregledService` su ažurirane da koriste `IFinansijeService`:

- `Components/Pages/Ambalaza.razor`
- `Components/Pages/FinansijeHome.razor` ← **DODATI NOVI DUGMIĆI**
- `Components/Pages/FinansijskiPregled.razor`
- `Components/Pages/Funansije.razor`
- `Components/Pages/LagerProizvodnje.razor`
- `Components/Pages/Nabavka.razor`
- `Components/Pages/NabavkaRoba.razor`
- `Components/Pages/PreradaHome.razor`
- `Components/Pages/Prodaja.razor`
- `Components/Pages/ProdajaRoba.razor`
- `Components/Pages/Proizvodnja.razor`
- `Components/Pages/RadniNaloziPregled.razor`
- `Components/Pages/SmenskiIzvestaji.razor`
- `Components/Pages/TroskoviHome.razor`

**Izmene:**
```csharp
// STARO:
@inject IFinansijskiPregledService FinansijskiPregledService

// NOVO:
@inject IFinansijeService FinansijeService
```

---

### 🛠️ Sistem Izmene

#### ServiceCollectionExtensions.cs
```csharp
// DODATO:
builder.Services.AddSingleton<KesaSelekcijaService>();

// OBRISANO:
builder.Services.AddScoped<IFinansijskiPregledService, FinansijskiPregledService>();

// Merged u:
builder.Services.AddScoped<IFinansijeService, FinansijeService>();
```

#### Program.cs
```csharp
// DODATO: Provera da Data folder postoji
var dataFolder = Path.Combine(builder.Environment.ContentRootPath, "Data");
if (!Directory.Exists(dataFolder))
{
    Directory.CreateDirectory(dataFolder);
}
```

#### Shared/NavMenu.razor
```csharp
// Možda ažurirano sa novim linkovima (proveri)
```

---

### 📦 Deployment Izmene

#### Novi Deployment Fajlovi:
- `deploy-to-windows-server-v1.2.0.ps1` - Ažurirana skripta sa:
  - Backup Data foldera
  - Restore Data foldera posle deployment-a
  - Permissions za Data folder (IIS_IUSRS write access)
  - Verifikacija `stanje-kese-selekcija.json`

#### Data Folder Requirements:
- **Path:** `C:\FruitSysWeb\Data\`
- **Permissions:** IIS_IUSRS - Modify (Read + Write)
- **Fajl:** `stanje-kese-selekcija.json` (auto-created)

---

### ⚠️ Breaking Changes

**VAŽNO:** Ova verzija ukida `IFinansijskiPregledService`!

**Ako imate custom kod koji koristi:**
```csharp
@inject IFinansijskiPregledService FinansijskiPregledService
```

**Morate ga zameniti sa:**
```csharp
@inject IFinansijeService FinansijeService
```

**Svi metodi su identični** - samo se promenio naziv servisa.

---

### 🐛 Bug Fixes

_(Nema eksplicitnih bug fix-ova u ovoj verziji - čisto nove funkcionalnosti i refactoring)_

---

### 📊 Statistika Izmena

```
Dodato:
  - 2 nove stranice (.razor)
  - 2 nova modela (.cs)
  - 1 novi servis (.cs)
  - 1 novi Data folder sa JSON persistence

Izmenjeno:
  - 27 fajlova (stranice + servisi)
  - 3 core servisa (FinansijeService, ServiceCollectionExtensions, Program)

Obrisano:
  - 2 servisa (FinansijskiPregledService + interface)
  - 1 model (RobaZalihaModel)
```

---

### 🔐 Security & Permissions

**Nova Konfiguracija:**

```powershell
# Data folder mora imati Write permissions:
icacls "C:\FruitSysWeb\Data" /grant "IIS_IUSRS:(OI)(CI)M" /T /Q
```

**Razlog:** KesaSelekcijaService zapisuje user preferences u JSON fajl.

---

### 🎨 UI/UX Izmene

**FinansijeHome.razor:**
- Dodati 2 nova dugmeta:
  - "Pregled Salda" (btn-primary)
  - "Stanje Kase Altiva" (btn-success)

**StanjeKaseAltiva.razor:**
- Dark theme responsive table
- Fixed header sa scroll-om
- Multi-select checkbox-i
- "Selekcija proizvoda" sekcija sa badge-ima

**PregledSalda.razor:**
- Responsive table sa sortiranjem
- Color-coded dugovanje (text-danger) i potraživanja (text-success)
- Export buttons

---

### 📚 Dokumentacija

**Novi Fajlovi:**
- `DEPLOYMENT-v1.2.0-INSTRUKCIJE.md` - Kompletan deployment guide
- `CHANGELOG-v1.2.0.md` - Ovaj fajl
- `PREGLED-SALDA-DOKUMENTACIJA.md` - Dokumentacija za Pregled Salda
- `SALDO-LOGIKA-ISPRAVKA.md` - Logika salda kalkulacije
- `REFACTORING-REPORT.md` - Detaljni refactoring report
- `REFACTORING-SUMMARY.md` - Kratak summary refactoring-a

---

### 🚀 Deployment Informacije

**Build:**
- Datum: 22. decembar 2025, 18:04:59
- .NET Runtime: 8.0
- Configuration: Release
- Platform: Any CPU
- Self-contained: Ne (zahteva .NET 8.0 Runtime na serveru)

**Deployment Paket:**
- Naziv: `FruitSysWeb-Production-v1.2.0-20251222-180459.zip`
- Veličina: ~39 MB
- Kompresija: ZIP (deflate)

**Deploy Skripta:**
- `deploy-to-windows-server-v1.2.0.ps1`
- Podržava: IIS, Windows Service, Standalone

---

### ✅ Testing Checklist

**Pre Release:**
- [x] Build uspešan (Release mode)
- [x] Deployment paket kreiran
- [x] Deployment skripta testirana
- [ ] Pregled Salda - manual testing (treba uraditi na serveru)
- [ ] Stanje Kase Altiva - manual testing (treba uraditi na serveru)
- [ ] Regression testing - stare funkcionalnosti (treba uraditi na serveru)

**Post-Deployment:**
- [ ] Login radi
- [ ] Pregled Salda učitava podatke
- [ ] Stanje Kase čuva selections
- [ ] Export to Excel radi
- [ ] Export to PDF radi
- [ ] Logovi nemaju ERROR poruka

---

### 🔗 Git Commit Info

**Branch:** `feature/centralize-types`
**Ahead of origin:** 1 commit

**Staged Changes:** Sve nove funkcionalnosti
**Untracked Files:** Dokumentacija i Data folder

---

### 📞 Support

**Za probleme sa deployment-om:**
1. Proveri `DEPLOYMENT-v1.2.0-INSTRUKCIJE.md`
2. Pogledaj Troubleshooting sekciju
3. Kontaktiraj razvoj tim

---

## Verzija 1.1.0 - 5. novembar 2024

_(Prethodna verzija - vidi prethodni changelog)_

---

## Verzija 1.0.0 - Inicijalna Verzija

_(Prvi production release)_

---

**Kraj Changelog-a za v1.2.0**
