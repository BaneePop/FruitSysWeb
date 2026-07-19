# Solar Dashboard — vizuelni primer za FruitSysWeb

**Zadatak za Cursor:** podaci se već učitavaju. Ne menjati API, bazu, servise ni logiku. Raditi samo vizuelni izgled Solar dashboard strane.

Cilj je da strana izgleda kao moderan **dark industrial energy monitoring dashboard** za fabriku.

---

## 1. Vizuelni stil

Koristiti tamnu temu sa industrijskim/energetskim izgledom.

### Glavna paleta boja

```css
:root {
  --bg-main: #07111f;
  --bg-panel: #0d1b2e;
  --bg-panel-soft: #10243d;
  --bg-card: rgba(15, 31, 53, 0.92);
  --border-soft: rgba(148, 163, 184, 0.18);

  --text-main: #e5edf7;
  --text-muted: #8ea3ba;
  --text-soft: #64748b;

  --solar-yellow: #facc15;
  --solar-orange: #fb923c;
  --electric-blue: #38bdf8;
  --cyan: #22d3ee;
  --green: #22c55e;
  --red: #ef4444;
  --purple: #a78bfa;
}
```

### Stil kartica

```css
.solar-card {
  background: linear-gradient(145deg, rgba(15,31,53,0.96), rgba(8,18,32,0.96));
  border: 1px solid rgba(148, 163, 184, 0.16);
  border-radius: 18px;
  box-shadow: 0 18px 45px rgba(0, 0, 0, 0.28);
  padding: 18px;
}
```

Kartice treba da budu kompaktne, ali čitljive. Ne praviti ogromne prazne prostore.

---

## 2. Struktura strane

Prvi ekran treba odmah da pokaže najvažnije stanje elektrane.

```text
┌──────────────────────────────────────────────────────────────┐
│ Header: Solarna elektrana / status / poslednje očitavanje     │
├──────────────────────────────────────────────────────────────┤
│ KPI kartice: Snaga | Danas | Mreža | Ukupno | Alarmi          │
├─────────────────────────────────────────────┬────────────────┤
│ Glavni veliki area/line chart                │ Vreme / status │
│ Proizvodnja kroz dan                         │ Mini info      │
├───────────────┬───────────────┬─────────────┴────────────────┤
│ Mini chart 1  │ Mini chart 2  │ Mini chart 3                  │
├───────────────┴───────────────┴──────────────────────────────┤
│ Inverteri | Mreža / PowerMeter | Alarmi | Dnevni rezime       │
└──────────────────────────────────────────────────────────────┘
```

---

## 3. Header

Header treba da bude nizak i informativan.

Sadržaj levo:

```text
Solarna elektrana
Odetta · Huawei SmartLogger3000
```

Sadržaj desno:

```text
Status: Online
Poslednje očitavanje: 14:35
[Danas] [7 dana] [Mesec] [Godina] [Osveži]
```

Primer CSS:

```css
.solar-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  margin-bottom: 16px;
}

.solar-title {
  font-size: 26px;
  font-weight: 700;
  color: var(--text-main);
}

.solar-subtitle {
  font-size: 13px;
  color: var(--text-muted);
}

.status-pill {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 7px 12px;
  border-radius: 999px;
  background: rgba(34, 197, 94, 0.12);
  border: 1px solid rgba(34, 197, 94, 0.28);
  color: #86efac;
  font-size: 13px;
}
```

---

## 4. KPI pločice

Prvi red ima 5 pločica.

### Pločice

```text
1. Trenutna snaga
2. Danas proizvedeno
3. Predato u mrežu
4. Ukupna proizvodnja
5. Status / alarmi
```

Primer izgleda jedne pločice:

```text
Trenutna snaga
245.6 kW
76.7% od 320 kW
```

CSS primer:

