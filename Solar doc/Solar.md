# Modul — Solar

## Cilj

Praćenje solarne elektrane za mašinsku službu i upravu.

## Rute

- `/solar-hala`
- `/solar-pregled`

## Arhitektura

- Produkcijski MySQL se ne dira.
- Lokalna istorija je SQLite `Data/solar.db`.
- FusionSolar API koristi se za live podatke.
- `FusionSolar:Enabled` kontroliše aktivaciju.

## UI smer

- dark industrial dashboard,
- KPI kartice,
- glavni graf proizvodnje,
- mini grafici,
- kompaktno i čitljivo,
- bez praznih prostora.
