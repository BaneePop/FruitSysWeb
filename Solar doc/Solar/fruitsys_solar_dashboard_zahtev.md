# FruitSys Solar Dashboard — zahtev za Claude Code

## Cilj dashboard-a

Ovaj dashboard nije prvenstveno za menadžment, nego za **mašinsku službu / proizvodnju**.

Glavna svrha je da radnici i mašinska služba brzo vide:

- koliko solar trenutno proizvodi,
- koliko fabrika/hala trenutno troši,
- koliko se uzima iz mreže,
- da li je sada dobar trenutak za paljenje velikih potrošača.

Dashboard mora da daje **jasnu odluku**, ne samo lep prikaz.

---

## Glavni koncept

Glavni graf mora biti **jedan line chart sa tri linije**:

1. **Sunce / solarna proizvodnja u kW**  
   Zelena linija.

2. **Potrošnja hale/fabrike u kW**  
   Plava linija.

3. **Iz mreže u kW**  
   Crvena linija.

Y osa mora da bude u **kW**.

Važno: ne sme da prikazuje vrednosti tipa:

```text
400.000
300.000
200.000
```

Treba da prikazuje:

```text
400 kW
300 kW
200 kW
```

---

## Namena za mašinsku službu

Dashboard treba da pomogne mašinskoj službi da odluči:

```text
Kada je dobro paliti veće potrošače?
```

Primer velikih potrošača:

- kompresori,
- tuneli,
- pumpe,
- hladnjače,
- IQF linije,
- veće mašine u proizvodnji.

Kada solarna proizvodnja pokriva veliki deo potrošnje, dashboard treba jasno da kaže da je dobar trenutak za paljenje mašina.

---

## Status kartica za odluku

Dodati gore jednu veliku status karticu.

Kartica treba da prikazuje jednu od tri poruke:

### 1. Dobar trenutak za paljenje

Prikazati kada solar pokriva više od 70% trenutne potrošnje.

```text
Sada: DOBAR TRENUTAK ZA PALJENJE MAŠINA
```

### 2. Normalan rad

Prikazati kada solar pokriva između 50% i 70% potrošnje.

```text
Sada: NORMALAN RAD
```

### 3. Sačekati

Prikazati kada solar pokriva manje od 50% potrošnje.

```text
SAČEKATI SA VELIKIM POTROŠAČIMA
```

---

## Formula za trenutnu odluku

Koristiti ovu logiku:

```js
const solarKw = 106;
const factoryKw = 272;
const gridKw = Math.max(factoryKw - solarKw, 0);
const solarCoverage = Math.round((solarKw / factoryKw) * 100);

let machineStatus = "Normalan rad";

if (solarCoverage >= 70) {
  machineStatus = "Dobar trenutak za paljenje mašina";
} else if (solarCoverage < 50) {
  machineStatus = "Sačekati sa velikim potrošačima";
}
```

Za trenutni primer:

```text
Solar: 106 kW
Potrošnja fabrike: 272 kW
Solarno pokriveno: oko 39%
Iz mreže: 166 kW
```

Dashboard treba da kaže:

```text
Sačekati sa velikim potrošačima
Solar pokriva samo 39% potrošnje
```

---

## Kartice na vrhu

Dodati sledeće kartice:

1. **Solarno trenutno**

```text
106 kW
```

2. **Potrošnja fabrike**

```text
272 kW
```

3. **Iz mreže**

```text
166 kW
```

4. **Solarno pokriveno**

```text
39%
```

5. **Preporučeni period za paljenje mašina**

Primer:

```text
Najbolje između 11:30 i 14:30
```

---

## Glavni line chart

Glavni chart treba da se zove:

```text
Proizvodnja, potrošnja i mreža tokom dana
```

Legenda:

```text
Sunce
Hala / Fabrika
Iz mreže
```

Serije:

```js
const chartData = [
  { time: "08:00", solar: 20, factory: 180, grid: 160 },
  { time: "09:00", solar: 60, factory: 210, grid: 150 },
  { time: "10:00", solar: 120, factory: 260, grid: 140 },
  { time: "11:00", solar: 220, factory: 280, grid: 60 },
  { time: "12:00", solar: 310, factory: 300, grid: 0 },
  { time: "13:00", solar: 340, factory: 310, grid: 0 },
  { time: "14:00", solar: 300, factory: 290, grid: 0 },
  { time: "15:00", solar: 210, factory: 275, grid: 65 },
  { time: "16:00", solar: 120, factory: 260, grid: 140 },
  { time: "17:00", solar: 50, factory: 230, grid: 180 }
];
```

---

## Vizuelna zona za paljenje mašina

