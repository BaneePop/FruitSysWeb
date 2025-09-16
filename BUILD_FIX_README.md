# 🔧 Build Fix Instructions

Ispravili smo package naziv i implementaciju:

## ✅ Promene
1. **Package**: `ApexCharts.Net` → `Blazor-ApexCharts 6.0.2`
2. **Namespace**: Dodao `@using ApexCharts` u _Imports.razor
3. **JavaScript**: Uklonili manualne skriptove (automatski se uključuju)
4. **Komponente**: Ažurirali ApexBarChart i ApexPieChart
5. **DashboardStats**: Kreali nedostajuću komponentu

## 🚀 Test Build
```bash
chmod +x test-build.sh
./test-build.sh
```

## 🎯 Ako build prođe
```bash
dotnet run
```

Onda idi na:
- Dashboard: http://localhost:5073/
- Charts Test: http://localhost:5073/charts-test

## 🐛 Mogući Problemi
- Ako ApexChartOptions ne postoji, možda treba stariju verziju
- Ako SeriesType.Bar ne postoji, možda sintaksa drugačija

Blazor-ApexCharts je glavni paket za ApexCharts u Blazor projektima.
