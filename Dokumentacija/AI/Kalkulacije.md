# Kalkulacija — Razvojni dnevnik

Stranica `/kalkulacija` u modulu **Poslovanje**. Zamenjuje ručni Excel fajl `Obračun 2025.xlsm`.  
Cilj: tačan finansijski pregled sezone u realnom vremenu — stanje robe, obračun otkupa, obračun poslovanja sa profitom.

---

## Urađeno — Faza 1: Stanje Robe

### Novi fajlovi

| Fajl | Uloga |
|------|-------|
| `Models/KalkulacijaModel.cs` | Modeli za sve tabele: `KalkulacijaStanjeRobeRed`, `KalkulacijaStanjeRobeResult`, `KalkulacijaPeriod` |
| `Services/Core/KalkulacijaKonfiguracijaService.cs` | Singleton JSON servis za čuvanje perioda i svih korisničkih unosa (Faze 2–4). Pattern identičan `KesaSelekcijaService`. Fajl: `Data/kalkulacija-konfiguracija.json` |
| `Services/Interfaces/IKalkulacijaService.cs` | Interfejs |
| `Services/Implementations/IzvestajService/KalkulacijaService.cs` | SQL logika za Tabelu 1 |
| `Components/Pages/Kalkulacija.razor` | Blazor stranica — period picker + Tabela 1 |

### Izmenjeni fajlovi

- `Extensions/ServiceCollectionExtensions.cs` — registracija `KalkulacijaKonfiguracijaService` (Singleton) i `IKalkulacijaService` (Scoped)
- `Shared/NavMenu.razor` — dodat link **Kalkulacija** u Poslovanje dropdown (posle Kartice Komitenata, sa separatorom)

---

### Tabela 1 — Stanje Robe (implementirana)

**Kolone:** Vrsta Voća | Kupljeno (kg) | Prenos Zaliha (kg) | Prodato (kg) | Kalo Smrzavanja (kg) | Kalo Prerade (kg) | Na Zalihama (kg)

**Redovi:** Malina, Kupina, Višnja, Šljiva, Kajsija, Jagoda, Borovnica  
Grupisanje po `PrvaKlasifikacijaID` (6, 10, 11, 15, 28, 34, 39). Prikazuju se samo vrste koje imaju podatke.

**SQL logika po koloni:**

| Kolona | Izvor | Napomena |
|--------|-------|----------|
| Kupljeno | `vPrometFinansijev9` WHERE `KL-%`, `MagacinID IN (2,3)` | Sveza roba i sirovine |
| Prenos Zaliha | `MagacinLager − NetoPromet(odSezone→danas)` iz `vPrometRobav6` | Identičan algoritam kao LagerHome chart |
| Prodato | `vPrometFinansijev9` WHERE `FK-%` | |
| Kalo Smrzavanja | `ProcesSmrzavanjeSema JOIN Artikal` WHERE `MagacinID=2` | `Kupljeno × ProcenatKaloSmrzavanja / 100` |
| Kalo Prerade | Fiksan % po vrsti × Kupljeno | Malina 2%, Kupina 3%, Višnja 5%, Šljiva 7%, Kajsija 8%, Jagoda/Borovnica 2% |
| Na Zalihama | Formula u C# modelu | `Kupljeno + PrenosaZaliha − Prodato − KaloSmrzavanja − KaloPrerade` |

**Ključne napomene za bazu:**
- `Artikal` tabela: `PrvaKlasifikacijaID` (direktna kolona)
- `vPrometFinansijev9` view: `ArtikalPrvaKlasifikacijaID` (alias u view-u)
- `vPrometRobav6`: `ArtikalPrvaKlasifikacijaID`, dokumenti PR=ulaz / OT=izlaz, `DokumentStatus=3`
- `MagacinLager` JOIN `ArtikalInstanca` JOIN `Artikal` — ne postoji direktan JOIN na `Artikal`

**UX ponašanje:**
- Period se čuva u JSON — pri sledećem otvaranju stranice izveštaj se automatski učitava
- Dugme "Tekuća sezona" postavlja period jun tekuće/prošle godine → danas
- Na Zalihama < 0 prikazuje se crvenom bojom

---

## Preostale faze

### Faza 2 — Tabela 2: Obračun Otkupa

**Kolone:** Dobavljač | Vrednost Roba | Vrednost sa Maržom | Isplata | Stanje Roba | Stanje Ukupno

**Redovi:** max 10 dobavljača, biraju se i snimaju u JSON (`KalkulacijaKonfiguracija.IzabraniDobavljaci`)

**Logika kolona:**
- `Vrednost sa Maržom` = SUM Potrazuje na `KL-` dokumentima po dobavljaču za period
- Marža = korisnik unosi cenu marže po kg za svakog dobavljača (JSON: `MarzaPoKgDobavljac`)
- `Vrednost Roba` = Vrednost sa Maržom − (marža/kg × kupljene kg)
- `Isplata` = SUM iznosa na `IS-` dokumentima po dobavljaču za period
- `Stanje Roba` = Isplata − Vrednost Roba
- `Stanje Ukupno` = Isplata − Vrednost Roba (za sad isto kao Stanje Roba)

**Šta treba uraditi:**
1. Dodati metodu `UcitajObracunOtkupa(period, dobavljaci)` u `IKalkulacijaService` i `KalkulacijaService`
   - SQL: `vPrometFinansijev9` WHERE `KL-%`, GROUP BY KomitentID
   - SQL: `vPrometFinansijev9` WHERE `IS-%`, GROUP BY KomitentID
