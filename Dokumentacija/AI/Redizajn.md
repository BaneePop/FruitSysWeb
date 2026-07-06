# FruitSysWeb — Redizajn UI tehnički brief

**Dokument za Claude Code / programera**  
**Cilj:** redizajn postojeće web aplikacije FruitSysWeb u kompaktniji, pregledniji i moderniji tamni ERP interfejs.

---

## 1. Kontekst

FruitSysWeb je poslovna web aplikacija za fabriku za preradu voća. Aplikacija se koristi za proizvodnju, kvalitet, finansije, poslovanje, lager, promet robe, izveštaje i administraciju.

Trenutni UI ima nekoliko problema:

- elementi su preveliki;
- početna strana ima veliki baner `FruitSysWeb` koji zauzima prostor;
- ima previše pločica;
- ima previše stranica i sličnih izveštaja;
- ima previše grafikona koji ne pomažu direktno radu;
- navigacija je preširoka i razbijena na previše modula;
- tabele treba da budu gušće i čitljivije;
- korisnik želi da tamna tema ostane, ali da bude mirnija i kompaktnija.

Vizuelni pravac koji se korisniku najviše dopao je:

```text
04_sidebar_modern_glass_v2_bane.html
```

Taj prototip koristiti kao osnovnu ideju: **dark modern glass**, sidebar navigacija, kompaktne KPI kartice, smirene boje, čiste tabele.

---

## 2. Glavni cilj redizajna

Napraviti novi UI za FruitSysWeb koji je:

- brz za svakodnevni rad;
- kompaktan;
- čitljiv;
- jednostavan za navigaciju;
- pogodan za proizvodnju, lager, finansije i administraciju;
- bez velikog vizuelnog šuma;
- bez nepotrebnih grafikona;
- sa manje stranica i bolje grupisanim izveštajima;
- sa tamnom temom;
- sa sidebar navigacijom koja može da se skupi.

Ne praviti “marketing sajt”. Ovo treba da bude:

```text
Dark Compact Factory ERP UI
```

---

## 3. Nova informaciona arhitektura

Trenutne module treba grupisati u 4 glavne celine:

```text
1. Proizvodnja i kvalitet
2. Finansije i poslovanje
3. Lager i promet robe
4. Administracija / podešavanja
```

Pored toga postoji stalni link:

```text
Početna
```

### Predlog sidebar strukture

```text
FruitSysWeb

Početna

Proizvodnja i kvalitet
  - Radni nalozi
  - Proizvodnja
  - Utrošak sirovina
  - Utrošak radne snage
  - Kontrola kvaliteta
  - Analize
  - Reklamacije
  - Dokumentacija kvaliteta

Finansije i poslovanje
  - Kupci
  - Dobavljači
  - Ugovori
  - Cene
  - Uplate / isplate
  - Troškovi
  - Dugovanja / potraživanja
  - Finansijski pregled

Lager i promet robe
  - Stanje lagera
  - Ulaz robe
  - Izlaz robe
  - Interni promet
  - Otprema
  - Najavljeni utovari
  - Lotovi
  - Kartica artikla

Administracija
  - Korisnici
  - Artikli
  - Šifarnici
  - Ambalaža
  - Podešavanja
```

---

## 4. Sidebar ponašanje

Sidebar treba da ima 2 desktop stanja:

```text
Expanded:
- širina oko 260px
- vide se ikonice i nazivi

Collapsed:
- širina oko 72px
- vide se samo ikonice
- podlinkovi mogu biti sakriveni
- naziv može da se vidi preko tooltip-a ili hover-a
```

Na manjim ekranima sidebar treba da se ponaša kao drawer/offcanvas meni.

Potrebno ponašanje:

- klik na `☰` skuplja ili otvara sidebar;
- stanje sidebara zapamtiti u `localStorage`;
- kada je sidebar skupljen, glavni sadržaj treba da se proširi;
- na manjim ekranima sidebar treba da ide preko sadržaja;
- ne sme da razbija tabele.

### Primer JavaScript logike

