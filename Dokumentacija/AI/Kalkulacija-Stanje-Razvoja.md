# Kalkulacija — Stanje Razvoja

**Datum:** 4. jul 2026.  
**Projekat:** FruitSysWeb (`/kalkulacija`)  
**Referenca:** Excel `Obračun 2026.xlsm`, `Objašnjenje klakulacija.txt`, `Kalkulacije.md`

---

## Cilj

Zameniti ručni Excel obračun sezone web izveštajem u realnom vremenu: stanje robe, obračun otkupa, obračun poslovanja sa profitom. Podaci dolaze iz produkcijske MySQL baze (read-only, verzija 5.1.54).

---

## Arhitektura

| Fajl | Uloga |
|------|-------|
| `Components/Pages/Kalkulacija.razor` | Glavna stranica — tri tabele + period |
| `Components/Pages/KalkulacijaKonfiguracija.razor` | Globalna konfiguracija (JSON) |
| `Services/Implementations/IzvestajService/KalkulacijaService.cs` | SQL i poslovna logika |
| `Services/Core/PrenosZalihaHelper.cs` | Zajednička logika Prenos Zaliha (LagerHome algoritam) |
| `Services/Core/KalkulacijaKonfiguracijaService.cs` | Čuvanje u `Data/kalkulacija-konfiguracija.json` |
| `Models/KalkulacijaModel.cs` | Modeli za sve tabele |

**Magacini u upotrebi:** 2 = Sveža roba, 3 = Sirovine, 5 = Poluproizvodi, 6 = Gotova roba.

**Vrste voća (PrvaKlasifikacijaID):** Malina 6, Kupina 10, Višnja 11, Šljiva 15, Kajsija 28, Jagoda 34. Borovnica (39) nije u glavnoj tabeli stanja.

**Isključenje usluga:** artikli sa `%‑ALTIVA%` ili `%FRIKOS%` u nazivu.

---

## Šta je urađeno

### Tabela 1 — Stanje Robe ✅ (stabilizovano)

Kolone: Kupljeno | Prenos Zaliha | Prodato | Kalo Smrzavanja | Kalo Prerade | Na Zalihama.

| Kolona | Logika |
|--------|--------|
| **Kupljeno** | `KL‑` dokumenti, `Potrazuje > 0`, bez ALTIVA/FRIKOS, magacini **2, 3, 5, 6** |
| **Prenos Zaliha** | v. poseban odeljak ispod — **potvrđeno tačno** (npr. Malina = 128.694,29 kg) |
| **Prodato** | `FK‑` dokumenti |
| **Kalo Smrzavanja** | `ProcesSmrzavanjeSema` × Kupljeno (sirovine, mag. 2) |
| **Kalo Prerade** | Fiksni % po vrsti × Kupljeno (sirovine) |
| **Na Zalihama** | `Kupljeno + Prenos − Prodato − KaloSmrz − KaloPrerade` |

Period se čuva u JSON; dugme „Tekuća sezona“ postavlja jun → danas.

---

### Prenos Zaliha — detaljna logika ✅

Glavni problem u razvoju bio je pogrešan zbir po artiklu (npr. Rolend 210.675 > ukupno 128.694). Rešenje:

1. **Ukupno po vrsti voća** na dan pre sezone (`OdDatum − 1`) = isti algoritam kao **LagerHome**:
   - `TrenutnoStanje(MagacinLager) − NetoPromet(OdSezona → danas)` iz `vPrometRobav6`
2. **Po artiklu/redu:** proporcionalna raspodela ukupnog prema trenutnom udelu reda u vrsti
3. **Sekcija 5 konfiguracije** učitava **sve** artikle (svi magacini + lager proizvodnje), bez filtera na učitavanju
4. **Prenos Zaliha u Tabeli 1** = zbir redova **minus** označeni artikli (`IskluceniArtikliPrenosZaliha` u JSON)

Helper: `PrenosZalihaHelper.cs` — `SqlStanjePoVrsti`, `SqlPrometPoVrsti`, `RekonstruisiPoVrsti`, `RaspodeliRed`, `SaberiPrenosIzRedova`.

---

### Tabela 2 — Obračun Otkupa ✅ (implementirano)