2. Dodati modele u `KalkulacijaModel.cs`: `KalkulacijaObracunOtkupaRed`, `KalkulacijaObracunOtkupaResult`
3. Napraviti stranicu/modal za izbor dobavljača (max 10) i unos marže po kg — može biti na posebnoj stranici `/kalkulacija-konfiguracija` ili inline
4. Dodati Tabelu 2 u `Kalkulacija.razor`

---

### Faza 3 — Tabela 3 redovi 1–4: Nabavna vrednost + Prerada + Prodaja

**Redovi:**
1. **Nabavna Vrednost Sezona** — SUM Potrazuje WHERE `KL-%` za period (DIN i EUR)
2. **Nabavna Vrednost Lager** — lager na dan pre sezone × prosečna nabavna cena iz prethodnih 12 meseci
3. **Prerada** — korisnik unosi cenu prerade po kg po sirovini (JSON: `CenaPrerade[ArtikalID]`) × kg sirovine na zalihama
4. **Prodaja** — SUM Duguje WHERE `FK-%` za period

**Šta treba uraditi:**
1. Dodati metodu `UcitajObracunPoslovanja(period)` u servis
2. Za "Nabavna Vrednost Lager": proveriti koji view/tabela daje prosečnu nabavnu cenu (možda `vPrometFinansijev9.Potrazuje / Kolicina`)
3. Napraviti stranicu `/kalkulacija-konfiguracija` sa formom za unos cena prerade po sirovini
4. Dodati modele u `KalkulacijaModel.cs`: `KalkulacijaObracunPosloStavka`, `KalkulacijaObracunPosloResult`
5. Dodati Tabelu 3 (parcijalno) u `Kalkulacija.razor`

---

### Faza 4 — Tabela 3 red 5: Vrednost Robe na Zalihama

Najsloženiji deo. Zahteva dva nova unosa u konfiguraciji:

**Unos A — Formula iskorišćenja** (JSON: `FormulaIskoriscenja`)  
Za svaku sirovinu: naziv gotovog proizvoda + % iskorišćenja.  
Primer: 1 kg D/Z Malina Original → 85% Rolend, 10% Griz, 4.5% Blok, 0.5% Otpad

**Unos B — Prodajne cene** (JSON: `ProdajneCene[ArtikalID]`)  
Može da se povuče iz otvorenih ugovora (`FK-` tip, `DokumentStatus != zatvoreno`) ili ručni unos.

**Logika:**
1. Uzeti lager sirovina iz `MagacinLager`
2. Primeniti formulu iskorišćenja → predviđene kg gotovih proizvoda
3. Dodati već postojeći lager gotovih proizvoda iz `MagacinLager`
4. Pomnožiti sa prodajnom cenom → Vrednost Robe na Zalihama

**Šta treba uraditi:**
1. Proširiti `/kalkulacija-konfiguracija` sa sekcijom za formule iskorišćenja
2. Proširiti sa sekcijom za prodajne cene (sa opcijom "povuci iz ugovora")
3. Dodati metodu `UcitajVrednostZaliha(period)` u servis
4. Dodati red 5 u Tabelu 3

---

### Faza 5 — Tabela 3 redovi 6–8: Profit, Profitna Stopa, Profit/kg

Čisto računanje iz vrednosti iz Faza 3 i 4 — nema novih SQL upita.

| Red | Formula |
|-----|---------|
| Profit | (Prodaja + Vrednost Zaliha) − (Nabavna Vrednost Sezona + Nabavna Vrednost Lager + Prerada) |
| Profitna Stopa | Profit / (Nabavna Vrednost Sezona + Nabavna Vrednost Lager + Prerada) × 100% |
| Profit po kg | Profit / (kg Kupljeno u sezoni + kg Prenos Zaliha) |

---

### Faza 6 — Stranica `/kalkulacija-konfiguracija`

Posebna stranica (link na dnu Kalkulacija stranice) za sve korisničke unose koji se snimaju u JSON.

**Sekcije:**
1. **Izbor dobavljača** (Faza 2) — max 10, sa unosom marže po kg po vrsti robe
2. **Cene prerade** (Faza 3) — tabela: sirovina | cena/kg | (dugme Sačuvaj)
3. **Formula iskorišćenja** (Faza 4) — sirovina → lista (gotov proizvod, %)
4. **Prodajne cene** (Faza 4) — dugme "Povuci iz ugovora" + ručni unos

Sve globalno za sve korisnike (kao `BrziPregledKonfiguracija`). Upozorenje na vrhu: "Promene važe za SVE korisnike."

---

## Arhitektura — pregled fajlova

```
Components/Pages/
  Kalkulacija.razor                    ✅ postoji (Faza 1)
  KalkulacijaKonfiguracija.razor       ⬜ treba kreirati (Faza 6)

Models/
  KalkulacijaModel.cs                  ✅ postoji — proširivati po fazama

Services/Interfaces/
  IKalkulacijaService.cs               ✅ postoji — dodavati metode po fazama

Services/Implementations/IzvestajService/
  KalkulacijaService.cs                ✅ postoji — dodavati metode po fazama

Services/Core/
  KalkulacijaKonfiguracijaService.cs   ✅ postoji — JSON model već ima mesta za Faze 2–4

Data/
  kalkulacija-konfiguracija.json       auto-kreira se pri prvom pokretanju
```

---

## Otvorena pitanja pre Faze 3

1. **Prosečna nabavna cena za Nabavna Vrednost Lager** — koji view/tabela? Predlog: `SUM(Potrazuje) / SUM(ABS(Kolicina))` iz `vPrometFinansijev9` WHERE `KL-%` za prethodnih 12 meseci.
2. **Valuta EUR** — odakle kurs? Ručni unos ili iz neke tabele u bazi?
3. **IS- dokumenti za Isplatu** — proveriti da li `vPrometFinansijev9` pokriva IS- dokumente ili treba drugačiji view/tabela.