```css
.kpi-grid {
  display: grid;
  grid-template-columns: repeat(5, minmax(0, 1fr));
  gap: 14px;
  margin-bottom: 16px;
}

.kpi-card {
  min-height: 112px;
  position: relative;
  overflow: hidden;
}

.kpi-label {
  font-size: 13px;
  color: var(--text-muted);
  margin-bottom: 8px;
}

.kpi-value {
  font-size: 30px;
  line-height: 1;
  font-weight: 800;
  color: var(--text-main);
  letter-spacing: -0.02em;
}

.kpi-unit {
  font-size: 15px;
  color: var(--text-muted);
  margin-left: 4px;
}

.kpi-note {
  margin-top: 10px;
  font-size: 12px;
  color: var(--text-soft);
}

.kpi-glow {
  position: absolute;
  right: -35px;
  top: -35px;
  width: 95px;
  height: 95px;
  border-radius: 50%;
  background: radial-gradient(circle, rgba(56,189,248,0.28), transparent 65%);
}
```

Boje po tipu pločice:

```css
.kpi-power .kpi-value { color: #facc15; }
.kpi-today .kpi-value { color: #38bdf8; }
.kpi-grid-export .kpi-value { color: #22d3ee; }
.kpi-total .kpi-value { color: #a78bfa; }
.kpi-status .kpi-value { color: #22c55e; }
```

---

## 5. Glavni chart — proizvodnja kroz dan

Glavni grafikon mora biti najveći element na strani.

### Sadržaj

```text
Naslov: Proizvodnja kroz dan
Linije:
- Ukupna snaga elektrane
- Inverter 1
- Inverter 2
- Inverter 3
```

### Vizuelna pravila

- Ukupna snaga je dominantna linija.
- Inverteri su tanje linije.
- Koristiti smooth line.
- Koristiti transparentni area fill ispod linija.
- Chart ne sme da skače na hover.
- Tooltip ne sme da menja layout.
- X osa: vreme.
- Y osa: kW.

### ApexCharts primer

```js
const productionChartOptions = {
  chart: {
    type: 'area',
    height: 380,
    background: 'transparent',
    toolbar: { show: false },
    animations: { enabled: true, easing: 'easeinout', speed: 450 },
    zoom: { enabled: false }
  },
  stroke: {
    curve: 'smooth',
    width: [4, 2, 2, 2]
  },
  fill: {
    type: 'gradient',
    gradient: {
      shadeIntensity: 0.3,
      opacityFrom: 0.35,
      opacityTo: 0.03,
      stops: [0, 90, 100]
    }
  },
  colors: ['#facc15', '#38bdf8', '#22d3ee', '#a78bfa'],
  dataLabels: { enabled: false },
  grid: {
    borderColor: 'rgba(148, 163, 184, 0.12)',
    strokeDashArray: 4
  },
  xaxis: {
    categories: ['06:00', '07:00', '08:00', '09:00', '10:00', '11:00', '12:00', '13:00', '14:00'],
    labels: { style: { colors: '#8ea3ba' } },
    axisBorder: { show: false },
    axisTicks: { show: false }
  },
  yaxis: {
    labels: {
      style: { colors: '#8ea3ba' },
      formatter: value => `${value} kW`
    }
  },
  legend: {
    position: 'top',
    horizontalAlign: 'right',
    labels: { colors: '#cbd5e1' },
    markers: { radius: 12 }
  },
  tooltip: {
    theme: 'dark',
    shared: true,
    intersect: false,
    y: { formatter: value => `${value.toFixed(1)} kW` }
  }
};

const productionChartSeries = [
  { name: 'Ukupna snaga', data: [0, 20, 75, 140, 210, 265, 286, 260, 220] },
  { name: 'Inverter 1', data: [0, 7, 25, 46, 72, 88, 96, 85, 74] },
  { name: 'Inverter 2', data: [0, 6, 24, 45, 70, 86, 94, 84, 72] },
  { name: 'Inverter 3', data: [0, 5, 22, 41, 65, 79, 86, 78, 68] }
];
```

