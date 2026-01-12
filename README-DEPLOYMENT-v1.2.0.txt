╔══════════════════════════════════════════════════════════════════════════════╗
║                                                                              ║
║                  FruitSysWeb v1.2.0 - DEPLOYMENT PAKET                       ║
║                                                                              ║
║                      Datum: 22. decembar 2025                                ║
║                                                                              ║
╚══════════════════════════════════════════════════════════════════════════════╝


📦 SADRŽAJ DEPLOYMENT PAKETA
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  DEPLOYMENT FAJLOVI (KOPIRAJ NA SERVER):
  ────────────────────────────────────────────────────────────────────────────
  ✅ FruitSysWeb-Production-v1.2.0-20251222-181217.zip  (39 MB)
  ✅ deploy-to-windows-server-v1.2.0.ps1                (11 KB)


  DOKUMENTACIJA (OPCIONO):
  ────────────────────────────────────────────────────────────────────────────
  📖 QUICK-START-DEPLOYMENT.txt           → Brzi start za deployment
  📖 DEPLOYMENT-SUMMARY-v1.2.0.txt        → Sažetak deployment-a
  📖 DEPLOYMENT-v1.2.0-INSTRUKCIJE.md     → Kompletan deployment guide
  📖 CHANGELOG-v1.2.0.md                  → Detaljni changelog
  📖 README-DEPLOYMENT-v1.2.0.txt         → Ovaj fajl


━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
⚡ BRZI START
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  KORAK 1: Kopiraj na Windows Server
  ──────────────────────────────────────────────────────────────────────────
  Kopiraj ova 2 fajla u:  C:\Temp\FruitSysWeb-Deployment\

    • FruitSysWeb-Production-v1.2.0-20251222-181217.zip
    • deploy-to-windows-server-v1.2.0.ps1


  KORAK 2: Pokreni Deployment (PowerShell kao Administrator)
  ──────────────────────────────────────────────────────────────────────────
  cd C:\Temp\FruitSysWeb-Deployment
  .\deploy-to-windows-server-v1.2.0.ps1 -IIS -AppPoolName "FruitSysWeb"


  KORAK 3: Testiraj
  ──────────────────────────────────────────────────────────────────────────
  http://192.168.1.11:6001

  Testiraj nove funkcionalnosti:
    • FinansijeHome → Pregled Salda
    • FinansijeHome → Stanje Kase Altiva


━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
🎯 ŠTA JE NOVO U v1.2.0
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  ✅ Pregled Salda Po Komitentu
     - Kompletna stranica za pregled dugovanja i potraživanja
     - Export to Excel i PDF

  ✅ Stanje Kase Altiva
     - Pregled stanja kese sa selekcijom proizvoda
     - Čuva user izbore između sessions (JSON persistence)

  ✅ Refactoring
     - Centralizacija tipova (FinansijskiPregledService → FinansijeService)
     - Brisanje nekorišćenih fajlova
     - Novi KesaSelekcijaService za user preferences


━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
📋 DEPLOYMENT OPCIJE
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  IIS Application Pool (Preporučeno):
  ──────────────────────────────────────────────────────────────────────────
  .\deploy-to-windows-server-v1.2.0.ps1 -IIS -AppPoolName "FruitSysWeb"

  Windows Service:
  ──────────────────────────────────────────────────────────────────────────
  .\deploy-to-windows-server-v1.2.0.ps1 -Service

  Standalone:
  ──────────────────────────────────────────────────────────────────────────
  .\deploy-to-windows-server-v1.2.0.ps1 -Standalone

  Custom Path:
  ──────────────────────────────────────────────────────────────────────────
  .\deploy-to-windows-server-v1.2.0.ps1 -IIS `
      -TargetPath "D:\MyApps\FruitSysWeb" `
      -AppPoolName "MyAppPool"


━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
🔧 ŠTA DEPLOYMENT SKRIPTA RADI
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  1. ✅ Zaustavi aplikaciju (IIS App Pool / Service / Standalone)
  2. ✅ Kreira backup postojeće aplikacije
  3. ✅ Čuva appsettings.Production.json
  4. ✅ Čuva Data folder (user preferences)
  5. ✅ Briše stare fajlove
  6. ✅ Raspakuje novi deployment ZIP
  7. ✅ Vraća appsettings.Production.json
  8. ✅ Vraća Data folder
  9. ✅ Postavlja permissions (IIS_IUSRS)
  10. ✅ Pokreće aplikaciju
  11. ✅ Verifikuje deployment
  12. ✅ Čisti stare backup-e (čuva zadnjih 3)


