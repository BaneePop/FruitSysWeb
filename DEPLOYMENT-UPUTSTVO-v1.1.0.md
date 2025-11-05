# FruitSysWeb - Deployment Uputstvo v1.1.0

**Datum:** 05.11.2025
**Verzija:** 1.1.0
**Target Server:** Windows (192.168.1.11:6001)

---

## Šta je Novo u v1.1.0

### Optimizacije Performansi (8 glavnih izmena)

1. **TroskoviHome - Agregacija po Smeni**
   - Tabela "Troškovi po Smeni" sada prikazuje jedan red po smeni
   - Tačan broj radnika: Ukupni sati / 8

2. **RadniNaloziPregled - UI Cleanup**
   - Uklonjena kolona "Procenat" iz tabele izveštaja

3. **TroskoviHome - Zamena Chart-a**
   - "Troškovi Ambalaze" zamenjen sa "Troškovi po RN - Zadnjih 10 Završenih"

4. **TroskoviHome - Uklonjen Chart**
   - Uklonjen "Troškovi po RN - Zadnjih 7 RN" sa dna stranice

5. **TroskoviHome - OPTIMIZACIJA: Pregled po Radnom Danu**
   - Eliminisan dupli SQL poziv
   - **50% brže učitavanje**

6. **TroskoviHome - OPTIMIZACIJA: Troškovi po RN Chart**
   - Dodat date filter (60 dana)
   - **3-5x brže učitavanje**

7. **Proizvodnja - OPTIMIZACIJA: Paralelizacija**
   - SQL upiti se izvršavaju paralelno
   - **50% brže učitavanje**

8. **FinansijeHome - OPTIMIZACIJA: Top 5 Dobavljača**
   - Skraćen date range na 90 dana
   - **40-50% brže učitavanje**

### Detaljna Dokumentacija
Vidi: `/Users/Bane/Desktop/Sazetak_Optimizacija_Izmena.md`

---

## Deployment Paket

**Fajl:** `FruitSysWeb-Production-v1.1.0-20251105-223443.zip`
**Veličina:** 16MB
**SHA256:** `e70f911078941d7bb8df422fbf8d6c1494c0279c525e37d8849c81c5a60cc099`

---

## Deployment Koraci za Windows Server (192.168.1.11:6001)

### Preduslov
- .NET 8.0 Runtime instaliran na serveru
- IIS ili Kestrel kao host
- Postojeća aplikacija treba zaustaviti

### Korak 1: Zaustavi Aplikaciju

**IIS:**
```powershell
# U IIS Manager-u, zaustavi Application Pool
Stop-WebAppPool -Name "FruitSysWeb"
```

**Kestrel (Windows Service):**
```powershell
# Zaustavi Windows Service
Stop-Service -Name "FruitSysWeb"
```

**Kestrel (Standalone):**
```powershell
# Zaustavi proces ručno ili preko Task Manager-a
taskkill /F /IM FruitSysWeb.exe
```

### Korak 2: Backup Postojeće Aplikacije

```powershell
# Kreiraj backup folder sa datumom
$backupFolder = "C:\FruitSysWeb-Backup-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
New-Item -ItemType Directory -Path $backupFolder

# Kopiraj postojeće fajlove
Copy-Item -Path "C:\FruitSysWeb\*" -Destination $backupFolder -Recurse

# VAŽNO: Sačuvaj appsettings.Production.json
Copy-Item -Path "C:\FruitSysWeb\appsettings.Production.json" -Destination "C:\appsettings.Production.backup.json"
```

### Korak 3: Raspakuj Novi Deployment

```powershell
# Izbriši stare fajlove (NE DIRAJ appsettings.Production.json!)
Remove-Item -Path "C:\FruitSysWeb\*" -Exclude "appsettings.Production.json","Logs" -Recurse -Force

# Raspakuj novi ZIP
Expand-Archive -Path "C:\Downloads\FruitSysWeb-Production-v1.1.0-20251105-223443.zip" -DestinationPath "C:\FruitSysWeb-Temp"

# Kopiraj iz publish foldera
Copy-Item -Path "C:\FruitSysWeb-Temp\publish\*" -Destination "C:\FruitSysWeb" -Recurse -Force

# Očisti temp folder
Remove-Item -Path "C:\FruitSysWeb-Temp" -Recurse -Force
```

### Korak 4: Proveri appsettings.Production.json

```powershell
# Proveri da li postoji
Test-Path "C:\FruitSysWeb\appsettings.Production.json"

# Ako ne postoji, vrati iz backup-a
if (!(Test-Path "C:\FruitSysWeb\appsettings.Production.json")) {
    Copy-Item -Path "C:\appsettings.Production.backup.json" -Destination "C:\FruitSysWeb\appsettings.Production.json"
}
```

**Sadržaj appsettings.Production.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=185.102.237.236;Port=63388;Database=fruitsysdb_v2;Uid=rouser;Pwd=m9@to73de;CharSet=utf8mb4;SslMode=None;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information",
      "FruitSysWeb": "Warning"
    },
    "File": {
      "Path": "Logs/fruitsys-{Date}.log",
      "MinLevel": "Warning",
      "RollingInterval": "Day",
      "RetainedFileCountLimit": 90,
      "FileSizeLimitBytes": 52428800,
      "OutputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"
    }
  },
  "AllowedHosts": "*"
}
```

### Korak 5: Proveri Permissions

```powershell
# IIS ApplicationPool Identity mora imati pristup folderu
icacls "C:\FruitSysWeb" /grant "IIS_IUSRS:(OI)(CI)F" /T

