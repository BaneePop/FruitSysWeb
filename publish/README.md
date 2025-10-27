# FruitSys Production Deployment Package

## SADRŽAJ OVOG FOLDERA

### 📋 Dokumentacija
- **BRZI-START.md** - Brza uputstva za deployment (3 jednostavna koraka)
- **DEPLOYMENT-UPUTSTVA.md** - Kompletan detaljni vodič sa svim fazama
- **README.md** - Ovaj fajl (pregled paketa)

### 🚀 PowerShell Skripte
- **deploy-to-server.ps1** - Automatski deployment na Windows 11 server
- **install-ssl.ps1** - Automatska instalacija SSL sertifikata (Let's Encrypt)
- **update-app.ps1** - Update aplikacije na nove verzije

### 📦 Aplikacioni fajlovi
- **FruitSysWeb.dll** - Glavna aplikacija
- **appsettings.json** - Development konfiguracija
- **appsettings.Production.json** - Production konfiguracija
- **web.config** - IIS konfiguracija
- **Logo.png** - Logo za PDF export
- Ostali DLL fajlovi i zavisnosti

---

## QUICK START

### 1️⃣ Prenesite ovaj folder na Windows 11 server
Kopirajte ceo `publish` folder na server (USB, mreža, cloud).

### 2️⃣ Otvorite PowerShell kao Administrator
```powershell
cd C:\putanja\do\publish\foldera
Set-ExecutionPolicy RemoteSigned -Scope CurrentUser -Force
```

### 3️⃣ Pokrenite deployment
```powershell
.\deploy-to-server.ps1
```

### 4️⃣ Testirajte
Otvorite browser: http://localhost

---

## KOJA UPUTSTVA KORISTITI?

### Ako ste u žurbi ili poznajete PowerShell:
👉 **BRZI-START.md** - 3 koraka, 5 minuta

### Ako ste prvi put deploy-ujete aplikaciju:
👉 **DEPLOYMENT-UPUTSTVA.md** - Kompletan vodič, sve detaljno objašnjeno

### Ako imate problema:
👉 **DEPLOYMENT-UPUTSTVA.md** → sekcija **TROUBLESHOOTING**

---

## DEPLOYMENT FAZE

1. **DNS Konfiguracija** - Podesite A record kod registrara domena
2. **Transfer fajlova** - Kopirajte ovaj folder na server
3. **Deployment** - Pokrenite `deploy-to-server.ps1`
4. **SSL Setup** - Pokrenite `install-ssl.ps1` (nakon DNS propagacije)
5. **Testiranje** - Proverite da aplikacija radi
6. **Automatski start** - Već konfigurisano! ✅

---

## SYSTEM REQUIREMENTS

### Windows 11 Server:
- ✅ Windows 11 (Professional ili Enterprise)
- ✅ 4GB+ RAM (preporučeno 8GB+)
- ✅ Stabilna internet konekcija
- ✅ Statična IP adresa

### Softver (MORA biti instaliran PRE deployment-a):
- ✅ .NET 8 SDK ili Runtime
- ✅ **.NET 8 Hosting Bundle** ⚠️ **VAŽNO!**
  - Preuzmite: https://dotnet.microsoft.com/download/dotnet/8.0
  - "ASP.NET Core Runtime 8.0 - Windows Hosting Bundle"

### Domen:
- ✅ www.fruitsys.rs (kupljen)
- ⏳ DNS konfiguracija (uradite u FAZI 1)

---

## ŠTA DEPLOYMENT SCRIPT RADI?

`deploy-to-server.ps1` automatski:
1. ✅ Instalira IIS web server
2. ✅ Kreira Application Pool sa pravim podešavanjima
3. ✅ Kreira Web Site u IIS-u
4. ✅ Kopira aplikacione fajlove
5. ✅ Podešava permisije
6. ✅ Konfigurisu Windows Firewall
7. ✅ Startuje aplikaciju
8. ✅ Podešava **automatski start** nakon restarta računara

---

## ŠTA SSL SCRIPT RADI?

`install-ssl.ps1` automatski:
1. ✅ Preuzima Win-ACME alat (Let's Encrypt klijent)
2. ✅ Dobija besplatni SSL sertifikat
3. ✅ Instalira sertifikat u IIS
4. ✅ Konfigurisu HTTPS binding
5. ✅ Podešava **automatsko obnavljanje** (svaka 3 meseca)

---

## KORISNE KOMANDE

### Restart aplikacije:
```powershell
Restart-WebAppPool -Name "FruitSysAppPool"
```

### Provera statusa:
```powershell
Get-WebAppPoolState -Name "FruitSysAppPool"
Get-Website -Name "FruitSys"
```

### Gledanje logova:
```powershell
Get-Content "C:\inetpub\wwwroot\FruitSys\Logs\fruitsys-*.log" -Wait -Tail 20
```

### Update aplikacije na novu verziju:
```powershell
.\update-app.ps1 -NewVersionPath "C:\putanja\do\nove\verzije"
```

---

## STRUCTURE NAKON DEPLOYMENT-A

```
C:\inetpub\wwwroot\FruitSys\
├── FruitSysWeb.dll                 (Glavna aplikacija)
├── appsettings.json                (Dev config)
├── appsettings.Production.json     (Production config)
├── web.config                      (IIS config)
├── Logo.png                        (Logo)
├── Logs\                           (Aplikacioni logovi)
│   └── fruitsys-YYYYMMDD.log
├── wwwroot\                        (Static fajlovi)
└── ... (ostali DLL-ovi)
```

---

## TROUBLESHOOTING - TOP 5 PROBLEMA

### 1. "HTTP Error 502.5"
**Rešenje:** Instalirajte .NET 8 Hosting Bundle i restartujte računar

### 2. "Cannot find FruitSysWeb.dll"
**Rešenje:** Pokrenite `.\deploy-to-server.ps1` ponovo

### 3. Blazor ne radi (SignalR greška)
**Rešenje:**
```powershell
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebSockets -All -NoRestart
iisreset
```

### 4. SSL ne radi
**Rešenje:** Sačekajte DNS propagaciju (1-24h), zatim pokrenite `.\install-ssl.ps1`

### 5. Aplikacija se ne startuje nakon restarta
**Rešenje:**
```powershell
Set-ItemProperty "IIS:\AppPools\FruitSysAppPool" -Name startMode -Value AlwaysRunning
Restart-WebAppPool -Name "FruitSysAppPool"
```

---

## DEPLOYMENT CHECKLIST

Pratite ovaj checklist dok deploy-ujete:

- [ ] .NET 8 Hosting Bundle instaliran
- [ ] Publish folder prebačen na server
- [ ] DNS A record podešen kod registrara
- [ ] `deploy-to-server.ps1` izvršen uspešno
- [ ] Aplikacija radi na http://localhost
- [ ] DNS propagacija završena (proverite: nslookup www.fruitsys.rs)
- [ ] `install-ssl.ps1` izvršen uspešno
- [ ] HTTPS radi (zeleni katanac u browseru)
- [ ] Test restarta računara (aplikacija se automatski pokreće)
- [ ] Backup setup konfigurisan
- [ ] Monitoring i logging proveren

---

## SECURITY NOTES

### ✅ Već konfigurisano:
- HTTPS redirekcija
- Security headers
- Windows Firewall pravila (samo 80, 443)
- Automatsko SSL obnavljanje

### ⚠️ Dodatno preporučujem:
- Omogućite Windows Update (ali kontrolisano)
- Koristite jak password za Windows korisnika
- Omogućite Remote Desktop SAMO ako je potreban
- Pravite redovne backup-e (koristite `update-app.ps1` kao template)
- Monitoring logova

---

## VERZIJA I CHANGELOG

**Verzija:** Production Release v1.0
**Datum:** 2025-10-27
**Build:** .NET 8.0

### Uključene komponente:
- Blazor Server aplikacija
- MySQL Dapper connection
- Serilog logging
- QuestPDF export
- ClosedXML Excel export
- ApexCharts vizualizacije

---

## KONTAKT I PODRŠKA

### Logovi aplikacije:
```
C:\inetpub\wwwroot\FruitSys\Logs\
```

### IIS logovi:
```
C:\inetpub\logs\LogFiles\
```

### Windows Event Viewer:
```powershell
eventvwr.msc
# Windows Logs → Application → Filter: IIS AspNetCore Module
```

---

## DODATNI RESURSI

- [IIS Dokumentacija](https://docs.microsoft.com/en-us/iis/)
- [ASP.NET Core Hosting na IIS](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/)
- [Win-ACME Dokumentacija](https://www.win-acme.com/)
- [Let's Encrypt](https://letsencrypt.org/)

---

**🎉 SPREMNO ZA DEPLOYMENT!**

Počnite sa: **BRZI-START.md** ili **DEPLOYMENT-UPUTSTVA.md**
