# FruitSysWeb Charts Implementation

## Pregled

Ovaj dokument opisuje kompletnu implementaciju charts sistema u FruitSysWeb aplikaciji. Sistem koristi **ChartJs.Blazor.Fork** biblioteku za kreiranje interaktivnih grafika na Dashboard stranici.

## 🎯 Implementirane Funkcionalnosti

### ✅ Kompletni Charts Sistem
- **Bar Charts** - za prikaz top kupaca, dobavljača i proizvodnje
- **Pie Charts** - za strukturu lagera (sirovine, gotovi proizvodi, ambalaža)
- **Line Charts** - za trendove (priprema za buduće funkcionalnosti)

### ✅ Dashboard Integration
- 6 različitih grafika na glavnoj stranici
- Real-time refresh funkcionalnost
- Loading estados i error handling
- Responsive design za sve uređaje

### ✅ Komponente i Arhitektura
- **ChartHelper** - utility klasa za kreiranje grafika
- **ChartExtensions** - extension metode za chart konfiguracije
- **ResponsiveChart** - napredna chart komponenta sa full feature setom
- **DashboardCharts** - glavna komponenta za dashboard
- **ChartShowcase** - test komponenta za sve tipove grafika

## 📁 Struktura Fajlova

```
FruitSysWeb/
├── Components/Shared/Charts/
│   ├── BarChart.razor              # Osnovna bar chart komponenta
│   ├── PieChart.razor              # Osnovna pie chart komponenta  
│   ├── LineChart.razor             # Osnovna line chart komponenta
│   ├── ResponsiveChart.razor       # Napredna chart komponenta
│   ├── DashboardCharts.razor       # Dashboard charts container
│   └── ChartShowcase.razor         # Test/demo komponenta
├── Utils/Charts/
│   ├── ChartHelper.cs              # Utility metode za charts
│   └── ChartExtensions.cs          # Extension metode
└── Components/Pages/
    ├── Home.razor                  # Glavna Dashboard stranica
    └── ChartsTest.razor            # Test stranica za charts
```

## 🚀 Korišćene Tehnologije

- **ChartJs.Blazor.Fork 2.0.2** - Blazor wrapper za Chart.js
- **Chart.js 3.9.1** - JavaScript charts biblioteka
- **Bootstrap 5** - CSS framework za styling
- **Bootstrap Icons** - Ikone za UI

## 📊 Implementirani Grafici

### 1. Top 5 Kupaca (Bar Chart)
- Prikazuje top 5 kupaca po količini (kg)
- Podaci iz `ProizvodnjaService.UcitajTopKupcePoKilogramima()`
- Zelena boja (`rgba(40, 167, 69, 0.8)`)
- Refresh button i export opcije

### 2. Top 5 Dobavljača (Bar Chart)
- Prikazuje top 5 dobavljača po količini (kg)
- Podaci iz `ProizvodnjaService.UcitajTopDobavljacePoKilogramima()`
- Crvena boja (`rgba(220, 53, 69, 0.8)`)
- Refresh button i export opcije

### 3. Proizvodnja po Artiklima (Bar Chart)
- Prikazuje top 8 artikala u proizvodnji
- Podaci iz `ProizvodnjaService.UcitajProizvodnjuPoArtiklima()`
- Plava boja (`rgba(13, 110, 253, 0.8)`)
- Collapse funkcionalnost

### 4. Struktura Sirovina (Pie Chart)
- Prikazuje top 5 sirovina u lageru
- Podaci iz `MagacinLagerService.UcitajStrukturuSirovina()`
- Multi-color palette
- Kompaktni prikaz

### 5. Struktura Gotovih Proizvoda (Pie Chart)
- Prikazuje top 5 gotovih proizvoda
- Podaci iz `MagacinLagerService.UcitajStrukturuGotovihProizvoda()`
- Multi-color palette
- Kompaktni prikaz

