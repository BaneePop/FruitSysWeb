# FruitSysWeb - ODETTA DOO

Sistem za upravljanje voćnom preradom - Blazor Server aplikacija

## 📋 Pregled Projekta

FruitSysWeb je kompleksna web aplikacija razvijena u **ASP.NET Core Blazor Server** za potrebe upravljanja voćnom preradom, proizvodnjom, finansijama i izveštavanjem za kompaniju **ODETTA DOO**.

## 🎯 Najnovije Izmene (24.10.2025)

### 1. **Tooltip sa Nazivima Artikala u Graficima**
- ✅ Dodati tooltips u grafikone prijema (`IzvestajPrijem` i `IzvestajPrijemIstorija`)
- ✅ Prikazuju se nazivi artikala kada se pređe mišem preko bara
- ✅ SQL query koristi `GROUP_CONCAT` za agregaciju artikala po dobavljaču
- 📁 Fajlovi: `PaletniListService.cs`, `IzvestajPrijem.razor`, `IzvestajPrijemIstorija.razor`, `ApexBarChart.razor`

### 2. **Export Samo Vidljivih Kolona (Excel & PDF)**
- ✅ Novi metodi: `ExportToExcelWithColumns()` i `ExportToPdfWithColumns()`
- ✅ Export samo kolona koje se prikazuju u tabeli (bez ID-eva, šifri, skrivenih polja)
- ✅ Ažurirano **11 stranica** sa export funkcijom:
  - Nabavka, Prodaja, Poslovođa, Roba, Ambalaza
  - Lager, Finansije, Ugovori, Smenski izveštaji
  - Radni nalozi, Proizvodnja, Lager proizvodnje
- 📁 Fajlovi: `ExportService.cs`, sve stranice sa export funkcijom

### 3. **Srpski Format Brojeva i Datuma**
- ✅ Format brojeva: `1.000.987,56` (umesto američkog `1,000,987.56`)
- ✅ Format datuma: `31.12.2025` (umesto `12/31/2025`)
- ✅ Koristi se `CultureInfo("sr-Latn-RS")` za formatiranje
- 📁 Fajlovi: `ExportService.cs`

### 4. **Moderni PDF Dizajn sa ODETTA Branding-om**

#### **Header PDF-a:**
- 🖼️ **Logo**: Logo.png u gornjem levom uglu
- 🏢 **Kompanija**: ODETTA DOO, Kralja Dragutina 5, 7/31, 15000 Šabac, Srbija
- 📅 **Info**: Datum, vreme generisanja, ukupan broj stavki
- 🎨 **Dizajn**: Svetlo plavi background (`Colors.Blue.Lighten2`)

#### **Tabela:**
- 🦓 **Zebra stripovi**: Alternativni redovi (svetlo siva/bela)
- 🔵 **Header**: Svetlo plavi sa belim bold tekstom
- 🔢 **Brojevi**: Automatski desno poravnati
- 🎨 **Moderni izgled**: Profesionalan i čitljiv

#### **Footer:**
- 📍 Kompletna adresa: "ODETTA DOO, Kralja Dragutina 5, 7/31, 15000 Šabac, www.odetta.rs"
- 📖 Paginacija: "Stranica X od Y"

📁 Fajlovi: `ExportService.cs`, `FruitSysWeb.csproj` (Logo.png copy config)

## 🛠️ Tehnologije

- **Framework**: ASP.NET Core 8.0 Blazor Server
- **Baza podataka**: MySQL (MySqlConnector + Dapper ORM)
- **Excel Export**: ClosedXML
- **PDF Export**: QuestPDF
- **Grafikoni**: Blazor-ApexCharts
- **Logging**: Serilog
- **Kultura**: Srpski (sr-Latn-RS)

## 📦 Instalacija

```bash
# 1. Klonirati repozitorijum
git clone <repository-url>
cd FruitSysWeb

# 2. Restore dependencies
dotnet restore

# 3. Konfigurisati connection string u appsettings.json
# ConnectionStrings:DefaultConnection

# 4. Pokrenuti aplikaciju
dotnet run
```

## 🏗️ Struktura Projekta

```
FruitSysWeb/
├── Components/
│   ├── Pages/           # Blazor stranice (Nabavka, Prodaja, Izveštaji)
│   ├── Charts/          # ApexBarChart, ChartDataHelper
│   └── Shared/          # Deljene komponente
├── Services/
│   ├── Interfaces/      # Service interfaces
│   └── Implementations/ # Service implementacije
│       ├── ExportService/      # Excel & PDF export
│       └── IzvestajService/    # Izveštaji i reports
├── Models/              # Data modeli
├── Logo.png            # Kompanijski logo (kopira se u bin/)
└── wwwroot/            # Statički fajlovi
```

## 🎨 Ključne Funkcionalnosti

### **Moduli:**
- 📊 **Nabavka & Prodaja**: Finansijski izveštaji sa filterima
- 🏭 **Proizvodnja**: Radni nalozi, smenski izveštaji
- 📦 **Lager**: Praćenje zaliha robe i ambalaže
- 💰 **Finansije**: Kompletan finansijski tracking
- 📋 **Ugovori**: Upravljanje ugovorima sa komitentima
- 📈 **Izveštaji**: Dinamički grafikon sa drill-down funkcijom

### **Export Sistem:**
- ✅ Excel export sa srpskim formatom
- ✅ PDF export sa ODETTA branding-om
- ✅ Samo vidljive kolone (bez internal ID-eva)
- ✅ Custom kolone po izveštaju

### **RBAC (Role-Based Access Control):**
- ✅ Username-based pristup (hardcoded usernames)
- ✅ Puни pristup vs Ograničeni pristup
- ✅ Različite home stranice po nivou pristupa

## 👥 Korisnici

### **Ograničeni korisnici:**
- zoran, jelena, pedja, radmila, masinska, BaneT
- Početna stranica: `/home-ograniceni`
- Nema pristup: Nabavka, Prodaja, Troškovi ambalаže/poslovanja

### **Ostali korisnici:**
- Puni pristup svim funkcijama
- Početna stranica: `/` (glavni dashboard)

## 📝 Commit Log

### Poslednji commit:
```
Feature: Moderni PDF export sa ODETTA branding-om i srpskim formatom

- Dodat logo i kompanijski branding u PDF header/footer
- Implementiran srpski format brojeva (1.000.987,56)
- Svetlo plavi dizajn sa zebra stripovima
- Export samo vidljivih kolona (11 stranica)
- Tooltips sa nazivima artikala u grafikonima
- Sve izmene testirane i build uspešan
```

## 📞 Kontakt

**ODETTA DOO**
Kralja Dragutina 5, 7/31
15000 Šabac, Srbija
www.odetta.rs

---

*Poslednja izmena: 24.10.2025*
