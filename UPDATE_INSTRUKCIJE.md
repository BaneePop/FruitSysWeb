# 🔄 INSTRUKCIJE ZA AŽURIRANJE FruitSysWeb APLIKACIJE

**Verzija**: 1.1.0
**Datum**: 24.10.2025
**Tip**: Feature Update - PDF Export & RBAC Improvements

---

## ⚠️ PRE NEGO ŠTO POČNETE

### 1. **NAPRAVITE BACKUP!**
```bash
# Na serveru, kopirajte trenutnu instalaciju
xcopy "C:\FruitSysWeb" "C:\FruitSysWeb_BACKUP_%date:~-4,4%%date:~-7,2%%date:~-10,2%" /E /I /H /Y

# Backup baze podataka
mysqldump -u root -p fruitsysdb_v2 > fruitsysdb_v2_backup_%date:~-4,4%%date:~-7,2%%date:~-10,2%.sql
```

### 2. **Zaustavite Aplikaciju**
```bash
# Ako je pokrenuta kao servis
net stop FruitSysWeb

# Ako je pokrenuta iz CMD-a
# Zatvorite CMD prozor ili pritisnite Ctrl+C
```

---

## 📦 METOD 1: Brzo Ažuriranje (Preporučeno)

### Korak 1: Preuzmite Update Paket
- Preuzmite `FruitSysWeb-Update-v1.1.0.zip`
- Ekstraujte u privremeni folder (npr. `C:\Temp\FruitSysWeb-Update`)

### Korak 2: Kopirajte Nove Fajlove
```bash
# Kopirajte sve fajlove OSIM appsettings.json i FruitSysWeb.db
xcopy "C:\Temp\FruitSysWeb-Update\*.*" "C:\FruitSysWeb\" /E /H /Y /EXCLUDE:exclude.txt
```

**Kreirati `exclude.txt` sa sledećim sadržajem:**
```
appsettings.json
appsettings.Production.json
FruitSysWeb.db
Logs\
```

### Korak 3: Kopirajte Logo
```bash
# Važno: Logo.png mora biti u root direktorijumu
copy "C:\Temp\FruitSysWeb-Update\Logo.png" "C:\FruitSysWeb\Logo.png"
```

### Korak 4: Pokrenite Aplikaciju
```bash
cd C:\FruitSysWeb
START-FRUITSYSWEB.bat
```

---

## 🔧 METOD 2: Git Pull (Ako koristite Git)

### Ako imate Git repozitorijum na serveru:
```bash
cd C:\FruitSysWeb

# Backup trenutnih izmena
git stash

# Pull najnovije izmene
git pull origin feature/centralize-types

# Restore build
dotnet build --configuration Release

# Kopiraj Logo.png ako nije tu
copy Logo.png bin\Release\net8.0\

# Pokrenuti aplikaciju
dotnet run --configuration Release
```

---

## 📋 PROVERA POSLE AŽURIRANJA

### 1. **Provera Logo-a**
- Otvorite aplikaciju u browseru
- Izvezite bilo koji izveštaj u PDF
- Proverite da li se prikazuje logo ODETTA DOO u header-u

### 2. **Provera PDF Export-a**
- Idite na Nabavka ili Prodaja
- Kliknite "PDF" dugme
- Proverite:
  - ✅ Logo u gornjem levom uglu
  - ✅ ODETTA DOO podaci (adresa, grad)
  - ✅ Svetlo plava boja header-a
  - ✅ Brojevi formatiran 1.000.987,56
  - ✅ Footer sa kompletnim podacima

### 3. **Provera Excel Export-a**
- Izvezite bilo koji izveštaj u Excel
- Proverite:
  - ✅ Samo vidljive kolone (bez ID-eva)
  - ✅ Brojevi sa tačkom za hiljade i zarezom za decimale

### 4. **Provera Grafika**
- Idite na "Izveštaj Prijem" ili "Izveštaj Prijem Istorija"
- Pređite mišem preko bara u grafikonu
- Proverite da li se prikazuju nazivi artikala