━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
⚙️ SISTEM ZAHTEVI
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  ✅ Windows Server 2016+ (ili Windows 10/11 Pro)
  ✅ IIS 10.0+ (ako koristite IIS)
  ✅ .NET 8.0 Runtime (ASP.NET Core Runtime)
     Download: https://dotnet.microsoft.com/download/dotnet/8.0
  ✅ MySQL Connection: 185.102.237.236:63388 (fruitsysdb_v2)
  ✅ Network access na port 6001


━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
📊 BUILD INFORMACIJE
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  Verzija:          1.2.0
  Build datum:      22. decembar 2025, 18:12
  .NET Runtime:     8.0
  Configuration:    Release
  Self-contained:   Ne (zahteva .NET 8.0 Runtime na serveru)
  DLL veličina:     2.0 MB (FruitSysWeb.dll)
  ZIP veličina:     39 MB


━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
🔐 PERMISSIONS (Automatski postavlja deployment skripta)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  C:\FruitSysWeb\              → IIS_IUSRS: Full Control
  C:\FruitSysWeb\Data\         → IIS_IUSRS: Modify (Read + Write) ← NOVO!
  C:\FruitSysWeb\Logs\         → IIS_IUSRS: Modify (Read + Write)


━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
✅ POST-DEPLOYMENT CHECKLIST
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  OSNOVNE PROVERE:
  ──────────────────────────────────────────────────────────────────────────
  [ ] Aplikacija se učitava (http://192.168.1.11:6001)
  [ ] Login radi
  [ ] Dashboard se prikazuje

  NOVE FUNKCIONALNOSTI:
  ──────────────────────────────────────────────────────────────────────────
  [ ] FinansijeHome → Pregled Salda (učitava podatke)
  [ ] Pregled Salda → Export to Excel
  [ ] Pregled Salda → Export to PDF
  [ ] FinansijeHome → Stanje Kase Altiva (učitava proizvode)
  [ ] Stanje Kase → Selektuj proizvode
  [ ] Stanje Kase → Refresh stranice (selections sačuvane?)
  [ ] Proveri: C:\FruitSysWeb\Data\stanje-kese-selekcija.json

  REGRESSION TESTING:
  ──────────────────────────────────────────────────────────────────────────
  [ ] PreradaHome → Proizvodnja
  [ ] PreradaHome → Radni Nalozi
  [ ] LagerHome → Magacin Lager
  [ ] FinansijeHome → Nabavka
  [ ] FinansijeHome → Prodaja


━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
❌ ROLLBACK (Ako nešto pođe naopako)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  Stop-WebAppPool -Name "FruitSysWeb"
  
  $backup = "C:\FruitSysWeb-Backup-YYYYMMDD-HHMMSS"
  Remove-Item -Path "C:\FruitSysWeb\*" -Recurse -Force -Exclude "Logs"
  Copy-Item -Path "$backup\*" -Destination "C:\FruitSysWeb\" -Recurse -Force
  
  Start-WebAppPool -Name "FruitSysWeb"

  (Zameni YYYYMMDD-HHMMSS sa pravim datumom iz backup foldera)


━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
📞 PODRŠKA
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  Za detaljne instrukcije:
  ──────────────────────────────────────────────────────────────────────────
  📖 QUICK-START-DEPLOYMENT.txt        → Brzi start
  📖 DEPLOYMENT-v1.2.0-INSTRUKCIJE.md  → Kompletan guide + Troubleshooting
  📖 DEPLOYMENT-SUMMARY-v1.2.0.txt     → Sažetak
  📖 CHANGELOG-v1.2.0.md                → Detaljni changelog

  Logovi:
  ──────────────────────────────────────────────────────────────────────────
  C:\FruitSysWeb\Logs\fruitsysweb-YYYYMMDD.txt


━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

🚀 SREĆAN DEPLOYMENT!

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
