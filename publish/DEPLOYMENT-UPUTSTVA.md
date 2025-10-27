# KOMPLETNA UPUTSTVA ZA DEPLOYMENT - FRUITSYS.RS
# Windows 11 Server Setup

## PREGLED

Ovaj dokument sadrži **kompletna detaljna uputstva** za podizanje FruitSys aplikacije na Windows 11 serveru sa:
- IIS web server konfiguracijom
- SSL sertifikat (Let's Encrypt - besplatno)
- Automatski restart aplikacije nakon restarta računara
- Production optimizacije

---

## PREDUSLOV - ŠTA TREBA DA IMATE

### 1. Hardver i OS
- ✅ Windows 11 računar (koji će biti server)
- ✅ Minimalno 4GB RAM (preporučeno 8GB+)
- ✅ Stabilna internet konekcija
- ✅ Statična IP adresa ili dinamički DNS

### 2. Softver - PRE DEPLOYMENT-A
- ✅ .NET 8 instaliran - **PROVERITE:**
  ```powershell
  dotnet --version
  # Trebalo bi da piše: 8.0.x
  ```

- ❌ **.NET 8 Hosting Bundle** - MORATE INSTALIRATI:
  1. Preuzmite: https://dotnet.microsoft.com/download/dotnet/8.0
  2. Kliknite na "Download .NET 8.0 Runtime" → **Windows Hosting Bundle**
  3. Instalirajte i restartujte računar

### 3. Domen
- ✅ Kupljen domen: **www.fruitsys.rs**
- ❌ DNS još nije konfigurisan - uradićete to u FAZI 1

### 4. Baza podataka
- ✅ MySQL baza već postoji na: `185.102.237.236:63388`
- ✅ Connection string već konfigurisan u aplikaciji

---

## FAZA 1: DNS KONFIGURACIJA (KOD REGISTRARA DOMENA)

### 1.1 Pronalaženje IP adrese servera

**Na Windows 11 serveru, otvorite PowerShell i izvršite:**

```powershell
# Pronađite vašu javnu IP adresu
(Invoke-WebRequest -Uri "https://api.ipify.org").Content
```

**Zapisite ovu IP adresu** - biće vam potrebna za DNS konfiguraciju.

**NAPOMENA:** Ako imate dinamičku IP adresu (menja se), morate:
- Kontaktirati ISP i tražiti statičnu IP adresu, ILI
- Koristiti dinamički DNS servis (DynDNS, No-IP, itd.)

### 1.2 Podešavanje DNS zapisa

Prijavite se na kontrolni panel registrara domena (gde ste kupili fruitsys.rs) i podesite:

**DNS Zapisi:**

```
Tip: A Record
Host: @
Value: [VASA_IP_ADRESA_IZ_KORAKA_1.1]
TTL: 3600

Tip: A Record
Host: www
Value: [VASA_IP_ADRESA_IZ_KORAKA_1.1]
TTL: 3600
```

**VAŽNO:**
- DNS propagacija može trajati 24-48h (obično 1-2h)
- Možete testirati propagaciju na: https://www.whatsmydns.net/
- Nastavite sa FAZOM 2 dok čekate DNS propagaciju

---

## FAZA 2: TRANSFER PUBLISH FOLDERA NA SERVER

### OPCIJA A: USB/Eksterni disk (najlakše)

1. **Kopirajte ceo `publish` folder** na USB
2. Prebacite na Windows 11 server
3. Ekstraktujte u: `C:\FruitSys-Deploy`

### OPCIJA B: Mreža/OneDrive/Google Drive

1. Kompresujte `publish` folder u ZIP
2. Uploadujte na cloud storage
3. Preuzmite na Windows 11 server
4. Ekstraktujte u: `C:\FruitSys-Deploy`

### Provera

Na serveru, proverite da postoje fajlovi:
```powershell
dir C:\FruitSys-Deploy
# Trebali bi videti: FruitSysWeb.dll, deploy-to-server.ps1, itd.
```

---

## FAZA 3: DEPLOYMENT APLIKACIJE

### 3.1 Pokretanje PowerShell kao Administrator

1. Desni klik na **Start Menu**
2. Kliknite **Windows Terminal (Admin)**
3. Navigirajte do deploy foldera:

```powershell
cd C:\FruitSys-Deploy
```

### 3.2 Omogućavanje izvršavanja PowerShell skripti

```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser -Force
```

### 3.3 Pokretanje deployment skripta

```powershell
.\deploy-to-server.ps1
```

**Šta ovaj script radi:**
1. ✅ Instalira IIS web server i sve potrebne komponente
2. ✅ Kreira Application Pool sa pravim podešavanjima
3. ✅ Kreira Web Site u IIS-u
4. ✅ Kopira aplikacione fajlove u `C:\inetpub\wwwroot\FruitSys`
5. ✅ Podešava permisije za IIS pristup
6. ✅ Konfigurisu Windows Firewall pravila (portovi 80 i 443)
7. ✅ Startuje aplikaciju

**Vreme izvršavanja:** 2-5 minuta

### 3.4 Provera da li aplikacija radi

Otvorite browser i idite na:
```
http://localhost
```

**Trebali bi videti FruitSys aplikaciju!** 🎉

Ako ne radi, pogledajte sekciju **TROUBLESHOOTING** na kraju dokumenta.

---

## FAZA 4: SSL SERTIFIKAT (HTTPS)

**VAŽNO:** Čekajte da DNS propagacija bude završena pre nego što nastavite!

### 4.1 Provera DNS propagacije

```powershell
nslookup www.fruitsys.rs
nslookup fruitsys.rs
```

Trebali bi videti vašu IP adresu. Ako ne, sačekajte još.

### 4.2 Instalacija SSL sertifikata

```powershell
cd C:\FruitSys-Deploy
.\install-ssl.ps1
```

**Šta ovaj script radi:**
1. ✅ Preuzima Win-ACME alat (Let's Encrypt klijent za Windows)
2. ✅ Automatski dobija besplatni SSL sertifikat
3. ✅ Instalira sertifikat u IIS
4. ✅ Konfigurisu HTTPS binding
5. ✅ Podešava automatsko obnavljanje (svaka 3 meseca)

**NAPOMENA:** Pratite uputstva na ekranu. Možda će biti potrebno da:
- Potvrdite email adresu
- Prihvatite Let's Encrypt Terms of Service

### 4.3 Provera HTTPS-a

Otvorite browser i idite na:
```
https://www.fruitsys.rs
https://fruitsys.rs
```

**Trebali bi videti zeleni katanac u browseru!** 🔒

---

## FAZA 5: AUTOMATSKI START NAKON RESTARTA

IIS je već konfigurisan da automatski startuje aplikaciju!

### 5.1 Provera Application Pool podešavanja

```powershell
Import-Module WebAdministration
Get-ItemProperty IIS:\AppPools\FruitSysAppPool | Select-Object name, startMode, state
```

Trebalo bi da vidite:
- **startMode:** AlwaysRunning
- **state:** Started

### 5.2 Test restarta

**1. Restartujte računar:**
```powershell
Restart-Computer
```

**2. Nakon restarta, odmah otvorite browser:**
```
https://www.fruitsys.rs
```

Aplikacija bi trebala da radi odmah! ✅

### 5.3 Ako aplikacija ne startuje automatski

Ako aplikacija ne radi nakon restarta:

```powershell
# Otvorite PowerShell kao Admin
Import-Module WebAdministration

# Startujte Application Pool i Web Site
Start-WebAppPool -Name "FruitSysAppPool"
Start-WebSite -Name "FruitSys"

# Proverite status
Get-WebAppPoolState -Name "FruitSysAppPool"
```

---

## FAZA 6: DODATNA PODEŠAVANJA (OPCIONO ALI PREPORUČENO)

### 6.1 Konfiguracija Windows za server upotrebu

```powershell
# Onemogućite Sleep mode
powercfg /change standby-timeout-ac 0
powercfg /change hibernate-timeout-ac 0

# Onemogućite automatske restarte za Windows Update
# Settings → Windows Update → Advanced options → Active hours
```

### 6.2 Monitoring i logging

**Logovi aplikacije:**
```
C:\inetpub\wwwroot\FruitSys\Logs
```

**IIS logovi:**
```
C:\inetpub\logs\LogFiles
```

**Windows Event Viewer:**
```powershell
eventvwr.msc
# Navigirajte do: Windows Logs → Application
# Filtrirajte po: Source = "IIS AspNetCore Module"
```

### 6.3 Performance monitoring

**Task Manager:**
- Pratite CPU i Memory usage
- Proces: `w3wp.exe` (IIS Worker Process)

**Resource Monitor:**
```powershell
resmon
```

### 6.4 Backup setup

Kreirajte scheduled task za automatski backup:

```powershell
# Kreirajte backup script: C:\Scripts\backup-fruitsys.ps1
$BackupPath = "D:\Backups\FruitSys"  # Promenite ako treba
$Date = Get-Date -Format "yyyyMMdd_HHmmss"

# Backup aplikacije
Compress-Archive -Path "C:\inetpub\wwwroot\FruitSys" `
    -DestinationPath "$BackupPath\FruitSys-$Date.zip"

# Čuvaj samo poslednjih 30 backup-a
Get-ChildItem $BackupPath -Filter "FruitSys-*.zip" |
    Sort-Object LastWriteTime -Descending |
    Select-Object -Skip 30 |
    Remove-Item
```

**Kreirajte Scheduled Task:**
```powershell
$action = New-ScheduledTaskAction -Execute "PowerShell.exe" `
    -Argument "-File C:\Scripts\backup-fruitsys.ps1"

$trigger = New-ScheduledTaskTrigger -Daily -At 2AM

Register-ScheduledTask -TaskName "FruitSys Backup" `
    -Action $action `
    -Trigger $trigger `
    -User "SYSTEM" `
    -RunLevel Highest
```

---

## KORISNE KOMANDE

### Restart aplikacije

```powershell
# Restart Application Pool-a (brži način)
Restart-WebAppPool -Name "FruitSysAppPool"

# Ili restart celog IIS-a
iisreset
```

### Provera statusa

```powershell
Import-Module WebAdministration

# Status Application Pool-a
Get-WebAppPoolState -Name "FruitSysAppPool"

# Status Web Site-a
Get-Website -Name "FruitSys"

# Lista svih binding-a
Get-WebBinding -Name "FruitSys"
```

### Gledanje logova u real-time

```powershell
# PowerShell real-time monitoring
Get-Content "C:\inetpub\wwwroot\FruitSys\Logs\fruitsys-*.log" -Wait -Tail 20
```

---

## TROUBLESHOOTING

### Problem 1: "HTTP Error 502.5 - Process Failure"

**Uzrok:** .NET 8 Hosting Bundle nije instaliran

**Rešenje:**
1. Preuzmite: https://dotnet.microsoft.com/download/dotnet/8.0
2. Instalirajte "ASP.NET Core Runtime 8.0 - Windows Hosting Bundle"
3. Restartujte računar
4. Pokrenite: `iisreset`

### Problem 2: "HTTP Error 500.0 - Internal Server Error"

**Uzrok:** Greška u aplikaciji

**Rešenje:**
```powershell
# Proverite logove
Get-Content "C:\inetpub\wwwroot\FruitSys\Logs\fruitsys-*.log" -Tail 50

# Ili pokrenite aplikaciju ručno da vidite grešku
cd C:\inetpub\wwwroot\FruitSys
dotnet FruitSysWeb.dll
```

### Problem 3: Ne može da se konektuje na bazu

**Uzrok:** Firewall ili mrežni problem

**Rešenje:**
```powershell
# Test konekcije na MySQL server
Test-NetConnection -ComputerName 185.102.237.236 -Port 63388

# Ako ne radi, proverite:
# - Firewall na MySQL serveru
# - Firewall na Windows 11 serveru
# - Da li je MySQL server dostupan spoljašnjim konekcijama
```

### Problem 4: SSL sertifikat ne radi

**Uzrok:** DNS nije propagiran ili firewall blokira port 443

**Rešenje:**
```powershell
# Provera DNS-a
nslookup www.fruitsys.rs

# Provera porta 443
Test-NetConnection -ComputerName www.fruitsys.rs -Port 443

# Ručna instalacija SSL-a
cd C:\Tools\win-acme
.\wacs.exe
# Izaberite opciju N (New certificate)
# Izaberite opciju 1 (IIS)
# Pratite uputstva
```

### Problem 5: "Cannot find FruitSysWeb.dll"

**Uzrok:** Fajlovi nisu dobro kopirani

**Rešenje:**
```powershell
# Proverite da li fajlovi postoje
dir C:\inetpub\wwwroot\FruitSys\FruitSysWeb.dll

# Ako ne postoji, kopirajte ponovo
xcopy C:\FruitSys-Deploy\* C:\inetpub\wwwroot\FruitSys\ /E /Y
```

### Problem 6: Blazor SignalR ne radi (česta greška)

**Uzrok:** WebSockets nije omogućen u IIS-u

**Rešenje:**
```powershell
# Omogućite WebSockets feature
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebSockets -All -NoRestart

# Provera
Get-WindowsOptionalFeature -Online -FeatureName IIS-WebSockets

# Restart IIS
iisreset
```

### Problem 7: Sporije performanse

**Rešenje:**
```powershell
# Dodajte Application Initialization
Import-Module WebAdministration

# Enable Application Initialization
Set-ItemProperty "IIS:\Sites\FruitSys" -Name applicationDefaults.preloadEnabled -Value $true
Set-ItemProperty "IIS:\AppPools\FruitSysAppPool" -Name startMode -Value AlwaysRunning

# Restart
Restart-WebAppPool -Name "FruitSysAppPool"
```

---

## SECURITY CHECKLIST

- [ ] SSL sertifikat instaliran i validan
- [ ] HTTP automatski redirektuje na HTTPS
- [ ] Windows Firewall omogućen i konfigurisan
- [ ] Windows Update omogućen (ali sa kontrolisanim restartom)
- [ ] Jak password za Windows korisnika
- [ ] Remote Desktop omogućen SAMO ako je potreban (i sa jakim passwordom)
- [ ] MySQL pristup samo sa ovog servera (provera firewall pravila na MySQL serveru)
- [ ] Redovni backup-i podešeni
- [ ] Monitoring logova aktivan

---

## UPDATE APLIKACIJE (BUDUĆE VERZIJE)

Kada želite da deploy-ujete novu verziju aplikacije:

```powershell
# 1. Stop Application Pool
Stop-WebAppPool -Name "FruitSysAppPool"
Start-Sleep -Seconds 5

# 2. Backup trenutne verzije
$Date = Get-Date -Format "yyyyMMdd_HHmmss"
Compress-Archive -Path "C:\inetpub\wwwroot\FruitSys" `
    -DestinationPath "D:\Backups\FruitSys-BEFORE-UPDATE-$Date.zip"

# 3. Kopirajte nove fajlove
xcopy C:\FruitSys-Deploy-NEW\* C:\inetpub\wwwroot\FruitSys\ /E /Y

# 4. Start Application Pool
Start-WebAppPool -Name "FruitSysAppPool"

# 5. Test
Start-Process "https://www.fruitsys.rs"
```

---

## KONTAKT ZA PODRŠKU

Ako imate problema ili pitanja:
- Logove aplikacije: `C:\inetpub\wwwroot\FruitSys\Logs`
- Windows Event Viewer: `eventvwr.msc`

---

## DODATNI RESURSI

- IIS Dokumentacija: https://docs.microsoft.com/en-us/iis/
- ASP.NET Core Hosting: https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/
- Win-ACME Dokumentacija: https://www.win-acme.com/
- Let's Encrypt: https://letsencrypt.org/

---

**USPEŠAN DEPLOYMENT! 🎉**

Vaša aplikacija je sada dostupna na:
- **https://www.fruitsys.rs**
- **https://fruitsys.rs**

I automatski će se pokretati pri svakom restartu servera!