Metoda `UcitajObracunOtkupa`: dobavljači iz JSON (max 10), `KL‑` vrednosti, `IS‑` isplate, početno stanje po dobavljaču. Prikaz na glavnoj stranici.

---

### Tabela 3 — Obračun Poslovanja 🟡 (implementirano, verifikacija u toku)

Metoda `UcitajObracunPoslovanja`:

| Red | Šta računa | Napomena |
|-----|------------|----------|
| Nabavna vrednost sezone | SUM(`Potrazuje − PorezIznos`) na `KL‑` | Bez PDV, bez usluga |
| Nabavna vrednost lagera | Prenete zalihe × cena po artiklu | Ručne cene iz JSON ili prosečna iz baze (12 mes. pre sezone); isključeni artikli iz Sekcije 5 |
| Prerada | `(Kupljeno_sirovine + Prenos_sirovine) × cena prerade` | Cene iz konfiguracije po vrsti |
| Prodaja | SUM `FK‑`, domaće × 10/11 | Bez PDV |
| Vrednost zaliha | `UcitajVrednostZaliha` | Formula iskorišćenja + prodajne cene iz JSON |
| Profit / stopa / kg | C# formula iz gornjih redova | Bez novih SQL upita |

**Poslednji bug (popravljen):** SQL u `sqlKupljenoKg` slao literal `{FilterMagacinSirovine}` umesto interpolacije — Obračun Poslovanja padao sa MySQL syntax error. Ispravljeno na `$@"..."` sa `{FilterMagacinSirovine}` i `{FilterUsluge}`.

---

### Stranica `/kalkulacija-konfiguracija` ✅

Globalni JSON (važi za sve korisnike):

| Sekcija | Sadržaj |
|---------|---------|
| 1 | Cene prerade po vrsti voća (din/kg) |
| 2 | Nabavne cene na lageru (ručni unos po ArtikalID) |
| 3 | Formula iskorišćenja (sirovina → gotov proizvod, %) |
| 4 | Prodajne cene gotovih proizvoda |
| 5 | **Prenos Zaliha** — lista artikala sa količinom na `OdDatum − 1`, checkbox za isključivanje |

---

## Gde smo stigli

| Oblast | Status |
|--------|--------|
| Prenos Zaliha (Tabela 1) | ✅ Tačno — potvrđeno u odnosu na LagerHome |
| Kupljeno (mag. 2,3,5,6, bez cene 0) | ✅ U kodu; uporediti sa Excelom |
| Obračun Otkupa (Tabela 2) | ✅ Radi; finansijska provera po potrebi |
| Obračun Poslovanja (Tabela 3) | 🟡 SQL bug popravljen; Bane treba da potvrdi učitavanje i brojeve |
| Vrednost zaliha (formula + cene) | 🟡 Implementirano; zavisi od popunjene konfiguracije |
| Brzina učitavanja | ⬜ Nije optimizovano |
| UI (tabovi, kompaktnost) | ⬜ Planirano posle stabilizacije brojeva |
| Sledljivost | ⬜ Sledeći prioritet posle Kalkulacije |

**Git:** grana `feature/centralize-types`, puno necommitovanih izmena. Commit nije rađen po eksplicitnom zahtevu.

---

## Šta sledi (redosled)

1. **Bane:** restart aplikacije, provera Obračuna Poslovanja posle SQL fix-a
2. Uporediti **Kupljeno** i **Obračun Poslovanja** sa Excelom (`Obračun 2026.xlsm`)
3. PDV korekcije i formule po `Objašnjenje klakulacija.txt` (npr. domaće × 0,9090 gde treba)
4. Popuniti konfiguraciju (cene prerade, formule, prodajne cene) za red „Vrednost zaliha“
5. Performanse, UI, zatim modul Sledljivost

---

## Tehnička ograničenja

- MySQL **5.1.54** — bez CTE i window funkcija
- Produkcijska baza — samo SELECT
- Konfiguracija u JSON — jedan fajl za sve korisnike servera

---

## Povezani dokumenti

- `Kalkulacije.md` — originalni razvojni plan po fazama
- `Objašnjenje klakulacija.txt` — poslovna pravila iz Excela
- `Kalkulacija.png`, `Lager.png` — reference UI / brojeva
