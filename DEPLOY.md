# FruitSysWeb - Deployment i Update

## Sadržaj

- [Quick Deploy (sitne izmene)](#quick-deploy-sitne-izmene)
- [Full Deployment (nova verzija)](#full-deployment-nova-verzija)
- [Ručni Deployment](#ručni-deployment)
- [Post-Deployment Verifikacija](#post-deployment-verifikacija)
- [Rollback](#rollback)
- [Korisne Komande](#korisne-komande)
- [Troubleshooting](#troubleshooting)

---

## Quick Deploy (sitne izmene)

Za izmene u `.razor` fajlovima, servisima i modelima — bez novih NuGet paketa ili DB promena.

**Trajanje: ~1 minut**

### Korak 1: Build na Mac-u

```bash
cd /Users/Bane/FruitSysWeb
dotnet publish FruitSysWeb.csproj -c Release -o ./publish
```

### Korak 2: Prenesi na server

Kopiraj `./publish/FruitSysWeb.dll` (i opciono `.pdb`) na server u `C:\FruitSysWeb\`.

### Korak 3: Restart servisa na serveru

```powershell
net stop FruitSysWeb
Start-Sleep -Seconds 3
net start FruitSysWeb
```

Ili za IIS App Pool:
```powershell
Restart-WebAppPool -Name "FruitSysWeb"
```

**GOTOVO!** Osvežite browser sa `Ctrl+F5`.

### Kada NE koristiti Quick Deploy

- Dodavanje novih NuGet paketa
- Izmene u `appsettings.json` ili `Program.cs`
- Database schema promene
- Dodavanje novih fajlova u `wwwroot/`

Za ove slučajeve, koristiti Full Deployment.

---

## Full Deployment (nova verzija)

**Trajanje: ~5 minuta**

### Korak 1: Build deployment paketa na Mac-u

```bash
cd /Users/Bane/FruitSysWeb
dotnet publish FruitSysWeb.csproj -c Release -o ./publish
```

### Korak 2: Automatski deployment na serveru

```powershell
# IIS
.\deploy-to-windows-server-v1.2.0.ps1 -IIS -AppPoolName "FruitSysWeb"

# Windows Service
.\deploy-to-windows-server-v1.2.0.ps1 -Service

# Standalone
.\deploy-to-windows-server-v1.2.0.ps1 -Standalone
```

Skripta automatski: zaustavlja aplikaciju, pravi backup, kopira fajlove, vraća config, startuje aplikaciju.

---

## Ručni Deployment

### Korak 1: Backup

```powershell
Stop-WebAppPool -Name "FruitSysWeb"

$backup = "C:\FruitSysWeb-Backup-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
Copy-Item -Path "C:\FruitSysWeb\*" -Destination $backup -Recurse -Force

# Posebno backup config fajlova
Copy-Item "C:\FruitSysWeb\appsettings.Production.json" "C:\Temp\" -Force
Copy-Item "C:\FruitSysWeb\Data" "C:\Temp\Data-backup" -Recurse -Force
```

### Korak 2: Očisti stare fajlove

```powershell
# NE brisati: appsettings.Production.json, Logs, Data
Get-ChildItem "C:\FruitSysWeb" -Exclude "appsettings.Production.json","Logs","Data" |
    Remove-Item -Recurse -Force
```

### Korak 3: Kopiraj nove fajlove

```powershell
Copy-Item -Path "C:\Temp\Deploy\publish\*" -Destination "C:\FruitSysWeb\" -Recurse -Force
```

### Korak 4: Vrati config fajlove

```powershell
Copy-Item "C:\Temp\appsettings.Production.json" "C:\FruitSysWeb\" -Force

# Vrati Data folder (user preferences)
if (Test-Path "C:\Temp\Data-backup\stanje-kese-selekcija.json") {
    Copy-Item "C:\Temp\Data-backup\*" "C:\FruitSysWeb\Data\" -Force
}
```

### Korak 5: Permissions

```powershell
icacls "C:\FruitSysWeb" /grant "IIS_IUSRS:(OI)(CI)F" /T /Q
icacls "C:\FruitSysWeb\Data" /grant "IIS_IUSRS:(OI)(CI)M" /T /Q
icacls "C:\FruitSysWeb\Logs" /grant "IIS_IUSRS:(OI)(CI)M" /T /Q
```

### Korak 6: Pokreni

```powershell
Start-WebAppPool -Name "FruitSysWeb"
```

---

## Post-Deployment Verifikacija

### Osnovna provera

```
http://192.168.1.11:6001
```

- Login stranica se učitava? ✅
- Login radi? ✅
- Logovi nemaju ERR? ✅

### Provera logova

```powershell
Get-Content "C:\FruitSysWeb\Logs\fruitsysweb-*.txt" -Tail 50
# [INF] = OK, [WRN] = proveri, [ERR] = greška
```

### Regression testing

| Modul | Test |
|-------|------|
| Prerada → Proizvodnja | Učitavanje podataka |
| Prerada → Radni Nalozi | Filter po datumu |
| Prerada → Sledljivost | Pretraga po šifri |
| Lager | Prikaz svih magacina |
| Finansije → Nabavka | Export to Excel |
| Finansije → Pregled Salda | Prikaz liste komitenata |
| Finansije → Stanje Kase | Selekcija se čuva posle refresh-a |

### Provera Data foldera

```powershell
# Mora postojati i imati Write permissions
Test-Path "C:\FruitSysWeb\Data"
icacls "C:\FruitSysWeb\Data"
```

---

## Rollback

```powershell
Stop-WebAppPool -Name "FruitSysWeb"

# Obriši novu verziju (čuvaj Logs)
Remove-Item "C:\FruitSysWeb\*" -Recurse -Force -Exclude "Logs"

# Vrati backup
$backup = "C:\FruitSysWeb-Backup-20251222-180500"  # prilagodi datum
Copy-Item "$backup\*" "C:\FruitSysWeb\" -Recurse -Force

Start-WebAppPool -Name "FruitSysWeb"
```

Za Quick Deploy rollback:
```powershell
# Vrati stari DLL iz automatski kreiranog backupa
Copy-Item "C:\FruitSysWeb\FruitSysWeb.dll.backup-YYYYMMDD-HHMMSS" `
          "C:\FruitSysWeb\FruitSysWeb.dll" -Force
Restart-WebAppPool -Name "FruitSysWeb"
```

---

## Korisne Komande

```powershell
# Status aplikacije
Import-Module WebAdministration
Get-WebAppPoolState -Name "FruitSysWeb"
Get-Website -Name "FruitSys"

# Restart
Restart-WebAppPool -Name "FruitSysWeb"
iisreset

# Real-time logovi
Get-Content "C:\FruitSysWeb\Logs\fruitsysweb-*.txt" -Wait -Tail 20

# Test konekcije na bazu
Test-NetConnection -ComputerName 185.102.237.236 -Port 63388
```

---

## Troubleshooting

### "502.5 - Process Failure"

.NET 8.0 Hosting Bundle nije instaliran.

```powershell
dotnet --version  # trebalo bi: 8.0.x
# Ako nije: https://dotnet.microsoft.com/download/dotnet/8.0 → Windows Hosting Bundle
Restart-Computer
```

### "500.0 - Internal Server Error"

```powershell
# Pokrenuti ručno da bi videli grešku
cd C:\FruitSysWeb
dotnet FruitSysWeb.dll
```

### DLL zaključan, ne može se kopirati

```powershell
Stop-WebAppPool -Name "FruitSysWeb"
Copy-Item "FruitSysWeb.dll" "C:\FruitSysWeb\" -Force
Start-WebAppPool -Name "FruitSysWeb"
```

### Blazor SignalR ne radi

```powershell
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebSockets -All -NoRestart
iisreset
```

### Logo se ne prikazuje u PDF-u

```powershell
Test-Path "C:\FruitSysWeb\Logo.png"
# Ako ne postoji, kopiraj iz deployment paketa
```

### "Stanje Kase" ne čuva selections

```powershell
New-Item -ItemType Directory -Path "C:\FruitSysWeb\Data" -Force
icacls "C:\FruitSysWeb\Data" /grant "IIS_IUSRS:(OI)(CI)M" /T /Q
```

### Izmene se ne vide u browseru

Hard refresh: `Ctrl+F5`. Ako ne pomaže, proveri da li je App Pool restartovan i da li su novi fajlovi zaista kopirani.

---

## Preduslovi na serveru

- Windows Server sa IIS ili Windows Service
- **.NET 8.0 Hosting Bundle** (obavezno)
- IIS komponenta: **WebSockets** (za Blazor SignalR)
- Firewall: portovi 80 i 443 otvoreni
- `C:\FruitSysWeb\Data\` folder sa Write permissions za IIS_IUSRS