```js
const app = document.querySelector(".app-shell");
const toggle = document.querySelector("[data-sidebar-toggle]");

const saved = localStorage.getItem("fruitsys.sidebar");
if (saved === "collapsed") {
  app.classList.add("sidebar-collapsed");
}

toggle?.addEventListener("click", () => {
  app.classList.toggle("sidebar-collapsed");

  localStorage.setItem(
    "fruitsys.sidebar",
    app.classList.contains("sidebar-collapsed") ? "collapsed" : "expanded"
  );
});
```

---

## 5. Novi layout

Ako je aplikacija ASP.NET MVC ili Razor Pages, novi layout najverovatnije ide u:

```text
Views/Shared/_Layout.cshtml
```

Ako je Blazor:

```text
Shared/MainLayout.razor
Shared/NavMenu.razor
```

Osnovna struktura:

```html
<div class="app-shell">
  <aside class="sidebar">
    <div class="brand">
      <div class="brand-mark">FS</div>
      <div class="brand-text">
        <strong>FruitSysWeb</strong>
        <span>Factory ERP</span>
      </div>
    </div>

    <nav class="sidebar-nav">
      <a class="nav-link active" href="/">
        <span class="icon">⌂</span>
        <span class="label">Početna</span>
      </a>

      <details class="nav-group" open>
        <summary>
          <span class="icon">⚙</span>
          <span class="label">Proizvodnja i kvalitet</span>
        </summary>

        <div class="subnav">
          <a href="/Production/WorkOrders">Radni nalozi</a>
          <a href="/Production">Proizvodnja</a>
          <a href="/Quality">Kontrola kvaliteta</a>
        </div>
      </details>
    </nav>
  </aside>

  <main class="main">
    <header class="topbar">
      <button type="button" data-sidebar-toggle>☰</button>
      <h1>Pregled dana</h1>
      <div class="topbar-actions">
        <input type="search" placeholder="Pretraga..." />
        <span class="date">02.07.2026</span>
        <span class="user">Admin</span>
      </div>
    </header>

    <section class="content">
      <!-- page content -->
    </section>
  </main>
</div>
```

---

## 6. Novi CSS fajl

Dodati novi CSS fajl ili preurediti postojeći:

```text
wwwroot/css/fruitsys-ui.css
```

U njemu definisati:

- CSS promenljive;
- tamnu temu;
- sidebar;
- topbar;
- kartice;
- KPI kartice;
- tabele;
- badge oznake;
- dugmad;
- responsive pravila.

### Predlog CSS promenljivih

```css
:root {
  --bg: #08111f;
  --bg-soft: #0d1829;
  --sidebar: rgba(13, 24, 41, 0.88);
  --panel: rgba(18, 30, 49, 0.78);
  --panel-solid: #121e31;
  --panel-hover: #192a43;

  --border: rgba(148, 163, 184, 0.16);
  --border-strong: rgba(148, 163, 184, 0.28);

  --text: #e5edf7;
  --text-muted: #93a4b8;
  --text-soft: #c7d2e1;

  --green: #22c55e;
  --green-soft: rgba(34, 197, 94, 0.14);

  --blue: #38bdf8;
  --blue-soft: rgba(56, 189, 248, 0.14);

  --warning: #f59e0b;
  --warning-soft: rgba(245, 158, 11, 0.15);

  --danger: #ef4444;
  --danger-soft: rgba(239, 68, 68, 0.15);

  --radius-sm: 9px;
  --radius-md: 12px;
  --radius-lg: 18px;

  --sidebar-width: 260px;
  --sidebar-collapsed-width: 72px;
  --topbar-height: 58px;
}
```

---

## 7. Preporučene dimenzije

UI treba da bude kompaktniji od trenutnog.

```css
body {
  font-size: 14px;
}

.topbar {
  height: 56px;
}

.sidebar {
  width: 260px;
}

.app-shell.sidebar-collapsed .sidebar {
  width: 72px;
}

.sidebar-main-link,
.nav-link,
summary {
  font-size: 15px;
  min-height: 42px;
  padding: 9px 12px;
}

.sidebar-sub-link,
.subnav a {
  font-size: 15px;
  padding: 8px 12px 8px 38px;
}

.kpi-card {
  min-height: 72px;
  padding: 9px 10px;
  border-radius: 11px;
}

.kpi-card .value {
  font-size: 18px;
  font-weight: 700;
}

.kpi-card .label {
  font-size: 12px;
}

.table {
  font-size: 13px;
}

.table th {
  height: 38px;
  padding: 8px 10px;
}

.table td {
  height: 38px;
  padding: 8px 10px;
}

.badge {
  font-size: 12px;
  padding: 4px 8px;
  border-radius: 999px;
}
```

