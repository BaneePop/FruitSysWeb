# Globalna Konfiguracija Brzog Pregleda - Uputstvo za Instalaciju

## Šta je izmenjeno?

Ranije se konfiguracija Brzog Pregleda čuvala u **localStorage** browsera, što je značilo da:
- Svaki korisnik mora da podesi svoju konfiguraciju
- Konfiguracija se gubi ako se promeni browser ili uređaj
- Nema deljenja postavki između korisnika

**Nova verzija** čuva konfiguraciju u **JSON fajlu** u `publish/config` folderu, što omogućava:
- ✅ **Jedna globalna konfiguracija za sve korisnike**
- ✅ Sve promene se automatski čuvaju u fajl
- ✅ Svi korisnici vide istu konfiguraciju
- ✅ Konfiguracija se ne gubi pri promeni browsera
- ✅ **Nema promene u bazi podataka** - samo se čita

---

## Koraci za primenu izmena

### 1. Build projekta

```bash
dotnet build -c Release
```

### 2. Verifikuj izmene u kodu

Izmenjeni fajlovi:

#### **Services/Implementations/IzvestajService/BrziPregledService.cs**
- `SacuvajKonfiguraciju()` - Sada čuva u JSON fajl (`publish/config/brzi_pregled_config.json`)
- `UcitajKonfiguraciju()` - Sada učitava iz JSON fajla
- Automatski kreira `config` folder ako ne postoji

#### **Components/Pages/Brzi-pregled-konfiguracija.razor**
- Uklonjena zavisnost od `IJSRuntime` (localStorage)
- Sve promene se automatski čuvaju kroz servis
- Obaveštenje korisnika da je konfiguracija globalna

#### **Components/Pages/FinansijskiPregled.razor**
- Uklonjena zavisnost od `IJSRuntime` (localStorage)
- Učitavanje konfiguracije iz servisa

### 3. Publish aplikacije (opciono)

Ako želiš da publish-uješ novo:

```bash
dotnet publish -c Release -o ./publish
```

**NAPOMENA**: Config folder (`publish/config`) će se automatski kreirati pri prvom čuvanju konfiguracije.

### 4. Restartuj aplikaciju

Nakon restarta, novi sistem će biti aktivan.

---

## Kako testirati?

1. **Otvori Brzi Pregled konfiguraciju** (`/brzi-pregled-konfiguracija`)
   - Videćeš obaveštenje: **"Globalna konfiguracija! Sve promene se automatski čuvaju u bazu i važe za SVE korisnike."**

2. **Izaberi dobavljače, kupce i artikle**
   - Svaka promena se automatski čuva u bazu

3. **Otvori drugi browser ili drugi računar**
   - Uloguj se kao drugi korisnik
   - Otvori Brzi Pregled konfiguraciju
   - **Trebalo bi da vidiš ISTU konfiguraciju!**

4. **Promeni nešto na drugom browseru**
   - Vrati se na prvi browser
   - Refreshuj stranicu
   - **Trebalo bi da vidiš nove promene!**

---

## Migracija postojećih podataka (opciono)

Ako korisnici imaju postojeće konfiguracije u localStorage-u:

1. Neka jedan korisnik otvori `/brzi-pregled-konfiguracija`
2. Njegova localStorage konfiguracija NEĆE biti automatski učitana
3. Potrebno je **ručno podesiti** dobavljače, kupce i artikle
4. Od tog momenta, svi korisnici koriste istu konfiguraciju iz JSON fajla

---

## Rollback (vraćanje na staru verziju)

Ako želiš da se vratiš na localStorage sistem:

1. Checkout prethodnu verziju koda
2. Obriši config folder:
   ```bash
   rm -rf publish/config
   ```
3. Build i restartuj aplikaciju

---

## Struktura JSON fajla

Fajl se nalazi na: `publish/config/brzi_pregled_config.json`

Primer sadržaja:
```json
{
  "IzabraniDobavljaci": [1, 2, 3],
  "IzabraniKupci": [10, 20],
  "ArtikliPoVrstama": {
    "Malina": [100, 101, 102],
    "Kupina": [200, 201]
  }
}
```

---

## Pitanja i Problemi

Ako nešto ne radi kako treba:

1. **Proveri da li postoji config folder**:
   ```bash
   ls -la publish/config/
   ```
   Trebalo bi da vidiš `brzi_pregled_config.json`

2. **Proveri sadržaj JSON fajla**:
   ```bash
   cat publish/config/brzi_pregled_config.json
   ```

3. **Proveri logove aplikacije**:
   - Traži poruke: "✅ Konfiguracija sačuvana u..." i "✅ Konfiguracija učitana iz..."

4. **Proveri browser konzolu** (F12):
   - Traži poruke: "✅ Globalna konfiguracija učitana iz baze"

5. **Proveri permissions na folderu**:
   ```bash
   chmod 755 publish/config
   chmod 644 publish/config/brzi_pregled_config.json
   ```

---

## Verzija

- **Datum**: 2025-10-23
- **Verzija**: 1.0.0
- **Autor**: Claude Code
