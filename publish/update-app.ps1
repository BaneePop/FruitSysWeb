# FruitSys Update Script
# Koristite ovaj script za update aplikacije na produkciji

param(
    [Parameter(Mandatory=$false)]
    [string]$NewVersionPath = "",

    [Parameter(Mandatory=$false)]
    [string]$InstallPath = "C:\inetpub\wwwroot\FruitSys",

    [Parameter(Mandatory=$false)]
    [string]$BackupPath = "D:\Backups\FruitSys"
)

# Provera admin prava
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Error "Ovaj script mora biti pokrenut kao Administrator!"
    Exit 1
}

Write-Host "=== FruitSys Update Script ===" -ForegroundColor Green
Write-Host ""

# Ako nije naveden put do nove verzije, pitaj korisnika
if ([string]::IsNullOrEmpty($NewVersionPath)) {
    $NewVersionPath = Read-Host "Unesite punu putanju do nove verzije (folder sa FruitSysWeb.dll)"
    if ([string]::IsNullOrEmpty($NewVersionPath)) {
        Write-Error "Putanja je obavezna!"
        Exit 1
    }
}

# Provera da li nova verzija postoji
if (-not (Test-Path (Join-Path $NewVersionPath "FruitSysWeb.dll"))) {
    Write-Error "FruitSysWeb.dll nije pronađen u: $NewVersionPath"
    Exit 1
}

Write-Host "Nova verzija: $NewVersionPath" -ForegroundColor Cyan
Write-Host "Instalaciona putanja: $InstallPath" -ForegroundColor Cyan
Write-Host ""

# Provera da li postoji backup folder
if (-not (Test-Path $BackupPath)) {
    Write-Host "Kreiram backup folder: $BackupPath" -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $BackupPath -Force | Out-Null
}

# 1. Stop Application Pool
Write-Host "[1/6] Zaustavljam aplikaciju..." -ForegroundColor Yellow
Import-Module WebAdministration
$appPoolName = "FruitSysAppPool"

$poolState = Get-WebAppPoolState -Name $appPoolName
if ($poolState.Value -eq "Started") {
    Stop-WebAppPool -Name $appPoolName
    Write-Host "  Čekam da se Application Pool zaustavi..."
    Start-Sleep -Seconds 5

    # Proveri da li je stvarno zaustavljen
    $retries = 0
    while ((Get-WebAppPoolState -Name $appPoolName).Value -ne "Stopped" -and $retries -lt 10) {
        Start-Sleep -Seconds 2
        $retries++
    }

    if ((Get-WebAppPoolState -Name $appPoolName).Value -eq "Stopped") {
        Write-Host "  Application Pool zaustavljen!" -ForegroundColor Green
    } else {
        Write-Warning "Application Pool se nije zaustavio nakon $($retries * 2) sekundi. Nastavljam..."
    }
} else {
    Write-Host "  Application Pool već zaustavljen." -ForegroundColor Cyan
}

# 2. Backup trenutne verzije
Write-Host "[2/6] Pravim backup trenutne verzije..." -ForegroundColor Yellow
$date = Get-Date -Format "yyyyMMdd_HHmmss"
$backupFile = Join-Path $BackupPath "FruitSys-BEFORE-UPDATE-$date.zip"

try {
    Compress-Archive -Path $InstallPath -DestinationPath $backupFile -Force
    Write-Host "  Backup sačuvan: $backupFile" -ForegroundColor Green
} catch {
    Write-Error "Greška pri kreiranju backup-a: $_"
    Write-Host "Nastavljam bez backup-a..."
}

# 3. Brisanje starih fajlova (osim Logs foldera i appsettings.Production.json)
Write-Host "[3/6] Brisem stare fajlove..." -ForegroundColor Yellow

$itemsToDelete = Get-ChildItem -Path $InstallPath -Exclude "Logs", "appsettings.Production.json"
foreach ($item in $itemsToDelete) {
    try {
        Remove-Item $item.FullName -Recurse -Force -ErrorAction Stop
    } catch {
        Write-Warning "Ne mogu obrisati: $($item.FullName)"
    }
}

Write-Host "  Stari fajlovi obrisani!" -ForegroundColor Green

# 4. Kopiranje novih fajlova
Write-Host "[4/6] Kopiram nove fajlove..." -ForegroundColor Yellow