### 5. **Provera RBAC-a**
- Ulogujte se kao ograničeni korisnik (npr. `zoran`, `jelena`, `pedja`)
- Proverite da se otvara `/home-ograniceni` stranica
- Proverite da nema pristup Nabavka/Prodaja opcijama

---

## 🆕 NOVE FUNKCIONALNOSTI

### 1. **PDF Export sa Branding-om**
- Logo ODETTA DOO u header-u
- Kompletni kontakt podaci u footer-u
- Svetlo plavi moderni dizajn
- Profesionalan izgled sa zebra stripovima

### 2. **Srpski Format**
- Brojevi: `1.000.987,56` umesto `1,000,987.56`
- Datumi: `31.12.2025` umesto `12/31/2025`

### 3. **Export Samo Vidljivih Kolona**
- Excel i PDF izvoz bez ID-eva, šifri i skrivenih polja
- Samo kolone koje se vide u tabeli

### 4. **Tooltips u Grafikonima**
- Prikazuju se nazivi artikala pri hover-u
- Lakše praćenje podataka po dobavljačima

### 5. **Poboljšan RBAC**
- Username-based pristup (stabilniji od ID-based)
- Nova home stranica za ograničene korisnike
- Bolje skrivanje nedostupnih opcija

---

## 🚨 TROUBLESHOOTING

### Problem: Logo se ne prikazuje u PDF-u
**Rešenje:**
```bash
# Proverite da li Logo.png postoji
dir C:\FruitSysWeb\Logo.png

# Ako ne postoji, kopirajte ga
copy Logo.png C:\FruitSysWeb\
```

### Problem: "File not found" greška za Logo.png
**Rešenje:**
```bash
# Logo mora biti i u bin direktorijumu
copy C:\FruitSysWeb\Logo.png C:\FruitSysWeb\bin\Release\net8.0\Logo.png
```

### Problem: Format brojeva nije srpski
**Rešenje:**
- Ovo je automatski primenjeno kroz `CultureInfo("sr-Latn-RS")`
- Ako ne radi, proverite da li je aplikacija ponovo pokrenuta posle update-a

### Problem: Ograničeni korisnici vide sve opcije
**Rešenje:**
- Proverite da li su usernames tačno upisani: `zoran`, `jelena`, `pedja`, `radmila`, `masinska`, `BaneT`
- Username je case-insensitive
- Clear browser cache i ponovo se ulogujte

### Problem: PDF export ne radi
**Rešenje:**
```bash
# Proverite QuestPDF licencu u logovima
# Ako ima grešku, kontaktirajte podršku
```

---

## 📞 KONTAKT ZA PODRŠKU

Ako imate problema sa ažuriranjem:
1. Proverite log fajlove u `Logs/` direktorijumu
2. Napravite screenshot greške
3. Kontaktirajte IT podršku

---

## 🔙 ROLLBACK (Vraćanje na Staru Verziju)

Ako nešto pođe po zlu:

```bash
# Zaustavite aplikaciju
net stop FruitSysWeb

# Vratite backup
xcopy "C:\FruitSysWeb_BACKUP_*" "C:\FruitSysWeb" /E /I /H /Y

# Pokrenite staru verziju
cd C:\FruitSysWeb
START-FRUITSYSWEB.bat
```

---

## ✅ CHECKLIST

- [ ] Napravljen backup aplikacije
- [ ] Napravljen backup baze podataka
- [ ] Zaustavljena aplikacija
- [ ] Kopirani novi fajlovi
- [ ] Logo.png kopiran u root i bin direktorijum
- [ ] Aplikacija pokrenuta
- [ ] PDF export testiran (logo, boje, format)
- [ ] Excel export testiran (format brojeva)
- [ ] Grafici testirani (tooltips)
- [ ] RBAC testiran (ograničeni korisnici)
- [ ] Svi moduli testirani (Nabavka, Prodaja, Izveštaji)

---

**NAPOMENA**: Ažuriranje traje 5-10 minuta. Nemojte prekidati proces!

*Poslednja izmena: 24.10.2025*
