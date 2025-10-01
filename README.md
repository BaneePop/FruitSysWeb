# 🍎 FruitSysWeb - Sistem za Upravljanje Voćnom Proizvodnjom

![.NET](https://img.shields.io/badge/.NET-8.0-blue)
![Blazor](https://img.shields.io/badge/Blazor-Server-purple)
![MySQL](https://img.shields.io/badge/MySQL-8.0-orange)
![Bootstrap](https://img.shields.io/badge/Bootstrap-5.3-563d7c)

> Kompletan sistem za upravljanje voćnom proizvodnjom, prerađivačkim procesom, lagerom, finansijama i izveštavanjem.

---

## 📋 Sadržaj

- [O Projektu](#-o-projektu)
- [Tehnologije](#-tehnologije)
- [Glavne Funkcionalnosti](#-glavne-funkcionalnosti)
- [Status Implementacije](#-status-implementacije)
- [Struktura Projekta](#-struktura-projekta)
- [Instalacija i Pokretanje](#-instalacija-i-pokretanje)
- [Baza Podataka](#-baza-podataka)
- [Najnovije Izmene](#-najnovije-izmene)
- [Roadmap](#-roadmap)

---

## 🎯 O Projektu

FruitSysWeb je kompletan ERP sistem dizajniran za voćarsku industriju, sa fokusom na:
- Upravljanje proizvodnjom i preradom voća
- Praćenje lagera sirovina, ambalaže i gotovih proizvoda
- Finansijsko izveštavanje i analitiku
- Upravljanje komitentima (dobavljači, kupci)
- Brzi pregled poslovnih pokazatelja

### Ciljevi Sistema:
- ✅ Real-time praćenje proizvodnje i zaliha
- ✅ Automatizovano finansijsko izveštavanje
- ✅ Brz pregled ključnih poslovnih indikatora
- ✅ Preglednost svih poslovnih procesa
- ✅ Export u Excel/PDF za sve izveštaje

---

## 🛠️ Tehnologije

### Backend
- **.NET 8.0** - Application Framework
- **Blazor Server** - UI Framework
- **Dapper** - Micro ORM
- **MySQL 8.0+** - Baza podataka

### Frontend
- **Bootstrap 5.3** - CSS Framework
- **Bootstrap Icons** - Ikone
- **Blazor Components** - Reusable UI komponente

### Export & Reporting
- **ClosedXML** - Excel export
- **QuestPDF** - PDF generisanje

### Dependency Injection Services
- `DatabaseService` - Dapper wrapper
- `IProizvodnjaService` - Proizvodnja logika
- `IFinansijeService` - Finansije logika
- `IMagacinLagerService` - Lager management
- `IBrziPregledService` - Brzi pregled funkcionalnost
- `ITypeMappingService` - Formatiranje i mapiranje tipova
- `IExportService` - Export funkcionalnosti

---

## 📊 Glavne Funkcionalnosti

### 🏠 Dashboard
- **Real-time statistike:**
  - Ukupno saldo
  - Vrednost lagera
  - Ukupna proizvodnja
  - Broj aktivnih naloga
- **Top 5 liste:**
  - Kupci po kilaži
  - Dobavljači sirovina
  - Struktura lagera (sirovine, gotovi proizvodi, ambalaža)
- **Brza statistika** od 01.01.2025

### 💰 Finansije Modul
- Pregled prihoda i rashoda po komitentima
- Analiza top kupaca i dobavljača
- Filtriranje po datumu, komitentu, artiklima
- Export u Excel/PDF format
- Salda po mesecima

### 🏭 Proizvodnja Modul
- Detaljni pregled radnih naloga
- Analiza proizvodnje po artiklima i komitentima
- Podaci o količinama (roba, ambalaža, gotov proizvod)
- Top 10 najproduktivnijih naloga
- Prosečna dnevna proizvodnja
- **Analytics sekcije:**
  - Top 10 artikala po proizvodnji
  - Top 10 komitenata po proizvodnji
  - Proizvodnja po tipovima artikala

### 📦 Lager Modul
- **Magacin Lager:**
  - Pregled stanja sirovina i gotovih proizvoda
  - Indikatori stanja (dostupno/ograničeno/ispod minimuma)
  - Filtriranje po tipu artikla i pakovanju
- **Lager Proizvodnje:**
  - Praćenje radnih naloga u toku
  - Rezervacije materijala

### ⚡ Brzi Pregled
- **Konfiguracija:**
  - Odabir do 6 dobavljača
  - Odabir do 6 kupaca
  - Čuvanje konfiguracije u localStorage
- **Pregled Nabavke:**
  - Vrednost robe po dobavljaču
  - Isplate
  - Stanje (Isplata - Vrednost Robe)
  - Period: 01.06.2025 - danas
- **Pregled Prodaje:**
  - Vrednost robe po kupcu
  - Uplate
  - Stanje (Uplata - Vrednost Robe)
  - Period: 01.06.2025 - danas

### 📄 Ulaz-Izlaz Modul
- **Fakture (FK-):** Prodaja gotovih proizvoda
- **Otkupni Listovi (KL-):** Nabavka sirovina
- **Prijemnice:** Ulaz robe u magacin
- **Otpremnice:** Izlaz robe iz magacina
- Statistike i analitika po dokumentima

### 🏗️ Prerada Modul
- **Radni Nalozi:** Pregled evidencije rada
- **Smenski Izveštaji:** Analiza po smenama i danima
- **Evidencije:** Osnovni izveštaji
- **Statistike:** Produktivnost i efikasnost

---

## ✅ Status Implementacije

### 🟢 Kompletno Implementirano (100%)
- ✅ **Dashboard** - Kompletna analitika i statistike
- ✅ **Proizvodnja** - Svi izveštaji i analytics
- ✅ **Lager** - Oba view-a (Magacin i Proizvodnja)
- ✅ **Brzi Pregled** - Konfiguracija i prikaz
- ✅ **TypeMapping** - Srpsko formatiranje (datumi, brojevi, valuta)
- ✅ **Export** - Excel i PDF za sve module
- ✅ **UI/UX** - Responsive design, Bootstrap 5
- ✅ **Finansije** - Osnovni izveštaji

### 🟡 U Razvoju (50-90%)
- 🔄 **Ulaz-Izlaz** - Realne SQL implementacije (70%)
- 🔄 **Prerada** - SQL upiti optimizacija (60%)
- 🔄 **Navigation** - Dropdown meniji (80%)

### 🔴 Planirano
- 🔮 **User Authentication** - Login sistem
- 🔮 **Audit Trail** - Log svih promena
- 🔮 **Real-time Notifications** - WebSocket integracija
- 🔮 **Mobile App** - React Native verzija
- 🔮 **API** - RESTful API za integracije
- 🔮 **Backup/Restore** - Automatski backup sistem

---

## 📁 Struktura Projekta

```
FruitSysWeb/
├── Components/
│   ├── Pages/              # Blazor stranice
│   │   ├── Home.razor
│   │   ├── Proizvodnja.razor
│   │   ├── Lager.razor
│   │   ├── Finansije.razor
│   │   ├── FinansijskiPregled.razor
│   │   ├── Brzi-pregled-konfiguracija.razor
│   │   ├── UlazIzlaz.razor
│   │   └── Prerada.razor
│   ├── Shared/             # Reusable komponente
│   │   ├── BrziPregledTabela.razor
│   │   ├── RobaNaZalihama.razor
│   │   └── ...
│   ├── Layout/             # Layout komponente
│   └── Charts/             # Chart komponente
│       └── DashboardCharts.razor
├── Models/                 # Data modeli
│   ├── ProizvodnjaModel.cs
│   ├── FinansijeModel.cs
│   ├── MagacinLagerModel.cs
│   ├── BrziPregledKonfiguracija.cs
│   └── ...
├── Services/
│   ├── Core/               # Core servisi
│   │   ├── DatabaseService.cs
│   │   └── TypeMappingService.cs
│   ├── Interfaces/         # Service interfejsi
│   └── Implementations/    # Service implementacije
│       └── IzvestajService/
│           ├── ProizvodnjaService.cs
│           ├── FinansijeService.cs
│           ├── MagacinLagerService.cs
│           └── BrziPregledService.cs
├── Constants/              # Konstante
│   ├── MagacinTypes.cs
│   ├── DocumentStatus.cs
│   └── SystemConstants.cs
├── Utils/                  # Helper klase
├── Extensions/             # Extension metode
├── wwwroot/                # Static fajlovi
│   ├── css/
│   ├── js/
│   └── lib/
├── appsettings.json        # Konfiguracija
├── Program.cs              # Application entry point
└── README.md               # Ovaj fajl
```

---

## 🚀 Instalacija i Pokretanje

### Preduslovi
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- [MySQL Server 8.0+](https://dev.mysql.com/downloads/mysql/)
- IDE: Visual Studio 2022 / VS Code / Rider

### Koraci

1. **Clone repository:**
```bash
git clone https://github.com/YOUR_USERNAME/FruitSysWeb.git
cd FruitSysWeb
```

2. **Konfiguriši connection string:**

Otvori `appsettings.json` i podesi MySQL connection:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=fruitsysdb_v2;Uid=root;Pwd=your_password;CharSet=utf8mb4;SslMode=None;"
  }
}
```

3. **Restore dependencies:**
```bash
dotnet restore
```

4. **Build projekat:**
```bash
dotnet build
```

5. **Pokreni aplikaciju:**
```bash
dotnet run
```

6. **Otvori u browseru:**
```
https://localhost:5001
ili
http://localhost:5000
```

---

## 🗄️ Baza Podataka

### Glavne Tabele

| Tabela | Opis |
|--------|------|
| `Artikal` | Artikli/proizvodi (ID, Naziv, Tip, Aktivno) |
| `Komitent` | Poslovni partneri (kupci, dobavljači) |
| `RadniNalog` | Radni nalozi za proizvodnju |
| `MagacinLager` | Stanje lagera |
| `Dokument` | Finansijski dokumenti |
| `Faktura` | Prodajne fakture |
| `OtkupniList` | Otkupni listovi za nabavku |
| `EvidencijaRada` | Evidencija rada po smenama |
| `SmenskiIzvestaj` | Smenski izveštaji |

### Ključni View-ovi

| View | Opis |
|------|------|
| `vwMagacinLager` | Konsolidovano stanje lagera |
| `vPreradaPregled` | Detaljni pregled prerade |
| `vPrometFinansijev9` | Finansijski promet |
| `vwRadniNalogLager` | Lager proizvodnje |

### Tipovi Artikala (MagacinID)

| ID | Tip | Boja Badge |
|----|-----|------------|
| 2 | Sveza Roba | `bg-danger` (crvena) |
| 3 | Sirovine | `bg-secondary` (siva) |
| 4 | Ambalaza | `bg-info` (plava) |
| 5 | Polu Proizvod | `bg-secondary` (siva) |
| 6 | Gotov Proizvod | `bg-success` (zelena) |
| 7 | Kalo i Rastur | `bg-dark` (crna) |
| 8 | Usl.Mleko | `bg-primary` (plava) |
| 9 | Repromaterijal | `bg-warning` (braon) |

### Status Dokumenata

| Status | Naziv | Boja |
|--------|-------|------|
| 2 | Otvoren | `bg-primary` (plava) |
| 3 | Zaključen | `bg-success` (zelena) |
| 4 | Storno | `bg-danger` (crvena) |

---

## 🎨 Najnovije Izmene

### 📅 Oktobar 2025

#### ✨ Brzi Pregled - Ispravke i Pojednostavljenje
**Datum:** 02.10.2025

**🔧 Izmene:**
- ✅ Ispravljena formula računanja stanja: `Stanje = Isplata - VrednostRobe`
- ✅ Dodat prikaz perioda na headerima tabela (01.06.2025 - danas)
- ✅ Poboljšana čitljivost headera (`table-light` → `table-dark`)
- ✅ Tabele sada zauzimaju punu širinu ekrana (`col-md-6` → `col-12`)

**🧹 Čišćenje FinansijskiPregled.razor:**
- ❌ Uklonjeni svi tabovi (Roba na Zalihama, Obračun Otkupa, Obračun Sva Roba)
- ❌ Uklonjene Statistics cards
- ✅ Stranica sada prikazuje samo Brzi Pregled sa 2 tabele

**✅ Rezultat:**
- Čistiji i fokusiraniji UI
- ~300 linija manje koda
- Jednostavnija navigacija
- Bolja preglednost podataka

#### 🎯 Dashboard Analytics - Finalne Ispravke
**Datum:** 18.09.2025

**Ispravke:**
- ✅ Top 5 Kupaca - sada učitava prave kupce (negativne količine)
- ✅ Top 5 Dobavljača - sada učitava prave dobavljače (pozitivne količine)
- ✅ Struktura lagera - limitirana na 5 stavki za čitljivost
- ✅ Brza Statistika - tačni brojevi sa TypeMapping formatiranjem

#### 🎨 TypeMapping Sistem
**Datum:** 17.09.2025

**Implementirano:**
- ✅ Srpsko formatiranje brojeva: `1.234,56`
- ✅ Srpski format datuma: `19.09.2025`
- ✅ Formatiranje valute: `1.245.455,88 RSD`
- ✅ Formatiranje težine: `10.456,90 kg`
- ✅ Badge boje za tipove artikala i statuse
- ✅ Dropdown helper metode

---

## 🗺️ Roadmap

### Q4 2025
- [ ] **Authentication** - Implementacija korisničkog sistema
- [ ] **Authorization** - Role-based access control
- [ ] **Audit Trail** - Logging svih akcija
- [ ] **Mobile Responsive** - Dodatne optimizacije za mobilne uređaje

### Q1 2026
- [ ] **Advanced Charts** - Chart.js/ApexCharts integracija
- [ ] **Real-time Updates** - SignalR za live updates
- [ ] **Email Reports** - Automatsko slanje izveštaja
- [ ] **API Documentation** - Swagger/OpenAPI

### Q2 2026
- [ ] **Mobile App** - React Native verzija
- [ ] **Desktop App** - Electron verzija
- [ ] **Multi-language** - Podrška za više jezika
- [ ] **Dark Mode** - Tamna tema

---

## 📝 Konvencije Koda

### Imenovanje
- **Klase:** PascalCase (`ProizvodnjaService`)
- **Metode:** PascalCase (`UcitajProizvodnju`)
- **Promenljive:** camelCase (`ukupnaKolicina`)
- **Konstante:** UPPER_CASE (`DEFAULT_PAGE_SIZE`)

### Organizacija Fajlova
- Svaka stranica u `Components/Pages/`
- Reusable komponente u `Components/Shared/`
- Servisi grupisani po funkcionalnosti
- Modeli grupisani po modulima

### TypeMapping Guidelines
- Uvek koristiti `TypeMapping.FormatXXX()` za prikaz
- Srpska kultura za sve formate
- Badge klase kroz `TypeMapping.GetXXXBadgeClass()`

---

## 🤝 Contributing

Trenutno je projekat u privatnom razvoju. Za predloge i pitanja, kontaktirajte vlasnika repozitorijuma.

---

## 📄 Licenca

Proprietary - All rights reserved

---

## 👥 Tim

**Developer:** Bane  
**Started:** 2025  
**Status:** Active Development

---

## 📞 Kontakt

Za više informacija:
- **Email:** [your-email@example.com]
- **GitHub:** [https://github.com/YOUR_USERNAME]

---

## 🙏 Acknowledgments

- Bootstrap Icons za ikone
- Bootstrap team za CSS framework
- Blazor community za komponente
- MySQL team za database

---

**Last Updated:** 02.10.2025  
**Version:** 1.0.0  
**Build:** Stable