Na glavnom grafiku dodati blago zelenu zonu ili highlight za period kada je:

```text
solarna proizvodnja veća ili približno jednaka potrošnji
```

To treba označiti kao:

```text
Preporučeno za paljenje mašina
```

Primer:

```text
11:30 - 14:30
```

---

## Oblačići / glow efekat u pozadini

Za moderni dark UI dodati blur/glow oblake preko CSS-a.

CSS primer:

```css
.solar-dashboard {
  position: relative;
  overflow: hidden;
  background:
    radial-gradient(circle at 20% 10%, rgba(34, 197, 94, 0.18), transparent 28%),
    radial-gradient(circle at 70% 20%, rgba(59, 130, 246, 0.16), transparent 30%),
    radial-gradient(circle at 50% 80%, rgba(250, 204, 21, 0.08), transparent 35%),
    #0b1020;
}

.solar-dashboard::before {
  content: "";
  position: absolute;
  width: 520px;
  height: 520px;
  left: -180px;
  top: 80px;
  background: rgba(34, 197, 94, 0.16);
  filter: blur(90px);
  border-radius: 999px;
  pointer-events: none;
}

.solar-dashboard::after {
  content: "";
  position: absolute;
  width: 520px;
  height: 520px;
  right: -180px;
  top: 180px;
  background: rgba(59, 130, 246, 0.14);
  filter: blur(100px);
  border-radius: 999px;
  pointer-events: none;
}
```

Paneli preko te pozadine treba da imaju glass efekat:

```css
.dashboard-card,
.chart-panel {
  position: relative;
  z-index: 1;
  background: rgba(15, 23, 42, 0.78);
  backdrop-filter: blur(14px);
  border: 1px solid rgba(255, 255, 255, 0.08);
}
```

---

## Vizuelni stil

Zadržati:

- FruitSysWeb navigaciju,
- postojeći dark stil,
- moderan glass/dark izgled,
- zelene akcente za solar,
- plave akcente za potrošnju,
- crvene akcente za mrežu / manjak.

Dashboard treba da bude čitljiv za ljude u proizvodnji, ne samo za programere.

---

## Predložena struktura dashboard-a

### Gornji deo

- naziv: `FruitSys Solar Dashboard`
- podnaslov: `Pregled proizvodnje i potrošnje za mašinsku službu`
- status: `Sistem online`
- velika odluka: `Dobar trenutak / Normalan rad / Sačekati`

### Kartice

- Solarno trenutno kW
- Potrošnja fabrike kW
- Iz mreže kW
- Solarno pokriveno %
- Preporučeni period za paljenje mašina

### Glavni deo

Jedan veliki line chart sa 3 linije:

- Sunce
- Fabrika / hala
- Iz mreže

### Donji deo

Može se dodati kasnije:

- status invertera,
- aktivni alarmi,
- temperatura panela,
- napon mreže,
- frekvencija,
- dnevna proizvodnja,
- mesečna proizvodnja.

Ali trenutno je najbitniji **glavni graf i odluka za paljenje mašina**.

---

## Dodatni zahtev za Claude Code

Kada budeš radio izmene:

```text
Nemoj da praviš više od jednog glavnog grafa.
Glavni graf mora da ima tri linije.
Ne komplikuj dashboard.
Cilj je da mašinska služba za 5 sekundi razume da li je sada dobro paliti veće potrošače.
```

---

## Kratak prompt za Claude Code

Možeš koristiti i ovu kraću verziju:

```text
Izmeni solar dashboard za FruitSysWeb.

Dashboard je za mašinsku službu, da znaju kada da pale velike potrošače.

Glavni graf mora biti jedan line chart sa 3 linije:
- Sunce / solarna proizvodnja kW, zelena
- Potrošnja fabrike/hale kW, plava
- Iz mreže kW, crvena

Y osa mora biti u kW, bez pogrešnog formata 400.000. Treba 400 kW.

Dodaj veliku status karticu:
- ako solar pokriva >= 70% potrošnje: DOBAR TRENUTAK ZA PALJENJE MAŠINA
- ako solar pokriva 50-70%: NORMALAN RAD
- ako solar pokriva < 50%: SAČEKATI SA VELIKIM POTROŠAČIMA

Dodaj kartice:
- Solarno trenutno
- Potrošnja fabrike
- Iz mreže
- Solarno pokriveno %
- Preporučeni period za paljenje mašina

Dodaj modern dark glow/blur oblake u pozadini preko radial-gradient i ::before / ::after.

Zadrži FruitSysWeb navigaciju i postojeći dark stil.
Dashboard mora biti jednostavan, čitljiv i koristan za proizvodnju.
```
