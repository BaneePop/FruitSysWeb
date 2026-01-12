# FruitSysWeb Deployment v1.2.0 - Instrukcije

**Datum:** 22. decembar 2025
**Verzija:** 1.2.0
**Build:** FruitSysWeb-Production-v1.2.0-20251222-180459.zip

---

## 🎯 Nove Funkcionalnosti u v1.2.0

### 1. **Pregled Salda Po Komitentu** (`PregledSalda.razor`)
   - Nova stranica za pregled salda svih komitenata
   - Detaljni prikaz finansijskih obaveza i potraživanja
   - Export u PDF i Excel format
   - Pristup: FinansijeHome -> Pregled Salda

### 2. **Stanje Kase Altiva** (`StanjeKaseAltiva.razor`)
   - Pregled stanja kese po proizvodima
   - Selekcija proizvoda sa čuvanjem user preferences
   - KesaSelekcijaService za persistenciju izbora
   - Data folder: `stanje-kese-selekcija.json`
   - Pristup: FinansijeHome -> Stanje Kase Altiva

### 3. **Refactoring i Centralizacija Tipova**
   - Uklonjeni duplicirani servisi:
     - `FinansijskiPregledService` → Merged u `FinansijeService`
     - `RobaZalihaModel` → Obrisano (nekorišćeno)
   - Novi modeli:
     - `SaldoPoKomitentuModel.cs`
     - `StanjeKeseSelekcija.cs`
   - Novi servis:
     - `KesaSelekcijaService.cs` (Singleton za user preferences)

### 4. **Ostale Izmene**
   - Ažurirane sve stranice koje su koristile stare servise
   - ServiceCollectionExtensions.cs - dodati novi servisi
   - Program.cs - konfiguracija za Data folder

---

## 📦 Deployment Paket

### Sadržaj:
```
FruitSysWeb-Production-v1.2.0-20251222-180459.zip
├── publish/
│   ├── FruitSysWeb.dll (2.1 MB - GLAVNA APLIKACIJA)
│   ├── appsettings.json
│   ├── Logo.png (OBAVEZNO za PDF export!)
│   ├── Data/
│   │   └── stanje-kese-selekcija.json (User preferences)
│   ├── LatoFont/ (Fontovi za PDF)
│   ├── wwwroot/ (CSS, JS, Bootstrap, ApexCharts)
│   └── [Sve biblioteke: Dapper, QuestPDF, ClosedXML, Serilog...]
```

**Veličina:** ~39 MB

---

## 🚀 Deployment Postupak

### **OPCIJA 1: Automatski Deployment (Preporučeno)**

#### Windows Server sa IIS:

```powershell
# 1. Kopiraj ZIP i skriptu na server
# 2. Otvori PowerShell kao Administrator
# 3. Navigiraj do foldera sa ZIP-om

cd C:\Temp\FruitSysWeb-Deployment

# 4. Pokreni deployment skriptu
.\deploy-to-windows-server-v1.2.0.ps1 -IIS -AppPoolName "FruitSysWeb"
```

#### Windows Server sa Windows Service:

```powershell
.\deploy-to-windows-server-v1.2.0.ps1 -Service
```

#### Standalone (bez IIS):

```powershell
.\deploy-to-windows-server-v1.2.0.ps1 -Standalone
```

---

### **OPCIJA 2: Ručni Deployment**

#### Korak 1: Backup Postojeće Aplikacije

```powershell
# Zaustavi IIS Application Pool
Stop-WebAppPool -Name "FruitSysWeb"

# Kreiraj backup
$backupFolder = "C:\FruitSysWeb-Backup-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
Copy-Item -Path "C:\FruitSysWeb\*" -Destination $backupFolder -Recurse -Force

# Backup appsettings.Production.json (VAŽNO!)
Copy-Item -Path "C:\FruitSysWeb\appsettings.Production.json" -Destination "C:\Temp\" -Force

# Backup Data foldera (NOVO!)
Copy-Item -Path "C:\FruitSysWeb\Data" -Destination "C:\Temp\Data-backup" -Recurse -Force
```

#### Korak 2: Očisti Stare Fajlove

```powershell
# PAŽNJA: NE brišite Logs, appsettings.Production.json, i Data folder!
Get-ChildItem -Path "C:\FruitSysWeb" -Exclude "appsettings.Production.json","Logs","Data" | Remove-Item -Recurse -Force
```

