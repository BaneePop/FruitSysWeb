================================================================================
FruitSysWeb - Production Deployment v1.1.0
================================================================================

DEPLOYMENT PAKET KREIRAN: 05.11.2025 22:34
TARGET SERVER: Windows (192.168.1.11:6001)

================================================================================
FAJLOVI U DEPLOYMENT PAKETU
================================================================================

1. FruitSysWeb-Production-v1.1.0-20251105-223443.zip (16MB)
   - Glavni deployment paket sa svim fajlovima

2. FruitSysWeb-Production-v1.1.0-20251105-223443.zip.sha256
   - SHA256 checksum za verifikaciju integriteta paketa
   - Checksum: e70f911078941d7bb8df422fbf8d6c1494c0279c525e37d8849c81c5a60cc099

3. deploy-to-windows-server.ps1
   - PowerShell script za automatski deployment

4. DEPLOYMENT-UPUTSTVO-v1.1.0.md
   - Detaljno uputstvo za deployment (170+ linija instrukcija)

5. DEPLOYMENT-README.txt
   - Ovaj fajl (kratak pregled)

================================================================================
BRZI START - AUTOMATSKI DEPLOYMENT
================================================================================

KORAK 1: Kopiraj fajlove na Windows server (192.168.1.11)

   - FruitSysWeb-Production-v1.1.0-20251105-223443.zip
   - deploy-to-windows-server.ps1

KORAK 2: Otvori PowerShell kao Administrator na Windows serveru

KORAK 3: Pokreni deployment script:

   # Za IIS:
   .\deploy-to-windows-server.ps1 -IIS -AppPoolName "FruitSysWeb"

   # Za Windows Service:
   .\deploy-to-windows-server.ps1 -Service

   # Za Standalone:
   .\deploy-to-windows-server.ps1 -Standalone

KORAK 4: Proveri aplikaciju u browser-u:

   http://192.168.1.11:6001

================================================================================
RUČNI DEPLOYMENT
================================================================================

1. Zaustavi aplikaciju (IIS Application Pool ili Windows Service)

2. Backup postojećeg foldera C:\FruitSysWeb

3. Raspakuj ZIP u temp folder

4. Kopiraj fajlove iz "publish" foldera u C:\FruitSysWeb

5. VAŽNO: NE PREPISUJ appsettings.Production.json!

6. Pokreni aplikaciju

Detaljne instrukcije: Vidi DEPLOYMENT-UPUTSTVO-v1.1.0.md

================================================================================
ŠTA JE NOVO U v1.1.0
================================================================================

PERFORMANSE:
  ✓ TroskoviHome - Pregled po Radnom Danu: 50% brže
  ✓ TroskoviHome - Troškovi po RN Chart: 3-5x brže
  ✓ Proizvodnja - Svi radni nalozi: 50% brže
  ✓ FinansijeHome - Top 5 Dobavljača: 40-50% brže

UI/UX:
  ✓ TroskoviHome - Agregacija po smeni (tačniji podaci)
  ✓ RadniNaloziPregled - Uklonjena kolona "Procenat"
  ✓ TroskoviHome - Novi chart "Troškovi po RN - Zadnjih 10"

================================================================================
ROLLBACK (Ako Nešto Ne Radi)
================================================================================

PowerShell:

  Stop-WebAppPool -Name "FruitSysWeb"
  Remove-Item -Path "C:\FruitSysWeb\*" -Recurse -Force
  Copy-Item -Path "C:\FruitSysWeb-Backup-DATUM\*" -Destination "C:\FruitSysWeb" -Recurse
  Start-WebAppPool -Name "FruitSysWeb"

================================================================================
TROUBLESHOOTING
================================================================================

Problem: Aplikacija se ne pokreće
Rešenje: Proveri da li postoji appsettings.Production.json

Problem: "Connection String" greška
Rešenje: Vrati appsettings.Production.json iz backup-a

Problem: Stranice se sporo učitavaju
Rešenje: Proveri database indekse (OptimizacijeIndeksa.sql)

Problem: Chart-ovi ne prikazuju podatke
Rešenje: Očisti browser cache (Ctrl+Shift+Delete)

================================================================================
KONTAKT
================================================================================

GitHub: https://github.com/BaneePop/FruitSysWeb
Branch: feature/centralize-types

Commits:
  - ce025cc: Feature: 8 optimizacija i izmena
  - 2cbb639: Chore: Čišćenje projekta

================================================================================
DEPLOYMENT CHECKLIST
================================================================================

Pre Deployment:
  [ ] Backup postojeće aplikacije
  [ ] Backup appsettings.Production.json
  [ ] Zaustavi aplikaciju
  [ ] Proveri da li .NET 8.0 Runtime je instaliran

Posle Deployment:
  [ ] Proveri login stranicu (http://192.168.1.11:6001)
  [ ] Testiraj TroskoviHome - Nove chart-ove
  [ ] Testiraj Proizvodnja - Brzinu učitavanja
  [ ] Testiraj FinansijeHome - Top 5 Dobavljača
  [ ] Testiraj RadniNaloziPregled - Uklonjena kolona "Procenat"
  [ ] Proveri logove (C:\FruitSysWeb\Logs\fruitsys-DATUM.log)

================================================================================
KRAJ
================================================================================