---

## 6. Mini line chart pločice

Ispod glavnog grafikona dodati 3 male chart pločice.

### Pločica 1 — Snaga danas

```text
Snaga danas
245.6 kW
mini area chart
```

Boja: žuta / solar.

### Pločica 2 — Predaja u mrežu

```text
Predaja u mrežu
35.0 kW
mini area chart
```

Boja: cyan.

### Pločica 3 — Efikasnost invertera

```text
Balans invertera
96.4%
mini line chart
```

Boja: zelena.

### CSS za mini kartice

```css
.mini-chart-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 14px;
  margin-top: 14px;
}

.mini-chart-card {
  height: 150px;
  padding: 14px 16px;
}

.mini-chart-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 8px;
}

.mini-chart-title {
  color: var(--text-muted);
  font-size: 12px;
}

.mini-chart-value {
  font-size: 22px;
  font-weight: 800;
  color: var(--text-main);
}
```

### Apex mini chart primer

```js
const miniChartBaseOptions = {
  chart: {
    type: 'area',
    height: 70,
    sparkline: { enabled: true },
    animations: { enabled: false }
  },
  stroke: { curve: 'smooth', width: 2.5 },
  fill: {
    type: 'gradient',
    gradient: { opacityFrom: 0.32, opacityTo: 0.02 }
  },
  tooltip: {
    theme: 'dark',
    fixed: { enabled: false },
    x: { show: false }
  }
};

const miniPowerOptions = {
  ...miniChartBaseOptions,
  colors: ['#facc15']
};

const miniGridOptions = {
  ...miniChartBaseOptions,
  colors: ['#22d3ee']
};

const miniBalanceOptions = {
  ...miniChartBaseOptions,
  colors: ['#22c55e']
};
```

---

## 7. Donji blokovi

Ispod grafika ide grid sa 4 kartice.

```text
Inverteri | Mreža / PowerMeter | Alarmi | Dnevni rezime
```

### CSS

```css
.solar-bottom-grid {
  display: grid;
  grid-template-columns: 1.4fr 1fr 1fr 1fr;
  gap: 14px;
  margin-top: 16px;
}

.panel-title {
  font-size: 15px;
  font-weight: 700;
  color: var(--text-main);
  margin-bottom: 12px;
}
```

---

## 8. Tabela invertera

Tabela mora biti kompaktna, bez velikih praznih kolona.

```text
Inverter              Snaga      Status      Danas
SUN2000-110KTL-M2     84 kW      Online      760 kWh
SUN2000-110KTL-M2     81 kW      Online      745 kWh
SUN2000-100KTL-M2     73 kW      Online      680 kWh
```

CSS:

```css
.inverter-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 13px;
}

.inverter-table th {
  color: var(--text-soft);
  font-weight: 600;
  text-align: left;
  padding: 8px 6px;
  border-bottom: 1px solid rgba(148, 163, 184, 0.12);
}

.inverter-table td {
  color: var(--text-main);
  padding: 9px 6px;
  border-bottom: 1px solid rgba(148, 163, 184, 0.08);
}

.status-online {
  color: #86efac;
  background: rgba(34, 197, 94, 0.12);
  border: 1px solid rgba(34, 197, 94, 0.22);
  border-radius: 999px;
  padding: 3px 8px;
  font-size: 12px;
}
```

---

## 9. Vremenska / nebo kartica

Ako postoji weather/sky kartica, neka bude estetska, ali mala.

```text
Vremenski uslovi
Sunčano
28°C
Uticaj na proizvodnju: visok
```

Primer CSS:

