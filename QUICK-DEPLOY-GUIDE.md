# 🚀 FruitSysWeb - Quick Deploy Guide

Brzo ažuriranje aplikacije za sitne izmene u Razor komponentama.

**VREME: ~1 minut umesto 5 minuta!**

---

## 📋 Kada Koristiti Quick Deploy?

✅ **KORISTI Quick Deploy za:**
- Izmene u `.razor` fajlovima (FinansijskiPregled.razor, PregledSalda.razor, itd.)
- Izmene u C# logici komponenti
- Izmene u servisima (FinansijeService.cs, MagacinService.cs, itd.)
- Izmene u modelima (samo dodavanje/izmena properti-ja)

❌ **NE KORISTI Quick Deploy za:**
- Dodavanje novih NuGet paketa
- Izmene u `appsettings.json`
- Database migracije ili schema promene
- Izmene u `Program.cs` ili Dependency Injection konfiguraciji
- Dodavanje potpuno novih fajlova u `wwwroot/`

---

## 🖥️ NAČIN 1: NAJBRŽI - Direktna Zamena (30 sekundi)

### Na Mac-u (lokalni računar):

```bash
cd /Users/Bane/FruitSysWeb

# Publish PROJEKTA (ne solution!)
dotnet publish FruitSysWeb.csproj -c Release -o ./publish
```

Ovo kreira: `./publish/FruitSysWeb.dll` (i opcionalno `.pdb` za debugging)

### Kopiraj na server:

**Preko USB/RDP/SMB** kopirajte ovaj fajl:
- `./publish/FruitSysWeb.dll` → `C:\FruitSysWeb\FruitSysWeb.dll` (zameni postojeći)

**Opcionalno** (ako želite detaljnije error poruke):
- `./publish/FruitSysWeb.pdb` → `C:\FruitSysWeb\FruitSysWeb.pdb` (debugging simboli)

### Na Windows Serveru (192.168.1.11):

**Restart NSSM servisa:**

```powershell
# Zaustavi servis
nssm stop FruitSysWeb

# ILI preko standardne komande
net stop FruitSysWeb

# Sačekaj 2-3 sekunde da se proces zaustavi
Start-Sleep -Seconds 3

# Pokreni servis
nssm start FruitSysWeb

# ILI
net start FruitSysWeb
```

**GOTOVO!** Osvežite browser (Ctrl+F5)

---

## 🛡️ NAČIN 2: Sa Automatskim Backup-om (Sigurniji)

Koristi skriptu koja pravi automatski backup pre zamene.

### Na Mac-u:

```bash
cd /Users/Bane/FruitSysWeb
./quick-deploy.sh
```

### Na Serveru:

```powershell
cd C:\FruitSysWeb
.\quick-update-server.ps1 -SourceDLL "FruitSysWeb.dll" -Service
```

**Napomena:** Dodao sam `-Service` flag koji koristi NSSM umesto IIS.

---

## ⚡ NAČIN 2: Potpuno Automatski (Potrebna Setup)

Ako imate SSH ili PowerShell Remoting podešen:

### Setup (jednom):

Na Windows serveru:
```powershell
# Omogući PowerShell Remoting
Enable-PSRemoting -Force
Set-Item WSMan:\localhost\Client\TrustedHosts -Value "192.168.1.11" -Force
```

Na Mac-u:
```bash
# Instaliraj PowerShell
brew install --cask powershell
```

### Deploy:

```bash
cd /Users/Bane/FruitSysWeb
./quick-deploy.sh
# (Odkomentiraj automatski deploy sekciju u skripti)
```

---

## 🛠️ NAČIN 3: Samo Server-Side Update

Ako već imate novi DLL na serveru:

```powershell
cd C:\FruitSysWeb

# Kopiraj novi DLL u folder (preko USB, network share, itd.)
# Zatim pokreni:

.\quick-update-server.ps1 -SourceDLL "FruitSysWeb.dll"
```

---

## 📊 Poređenje Vremena

| Metod | Vreme | Kada Koristiti |
|-------|-------|----------------|
| **Quick Deploy** | ~1 minut | Izmene u Razor/C# kodu |
| **CSS/wwwroot** | ~10 sekundi | Samo CSS/JS izmene (direktna zamena) |
| **Full Deploy** | ~5 minuta | Novi paketi, DB promene, production release |

