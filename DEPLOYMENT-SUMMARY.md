# 🎉 FRUITSYS - PRODUCTION DEPLOYMENT PACKAGE GOTOV!

## ŠTA JE URAĐENO

### ✅ 1. Aplikacija spremna za produkciju
- **Production build** kreiran u `publish/` folderu
- **ForwardedHeaders** podrška dodata (za rad iza IIS reverse proxy-ja)
- **Production konfiguracija** optimizovana (`appsettings.Production.json`)

### ✅ 2. Automatizacione skripte
Kreirane PowerShell skripte za potpunu automatizaciju:
- `deploy-to-server.ps1` - Kompletan deployment na Windows 11
- `install-ssl.ps1` - Automatska SSL instalacija (Let's Encrypt)
- `update-app.ps1` - Update na nove verzije

### ✅ 3. Kompletna dokumentacija
- `START-HERE.txt` - Brzi pregled odakle početi
- `README.md` - Kompletan pregled paketa
- `BRZI-START.md` - 3 koraka za deployment
- `DEPLOYMENT-UPUTSTVA.md` - Detaljan vodič (12KB)
- `DNS-SETUP.md` - DNS konfiguracija sa primerima

---

## SLEDEĆI KORACI ZA VAS

### FAZA 1: PREBACIVANJE NA SERVER (5 minuta)

1. **Kopirajte ceo `publish` folder** na Windows 11 server:
   - USB disk
   - Mreža (File Share)
   - Cloud (OneDrive, Google Drive)
   - Ili bilo koji drugi način

2. **Lokacija na serveru:** `C:\FruitSys-Deploy`

### FAZA 2: DNS KONFIGURACIJA (10 minuta + čekanje)

1. **Pronađite IP adresu servera:**
   ```powershell
   (Invoke-WebRequest -Uri "https://api.ipify.org").Content
   ```

2. **Kod registrara domena, podesite:**
   ```
   A Record: @   → VASA_IP
   A Record: www → VASA_IP
   ```

3. **Sačekajte DNS propagaciju:** 1-24h (prosek 2-4h)

4. **Detaljna uputstva:** `publish/DNS-SETUP.md`

### FAZA 3: INSTALACIJA HOSTING BUNDLE (5 minuta)

**⚠️ KRITIČNO - BEZ OVOGA NEĆE RADITI!**

1. Idite na: https://dotnet.microsoft.com/download/dotnet/8.0
2. Kliknite: **"Download .NET 8.0 Runtime"**
3. Izaberite: **"Windows Hosting Bundle"**
4. Instalirajte i **restartujte računar**

### FAZA 4: DEPLOYMENT (5 minuta)

Na Windows 11 serveru:

```powershell
# Otvorite PowerShell kao Administrator
cd C:\FruitSys-Deploy

# Omogućite izvršavanje skripti
Set-ExecutionPolicy RemoteSigned -Scope CurrentUser -Force

# Pokrenite deployment
.\deploy-to-server.ps1
```

**Testirajte:** http://localhost

### FAZA 5: SSL INSTALACIJA (5 minuta)

**Sačekajte da DNS propagacija bude gotova!**

```powershell
cd C:\FruitSys-Deploy
.\install-ssl.ps1
```

**Testirajte:** https://www.fruitsys.rs

---

## ŠTA DEPLOYMENT AUTOMATSKI RADI

Deployment script automatski:
1. ✅ Instalira IIS web server sa svim komponentama
2. ✅ Kreira Application Pool (`FruitSysAppPool`) sa optimalnim podešavanjima
3. ✅ Kreira Web Site (`FruitSys`) u IIS-u
4. ✅ Kopira sve aplikacione fajlove u `C:\inetpub\wwwroot\FruitSys`
5. ✅ Podešava permisije za IIS_IUSRS
6. ✅ Konfigurisu Windows Firewall (portovi 80 i 443)
7. ✅ **Podešava automatski start nakon restarta računara**
8. ✅ Startuje aplikaciju

SSL script automatski:
1. ✅ Preuzima Win-ACME (Let's Encrypt klijent)
2. ✅ Dobija besplatni SSL sertifikat (validan 3 meseca)
3. ✅ Instalira sertifikat u IIS
4. ✅ Konfigurisu HTTPS binding
5. ✅ **Podešava automatsko obnavljanje sertifikata**

---

## STRUCTURE NAKON DEPLOYMENT-A

```
C:\inetpub\wwwroot\FruitSys\
├── FruitSysWeb.dll                 (Glavna aplikacija)
├── appsettings.json                
├── appsettings.Production.json     (Production config)
├── web.config                      (IIS config)
├── Logo.png                        
├── Logs\                           (Logovi)
│   └── fruitsys-YYYYMMDD.log
├── wwwroot\                        (Static files)
└── ... (DLL zavisnosti)
```

---

## AUTOMATSKI START - KAKO RADI

### Application Pool podešavanja:
- **Start Mode:** `AlwaysRunning` (automatski se startuje)
- **Idle Timeout:** `00:00:00` (nikad ne gasi aplikaciju)
- **Recycle:** Onemogućen periodični restart

### Šta to znači:
- ✅ Aplikacija se automatski pokreće pri startu računara
- ✅ Aplikacija ostaje pokrenuta 24/7
- ✅ IIS automatski restartuje aplikaciju ako padne
- ✅ Nema potrebe za ručnim pokretanjem

### Test:
```powershell
# Restartujte računar
Restart-Computer

# Nakon restarta, odmah otvorite browser
Start-Process "https://www.fruitsys.rs"
# Aplikacija će raditi! ✅
```

---

## KORISNE KOMANDE ZA SERVER

### Restart aplikacije
```powershell
Restart-WebAppPool -Name "FruitSysAppPool"
```

### Provera statusa
```powershell
Import-Module WebAdministration
Get-WebAppPoolState -Name "FruitSysAppPool"
Get-Website -Name "FruitSys"
```

### Gledanje logova
```powershell
# Real-time monitoring
Get-Content "C:\inetpub\wwwroot\FruitSys\Logs\fruitsys-*.log" -Wait -Tail 20

# Poslednjih 50 linija
Get-Content "C:\inetpub\wwwroot\FruitSys\Logs\fruitsys-*.log" -Tail 50
```

### IIS restart (ako treba)
```powershell
iisreset
```

---

## UPDATE NA NOVU VERZIJU

Kada budete imali novu verziju aplikacije:

1. **Build nova verzija:**
   ```bash
   dotnet publish -c Release -o ./publish-new
   ```

2. **Prebacite na server** (USB, mreža, itd.)

3. **Pokrenite update script:**
   ```powershell
   cd C:\FruitSys-Deploy
   .\update-app.ps1 -NewVersionPath "C:\putanja\do\nove\verzije"
   ```

Update script automatski:
- ✅ Zaustavlja aplikaciju
- ✅ Pravi backup trenutne verzije
- ✅ Briše stare fajlove
- ✅ Kopira nove fajlove
- ✅ Startuje aplikaciju
- ✅ Testira da li radi

---

## TROUBLESHOOTING - TOP 5 PROBLEMA

### 1. "HTTP Error 502.5 - Process Failure"
**Uzrok:** .NET 8 Hosting Bundle nije instaliran

**Rešenje:**
```powershell
# Preuzmite i instalirajte Hosting Bundle
# https://dotnet.microsoft.com/download/dotnet/8.0
# Restartujte računar
Restart-Computer
```

### 2. "HTTP Error 500.0"
**Uzrok:** Greška u aplikaciji ili konfiguraciji

**Rešenje:**
```powershell
# Proverite logove
Get-Content "C:\inetpub\wwwroot\FruitSys\Logs\fruitsys-*.log" -Tail 50

# Ili pokrenite ručno da vidite grešku
cd C:\inetpub\wwwroot\FruitSys
dotnet FruitSysWeb.dll
```

### 3. Blazor SignalR ne radi
**Uzrok:** WebSockets nisu omogućeni

**Rešenje:**
```powershell
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebSockets -All -NoRestart
iisreset
```

### 4. SSL ne radi
**Uzrok:** DNS nije propagiran ili firewall blokira port 443

**Rešenje:**
```powershell
# Proverite DNS
nslookup www.fruitsys.rs

# Proverite port 443
Test-NetConnection -ComputerName www.fruitsys.rs -Port 443

# Ako i dalje ne radi, ručno instalirajte SSL
cd C:\Tools\win-acme
.\wacs.exe
```

### 5. Ne može da se konektuje na bazu
**Uzrok:** Firewall ili mrežni problem

**Rešenje:**
```powershell
# Test konekcije
Test-NetConnection -ComputerName 185.102.237.236 -Port 63388

# Ako ne radi, proverite firewall na MySQL serveru
```

---

## SECURITY CHECKLIST

- [ ] ✅ SSL sertifikat instaliran (automatski)
- [ ] ✅ HTTPS redirekcija (automatski)
- [ ] ✅ Windows Firewall konfigurisan (automatski)
- [ ] ⚠️ Jak password za Windows korisnika
- [ ] ⚠️ Windows Update omogućen
- [ ] ⚠️ Remote Desktop SAMO ako je potreban
- [ ] ⚠️ Redovni backup-i (setup ručno)

---

## MONITORING

### Performance
```powershell
# Task Manager
taskmgr

# Resource Monitor
resmon
```

### Logovi
- **Aplikacija:** `C:\inetpub\wwwroot\FruitSys\Logs\`
- **IIS:** `C:\inetpub\logs\LogFiles\`
- **Windows Event Viewer:** `eventvwr.msc`

---

## BACKUP SETUP (OPCIONO ALI PREPORUČENO)

Kreirajte `C:\Scripts\backup-fruitsys.ps1`:

```powershell
$BackupPath = "D:\Backups\FruitSys"
$Date = Get-Date -Format "yyyyMMdd_HHmmss"

# Backup aplikacije
Compress-Archive -Path "C:\inetpub\wwwroot\FruitSys" `
    -DestinationPath "$BackupPath\FruitSys-$Date.zip"

# Čuvaj samo poslednjih 30
Get-ChildItem $BackupPath -Filter "FruitSys-*.zip" |
    Sort-Object LastWriteTime -Descending |
    Select-Object -Skip 30 |
    Remove-Item
```

Dodajte u Task Scheduler (dnevno u 2AM):
```powershell
$action = New-ScheduledTaskAction -Execute "PowerShell.exe" `
    -Argument "-File C:\Scripts\backup-fruitsys.ps1"
$trigger = New-ScheduledTaskTrigger -Daily -At 2AM
Register-ScheduledTask -TaskName "FruitSys Backup" `
    -Action $action -Trigger $trigger -User "SYSTEM" -RunLevel Highest
```

---

## DODATNI RESURSI

- [IIS Documentation](https://docs.microsoft.com/en-us/iis/)
- [ASP.NET Core Hosting](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/)
- [Win-ACME](https://www.win-acme.com/)
- [Let's Encrypt](https://letsencrypt.org/)

---

## UKUPNO VREME DEPLOYMENT-A

| Faza | Vreme | Aktivnost |
|------|-------|-----------|
| DNS Setup | 10min + čekanje | Podešavanje DNS zapisa kod registrara |
| DNS Propagacija | 1-24h | Automatski (čekanje) |
| Hosting Bundle | 5min | Download i instalacija |
| Deployment | 5min | Pokretanje `deploy-to-server.ps1` |
| SSL Setup | 5min | Pokretanje `install-ssl.ps1` |
| **UKUPNO** | **~30min** | (+ DNS propagacija) |

---

## FINALNI REZULTAT

Nakon završenog deployment-a, imaćete:

✅ **Aplikacija dostupna na:**
   - https://www.fruitsys.rs
   - https://fruitsys.rs

✅ **Automatski start:**
   - Aplikacija se pokreće pri boot-u računara
   - 24/7 dostupnost

✅ **Sigurnost:**
   - SSL sertifikat (HTTPS)
   - Automatsko obnavljanje sertifikata
   - Firewall konfigurisan

✅ **Monitoring:**
   - Detaljni logovi
   - Windows Event Viewer integracija
   - Performance monitoring

✅ **Jednostavno održavanje:**
   - Update script za nove verzije
   - Automatski backup možete podesiti
   - Sve komande dokumentovane

---

## KONTAKT

Sve informacije su u `publish/` folderu:
- **Početak:** `START-HERE.txt`
- **Brzo:** `BRZI-START.md`
- **Detaljno:** `DEPLOYMENT-UPUTSTVA.md`
- **DNS:** `DNS-SETUP.md`

---

**🎉 SVE JE SPREMNO ZA DEPLOYMENT!**

**Sledeći korak:** Prebacite `publish` folder na Windows 11 server i pratite `START-HERE.txt`

**Sreća!** 🚀