```css
.weather-card {
  background:
    radial-gradient(circle at 20% 20%, rgba(250, 204, 21, 0.22), transparent 30%),
    radial-gradient(circle at 80% 30%, rgba(56, 189, 248, 0.18), transparent 34%),
    linear-gradient(145deg, rgba(14, 35, 62, 0.98), rgba(8, 18, 32, 0.98));
  border-radius: 18px;
  border: 1px solid rgba(148, 163, 184, 0.16);
  min-height: 180px;
  position: relative;
  overflow: hidden;
}

.cloud-shape {
  position: absolute;
  right: 20px;
  top: 28px;
  width: 96px;
  height: 34px;
  border-radius: 999px;
  background: rgba(255,255,255,0.16);
  filter: blur(0.2px);
}
```

---

## 10. Responsive pravila

Desktop je prioritet, ali mora da se složi i na manjem ekranu.

```css
@media (max-width: 1400px) {
  .kpi-grid { grid-template-columns: repeat(3, minmax(0, 1fr)); }
  .solar-bottom-grid { grid-template-columns: repeat(2, minmax(0, 1fr)); }
}

@media (max-width: 900px) {
  .solar-header { flex-direction: column; align-items: flex-start; }
  .kpi-grid { grid-template-columns: 1fr; }
  .mini-chart-grid { grid-template-columns: 1fr; }
  .solar-bottom-grid { grid-template-columns: 1fr; }
}
```

---

## 11. Mock primer za vizuelni test

Koristiti ako treba da se testira dizajn bez pravih podataka.

```js
const solarMock = {
  stationName: 'Odetta',
  loggerName: 'SmartLogger3000',
  status: 'Online',
  lastReadTime: '14:35',
  nominalPowerKw: 320,
  currentPowerKw: 245.6,
  dailyEnergyKwh: 2292,
  dailyOnGridEnergyKwh: 1850,
  cumulativeEnergyKwh: 273931.31,
  alarmCount: 0,
  inverters: [
    { name: 'SUN2000-110KTL-M2', serial: '6T2429013142', powerKw: 84, status: 'Online', dailyKwh: 760 },
    { name: 'SUN2000-110KTL-M2', serial: '6T2529007129', powerKw: 81, status: 'Online', dailyKwh: 745 },
    { name: 'SUN2000-100KTL-M2', serial: '6T2579002759', powerKw: 73, status: 'Online', dailyKwh: 680 }
  ],
  chart: [
    { time: '06:00', total: 0, inv1: 0, inv2: 0, inv3: 0 },
    { time: '07:00', total: 20, inv1: 7, inv2: 6, inv3: 5 },
    { time: '08:00', total: 75, inv1: 25, inv2: 24, inv3: 22 },
    { time: '09:00', total: 140, inv1: 46, inv2: 45, inv3: 41 },
    { time: '10:00', total: 210, inv1: 72, inv2: 70, inv3: 65 },
    { time: '11:00', total: 265, inv1: 88, inv2: 86, inv3: 79 },
    { time: '12:00', total: 286, inv1: 96, inv2: 94, inv3: 86 },
    { time: '13:00', total: 260, inv1: 85, inv2: 84, inv3: 78 },
    { time: '14:00', total: 220, inv1: 74, inv2: 72, inv3: 68 }
  ]
};
```

---

## 12. Šta ne treba raditi

```text
- Ne menjati API logiku.
- Ne menjati bazu.
- Ne menjati servis koji učitava podatke.
- Ne praviti običan Bootstrap admin izgled.
- Ne praviti ogromne prazne kartice.
- Ne praviti male KPI brojeve.
- Ne dozvoliti da chart menja veličinu na hover.
- Ne koristiti previše animacija.
```

---

## 13. Kratka komanda za Cursor

```text
Uradi samo vizuelni redizajn Solar dashboard strane. Podaci se već učitavaju, ne menjaj logiku. Želim moderan dark industrial energy monitoring dashboard: kompaktne KPI pločice, veliki centralni ApexCharts area/line chart, 3 mini line chart pločice, donje kartice za invertere/mrežu/alarme/rezime, jasne boje, bez praznog prostora i bez skakanja grafikona na hover.
```
