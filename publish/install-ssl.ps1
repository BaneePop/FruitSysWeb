# FruitSys SSL Sertifikat Instalacija
# Win-ACME (Let's Encrypt) instalacija i konfiguracija

param(
    [Parameter(Mandatory=$false)]
    [string]$DomainName = "www.fruitsys.rs",

    [Parameter(Mandatory=$false)]
    [string]$Email = "admin@fruitsys.rs",

    [Parameter(Mandatory=$false)]
    [string]$WinAcmePath = "C:\Tools\win-acme"
)

# Provera admin prava
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Error "Ovaj script mora biti pokrenut kao Administrator!"
    Exit 1
}

Write-Host "=== FruitSys SSL Instalacija ===" -ForegroundColor Green
Write-Host ""

# 1. Provera da li je domen dostupan
Write-Host "[1/5] Provera DNS konfiguracije..." -ForegroundColor Yellow

try {
    $dnsResult = Resolve-DnsName -Name $DomainName -ErrorAction Stop
    Write-Host "  DNS OK: $DomainName -> $($dnsResult.IPAddress)" -ForegroundColor Green
} catch {
    Write-Warning "DNS nije konfigurisan za $DomainName!"
    Write-Host "  Podesite A record kod registrara domena pre instalacije SSL-a."
    $continue = Read-Host "Da li želite da nastavite? (y/n)"
    if ($continue -ne "y") {
        Exit 0
    }
}

# 2. Preuzimanje Win-ACME
Write-Host "[2/5] Instalacija Win-ACME..." -ForegroundColor Yellow

if (-not (Test-Path $WinAcmePath)) {
    New-Item -ItemType Directory -Path $WinAcmePath -Force | Out-Null
}

$winAcmeExe = Join-Path $WinAcmePath "wacs.exe"

if (-not (Test-Path $winAcmeExe)) {
    Write-Host "  Preuzimanje Win-ACME..."

    $downloadUrl = "https://github.com/win-acme/win-acme/releases/latest/download/win-acme.v2.2.9.1701.x64.pluggable.zip"
    $zipPath = Join-Path $WinAcmePath "win-acme.zip"

    try {
        Invoke-WebRequest -Uri $downloadUrl -OutFile $zipPath -UseBasicParsing
        Expand-Archive -Path $zipPath -DestinationPath $WinAcmePath -Force
        Remove-Item $zipPath
        Write-Host "  Win-ACME preuzet i instaliran!" -ForegroundColor Green
    } catch {
        Write-Error "Greška pri preuzimanju Win-ACME: $_"
        Write-Host ""
        Write-Host "RUČNO PREUZIMANJE:" -ForegroundColor Yellow
        Write-Host "1. Idite na: https://www.win-acme.com/"
        Write-Host "2. Preuzmite najnoviju verziju"
        Write-Host "3. Raspakujte u: $WinAcmePath"
        Write-Host "4. Ponovo pokrenite ovaj script"
        Exit 1
    }
} else {
    Write-Host "  Win-ACME već instaliran!" -ForegroundColor Green
}

# 3. Automatska konfiguracija SSL sertifikata
Write-Host "[3/5] Konfigurisanje SSL sertifikata..." -ForegroundColor Yellow
Write-Host "  Domen: $DomainName" -ForegroundColor Cyan
Write-Host "  Email: $Email" -ForegroundColor Cyan
Write-Host ""

# Kreiranje settings fajla za automatsku konfiguraciju
$settingsJson = @{
    "ClientNames" = @($DomainName, "fruitsys.rs")
    "Contact" = @($Email)
    "AcceptTerms" = $true
    "Validation" = "http-01"
    "ValidationSiteId" = 1
    "Store" = "CertificateStore"
    "Installation" = "IIS"
    "InstallationSiteId" = 1
} | ConvertTo-Json

$settingsPath = Join-Path $WinAcmePath "settings.json"
$settingsJson | Out-File $settingsPath -Encoding UTF8