### 6. Struktura Ambalaže (Pie Chart)
- Prikazuje top 5 tipova ambalaže
- Podaci iz `MagacinLagerService.UcitajStrukturuAmbalaze()`
- Multi-color palette
- Kompaktni prikaz

## 🛠️ API i Servisi

### ProizvodnjaService Metode
```csharp
// Top kupci po kilogramima (zadnji mesec)
Task<Dictionary<string, decimal>> UcitajTopKupcePoKilogramima(FilterRequest filter)

// Top dobavljači po kilogramima (zadnji mesec)
Task<Dictionary<string, decimal>> UcitajTopDobavljacePoKilogramima(FilterRequest filter)

// Proizvodnja po artiklima (zadnji mesec)
Task<Dictionary<string, decimal>> UcitajProizvodnjuPoArtiklima(FilterRequest filter)
```

### MagacinLagerService Metode
```csharp
// Struktura sirovina u lageru (top 10)
Task<Dictionary<string, decimal>> UcitajStrukturuSirovina()

// Struktura gotovih proizvoda (top 10)
Task<Dictionary<string, decimal>> UcitajStrukturuGotovihProizvoda()

// Struktura ambalaže (top 10)
Task<Dictionary<string, decimal>> UcitajStrukturuAmbalaze()
```

## 🎨 ChartHelper Utility

### Osnovne Metode
```csharp
// Kreiranje bar chart-a
BarConfig CreateBarChart(string[] labels, decimal[] data, string label, string color = null)

// Kreiranje pie chart-a
PieConfig CreatePieChart(string[] labels, decimal[] data, string title = null)

// Kreiranje line chart-a
LineConfig CreateLineChart(string[] labels, decimal[] data, string label, string color = null)

// Custom chart kreiranje iz Dictionary
BarConfig CreateCustomBarChart(Dictionary<string, decimal> data, string title, string color = null, int maxItems = 5)
PieConfig CreateCustomPieChart(Dictionary<string, decimal> data, string title = null, int maxItems = 8)
```

### Utility Metode
```csharp
// Konvertovanje Dictionary u arrays
(string[] labels, decimal[] values) ConvertDictionaryToArrays(Dictionary<string, decimal> data, int maxItems = 10)

// Skraćivanje dugih labela
string[] TruncateLabels(string[] labels, int maxLength = 20)

// Formatiranje brojeva
string FormatNumber(decimal number)
```

## 🔧 ResponsiveChart Komponenta

### Features
- **Multiple Chart Types** - Bar, Pie, Line charts
- **Loading States** - Spinner i loading tekst
- **Error Handling** - Error states sa retry opcijama
- **Refresh Functionality** - Manual refresh button
- **Export Options** - PNG/PDF export (ready for implementation)
- **Collapse/Expand** - Minimizovanje chart-a
- **Data Summary** - Prikaz ukupnih vrednosti
- **Responsive Design** - Prilagođava se svim screen sizes

### Korišćenje
```razor
<ResponsiveChart Title="Moj Chart"
               Icon="bi-bar-chart"
               BarConfig="@myBarConfig"
               IsLoading="@isLoading"
               Unit="kg"
               Height="250"
               OnRefresh="RefreshData"
               ShowExportButton="true"
               ShowCollapseButton="true" />
```

## 🎯 Dashboard Integration

### Home.razor
- Koristi `<DashboardCharts />` komponentu
- Statistike se učitavaju nezavisno od chart-ova
- Clean i organizovan kod

### DashboardCharts.razor
- Centralizovana logika za sve chart-ove
- Paralelno učitavanje podataka
- Quick stats panel sa osnovnim podacima
- "Osveži sve grafike" funkcionalnost

## 🧪 Testing

### ChartsTest.razor stranica
- Dostupna na `/charts-test` route
- `ChartShowcase` komponenta za demonstraciju
- Sample data generation
- Live data loading test
- Auto-refresh functionality test