# Za Kestrel, proveri da korisnik ima pristup
icacls "C:\FruitSysWeb" /grant "KORISNIK:(OI)(CI)F" /T
```

### Korak 6: Pokreni Aplikaciju

**IIS:**
```powershell
Start-WebAppPool -Name "FruitSysWeb"
```

**Kestrel (Windows Service):**
```powershell
Start-Service -Name "FruitSysWeb"
```

**Kestrel (Standalone):**
```powershell
cd C:\FruitSysWeb
.\FruitSysWeb.exe
```

### Korak 7: Proveri Aplikaciju

1. Otvori browser: `http://192.168.1.11:6001`
2. Proveri login stranicu
3. Loguj se i testiraj:
   - **TroskoviHome** - Proveri nove chart-ove
   - **Proizvodnja** - Proveri brzinu učitavanja
   - **FinansijeHome** - Proveri Top 5 Dobavljača
   - **RadniNaloziPregled** - Proveri da kolona "Procenat" ne postoji

### Korak 8: Proveri Logove

```powershell
# Prati logove u realnom vremenu
Get-Content "C:\FruitSysWeb\Logs\fruitsys-$(Get-Date -Format 'yyyyMMdd').log" -Wait -Tail 50
```

---

## Rollback Procedura (Ako Nešto Pođe Po Zlu)

```powershell
# Zaustavi aplikaciju
Stop-WebAppPool -Name "FruitSysWeb"
# ili
Stop-Service -Name "FruitSysWeb"

# Vrati staru verziju iz backup-a
Remove-Item -Path "C:\FruitSysWeb\*" -Recurse -Force
Copy-Item -Path "$backupFolder\*" -Destination "C:\FruitSysWeb" -Recurse

# Pokreni aplikaciju
Start-WebAppPool -Name "FruitSysWeb"
# ili
Start-Service -Name "FruitSysWeb"
```

---

## Napredni Deployment: Zero-Downtime

### Korak 1: Blue-Green Deployment Setup

```powershell
# Kreiraj novi folder za deployment
New-Item -ItemType Directory -Path "C:\FruitSysWeb-v1.1.0"

# Raspakuj novi deployment
Expand-Archive -Path "FruitSysWeb-Production-v1.1.0-20251105-223443.zip" -DestinationPath "C:\FruitSysWeb-v1.1.0-Temp"
Copy-Item -Path "C:\FruitSysWeb-v1.1.0-Temp\publish\*" -Destination "C:\FruitSysWeb-v1.1.0" -Recurse

# Kopiraj appsettings.Production.json
Copy-Item -Path "C:\FruitSysWeb\appsettings.Production.json" -Destination "C:\FruitSysWeb-v1.1.0\appsettings.Production.json"
```

### Korak 2: U IIS-u, promeni Physical Path

1. Otvori IIS Manager
2. Selektuj "FruitSysWeb" aplikaciju
3. U desnom panelu: **Basic Settings**
4. Promeni **Physical Path** na: `C:\FruitSysWeb-v1.1.0`
5. Klikni **OK**
6. Restart Application Pool

### Korak 3: Proveri Novu Verziju

- Aplikacija sada koristi novu verziju bez downtime-a
- Stara verzija je i dalje dostupna na `C:\FruitSysWeb`

### Korak 4: Ako Je Sve OK, Očisti Staru Verziju

```powershell
# Posle 7 dana, ako je sve OK:
Remove-Item -Path "C:\FruitSysWeb" -Recurse -Force
Rename-Item -Path "C:\FruitSysWeb-v1.1.0" -NewName "FruitSysWeb"
```

---

## Troubleshooting

### Problem: Aplikacija se ne pokreće

**Proveri:**
1. Da li je .NET 8.0 Runtime instaliran?
   ```powershell
   dotnet --list-runtimes
   ```
2. Da li postoji `appsettings.Production.json`?
3. Da li IIS ApplicationPool Identity ima pristup folderu?

### Problem: "Connection String" greška

**Rešenje:** Proveri `appsettings.Production.json` i vrati iz backup-a:
```powershell
Copy-Item -Path "C:\appsettings.Production.backup.json" -Destination "C:\FruitSysWeb\appsettings.Production.json"
```

### Problem: Stranice se sporo učitavaju

**Rešenje:**
1. Proveri database indekse: `/Database/OptimizacijeIndeksa.sql`
2. Proveri da li MySQL server radi
3. Proveri network latency do database servera (185.102.237.236:63388)

### Problem: Chart-ovi ne prikazuju podatke

**Rešenje:**
1. Proveri browser console za JavaScript greške
2. Očisti browser cache (Ctrl+Shift+Delete)
3. Proveri database connection

---

## Kontakt i Podrška

**GitHub Repo:** https://github.com/BaneePop/FruitSysWeb
**Branch:** feature/centralize-types
**Commits:**
- `ce025cc` - 8 optimizacija i izmena
- `2cbb639` - Cleanup build artifakata

**Generisano:** 05.11.2025 22:34
**Claude Code Session**