#### Korak 3: Raspakuj Novi Deployment

```powershell
# Raspakuj ZIP
Expand-Archive -Path "FruitSysWeb-Production-v1.2.0-20251222-180459.zip" -DestinationPath "C:\Temp\Deploy" -Force

# Kopiraj fajlove
Copy-Item -Path "C:\Temp\Deploy\publish\*" -Destination "C:\FruitSysWeb\" -Recurse -Force
```

#### Korak 4: Vrati Backup Config Fajlove

```powershell
# Vrati appsettings.Production.json
Copy-Item -Path "C:\Temp\appsettings.Production.json" -Destination "C:\FruitSysWeb\" -Force

# Vrati Data folder (sačuvaj user preferences)
if (Test-Path "C:\Temp\Data-backup\stanje-kese-selekcija.json") {
    Copy-Item -Path "C:\Temp\Data-backup\*" -Destination "C:\FruitSysWeb\Data\" -Force
}
```

#### Korak 5: Proveri Permissions (IIS)

```powershell
# Postavi permissions za IIS_IUSRS
icacls "C:\FruitSysWeb" /grant "IIS_IUSRS:(OI)(CI)F" /T /Q
icacls "C:\FruitSysWeb\Data" /grant "IIS_IUSRS:(OI)(CI)M" /T /Q
icacls "C:\FruitSysWeb\Logs" /grant "IIS_IUSRS:(OI)(CI)M" /T /Q
```

#### Korak 6: Pokreni Aplikaciju

```powershell
# IIS
Start-WebAppPool -Name "FruitSysWeb"

# Ili Standalone
cd C:\FruitSysWeb
.\FruitSysWeb.exe
```

---

## ✅ Post-Deployment Verifikacija

### 1. **Proveri da li Aplikacija Radi**

```
http://192.168.1.11:6001
```

- Login stranica se učitava? ✅
- Login radi (username/password)? ✅

### 2. **Testiraj Nove Funkcionalnosti**

#### A) Pregled Salda Po Komitentu
1. Uloguj se kao admin korisnik
2. Idi na: **FinansijeHome** → **Pregled Salda**
3. Proveri da li se učitava lista komitenata
4. Testiraj Export to Excel i PDF

#### B) Stanje Kase Altiva
1. Uloguj se kao admin korisnik
2. Idi na: **FinansijeHome** → **Stanje Kase Altiva**
3. Proveri da li se učitava lista proizvoda
4. Selektuj neke proizvode
5. Refresh stranicu - proveri da li su selections sačuvane ✅
6. Proveri da li postoji fajl: `C:\FruitSysWeb\Data\stanje-kese-selekcija.json`

### 3. **Testiraj Stare Funkcionalnosti (Regression Testing)**

| Modul | Stranica | Test |
|-------|----------|------|
| Prerada | Proizvodnja | Učitavanje podataka |
| Prerada | Radni Nalozi | Filter po datumu |
| Prerada | Sledljivost | Pretraga po šifri |
| Lager | Magacin Lager | Prikaz svih magacina |
| Finansije | Nabavka | Export to Excel |
| Finansije | Prodaja | Chart prikaz |

### 4. **Proveri Logove**

```powershell
# Proveri da li ima grešaka
Get-Content "C:\FruitSysWeb\Logs\fruitsysweb-*.txt" -Tail 50
```

**Trebalo bi da vidite:**
- `[INF]` poruke - OK ✅
- `[WRN]` poruke - Proveri ⚠️
- `[ERR]` poruke - GREŠKA ❌

---

## 🔧 Troubleshooting

### Problem 1: Aplikacija se ne pokreće

**Proveri:**
```powershell
# 1. Da li je .NET 8.0 Runtime instaliran?
dotnet --version
# Trebalo bi: 8.0.x

# 2. Da li postoji FruitSysWeb.dll?
Test-Path "C:\FruitSysWeb\FruitSysWeb.dll"

# 3. Da li postoji appsettings.Production.json?
Test-Path "C:\FruitSysWeb\appsettings.Production.json"
```

**Rešenje:**
- Instaliraj .NET 8.0 Runtime: https://dotnet.microsoft.com/download/dotnet/8.0
- Vrati fajlove iz backup-a

---

### Problem 2: "Stanje Kase Altiva" ne čuva selections

**Proveri:**
```powershell
# Da li postoji Data folder?
Test-Path "C:\FruitSysWeb\Data"

# Da li IIS_IUSRS ima Write permissions?
icacls "C:\FruitSysWeb\Data"
```

