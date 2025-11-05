# FruitSysWeb Deployment Script - Windows Server
# Verzija: 1.1.0
# Target: 192.168.1.11:6001

param(
    [string]$DeploymentZip = "FruitSysWeb-Production-v1.1.0-20251105-223443.zip",
    [string]$TargetPath = "C:\FruitSysWeb",
    [string]$AppPoolName = "FruitSysWeb",
    [switch]$IIS = $false,
    [switch]$Service = $false,
    [switch]$Standalone = $false
)

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "FruitSysWeb Deployment Script v1.1.0" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Proveri da li ZIP postoji
if (!(Test-Path $DeploymentZip)) {
    Write-Host "ERROR: Deployment ZIP ne postoji: $DeploymentZip" -ForegroundColor Red
    exit 1
}

Write-Host "1. Pronađen deployment paket: $DeploymentZip" -ForegroundColor Green

# Zaustavi aplikaciju
Write-Host ""
Write-Host "2. Zaustavljam aplikaciju..." -ForegroundColor Yellow

if ($IIS) {
    Write-Host "   - Zaustavljam IIS Application Pool: $AppPoolName" -ForegroundColor Gray
    Stop-WebAppPool -Name $AppPoolName
    Start-Sleep -Seconds 3
}
elseif ($Service) {
    Write-Host "   - Zaustavljam Windows Service: FruitSysWeb" -ForegroundColor Gray
    Stop-Service -Name "FruitSysWeb" -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 3
}
elseif ($Standalone) {
    Write-Host "   - Zaustavljam standalone proces..." -ForegroundColor Gray
    Get-Process -Name "FruitSysWeb" -ErrorAction SilentlyContinue | Stop-Process -Force
    Start-Sleep -Seconds 3
}
else {
    Write-Host "   UPOZORENJE: Niste izabrali način pokretanja (IIS/Service/Standalone)" -ForegroundColor Yellow
    Write-Host "   Moraćete RUČNO zaustaviti aplikaciju!" -ForegroundColor Yellow
    $continue = Read-Host "Da li želite da nastavite? (y/n)"
    if ($continue -ne "y") {
        exit 0
    }
}

# Kreiraj backup
Write-Host ""
Write-Host "3. Kreiram backup postojeće aplikacije..." -ForegroundColor Yellow

$backupFolder = "$TargetPath-Backup-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
if (Test-Path $TargetPath) {
    New-Item -ItemType Directory -Path $backupFolder -Force | Out-Null

    # Backup SVIH fajlova
    Copy-Item -Path "$TargetPath\*" -Destination $backupFolder -Recurse -Force
    Write-Host "   - Backup kreiran: $backupFolder" -ForegroundColor Green

    # Dodatni backup appsettings.Production.json
    if (Test-Path "$TargetPath\appsettings.Production.json") {
        Copy-Item -Path "$TargetPath\appsettings.Production.json" -Destination "$env:TEMP\appsettings.Production.backup.json" -Force
        Write-Host "   - Sačuvan appsettings.Production.json u TEMP folderu" -ForegroundColor Green
    }
} else {
    Write-Host "   - Target folder ne postoji, preskačem backup" -ForegroundColor Gray
    New-Item -ItemType Directory -Path $TargetPath -Force | Out-Null
}

# Očisti stare fajlove (NE DIRAJ appsettings.Production.json i Logs)
Write-Host ""
Write-Host "4. Brišem stare fajlove..." -ForegroundColor Yellow

Get-ChildItem -Path $TargetPath -Exclude "appsettings.Production.json","Logs" | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "   - Stari fajlovi obrisani" -ForegroundColor Green

# Raspakuj novi deployment
Write-Host ""
Write-Host "5. Raspakujem novi deployment..." -ForegroundColor Yellow

$tempFolder = "$env:TEMP\FruitSysWeb-Deploy-Temp"
if (Test-Path $tempFolder) {
    Remove-Item -Path $tempFolder -Recurse -Force
}

Expand-Archive -Path $DeploymentZip -DestinationPath $tempFolder -Force
Write-Host "   - ZIP raspakovan u temp folder" -ForegroundColor Green