---

## 8. Početna strana

Trenutnu početnu stranu promeniti.

Ukloniti:

- veliki `FruitSysWeb` baner;
- velike modulske pločice;
- nepotrebne grafikone;
- višak praznog prostora.

Nova početna strana treba da bude:

```text
Pregled dana
```

### Predlog home layout-a

```text
Pregled dana                                    02.07.2026

[ Hitno: 3 ] [ Aktivni RN: 12 ] [ Ulaz: 48.2t ] [ Izlaz: 31.6t ] [ Upozorenja: 5 ]

Plan i aktivnosti
Filteri: [Danas] [7 dana] [Kupac] [Artikal] [Status]

Tabela:
Tip | Datum | RN/Dokument | Partner | Artikal | Količina | Status | Akcija

Brze akcije:
[Novi prijem] [Novi radni nalog] [Nova otprema] [Finansijski unos]
```

### KPI kartice

Predlog KPI kartica:

```text
- Hitni utovari
- Aktivni radni nalozi
- Ulaz danas
- Izlaz danas
- Upozorenja
- Otvorena dugovanja
```

KPI kartice treba da budu oko **30% manje** od prvobitnih velikih pločica.

---

## 9. Tabele

Tabele su najvažniji deo aplikacije i treba da budu gušće.

Standard:

```text
- font 13px
- red 38–42px
- sticky header gde ima mnogo redova
- filter bar iznad tabele
- status badge kolona
- brojevi desno poravnati
- datumi uvek isti format
- klik na red otvara detalje
- primarna akcija desno
```

Primer kolona za `Plan i aktivnosti`:

```text
Tip | Datum | RN/Dokument | Partner | Artikal | Količina | Status | Akcija
```

### Badge stilovi

```css
.badge-danger {
  background: var(--danger-soft);
  color: #fecaca;
  border: 1px solid rgba(239, 68, 68, 0.35);
}

.badge-warning {
  background: var(--warning-soft);
  color: #fde68a;
  border: 1px solid rgba(245, 158, 11, 0.35);
}

.badge-success {
  background: var(--green-soft);
  color: #bbf7d0;
  border: 1px solid rgba(34, 197, 94, 0.35);
}
```

---

## 10. Izveštaji — smanjiti broj stranica

Veliki cilj je da se ukloni mnogo posebnih izveštaja i da se srodni izveštaji spoje.

Ne praviti 10 različitih stranica za slične izveštaje. Bolje napraviti jedan ekran sa filterima i promenom prikaza.

### Proizvodni izveštaji

```text
Strana: Proizvodni izveštaji

Filteri:
- Period
- Radni nalog
- Artikal
- Kupac
- Lot
- Tip prikaza

Tip prikaza:
- Sažetak
- Po radnom nalogu
- Po artiklu
- Po kupcu
- Utrošak sirovina
- Utrošak radne snage
```

### Finansijski pregled

```text
Strana: Finansijski pregled

Filteri:
- Period
- Kupac / dobavljač
- Tip dokumenta
- Status
- Valuta

Tip prikaza:
- Sažetak
- Dugovanja
- Potraživanja
- Uplate
- Isplate
- Troškovi
```

### Lager i promet robe

```text
Strana: Lager i promet robe

Filteri:
- Period
- Artikal
- Lot
- Magacin
- Kupac / dobavljač
- Tip prometa

Tip prikaza:
- Stanje lagera
- Ulaz robe
- Izlaz robe
- Interni promet
- Kartica artikla
- Najavljeni utovari
```

---

## 11. Grafikoni

Grafikone smanjiti i ukloniti sve koji ne pomažu odluci.

Ostaviti samo:

```text
- prodaja / nabavka po mesecima
- proizvodnja po periodu
- stanje lagera po grupama
- troškovi po kategorijama
```

Grafikoni ne treba da budu na svakoj home strani. Većinu grafika premestiti u posebne analitičke preglede.

---

## 12. Responsive pravila