try {
    # Kopiraj sve osim appsettings.Production.json (da ne pregazimo production config)
    Get-ChildItem -Path $NewVersionPath | Where-Object { $_.Name -ne "appsettings.Production.json" } | ForEach-Object {
        Copy-Item -Path $_.FullName -Destination $InstallPath -Recurse -Force
    }
    Write-Host "  Novi fajlovi kopirani!" -ForegroundColor Green
} catch {
    Write-Error "Greška pri kopiranju fajlova: $_"
    Write-Host ""
    Write-Host "ROLLBACK:" -ForegroundColor Red
    Write-Host "Ako želite da vratite staru verziju, izvršite:"
    Write-Host "  Expand-Archive -Path '$backupFile' -DestinationPath '$InstallPath' -Force"
    Write-Host "  Restart-WebAppPool -Name '$appPoolName'"
    Exit 1
}

# 5. Provera integritet fajlova
Write-Host "[5/6] Proveram integritet fajlova..." -ForegroundColor Yellow

$requiredFiles = @(
    "FruitSysWeb.dll",
    "appsettings.json",
    "web.config"
)

$missingFiles = @()
foreach ($file in $requiredFiles) {
    $filePath = Join-Path $InstallPath $file
    if (-not (Test-Path $filePath)) {
        $missingFiles += $file
    }
}

if ($missingFiles.Count -gt 0) {
    Write-Error "Sledeći fajlovi nedostaju: $($missingFiles -join ', ')"
    Write-Host ""
    Write-Host "ROLLBACK:" -ForegroundColor Red
    Write-Host "  Expand-Archive -Path '$backupFile' -DestinationPath '$InstallPath' -Force"
    Write-Host "  Restart-WebAppPool -Name '$appPoolName'"
    Exit 1
}

Write-Host "  Svi potrebni fajlovi prisutni!" -ForegroundColor Green

# 6. Start Application Pool
Write-Host "[6/6] Pokrećem aplikaciju..." -ForegroundColor Yellow

Start-WebAppPool -Name $appPoolName
Start-Sleep -Seconds 3

$poolState = Get-WebAppPoolState -Name $appPoolName
if ($poolState.Value -eq "Started") {
    Write-Host "  Application Pool pokrenut!" -ForegroundColor Green
} else {
    Write-Warning "Application Pool nije pokrenut. Status: $($poolState.Value)"
    Write-Host "Pokušavam ponovo..."
    Start-WebAppPool -Name $appPoolName -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 5
}

Write-Host ""
Write-Host "=== UPDATE ZAVRŠEN ===" -ForegroundColor Green
Write-Host ""
Write-Host "Nova verzija je deploy-ovana u: $InstallPath" -ForegroundColor Cyan
Write-Host "Backup stare verzije: $backupFile" -ForegroundColor Cyan
Write-Host ""
Write-Host "TESTIRANJE:" -ForegroundColor Yellow
Write-Host "  1. Otvorite: https://www.fruitsys.rs"
Write-Host "  2. Proverite da li sve radi kako treba"
Write-Host "  3. Proverite logove: $InstallPath\Logs"
Write-Host ""

# Automatski test
Write-Host "Testiram aplikaciju..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

try {
    $response = Invoke-WebRequest -Uri "http://localhost" -UseBasicParsing -TimeoutSec 10 -ErrorAction Stop
    Write-Host "  Status: $($response.StatusCode) - Aplikacija radi!" -ForegroundColor Green
} catch {
    Write-Warning "Test neuspešan: $_"
    Write-Host ""
    Write-Host "Proverite logove:" -ForegroundColor Yellow
    Write-Host "  Get-Content '$InstallPath\Logs\fruitsys-*.log' -Tail 50"
    Write-Host ""
    Write-Host "Ako aplikacija ne radi, vratite staru verziju:" -ForegroundColor Yellow
    Write-Host "  Stop-WebAppPool -Name '$appPoolName'"
    Write-Host "  Expand-Archive -Path '$backupFile' -DestinationPath '$InstallPath' -Force"
    Write-Host "  Start-WebAppPool -Name '$appPoolName'"
}

Write-Host ""
Write-Host "Za gledanje logova u real-time:" -ForegroundColor Cyan
Write-Host "  Get-Content '$InstallPath\Logs\fruitsys-*.log' -Wait -Tail 20"
Write-Host ""

# Čišćenje starih backup-a (čuvaj samo poslednjih 10)
Write-Host "Čistim stare backup-ove (čuvam poslednjih 10)..." -ForegroundColor Yellow
Get-ChildItem $BackupPath -Filter "FruitSys-BEFORE-UPDATE-*.zip" |
    Sort-Object LastWriteTime -Descending |
    Select-Object -Skip 10 |
    Remove-Item -Force

Write-Host "Gotovo!" -ForegroundColor Green