# Kopiraj iz publish foldera
if (Test-Path "$tempFolder\publish") {
    Copy-Item -Path "$tempFolder\publish\*" -Destination $TargetPath -Recurse -Force
    Write-Host "   - Fajlovi kopirani u $TargetPath" -ForegroundColor Green
} else {
    Write-Host "   ERROR: publish folder ne postoji u ZIP-u!" -ForegroundColor Red
    exit 1
}

# Očisti temp folder
Remove-Item -Path $tempFolder -Recurse -Force
Write-Host "   - Temp folder očišćen" -ForegroundColor Green

# Proveri da li postoji appsettings.Production.json
Write-Host ""
Write-Host "6. Proveravam appsettings.Production.json..." -ForegroundColor Yellow

if (!(Test-Path "$TargetPath\appsettings.Production.json")) {
    Write-Host "   UPOZORENJE: appsettings.Production.json NE POSTOJI!" -ForegroundColor Red

    if (Test-Path "$env:TEMP\appsettings.Production.backup.json") {
        Copy-Item -Path "$env:TEMP\appsettings.Production.backup.json" -Destination "$TargetPath\appsettings.Production.json" -Force
        Write-Host "   - Vraćen iz backup-a" -ForegroundColor Green
    } else {
        Write-Host "   ERROR: Backup fajl takođe ne postoji!" -ForegroundColor Red
        Write-Host "   Moraćete RUČNO kreirati appsettings.Production.json!" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "   - appsettings.Production.json postoji" -ForegroundColor Green
}

# Proveri Logs folder
if (!(Test-Path "$TargetPath\Logs")) {
    New-Item -ItemType Directory -Path "$TargetPath\Logs" -Force | Out-Null
    Write-Host "   - Kreiran Logs folder" -ForegroundColor Green
}

# Postavi permissions (samo za IIS)
if ($IIS) {
    Write-Host ""
    Write-Host "7. Postavljam permissions za IIS..." -ForegroundColor Yellow

    icacls "$TargetPath" /grant "IIS_IUSRS:(OI)(CI)F" /T /Q
    Write-Host "   - Permissions postavljeni" -ForegroundColor Green
}

# Pokreni aplikaciju
Write-Host ""
Write-Host "8. Pokrećem aplikaciju..." -ForegroundColor Yellow

if ($IIS) {
    Start-WebAppPool -Name $AppPoolName
    Write-Host "   - IIS Application Pool pokrenut: $AppPoolName" -ForegroundColor Green
}
elseif ($Service) {
    Start-Service -Name "FruitSysWeb"
    Write-Host "   - Windows Service pokrenut: FruitSysWeb" -ForegroundColor Green
}
elseif ($Standalone) {
    Write-Host "   UPOZORENJE: Za standalone, pokrenite ručno:" -ForegroundColor Yellow
    Write-Host "   cd $TargetPath" -ForegroundColor Gray
    Write-Host "   .\FruitSysWeb.exe" -ForegroundColor Gray
}
else {
    Write-Host "   UPOZORENJE: Pokrenite aplikaciju RUČNO!" -ForegroundColor Yellow
}

# Zaključak
Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Deployment ZAVRŠEN!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Aplikacija bi trebala biti dostupna na: http://192.168.1.11:6001" -ForegroundColor White
Write-Host ""
Write-Host "Proveri:" -ForegroundColor Yellow
Write-Host "  1. Login stranicu" -ForegroundColor Gray
Write-Host "  2. TroskoviHome - Nove chart-ove" -ForegroundColor Gray
Write-Host "  3. Proizvodnja - Brzinu učitavanja" -ForegroundColor Gray
Write-Host "  4. FinansijeHome - Top 5 Dobavljača" -ForegroundColor Gray
Write-Host ""
Write-Host "Backup lokacija: $backupFolder" -ForegroundColor Cyan
Write-Host ""

# Rollback instrukcije
Write-Host "ROLLBACK (ako nešto ne radi):" -ForegroundColor Red
Write-Host "  Stop-WebAppPool -Name '$AppPoolName'" -ForegroundColor Gray
Write-Host "  Remove-Item -Path '$TargetPath\*' -Recurse -Force" -ForegroundColor Gray
Write-Host "  Copy-Item -Path '$backupFolder\*' -Destination '$TargetPath' -Recurse" -ForegroundColor Gray
Write-Host "  Start-WebAppPool -Name '$AppPoolName'" -ForegroundColor Gray
Write-Host ""