### Test Features
- Sample Bar Chart sa generated data
- Sample Pie Chart sa static data
- Live Production Data sa real API calls
- Chart statistics prikaz
- Auto-refresh toggle (10 sekundi interval)

## 📱 Responsive Design

### Breakpoints
- **Desktop** (≥992px) - Full 6-chart layout
- **Tablet** (768px-991px) - 2-column layout
- **Mobile** (≤767px) - Single column stack

### Mobile Optimizations
- Kompaktni chart headers
- Touch-friendly buttons
- Optimizovane chart dimenzije
- Horizontalno scrollovanje za tabele

## 🚀 Performance

### Optimizations
- **Parallel Loading** - Svi chart-ovi se učitavaju paralelno
- **Loading States** - UX feedback tokom učitavanja
- **Error Recovery** - Graceful handling grešaka
- **Memory Management** - Proper disposal of resources
- **Caching Ready** - Struktura spremna za caching implementaciju

### Loading Strategy
```csharp
// Paralelno učitavanje svih chart-ova
var tasks = new List<Task>
{
    RefreshKupci(),
    RefreshDobavljaci(),
    RefreshProizvodnja(),
    RefreshSirovine(),
    RefreshGotovi(),
    RefreshAmbalaza()
};

await Task.WhenAll(tasks);
```

## 🔮 Future Enhancements

### Ready for Implementation
- **Real Export** - PNG/PDF export functionality
- **Chart Interactions** - Click events, drill-down
- **Time Range Selectors** - Dynamic date filtering
- **Real-time Updates** - SignalR integration
- **Chart Themes** - Dark/Light mode toggle
- **Custom Colors** - User-selectable color palettes
- **Data Annotations** - Markers i annotations na chart-ovima

### Advanced Features
- **Chart Combinations** - Mixed chart types
- **Zoom & Pan** - Interactive chart navigation
- **Data Tables** - Chart data u tabeli format
- **Comparison Mode** - Side-by-side period comparison
- **Alerts & Thresholds** - Visual indicators za kritične vrednosti

## 🏗️ Instaliranje i Setup

### 1. Dependencies (već instalirano)
```xml
<PackageReference Include="ChartJs.Blazor.Fork" Version="2.0.2" />
```

### 2. JavaScript References (već dodano)
```html
<!-- Chart.js CDN -->
<script src="https://cdn.jsdelivr.net/npm/chart.js@3.9.1/dist/chart.min.js"></script>
```

### 3. Using Statements (već dodano)
```razor
@using FruitSysWeb.Components.Shared.Charts
@using FruitSysWeb.Utils.Charts
@using ChartJs.Blazor
@using ChartJs.Blazor.BarChart
@using ChartJs.Blazor.PieChart
@using ChartJs.Blazor.LineChart
```

## 📋 Implementacija Summary

### ✅ Kompletno implementirano:
1. **Chart Infrastructure** - Sve potrebne klase i komponente
2. **Dashboard Charts** - 6 funkcionalnih chart-ova
3. **Responsive Design** - Radi na svim uređajima
4. **Loading & Error States** - Proper UX feedback
5. **Test Environment** - ChartsTest stranica za development
6. **Navigation** - Charts Test link u glavnom meniju

### 🎯 Glavni rezultat:
Dashboard stranica (`/`) sada ima potpuno funkcionalne chart-ove koji prikazuju:
- Top kupce i dobavljače
- Proizvodnju po artiklima
- Strukturu lagera (sirovine, gotovi proizvodi, ambalaža)
- Real-time refresh funkcionalnost
- Responsive design za sve uređaje

### 🧪 Za testiranje:
- Idite na `/` za glavnu Dashboard stranicu
- Idite na `/charts-test` za test charts funkcionalnosti
- Testirajte refresh button-e
- Testirajte responsive design na različitim screen sizes

Sistem je spreman za production use i lako se može proširiti novim chart tipovima ili funkcionalnostima!
