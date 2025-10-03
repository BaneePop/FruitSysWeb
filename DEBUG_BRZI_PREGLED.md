# 🐛 DEBUG: Brzi Pregled - Dobavljači i Kupci ne učitavaju podatke

## 📅 Datum: 02.10.2025

---

## 🔍 PROBLEM

Nakon implementacije Roba na zalihama modula, tabele za dobavljače i kupce ne prikazuju uplate/isplate.

---

## ✅ REŠENJE - DETALJNO LOGOVANJE

### 1. **AŽURIRAN BrziPregledService.cs**

**Dodato:**
- ✅ Console logging za svaki korak
- ✅ Logovanje SQL parametara
- ✅ Logovanje rezultata za svaku stavku
- ✅ Stack trace za greške

**Fajl:** `/Services/Implementations/IzvestajService/BrziPregledService.cs`

---

### 2. **AŽURIRAN FinansijskiPregled.razor**

**Dodato:**
- ✅ Logovanje učitavanja konfiguracije
- ✅ Logovanje parsiranja JSON-a
- ✅ Logovanje svakog poziva servisa
- ✅ Prikazivanje alert poruka ako nema podataka

**Fajl:** `/Components/Pages/FinansijskiPregled.razor`

---

## 🧪 KAKO TESTIRATI

### **KORAK 1: Build i pokreni aplikaciju**

```bash
cd /Users/Bane/FruitSysWeb
dotnet build
dotnet run
```

---

### **KORAK 2: Konfiguriši Brzi Pregled**

1. Otvori: `http://localhost:5000/brzi-pregled-konfiguracija`
2. **Izaberi 2-3 dobavljača**
3. **Izaberi 2-3 kupca**
4. **Klikni "Sačuvaj Konfiguraciju"**
5. **Klikni "Prikaži Pregled"**

---

### **KORAK 3: Proveri Console Log**

Otvori **Browser Developer Tools** (F12) → **Console** tab

#### **Očekivane poruke:**

```
🏁 FinansijskiPregled - OnInitializedAsync started
📅 Period: 01.06.2025 - 02.10.2025
🔍 Učitavam konfiguraciju iz localStorage...
✅ Učitana konfiguracija iz localStorage: {"IzabraniDobavljaci":[...
📦 Konfiguracija parsirana:
  - Dobavljači: 3
  - Kupci: 3
  - Artikli po vrstama: 0

🚚 Učitavam 3 dobavljača: 17, 41, 49
🔍 UcitajBrziPregledDobavljaca:
  - Komitenti: 17, 41, 49
  - Period: 01.06.2025 - 02.10.2025
✅ Učitano 3 dobavljača:
  - Dobavljač A: VrednostRobe=125,678.90, Isplata=100,000.00, Stanje=-25,678.90
  - Dobavljač B: VrednostRobe=87,543.21, Isplata=87,543.21, Stanje=0.00
  - Dobavljač C: VrednostRobe=45,123.45, Isplata=40,000.00, Stanje=-5,123.45

🛒 Učitavam 3 kupaca: 82, 88, 95
🔍 UcitajBrziPregledKupaca:
  - Komitenti: 82, 88, 95
  - Period: 01.06.2025 - 02.10.2025
✅ Učitano 3 kupaca:
  - Kupac X: VrednostRobe=234,567.89, Isplata=200,000.00, Stanje=-34,567.89
  - Kupac Y: VrednostRobe=123,456.78, Isplata=123,456.78, Stanje=0.00
  - Kupac Z: VrednostRobe=98,765.43, Isplata=90,000.00, Stanje=-8,765.43
```

---

### **KORAK 4: Proveri Finansijski Pregled stranicu**

#### **AKO VIDITE ALERT PORUKE:**

- ⚠️ "Nema učitanih dobavljača. Proverite konfiguraciju."
- ⚠️ "Nema učitanih kupaca. Proverite konfiguraciju."

**To znači da servisi ne vraćaju podatke!**

---

## 🔍 DEBUGGING - ŠTA PROVERITI

### **Problem 1: localStorage je prazan**

**Console log:**
```
⚠️ localStorage je prazan - nema konfiguracije
```

**Rešenje:**
1. Idi na `/brzi-pregled-konfiguracija`
2. Izaberi dobavljače i kupce
3. Klikni "Sačuvaj Konfiguraciju"
4. Proveri alert poruku: "Konfiguracija uspešno sačuvana!"

---

### **Problem 2: Konfiguracija se ne deserijalizuje**

**Console log:**
```
❌ Deserijalizacija konfiguracije nije uspela
```

**Rešenje:**
1. Otvori Browser Developer Tools (F12)
2. Idi na **Application** → **Local Storage**
3. Pronađi `brzi_pregled_config`
4. Proveri da li JSON izgleda validno
5. Ako nije, obriši i sačuvaj ponovo

