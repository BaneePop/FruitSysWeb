@echo off
chcp 65001 >nul
:: ===================================================================
:: FruitSysWeb - Skripta za Ažuriranje Aplikacije
:: Verzija: 1.1.0
:: Datum: 24.10.2025
:: ===================================================================

COLOR 0A
echo.
echo ╔═══════════════════════════════════════════════════════════════╗
echo ║         FruitSysWeb - Ažuriranje Aplikacije v1.1.0           ║
echo ║                      ODETTA DOO                               ║
echo ╚═══════════════════════════════════════════════════════════════╝
echo.

:: Provera administratorskih prava
NET SESSION >nul 2>&1
IF %ERRORLEVEL% NEQ 0 (
    echo [GREŠKA] Ova skripta zahteva administratorske privilegije!
    echo Desni klik na skriptu i odaberite "Run as administrator"
    pause
    exit /b 1
)

:: Postavljanje promenljivih
set APP_DIR=C:\FruitSysWeb
set BACKUP_DIR=C:\FruitSysWeb_BACKUP_%date:~-4,4%%date:~-7,2%%date:~-10,2%
set UPDATE_DIR=%~dp0
set SERVICE_NAME=FruitSysWeb

echo [INFO] Direktorijum aplikacije: %APP_DIR%
echo [INFO] Direktorijum update-a: %UPDATE_DIR%
echo [INFO] Backup direktorijum: %BACKUP_DIR%
echo.

:: ===================================================================
:: KORAK 1: BACKUP
:: ===================================================================
echo ╔═══════════════════════════════════════════════════════════════╗
echo ║  KORAK 1/5: Kreiranje Backup-a                               ║
echo ╚═══════════════════════════════════════════════════════════════╝
echo.

IF NOT EXIST "%APP_DIR%" (
    echo [GREŠKA] Aplikacija nije pronađena u: %APP_DIR%
    echo Proverite da li je putanja tačna.
    pause
    exit /b 1
)

echo [INFO] Kreiranje backup-a trenutne verzije...
xcopy "%APP_DIR%" "%BACKUP_DIR%" /E /I /H /Y >nul 2>&1
IF %ERRORLEVEL% EQU 0 (
    echo [USPEH] Backup kreiran: %BACKUP_DIR%
) ELSE (
    echo [GREŠKA] Neuspelo kreiranje backup-a!
    pause
    exit /b 1
)
echo.

:: ===================================================================
:: KORAK 2: ZAUSTAVLJANJE APLIKACIJE
:: ===================================================================
echo ╔═══════════════════════════════════════════════════════════════╗
echo ║  KORAK 2/5: Zaustavljanje Aplikacije                         ║
echo ╚═══════════════════════════════════════════════════════════════╝
echo.

:: Provera da li je servis pokrenut
sc query %SERVICE_NAME% >nul 2>&1
IF %ERRORLEVEL% EQU 0 (
    echo [INFO] Zaustavljanje servisa: %SERVICE_NAME%
    net stop %SERVICE_NAME% >nul 2>&1
    timeout /t 3 /nobreak >nul
    echo [USPEH] Servis zaustavljen
) ELSE (
    echo [INFO] Servis nije pronađen - preskačem zaustavljanje
)

:: Zaustavljanje procesa ako je pokrenut
tasklist /FI "IMAGENAME eq FruitSysWeb.exe" 2>NUL | find /I /N "FruitSysWeb.exe" >NUL
IF %ERRORLEVEL% EQU 0 (
    echo [INFO] Zaustavljanje procesa FruitSysWeb.exe
    taskkill /F /IM FruitSysWeb.exe >nul 2>&1
    timeout /t 2 /nobreak >nul
    echo [USPEH] Proces zaustavljen
)
echo.

:: ===================================================================
:: KORAK 3: KOPIRANJE NOVIH FAJLOVA
:: ===================================================================
echo ╔═══════════════════════════════════════════════════════════════╗
echo ║  KORAK 3/5: Kopiranje Novih Fajlova                          ║
echo ╚═══════════════════════════════════════════════════════════════╝
echo.

echo [INFO] Kopiranje ažuriranih fajlova...

:: Kopiranje DLL fajlova
IF EXIST "%UPDATE_DIR%\bin\Release\net8.0\*.dll" (
    echo [INFO] Kopiranje DLL fajlova...
    xcopy "%UPDATE_DIR%\bin\Release\net8.0\*.dll" "%APP_DIR%\" /Y >nul 2>&1
)

:: Kopiranje EXE fajla
IF EXIST "%UPDATE_DIR%\bin\Release\net8.0\FruitSysWeb.exe" (
    echo [INFO] Kopiranje EXE fajla...
    copy "%UPDATE_DIR%\bin\Release\net8.0\FruitSysWeb.exe" "%APP_DIR%\FruitSysWeb.exe" /Y >nul 2>&1
)