---

## 🔄 Workflow Primer

### Scenario: Izmena u FinansijskiPregled.razor

1. **Izmeniš kod** u VSCode:
   ```csharp
   // FinansijskiPregled.razor
   <h3>Finansijski Pregled - NOVA VERZIJA</h3>
   ```

2. **Pokreneš quick deploy** na Mac-u:
   ```bash
   ./quick-deploy.sh
   ```

3. **Kopiraj DLL** na server (USB, RDP, SMB)

4. **Pokreneš update** na serveru:
   ```powershell
   .\quick-update-server.ps1 -SourceDLL "FruitSysWeb.dll"
   ```

5. **Osvežiš browser** (Ctrl+F5) - GOTOVO!

**Ukupno vreme: ~1 minut** ⚡

---

## 🧪 Testiranje Lokalno Pre Deploya

Pre nego što deployuješ na server, testiraj lokalno:

```bash
cd /Users/Bane/FruitSysWeb
dotnet watch run
```

- Izmene u `.razor` fajlovima se automatski primenjuju
- Browser se osvežava automatski
- Nema potrebe za restartom

Kada si zadovoljan, pokreni quick deploy.

---

## ❗ Troubleshooting

### Problem: "DLL je zaključan, ne može se kopirati"

**Rešenje:**
```powershell
# Zaustavi App Pool ručno
Stop-WebAppPool -Name "FruitSysWeb"

# Kopiraj DLL
Copy-Item "FruitSysWeb.dll" "C:\FruitSysWeb\" -Force

# Pokreni App Pool
Start-WebAppPool -Name "FruitSysWeb"
```

### Problem: "Izmene se ne vide u browseru"

**Rešenje:**
- Osvežite browser sa **Ctrl+F5** (hard refresh)
- Proveri da li je App Pool restartovan
- Proveri Logs folder: `C:\FruitSysWeb\Logs\`

### Problem: "Aplikacija crashuje posle update-a"

**Rešenje - ROLLBACK:**
```powershell
# Vrati stari DLL iz backup-a
Copy-Item "C:\FruitSysWeb\FruitSysWeb.dll.backup-YYYYMMDD-HHMMSS" "C:\FruitSysWeb\FruitSysWeb.dll" -Force

# Restartuj App Pool
Restart-WebAppPool -Name "FruitSysWeb"
```

---

## 📁 Struktura Fajlova

```
FruitSysWeb/
├── quick-deploy.sh              # Mac: Build & pripremi DLL
├── quick-update-server.ps1       # Server: Update DLL i restart
├── deploy-to-windows-server-v1.2.0.ps1  # Full deployment
└── quick-deploy-temp/            # Temp folder za DLL fajlove
    ├── FruitSysWeb.dll
    └── FruitSysWeb.Views.dll
```

---

## 💡 Pro Tips

1. **Uvek testiraj lokalno** sa `dotnet watch run` pre quick deploya
2. **Backupi se automatski kreiraju** - čuvaju se poslednja 3
3. **Za CSS izmene** - direktno zameni fajlove u `wwwroot/`, nije potreban restart
4. **Za hitne izmene** - quick deploy je dovoljno brz
5. **Za production release** - koristi puni deployment sa verzijom

---

## 🎯 Kada Koristi Svaki Metod

```
┌─────────────────────────────────────────────────────┐
│ Razvoj Lokalno                                      │
│ → dotnet watch run (1-2 sekunde)                   │
└─────────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────────┐
│ Testiranje & Sitne Izmene na Serveru               │
│ → quick-deploy.sh + quick-update-server.ps1         │
│   (~1 minut)                                        │
└─────────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────────┐
│ Production Release (nova verzija)                  │
│ → deploy-to-windows-server-v1.2.0.ps1               │
│   (~5 minuta)                                       │
└─────────────────────────────────────────────────────┘
```

---

## 📞 Support

Za probleme:
- Proveri Logs: `C:\FruitSysWeb\Logs\`
- Rollback na backup ako nešto ne radi
- Za kritične probleme, koristi puni deployment

**Srećan razvoj! 🚀**
