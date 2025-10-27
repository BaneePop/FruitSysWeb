# FruitSys Deployment Script za Windows 11 Server
# Ovaj script postavlja IIS, deploy-uje aplikaciju i konfigurisu SSL

param(
    [Parameter(Mandatory=$false)]
    [string]$InstallPath = "C:\inetpub\wwwroot\FruitSys",

    [Parameter(Mandatory=$false)]
    [string]$DomainName = "www.fruitsys.rs",

    [Parameter(Mandatory=$false)]
    [switch]$SkipIISInstall = $false
)

# Provera admin prava
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Error "Ovaj script mora biti pokrenut kao Administrator!"
    Exit 1
}

Write-Host "=== FruitSys Deployment Script ===" -ForegroundColor Green
Write-Host "Install Path: $InstallPath"
Write-Host "Domain: $DomainName"
Write-Host ""

# 1. Instalacija IIS i potrebnih komponenti
if (-not $SkipIISInstall) {
    Write-Host "[1/7] Instalacija IIS i potrebnih komponenti..." -ForegroundColor Yellow

    $features = @(
        "IIS-WebServerRole",
        "IIS-WebServer",
        "IIS-CommonHttpFeatures",
        "IIS-HttpErrors",
        "IIS-HttpRedirect",
        "IIS-ApplicationDevelopment",
        "IIS-NetFxExtensibility45",
        "IIS-HealthAndDiagnostics",
        "IIS-HttpLogging",
        "IIS-LoggingLibraries",
        "IIS-RequestMonitor",
        "IIS-HttpTracing",
        "IIS-Security",
        "IIS-RequestFiltering",
        "IIS-Performance",
        "IIS-WebServerManagementTools",
        "IIS-IIS6ManagementCompatibility",
        "IIS-Metabase",
        "IIS-ManagementConsole",
        "IIS-BasicAuthentication",
        "IIS-WindowsAuthentication",
        "IIS-StaticContent",
        "IIS-DefaultDocument",
        "IIS-DirectoryBrowsing",
        "IIS-WebSockets",
        "NetFx4Extended-ASPNET45",
        "IIS-ISAPIExtensions",
        "IIS-ISAPIFilter",
        "IIS-ASPNET45"
    )

    foreach ($feature in $features) {
        Write-Host "  Instalacija: $feature"
        Enable-WindowsOptionalFeature -Online -FeatureName $feature -All -NoRestart -ErrorAction SilentlyContinue
    }

    Write-Host "  IIS instaliran!" -ForegroundColor Green
} else {
    Write-Host "[1/7] Preskačem IIS instalaciju (već instaliran)" -ForegroundColor Cyan
}

# 2. Instalacija .NET 8 Hosting Bundle
Write-Host "[2/7] Provera .NET 8 Hosting Bundle..." -ForegroundColor Yellow

$dotnetVersion = dotnet --version 2>$null
if ($dotnetVersion) {
    Write-Host "  .NET verzija: $dotnetVersion" -ForegroundColor Green
} else {
    Write-Error ".NET 8 nije instaliran! Preuzmite sa: https://dotnet.microsoft.com/download/dotnet/8.0"
    Write-Host "Instalirajte 'ASP.NET Core Runtime 8.0.x - Windows Hosting Bundle' i ponovo pokrenite script."
    Exit 1
}

# 3. Kreiranje direktorijuma
Write-Host "[3/7] Kreiranje direktorijuma..." -ForegroundColor Yellow

if (Test-Path $InstallPath) {
    Write-Host "  Direktorijum već postoji: $InstallPath" -ForegroundColor Cyan
    $backup = "$InstallPath.backup.$(Get-Date -Format 'yyyyMMdd_HHmmss')"
    Write-Host "  Pravim backup u: $backup"
    Copy-Item -Path $InstallPath -Destination $backup -Recurse -Force
} else {
    New-Item -ItemType Directory -Path $InstallPath -Force | Out-Null
    Write-Host "  Direktorijum kreiran: $InstallPath" -ForegroundColor Green
}

# Kreiranje Logs foldera
$logsPath = Join-Path $InstallPath "Logs"
if (-not (Test-Path $logsPath)) {
    New-Item -ItemType Directory -Path $logsPath -Force | Out-Null
}

# 4. Kopiranje fajlova
Write-Host "[4/7] Kopiranje aplikacionih fajlova..." -ForegroundColor Yellow

$sourcePath = $PSScriptRoot
if (Test-Path "$sourcePath\FruitSysWeb.dll") {
    Copy-Item -Path "$sourcePath\*" -Destination $InstallPath -Recurse -Force
    Write-Host "  Fajlovi kopirani!" -ForegroundColor Green
} else {
    Write-Error "FruitSysWeb.dll nije pronađen u: $sourcePath"
    Write-Host "Proverite da li ste pokrenuli script iz 'publish' foldera."
    Exit 1
}

# 5. Konfigurisanje IIS
Write-Host "[5/7] Konfigurisanje IIS..." -ForegroundColor Yellow