**Rešenje:**
```powershell
# Kreiraj folder
New-Item -ItemType Directory -Path "C:\FruitSysWeb\Data" -Force

# Postavi permissions
icacls "C:\FruitSysWeb\Data" /grant "IIS_IUSRS:(OI)(CI)M" /T /Q
```

---

### Problem 3: PDF Export ne radi

**Proveri:**
```powershell
# Da li postoji Logo.png?
Test-Path "C:\FruitSysWeb\Logo.png"
```

**Rešenje:**
```powershell
# Kopiraj iz ZIP-a ponovo
Copy-Item -Path "C:\Temp\Deploy\publish\Logo.png" -Destination "C:\FruitSysWeb\" -Force
```

---

### Problem 4: Greška "FinansijskiPregledService not found"

**Uzrok:** Stara verzija aplikacije ne prepoznaje refactoring

**Rešenje:**
```powershell
# 1. Potpuno očisti application pool cache
Stop-WebAppPool -Name "FruitSysWeb"
Remove-Item -Path "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\Temporary ASP.NET Files\*" -Recurse -Force -ErrorAction SilentlyContinue

# 2. Recycle application pool
Start-WebAppPool -Name "FruitSysWeb"
```

---

## 🔄 Rollback (Vrati Staru Verziju)

Ako deployment ne radi:

```powershell
# 1. Zaustavi aplikaciju
Stop-WebAppPool -Name "FruitSysWeb"

# 2. Obriši novu verziju
Remove-Item -Path "C:\FruitSysWeb\*" -Recurse -Force -Exclude "Logs"

# 3. Vrati backup (npr. C:\FruitSysWeb-Backup-20251222-180500)
$backupFolder = "C:\FruitSysWeb-Backup-20251222-180500"
Copy-Item -Path "$backupFolder\*" -Destination "C:\FruitSysWeb\" -Recurse -Force

# 4. Pokreni aplikaciju
Start-WebAppPool -Name "FruitSysWeb"
```

---

## 📊 Changelog (v1.1.0 → v1.2.0)

### Dodato ✅
- `Components/Pages/PregledSalda.razor` - Pregled salda po komitentu
- `Components/Pages/StanjeKaseAltiva.razor` - Stanje kase sa selekcijom proizvoda
- `Models/SaldoPoKomitentuModel.cs` - Model za saldo podatke
- `Models/StanjeKeseSelekcija.cs` - Model za user preferences
- `Services/Core/KesaSelekcijaService.cs` - Singleton servis za selections
- `Data/stanje-kese-selekcija.json` - Persistence file

### Izmenjeno 🔄
- `Services/Implementations/IzvestajService/FinansijeService.cs` - Merged FinansijskiPregledService
- `Services/Interfaces/IFinansijeService.cs` - Dodati metodi iz FinansijskiPregledService
- `Extensions/ServiceCollectionExtensions.cs` - Registrovan KesaSelekcijaService
- `Program.cs` - Konfiguracija za Data folder
- Sve stranice koje su koristile FinansijskiPregledService

### Obrisano ❌
- `Services/Implementations/IzvestajService/FinansijskiPregledService.cs` - Merged u FinansijeService
- `Services/Interfaces/IFinansijskiPregledService.cs` - Merged u IFinansijeService
- `Models/RobaZalihaModel.cs` - Nekorišćen model

---

## 📞 Podrška

Ako imate problema sa deployment-om:

1. Proveri logove: `C:\FruitSysWeb\Logs\`
2. Proveri Windows Event Viewer
3. Kontaktiraj razvoj tim

---

## ✅ Deployment Checklist

Pre deployment-a:
- [ ] Backup postojeće aplikacije
- [ ] Backup appsettings.Production.json
- [ ] Backup Data foldera
- [ ] Proveri da li je server spreman

Tokom deployment-a:
- [ ] Zaustavi aplikaciju
- [ ] Raspakuj ZIP
- [ ] Vrati config fajlove
- [ ] Postavi permissions

Posle deployment-a:
- [ ] Testiraj login
- [ ] Testiraj Pregled Salda
- [ ] Testiraj Stanje Kase Altiva
- [ ] Testiraj Export funkcionalnosti
- [ ] Regression testing starih funkcionalnosti
- [ ] Proveri logove

---

**Srećan deployment!** 🚀