Aplikacija će se najviše koristiti na desktop/laptop ekranima, ali treba da radi i na manjim ekranima.

```css
@media (max-width: 1100px) {
  .kpi-grid {
    grid-template-columns: repeat(3, 1fr);
  }
}

@media (max-width: 800px) {
  .app-shell {
    grid-template-columns: 1fr;
  }

  .sidebar {
    position: fixed;
    inset: 0 auto 0 0;
    transform: translateX(-100%);
    z-index: 1000;
  }

  .app-shell.sidebar-open .sidebar {
    transform: translateX(0);
  }

  .kpi-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}

@media (max-width: 520px) {
  .kpi-grid {
    grid-template-columns: 1fr;
  }

  .table-wrapper {
    overflow-x: auto;
  }
}
```

---

## 13. Implementacioni prioritet

Raditi sledećim redom:

```text
1. Napraviti novi sidebar layout
2. Ubaciti dark modern glass temu
3. Napraviti sidebar collapse funkciju
4. Redizajnirati home stranu
5. Smanjiti KPI kartice
6. Srediti tabele
7. Grupisati navigaciju
8. Tek posle toga krenuti u spajanje izveštaja
```

---

## 14. Pravila za izmene u kodu

Važno:

```text
- ne menjati poslovnu logiku
- ne menjati rute ako nije neophodno
- prvo promeniti layout i CSS
- UI komponente napraviti reusable gde god može
- koristiti postojeće podatke i postojeće modele
- izbegavati inline style osim za privremeno testiranje
- ne brisati postojeće strane bez potvrde
- staru navigaciju možeš privremeno komentarisati dok se nova testira
```

---

## 15. Dodatne UX smernice

### Kontrast

Za tamnu temu voditi računa o kontrastu. Tekst mora biti čitljiv. Ne koristiti pretamne sive tekstove na tamnoj pozadini.

Preporuka:

```text
- glavni tekst: svetao, npr. #e5edf7
- sekundarni tekst: ne previše taman, npr. #93a4b8
- kritični statusi: crvena samo za stvarno kritično
- upozorenja: narandžasta
- pozitivno/završeno: zelena
```

### Boje

Ne koristiti previše jake boje istovremeno. Boje treba da znače nešto:

```text
Zelena    = uspešno, završeno, pozitivno
Plava     = informacija, link, neutralna akcija
Narandžasta = upozorenje, uskoro, treba pažnja
Crvena    = hitno, greška, kritično
Siva      = neutralno, arhiva, neaktivno
```

### Design tokens

Boje, spacing i veličine držati u CSS promenljivama. Ne hardkodovati iste boje na 20 mesta.

---

## 16. Reference za programera

Korisne reference za implementaciju i proveru UI principa:

- Bootstrap 5.3 Offcanvas: https://getbootstrap.com/docs/5.3/components/offcanvas/
- Bootstrap 5.3 CSS variables: https://getbootstrap.com/docs/5.3/customize/css-variables/
- Bootstrap 5.3 Color modes: https://getbootstrap.com/docs/5.3/customize/color-modes/
- Bootstrap 5.3 Breakpoints: https://getbootstrap.com/docs/5.3/layout/breakpoints/
- Fluent 2 Layout: https://fluent2.microsoft.design/layout
- Fluent 2 Color: https://fluent2.microsoft.design/color
- WCAG Contrast Minimum: https://www.w3.org/WAI/WCAG22/Understanding/contrast-minimum.html
- Design systems and design tokens: https://en.wikipedia.org/wiki/Design_system

---

## 17. Kratak rezime za Claude Code

Napravi redizajn FruitSysWeb aplikacije prema pravcu:

```text
Dark Compact Factory ERP UI
```

Najvažnije:

```text
- koristi tamnu modern glass temu
- napravi sidebar koji se skuplja
- smanji elemente i pločice
- ukloni veliki FruitSysWeb baner sa home strane
- napravi home kao Pregled dana
- grupiši navigaciju u 4 glavne celine
- sredi tabele da budu gušće i preglednije
- smanji broj izveštaja tako što ih spajaš u ekrane sa filterima
- ukloni nepotrebne grafikone
- ne menjaj poslovnu logiku dok se ne potvrdi UI
```
