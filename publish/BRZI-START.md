# BRZI START - FRUITSYS DEPLOYMENT

## 3 KORAKA DO POKRETANJA APLIKACIJE

### KORAK 1: Prenesite fajlove na Windows 11 server
- Kopirajte ceo `publish` folder na server (USB, mreža, itd.)
- Stavite u: `C:\FruitSys-Deploy`

### KORAK 2: Pokrenite deployment
```powershell
# Otvorite PowerShell kao Administrator
cd C:\FruitSys-Deploy
Set-ExecutionPolicy RemoteSigned -Scope CurrentUser -Force
.\deploy-to-server.ps1
```

### KORAK 3: Testirajte
Otvorite browser: `http://localhost`

---

## DODATNO: SSL SERTIFIKAT (NAKON DNS PROPAGACIJE)

```powershell
cd C:\FruitSys-Deploy
.\install-ssl.ps1
```

---

## ŠTA AKO NEŠTO NE RADI?

Pogledajte: `DEPLOYMENT-UPUTSTVA.md` → sekcija **TROUBLESHOOTING**

Ili pokrenite ručno:
```powershell
cd C:\inetpub\wwwroot\FruitSys
dotnet FruitSysWeb.dll
```

---

## DNS PODEŠAVANJA (KOD REGISTRARA DOMENA)

**1. Pronađite vašu IP:**
```powershell
(Invoke-WebRequest -Uri "https://api.ipify.org").Content
```

**2. Podesite kod registrara:**
```
A Record: @ → VASA_IP
A Record: www → VASA_IP
```

**3. Sačekajte 1-24h za DNS propagaciju**

---

## KORISNE KOMANDE

**Restart aplikacije:**
```powershell
Restart-WebAppPool -Name "FruitSysAppPool"
```

**Gledanje logova:**
```powershell
Get-Content "C:\inetpub\wwwroot\FruitSys\Logs\fruitsys-*.log" -Wait -Tail 20
```

**Provera statusa:**
```powershell
Get-WebAppPoolState -Name "FruitSysAppPool"
```

---

## AUTOMATSKI START

✅ **Već konfigurisano!**

Aplikacija će automatski pokrenuti nakon restarta računara.

---

**Za detaljnije informacije, pogledajte: `DEPLOYMENT-UPUTSTVA.md`**