Import-Module WebAdministration

# Kreiranje Application Pool-a
$appPoolName = "FruitSysAppPool"
if (Test-Path "IIS:\AppPools\$appPoolName") {
    Write-Host "  Application Pool već postoji: $appPoolName" -ForegroundColor Cyan
    Stop-WebAppPool -Name $appPoolName -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2
} else {
    New-WebAppPool -Name $appPoolName
    Write-Host "  Application Pool kreiran: $appPoolName" -ForegroundColor Green
}

# Konfigurisanje Application Pool-a
Set-ItemProperty -Path "IIS:\AppPools\$appPoolName" -Name "managedRuntimeVersion" -Value ""
Set-ItemProperty -Path "IIS:\AppPools\$appPoolName" -Name "startMode" -Value "AlwaysRunning"
Set-ItemProperty -Path "IIS:\AppPools\$appPoolName" -Name "processModel.idleTimeout" -Value "00:00:00"
Set-ItemProperty -Path "IIS:\AppPools\$appPoolName" -Name "recycling.periodicRestart.time" -Value "00:00:00"

# Kreiranje Web Site-a
$siteName = "FruitSys"
if (Test-Path "IIS:\Sites\$siteName") {
    Write-Host "  Web Site već postoji: $siteName" -ForegroundColor Cyan
    Remove-WebSite -Name $siteName
}

New-WebSite -Name $siteName `
    -Port 80 `
    -HostHeader $DomainName `
    -PhysicalPath $InstallPath `
    -ApplicationPool $appPoolName `
    -Force

# Dodavanje dodatnih binding-a
$bindings = @("fruitsys.rs", "localhost")
foreach ($binding in $bindings) {
    if ($binding -ne $DomainName) {
        New-WebBinding -Name $siteName -Protocol "http" -Port 80 -HostHeader $binding -ErrorAction SilentlyContinue
    }
}

Write-Host "  IIS konfigurisan!" -ForegroundColor Green

# 6. Postavljanje permisija
Write-Host "[6/7] Postavljanje permisija..." -ForegroundColor Yellow

$acl = Get-Acl $InstallPath
$permission = "IIS_IUSRS", "Read,ReadAndExecute", "ContainerInherit,ObjectInherit", "None", "Allow"
$accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule $permission
$acl.SetAccessRule($accessRule)

# Dodaj write permisije za Logs folder
$logsAcl = Get-Acl $logsPath
$logsPermission = "IIS_IUSRS", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow"
$logsAccessRule = New-Object System.Security.AccessControl.FileSystemAccessRule $logsPermission
$logsAcl.SetAccessRule($logsAccessRule)
Set-Acl $logsPath $logsAcl

Set-Acl $InstallPath $acl
Write-Host "  Permisije postavljene!" -ForegroundColor Green

# 7. Konfigurisanje Firewall-a
Write-Host "[7/7] Konfigurisanje Windows Firewall-a..." -ForegroundColor Yellow

# HTTP
if (-not (Get-NetFirewallRule -DisplayName "FruitSys HTTP" -ErrorAction SilentlyContinue)) {
    New-NetFirewallRule -DisplayName "FruitSys HTTP" `
        -Direction Inbound `
        -Protocol TCP `
        -LocalPort 80 `
        -Action Allow `
        -Profile Any
}

# HTTPS
if (-not (Get-NetFirewallRule -DisplayName "FruitSys HTTPS" -ErrorAction SilentlyContinue)) {
    New-NetFirewallRule -DisplayName "FruitSys HTTPS" `
        -Direction Inbound `
        -Protocol TCP `
        -LocalPort 443 `
        -Action Allow `
        -Profile Any
}

Write-Host "  Firewall konfigurisan!" -ForegroundColor Green

# Pokretanje sajta
Start-WebAppPool -Name $appPoolName
Start-WebSite -Name $siteName
Start-Sleep -Seconds 2

Write-Host ""
Write-Host "=== DEPLOYMENT ZAVRŠEN ===" -ForegroundColor Green
Write-Host ""
Write-Host "Aplikacija je dostupna na:" -ForegroundColor Cyan
Write-Host "  http://localhost"
Write-Host "  http://$DomainName (nakon DNS propagacije)"
Write-Host ""
Write-Host "SLEDEĆI KORACI:" -ForegroundColor Yellow
Write-Host "1. Podesite DNS A record da pokazuje na IP adresu ovog servera"
Write-Host "2. Sačekajte DNS propagaciju (1-24h)"
Write-Host "3. Instalirajte SSL sertifikat (pokrenite: .\install-ssl.ps1)"
Write-Host "4. Testiranje: Otvorite browser i idite na http://localhost"
Write-Host ""
Write-Host "Logovi aplikacije:" -ForegroundColor Cyan
Write-Host "  $logsPath"
Write-Host ""
Write-Host "Za restart aplikacije:" -ForegroundColor Cyan
Write-Host "  Restart-WebAppPool -Name $appPoolName"
Write-Host ""
