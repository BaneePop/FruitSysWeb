# 📋 Changelog

Sve značajne izmene u projektu će biti dokumentovane u ovom fajlu.

Format baziran na [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

---

## [Unreleased]

### Planirano
- User authentication sistem
- Audit trail funkcionalnost
- Real-time notifications
- Mobile aplikacija

---

## [1.0.0] - 2025-10-02

### ✨ Dodato

#### Brzi Pregled Sistem
- **BrziPregledKonfiguracija.razor** - Stranica za konfiguraciju
  - Odabir do 6 dobavljača
  - Odabir do 6 kupaca
  - Čuvanje u localStorage
- **FinansijskiPregled.razor** - Pojednostavljena stranica
  - Samo Brzi Pregled sa 2 tabele
  - Alert poruka kada nema partnera
- **BrziPregledTabela.razor** - Reusable komponenta
  - Prikaz perioda u headeru (01.06.2025 - danas)
  - Automatsko računanje stanja
  - Badge boje za pozitivna/negativna stanja
- **BrziPregledService** - Backend servis
  - SQL upiti za dobavljače (KL-, IS-)
  - SQL upiti za kupce (FK-, UP-)
  - Filtriranje po datumskom periodu

### 🔧 Izmenjeno

#### Brzi Pregled
- Formula računanja stanja: `VrednostRobe - Isplata` → `Isplata - VrednostRobe`
- Layout: Tabele sa `col-md-6` → `col-12` (puna širina)
- Header kolone: `table-light` → `table-dark` (bolja čitljivost)

#### FinansijskiPregled.razor
- Uklonjeni tabovi: Roba na Zalihama, Obračun Otkupa, Obračun Sva Roba
- Uklonjene Statistics cards
- Pojednostavljen @code sa ~300 linija manje koda

### 🐛 Ispravke

#### Dashboard Analytics
- **Top 5 Kupaca** - SQL upit popravljen da koristi negativne količine (prodaja)
- **Top 5 Dobavljača** - SQL upit popravljen da koristi pozitivne količine (nabavka)
- **Struktura Lagera** - Sve sekcije limitirane na 5 stavki
- **Brza Statistika** - Ispravljeno računanje ukupnih količina

#### TypeMapping
- Dodato `IJSRuntime` injection u FinansijskiPregled.razor
- Ispravljeno formatiranje perioda sa TypeMapping.FormatDate()

---

## [0.9.0] - 2025-09-18

### ✨ Dodato

#### TypeMapping Sistem
- **TypeMappingService.cs** - Centralizovani servis za formatiranje
  - Srpska kultura formatiranja (sr-Latn-RS)
  - `FormatDecimal()` - 10.456,90
  - `FormatDate()` - 19.09.2025
  - `FormatDateTime()` - 19.09.2025 14:30:15
  - `FormatCurrency()` - 1.245.455,88 RSD
  - `FormatWeight()` - 10.456,90 kg
  - Badge helper metode za sve tipove

#### Dashboard
- **DashboardCharts.razor** - Analytics sekcija
  - Top 5 Kupaca chart
  - Top 5 Dobavljača chart
  - Struktura Sirovina (5 stavki)
  - Struktura Gotovih Proizvoda (5 stavki)
  - Struktura Kutija/Džakova (5 stavki)
  - Struktura Kesa (5 stavki)
  - Brza Statistika (od 01.01.2025)

### 🔧 Izmenjeno

#### Constants
- **MagacinTypes.cs**
  - Ispravljen naziv: "PoluProizvodi" → "Polu Proizvod"
  - Ispravljen naziv: "Usl. Mlečni" → "Usl.Mleko"
  - Dodat: Kalo i Rastur (ID 7)
  - Ispravne boje:
    - Sveza Roba: `bg-danger` (crvena)
    - Gotov Proizvod: `bg-success` (zelena)
    - Repromaterijal: `bg-warning` (braon-ish)

- **DocumentStatus.cs**
  - Promenjen status: "Odustano" → "Storno"
  - Sve reference ažurirane kroz ceo projekat

### 🐛 Ispravke

#### ProizvodnjaService
- **UcitajProizvodnjuPoKomitentima()** - Ispravljen SQL
  - Koristi `a.MagacinID = 6` umesto nepostojećeg `vpp.RpArtikalTip`
  - Dodati LEFT JOIN sa Artikal tabelom
  - Filtrirani neaktivni artikli
  
- **UcitajProizvodnjuPoTipovima()** - Ispravljen SQL
  - Koristi `a.MagacinID` umesto nepostojećeg polja
  - Isključen MagacinID = 7 (Kalo i Rastur)
  - Dodato HAVING za pozitivne količine

#### MagacinLagerService
- **UcitajStrukturuKesa()** - Ispravljen SQL
  - Koristi `ml.Artikal` umesto `ml.BaseArtikal`
  - Dodat LIMIT 5 za čitljivost

---

## [0.8.0] - 2025-09-15

### ✨ Dodato

#### Ulaz-Izlaz Modul
- **UlazIzlaz.razor** - Nova stranica sa 5 tabova
  - Fakture (FK-)
  - Otkupni Listovi (KL-)
  - Prijemnice
  - Otpremnice
  - Statistike
- **UlazIzlazService** - Realne SQL implementacije
  - `UcitajSveFakture()` - SQL sa Faktura tabele
  - `UcitajSveOtkupneListove()` - SQL sa OtkupniList tabele
  - Statistike i top liste po komitentima

#### Prerada Modul
- **Prerada.razor** - Nova stranica sa 4 taba
  - Radni Nalozi
  - Smene i Dani
  - Evidencije
  - Statistike
- **PreradaService** - SQL implementacije
  - `UcitajRadniNalogIzvestaj()`
  - `UcitajSmeneDaniIzvestaj()`
  - `UcitajEvidencijeIzvestaj()`
  - `UcitajStatistike()`

### 🔧 Izmenjeno

#### FilterRequest
- Dodat `BrojUgovora` property za otkupne listove
- Refaktorisanje - jedan FilterRequest za sve tabove

---

## [0.7.0] - 2025-09-10

### ✨ Dodato

#### Proizvodnja Modul
- **Proizvodnja.razor** - Kompletna stranica
  - Statistics cards (4 metrike)
  - Analytics sekcije (Top 10 artikala, komitenata, tipovi)
  - Export u Excel/PDF
  - Napredni filteri (datum, status, radni nalog)
- **ProizvodnjaService** - Sve metode implementirane
  - `UcitajIzvestajProizvodnje()`
  - `UcitajProizvodnjuPoArtiklima()`
  - `UcitajProizvodnjuPoKomitentima()`
  - `UcitajNajproduktivnijeNaloge()`

#### Lager Modul
- **Lager.razor** - Stranica sa 2 taba
  - Magacin Lager (vwMagacinLager)
  - Lager Proizvodnje (vwRadniNalogLager)
- **MagacinLagerService** - Sve metode
  - `UcitajLagerStanje()`
  - `UcitajLagerProizvodnje()`
  - `UcitajArtikleIspodMinimuma()`

### 🔧 Izmenjeno

#### Navigation
- Dodati dropdown meniji za:
  - Proizvodnja
  - Lager
  - Finansije
  - Ulaz-Izlaz
  - Prerada

---

## [0.6.0] - 2025-09-01

### ✨ Dodato

#### Dashboard
- **Home.razor** - Početna stranica
  - 4 Quick action cards
  - Navigation ka svim modulima
- **DashboardService** - Statistics servisi
  - Ukupno saldo
  - Vrednost lagera
  - Ukupna proizvodnja
  - Aktivni nalozi

#### Export Funkcionalnosti
- **ExportService** - Excel i PDF export
  - ClosedXML za Excel
  - QuestPDF za PDF
  - Automatski timestamp u nazivima fajlova

### 🔧 Izmenjeno

#### Layout
- Novi NavMenu sa Bootstrap 5 stilom
- Responsive design za mobilne uređaje
- Dark mode priprema

---

## [0.5.0] - 2025-08-15

### ✨ Dodato

#### Core Infrastructure
- **DatabaseService** - Dapper wrapper
- **Dependency Injection** setup
- Connection string konfiguracija
- Logging setup

#### Models Layer
- Svi osnovni modeli kreirani:
  - ProizvodnjaModel
  - FinansijeModel
  - MagacinLagerModel
  - Komitent
  - Artikal

---

## [0.1.0] - 2025-08-01

### ✨ Dodato

#### Initial Setup
- Blazor Server projekat kreiran
- .NET 8.0 target framework
- MySQL connection setup
- Bootstrap 5 integracija
- Git repository inicijalizovan

---

## Tipovi Izmena

- **✨ Dodato** - Nova funkcionalnost
- **🔧 Izmenjeno** - Izmene postojeće funkcionalnosti
- **🐛 Ispravke** - Bug fix-evi
- **❌ Uklonjeno** - Uklonjena funkcionalnost
- **🔒 Sigurnost** - Sigurnosne ispravke
- **📝 Dokumentacija** - Izmene u dokumentaciji
- **⚡ Performance** - Performance poboljšanja
- **♻️ Refactoring** - Code refactoring

---

**Održava:** FruitSysWeb Team  
**Format:** [Keep a Changelog](https://keepachangelog.com/en/1.0.0/)  
**Versioning:** [Semantic Versioning](https://semver.org/)