---

### **Problem 3: SQL upiti ne vraćaju podatke**

**Console log:**
```
✅ Učitano 0 dobavljača:
```

**Mogući uzroci:**
- Pogrešni ID-evi komitenata
- Nema podataka u `vPrometFinansijev9` za zadati period
- SQL greška

**Kako proveriti:**

1. **Proveri da li postoje dokumenti u periodu:**
   ```sql
   SELECT COUNT(*) 
   FROM vPrometFinansijev9 
   WHERE Datum >= '2025-06-01' 
     AND Datum <= '2025-10-02'
     AND Dokument LIKE 'KL-%';
   ```

2. **Proveri da li izabrani komitenti imaju transakcije:**
   ```sql
   SELECT 
       k.ID, k.Naziv,
       COUNT(fm.ID) as BrojDokumenata,
       SUM(fm.Potrazuje) as UkupnaVrednost
   FROM Komitent k
   LEFT JOIN vPrometFinansijev9 fm ON k.ID = fm.KomitentID
   WHERE k.ID IN (17, 41, 49)  -- Tvoji izabrani ID-evi
     AND fm.Dokument LIKE 'KL-%'
     AND fm.Datum >= '2025-06-01'
   GROUP BY k.ID, k.Naziv;
   ```

---

### **Problem 4: VrednostRobe i Isplata su 0**

**Console log:**
```
  - Dobavljač A: VrednostRobe=0.00, Isplata=0.00, Stanje=0.00
```

**Mogući uzroci:**
- Dokumenti nemaju `KL-` ili `IS-` prefiks
- Datum je van opsega
- `Potrazuje` ili `Uplata` kolone su NULL

**SQL provera:**
```sql
-- Proveri dokumenta types
SELECT DISTINCT LEFT(Dokument, 3) as Prefix, COUNT(*) as Count
FROM vPrometFinansijev9
GROUP BY LEFT(Dokument, 3);

-- Proveri podatke za jednog komitenta
SELECT 
    Dokument, Datum, Potrazuje, Uplata, ArtikalID, Kolicina
FROM vPrometFinansijev9
WHERE KomitentID = 17
  AND Datum >= '2025-06-01'
ORDER BY Datum DESC
LIMIT 10;
```

---

## 🎯 OČEKIVANI REZULTAT

### **Kada sve radi pravilno:**

1. **Console log pokazuje:**
   - ✅ Konfiguracija učitana
   - ✅ Pozivi servisa uspešni
   - ✅ Podaci vraćeni za svaku stavku
   - ✅ VrednostRobe, Isplata i Stanje imaju vrednosti

2. **Na stranici se prikazuju:**
   - ✅ Tabela "Pregled Nabavke" sa podacima
   - ✅ Tabela "Pregled Prodaje" sa podacima
   - ✅ SALDO redovi sa ukupnim sumama

3. **Format podataka:**
   ```
   Dobavljač A    125.678,90    100.000,00    -25.678,90
   ```

---

## ⚠️ NAJČEŠĆI UZROCI PROBLEMA

### 1. **Period nema podataka**
- Period: 01.06.2025 - danas
- Ako u bazi nema dokumenata iz juna 2025, tabele će biti prazne

### 2. **Pogrešni ID-evi komitenata**
- Proveri da li su ID-evi ispravni u localStorage

### 3. **SQL view `vPrometFinansijev9` ne postoji**
- Proveri da li view postoji:
  ```sql
  SHOW TABLES LIKE 'vPrometFinansijev9';
  ```

### 4. **Prefiks dokumenata**
- Proveri da li dokumenti imaju tačne prefixe:
  - Nabavka: `KL-` (Komisionalni List)
  - Isplate: `IS-` (Isplatnica)
  - Prodaja: `FK-` (Faktura Kupac)
  - Uplate: `UP-` (Uplatnica)

---

## 📝 SLEDEĆI KORACI

1. **Build i pokreni aplikaciju**
2. **Konfiguriši brzi pregled**
3. **Otvori Console log i prati poruke**
4. **Ako nema podataka:**
   - Proveri SQL upite direktno u bazi
   - Proveri period (možda je suviše daleko u budućnosti)
   - Proveri da li su ID-evi validni

---

## 🎉 KADA SVE RADI

**Console log:**
```
✅ Učitano 3 dobavljača
✅ Učitano 3 kupaca
✅ Učitano 2 vrsta voća
```

**Stranica:**
- 3 tabele sa podacima
- SALDO redovi sa ukupnim sumama
- Formatiranje sa zarezom (1.234,56)

---

**🚀 Sve je spremno za detaljno testiranje!**