:: Kopiranje PDB fajla
IF EXIST "%UPDATE_DIR%\bin\Release\net8.0\FruitSysWeb.pdb" (
    echo [INFO] Kopiranje PDB fajla...
    copy "%UPDATE_DIR%\bin\Release\net8.0\FruitSysWeb.pdb" "%APP_DIR%\FruitSysWeb.pdb" /Y >nul 2>&1
)

:: Kopiranje wwwroot direktorijuma
IF EXIST "%UPDATE_DIR%\wwwroot" (
    echo [INFO] Kopiranje wwwroot fajlova...
    xcopy "%UPDATE_DIR%\wwwroot" "%APP_DIR%\wwwroot" /E /I /H /Y >nul 2>&1
)

:: Kopiranje Logo.png - VAŽNO!
IF EXIST "%UPDATE_DIR%\Logo.png" (
    echo [INFO] Kopiranje Logo.png...
    copy "%UPDATE_DIR%\Logo.png" "%APP_DIR%\Logo.png" /Y >nul 2>&1
    IF %ERRORLEVEL% EQU 0 (
        echo [USPEH] Logo.png kopiran
    ) ELSE (
        echo [UPOZORENJE] Logo.png nije kopiran - PDF export možda neće raditi!
    )
) ELSE (
    echo [UPOZORENJE] Logo.png nije pronađen u update paketu!
)

echo [USPEH] Fajlovi kopirani
echo.

:: ===================================================================
:: KORAK 4: PROVERA LOGO FAJLA
:: ===================================================================
echo ╔═══════════════════════════════════════════════════════════════╗
echo ║  KORAK 4/5: Provera Logo Fajla                               ║
echo ╚═══════════════════════════════════════════════════════════════╝
echo.

IF EXIST "%APP_DIR%\Logo.png" (
    echo [USPEH] Logo.png pronađen u: %APP_DIR%\Logo.png
) ELSE (
    echo [GREŠKA] Logo.png nije pronađen!
    echo PDF export neće raditi kako treba.
    echo.
    echo Da li želite da nastavite bez loga? (D/N)
    set /p CONTINUE_WITHOUT_LOGO=
    IF /I NOT "%CONTINUE_WITHOUT_LOGO%"=="D" (
        echo [INFO] Ažuriranje prekinuto od strane korisnika
        echo [INFO] Vraćam backup...
        xcopy "%BACKUP_DIR%\*" "%APP_DIR%" /E /I /H /Y >nul 2>&1
        echo [USPEH] Backup vraćen
        pause
        exit /b 1
    )
)
echo.

:: ===================================================================
:: KORAK 5: POKRETANJE APLIKACIJE
:: ===================================================================
echo ╔═══════════════════════════════════════════════════════════════╗
echo ║  KORAK 5/5: Pokretanje Aplikacije                            ║
echo ╚═══════════════════════════════════════════════════════════════╝
echo.

:: Provera da li postoji servis
sc query %SERVICE_NAME% >nul 2>&1
IF %ERRORLEVEL% EQU 0 (
    echo [INFO] Pokretanje servisa: %SERVICE_NAME%
    net start %SERVICE_NAME% >nul 2>&1
    IF %ERRORLEVEL% EQU 0 (
        echo [USPEH] Servis pokrenut
    ) ELSE (
        echo [GREŠKA] Neuspelo pokretanje servisa!
        echo Pokušajte ručno: net start %SERVICE_NAME%
    )
) ELSE (
    echo [INFO] Servis nije konfigurisan
    echo [INFO] Pokretanje aplikacije...
    cd /d "%APP_DIR%"
    IF EXIST "START-FRUITSYSWEB.bat" (
        start "" "START-FRUITSYSWEB.bat"
        echo [USPEH] Aplikacija pokrenuta
    ) ELSE (
        echo [INFO] Pokretanje direktno...
        start "" "%APP_DIR%\FruitSysWeb.exe"
        echo [USPEH] Aplikacija pokrenuta
    )
)
echo.

:: ===================================================================
:: ZAVRŠETAK
:: ===================================================================
echo ╔═══════════════════════════════════════════════════════════════╗
echo ║  AŽURIRANJE ZAVRŠENO USPEŠNO!                                ║
echo ╚═══════════════════════════════════════════════════════════════╝
echo.
echo [INFO] Verzija: 1.1.0
echo [INFO] Datum: 24.10.2025
echo.
echo [VAŽNO] Proverite sledeće:
echo   1. Logo se prikazuje u PDF export-u
echo   2. Format brojeva je 1.000.987,56 (srpski)
echo   3. Grafici prikazuju nazive artikala pri hover-u
echo   4. Ograničeni korisnici vide samo svoje opcije
echo.
echo [INFO] Backup lokacija: %BACKUP_DIR%
echo [INFO] Ako ima problema, koristite backup za vraćanje.
echo.
echo Pritisnite bilo koji taster za zatvaranje...
pause >nul

exit /b 0