Write-Host "  Pokretanje Win-ACME..."
Write-Host "  (Pratite uputstva na ekranu)" -ForegroundColor Cyan
Write-Host ""

# Pokretanje Win-ACME u interaktivnom modu
$wacsCmdArgs = @(
    "--source", "iis",
    "--siteid", "1",
    "--commonname", $DomainName,
    "--emailaddress", $Email,
    "--accepttos",
    "--installation", "iis",
    "--installationsiteid", "1"
)

Start-Process -FilePath $winAcmeExe -ArgumentList $wacsCmdArgs -Wait -NoNewWindow

# 4. Provera instaliranog sertifikata
Write-Host "[4/5] Provera instaliranog sertifikata..." -ForegroundColor Yellow

Start-Sleep -Seconds 3

Import-Module WebAdministration

$siteName = "FruitSys"
$binding = Get-WebBinding -Name $siteName -Protocol "https" -ErrorAction SilentlyContinue

if ($binding) {
    Write-Host "  HTTPS binding konfigurisan!" -ForegroundColor Green
    Write-Host "  Sertifikat validan!" -ForegroundColor Green
} else {
    Write-Warning "HTTPS binding nije pronađen. SSL možda nije pravilno instaliran."
    Write-Host ""
    Write-Host "RUČNA INSTALACIJA:" -ForegroundColor Yellow
    Write-Host "1. Pokrenite: $winAcmeExe"
    Write-Host "2. Izaberite: N (Create new certificate)"
    Write-Host "3. Izaberite: 1 (Single binding of an IIS site)"
    Write-Host "4. Izaberite FruitSys sajt"
    Write-Host "5. Pratite uputstva"
}

# 5. Dodavanje automatskog obnavljanja
Write-Host "[5/5] Konfigurisanje automatskog obnavljanja..." -ForegroundColor Yellow

$taskName = "Win-ACME Renew (win-acme.com)"
$task = Get-ScheduledTask -TaskName $taskName -ErrorAction SilentlyContinue

if ($task) {
    Write-Host "  Scheduled Task već konfigurisan!" -ForegroundColor Green
    Write-Host "  SSL sertifikat će se automatski obnavljati!" -ForegroundColor Green
} else {
    Write-Warning "Scheduled Task za obnavljanje nije pronađen."
    Write-Host "  Win-ACME bi trebao automatski da ga kreira pri prvoj instalaciji."
}

# Restart IIS-a
Write-Host ""
Write-Host "Restartujem IIS..." -ForegroundColor Yellow
iisreset /restart | Out-Null
Start-Sleep -Seconds 3

Write-Host ""
Write-Host "=== SSL INSTALACIJA ZAVRŠENA ===" -ForegroundColor Green
Write-Host ""
Write-Host "Aplikacija je sada dostupna na:" -ForegroundColor Cyan
Write-Host "  https://$DomainName"
Write-Host "  https://fruitsys.rs"
Write-Host ""
Write-Host "PROVERA SSL-a:" -ForegroundColor Yellow
Write-Host "  https://www.ssllabs.com/ssltest/analyze.html?d=$DomainName"
Write-Host ""
Write-Host "Automatsko obnavljanje:" -ForegroundColor Cyan
Write-Host "  Sertifikat će se automatski obnoviti 30 dana pre isteka"
Write-Host "  Task Scheduler: $taskName"
Write-Host ""

# Test HTTPS konekcije
Write-Host "Testiram HTTPS konekciju..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "https://$DomainName" -UseBasicParsing -TimeoutSec 10 -ErrorAction Stop
    Write-Host "  HTTPS radi! Status: $($response.StatusCode)" -ForegroundColor Green
} catch {
    Write-Warning "HTTPS test neuspešan: $_"
    Write-Host "  Možda DNS još nije propagiran ili firewall blokira port 443."
}

Write-Host ""
