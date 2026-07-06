# Vizuelne smernice — iz Solar modula

Referenca za budući redesign cele FruitSysWeb aplikacije (Faza 5).  
**Primena na Lager/Finansije još nije urađena.**

---

## Paleta

| Token | Vrednost | Upotreba |
|-------|----------|----------|
| `--sd-bg` | `#0b1020` | Tamna pozadina stranice |
| `--sd-text` | `#e5e7eb` | Glavni tekst |
| `--sd-muted` | `#9ca3af` | Sekundarni tekst |
| `--sd-green` | `#22c55e` | Solar / pozitivno |
| `--sd-blue` | `#38bdf8` | Potrošnja / neutralno |
| `--sd-red` | `#ef4444` | Mreža / upozorenje |
| `--sd-yellow` | `#facc15` | Procenat / akcent |
| `--sd-card-bg` | `rgba(15,23,42,0.78)` | Kartice + blur |

---

## Komponente

- **Kartice:** zaobljenje 20–22px, `backdrop-filter: blur(14px)`, blaga ivica `rgba(255,255,255,0.08)`
- **Status traka:** veliki naslov (24px, font-weight 800), boja po stanju (zelena/plava/crvena pozadina)
- **Grafici (ApexCharts):** tamna tema, Y osa u **kW** (ne `400.000`), 3 linije max na glavnom grafu
- **Pozadina:** radial-gradient „glow“ + `::before` / `::after` blur oblaci

---

## CSS fajlovi

- `wwwroot/css/solar-tokens.css` — samo varijable
- `wwwroot/css/solar.css` — komponente modula (prefiks `.solar-*`, `.sd-*`)

---

## Slike (Odetta)

`wwwroot/images/solar/` — logo, pozadina, reference screenshot-i za doterivanje.

---

## Pravilo migracije na celu app

1. Prvo finalizovati Solar stranice sa Bane-om  
2. Izvući tokeni u `design-tokens.css` (globalno)  
3. Stranica po stranica — bez big-bang refaktora
