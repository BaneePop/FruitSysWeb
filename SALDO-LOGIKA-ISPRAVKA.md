# Pregled Salda - SQL Logika

**Datum**: 06.12.2025
**Fajl**: `FinansijeService.cs` → `UcitajSaldoPoKomitentima()`

---

## 📝 Jednostavna SQL Logika

### ✅ Pravila:

**POTRAŽUJE** (šta oni duguju nama):
- **FK-** dokumenti (Fakture)
- **IS-** dokumenti (Isplate)

**DUGUJE** (šta mi dugujemo njima):
- **KL-** dokumenti (Otkupni list)
- **UP-** dokumenti (Uplate)

**STANJE**:
```
Stanje = Potražuje - Duguje

Ako Stanje > 0  →  Oni nam duguju (pozitivno)
Ako Stanje < 0  →  Mi njima dugujemo (negativno)
```

---

## 💻 SQL Kod

```sql
SELECT
    k.ID as KomitentID,
    k.Naziv as Komitent,

    -- POTRAŽUJE (šta oni duguju nama) = FK-, IS-
    COALESCE(SUM(CASE
        WHEN (fm.Dokument LIKE 'FK-%' OR fm.Dokument LIKE 'IS-%')
            AND fm.Datum >= @OdDatum
            AND fm.Datum <= @DoDatum
            AND fm.DokumentStatus NOT IN (2, 4)
        THEN fm.Potrazuje
        ELSE 0
    END), 0) as Potrazuje,

    -- DUGUJE (šta mi dugujemo njima) = KL-, UP-
    COALESCE(SUM(CASE
        WHEN (fm.Dokument LIKE 'KL-%' OR fm.Dokument LIKE 'UP-%')
            AND fm.Datum >= @OdDatum
            AND fm.Datum <= @DoDatum
            AND fm.DokumentStatus NOT IN (2, 4)
        THEN fm.Duguje
        ELSE 0
    END), 0) as Duguje

FROM Komitent k
LEFT JOIN vPrometFinansijev9 fm ON k.ID = fm.KomitentID
WHERE k.Aktivno = 1
GROUP BY k.ID, k.Naziv
HAVING ABS(Duguje - Potrazuje) > @MinimumStanje
ORDER BY ABS(Duguje - Potrazuje) DESC
```

---

## 📊 Primeri

### Primer 1: Kupac

**Transakcije:**
1. FK-001 (Faktura) - 200.000 RSD → Potražuje +200.000

**Rezultat:**
- **Potražuje**: 200.000 RSD
- **Duguje**: 0 RSD
- **Stanje**: 200.000 - 0 = **200.000 RSD** (on duguje nama) ✅

---

### Primer 2: Dobavljač

**Transakcije:**
1. KL-001 (Otkupni list) - 100.000 RSD → Duguje +100.000

**Rezultat:**
- **Potražuje**: 0 RSD
- **Duguje**: 100.000 RSD
- **Stanje**: 0 - 100.000 = **-100.000 RSD** (mi dugujemo njemu) ✅

---

### Primer 3: Mešoviti komitent (i kupac i dobavljač)

**Transakcije:**
1. FK-001 (Faktura) - 50.000 RSD → Potražuje +50.000
2. KL-001 (Otkupni list) - 120.000 RSD → Duguje +120.000

**Rezultat:**
- **Potražuje**: 50.000 RSD
- **Duguje**: 120.000 RSD
- **Stanje**: 50.000 - 120.000 = **-70.000 RSD** (mi dugujemo njemu 70.000) ✅

---

## 📌 Napomene

- Svi komitenti mogu biti **istovremeno i kupci i dobavljači**
- FK- i IS- dokumenti **povećavaju POTRAŽUJE**
- KL- i UP- dokumenti **povećavaju DUGUJE**
- Stanje = Potražuje - Duguje
- Prikazuju se samo komitenti gde je `ABS(Stanje) > 1.000 RSD`

---

## ✅ Build Status

```bash
dotnet build
```
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

---

**Autor**: Claude Code Agent
**Datum**: 06.12.2025
**Status**: ✅ Completed - JEDNOSTAVNA VERZIJA
