# FruitSysWeb — Specifikacija Print API-ja (za implementaciju na web strani)

> Specifikacija Print API-ja koji **FruitSysWeb** treba da implementira.
> Cilj: pozivalac traži PDF dokumenta od FruitSysWeb-a po tipu i ID-u, umesto da ga generiše lokalno.
> „Pozivalac" = aplikacija koja koristi ovaj API; sve što ona radi je jedan HTTP poziv sa tokenom.

---

## 1. Pregled i princip rada

- Pozivalac uvek zove **jedan endpoint**: `GET /api/v1/print/{tip}/{id}` sa auth header-om.
- **Web drži svu politiku** (web-orkestrirano). Kriterijum je status dokumenta
  **`DokumentStatusEnum.Zakljucen` (vrednost 3)**:
  - status **nije Zaključen** → generiši svež PDF iz baze, ne snimaj ga,
  - status **Zaključen i već postoji sačuvan PDF** → vrati sačuvani fajl sa DiskStation-a,
  - status **Zaključen a nema sačuvanog** → generiši, snimi na DiskStation, upiši putanju u bazu, vrati.
- Pozivalac ne zna za status, putanje na DiskStation-u, ni keširanje. To je sve interna stvar web-a.
- Izlaz je uvek **PDF** (Excel/Word izvozi se ne rade).

---

## 2. Autentikacija

Ne koristi se web cookie-login za API. Umesto toga koristi se **deljeni token u bazi**
(tabela `Korisnik` je zajednička):

1. Pozivalac pri prijavi korisnika upiše token u tabelu `WebPrintToken`; web je samo čita (vidi §5).
2. Svaki API poziv nosi header:
   ```
   X-Print-Token: <guid>
   ```
3. **Web mora da validira token** na svakom pozivu:
   ```sql
   SELECT `KorisnikId`
   FROM   `WebPrintToken`
   WHERE  `Token` = ?
     AND  `Istek` > NOW();      -- lokalno vreme, bez UTC
   ```
   - Nema reda ili istekao → **HTTP 401**.
   - Ima reda → nastavi kao taj `KorisnikId`.
4. **Autorizacija po dokumentu:** ako pronađeni korisnik nema pravo da vidi traženi dokument
   (po postojećim pravilima FruitSysWeb-a) → **HTTP 403**.

Napomena: token je autentikacija (ko je korisnik), ne autorizacija po dokumentu — autorizaciju
sprovodi web na osnovu `KorisnikId`.

---

## 3. Endpoint

### `GET /api/v1/print/{tip}/{id}`

| | |
|---|---|
| **Metoda** | `GET` |
| **Putanja** | `/api/v1/print/{tip}/{id}` |
| **`{tip}`** | slug tipa dokumenta iz registra (§4), npr. `otpremnica` |
| **`{id}`** | celobrojni ID dokumenta u bazi |
| **Obavezan header** | `X-Print-Token: <guid>` |

#### Ponašanje (web logika)
```
validiraj token            → 401 ako nevažeći
proveri prava na dokument  → 403 ako nema
učitaj dokument {tip}/{id} → 404 ako ne postoji

if (status != Zakljucen)                 // DokumentStatusEnum.Zakljucen = 3
    pdf = generiši iz baze
    return 200 pdf,  Cache-Control: no-store
else                                     // status == Zakljucen → snima se na DiskStation
    if (postoji red u DokumentPdf za {tip}/{id})
        pdf = pročitaj sa DokumentPdf.Putanja
    else
        pdf = generiši iz baze
        snimi pdf na DiskStation
        upsert DokumentPdf(TipDok, DokId, Putanja, KorisnikId)
    return 200 pdf
```

#### Odgovori
| Status | Kada | Telo |
|---|---|---|
| `200 OK` | PDF uspešno vraćen | `application/pdf` |
| `401 Unauthorized` | token nevažeći/istekao/nedostaje | `application/problem+json` |
| `403 Forbidden` | korisnik nema pravo na dokument | `application/problem+json` |
| `404 Not Found` | nepoznat `{tip}` ili `{id}` ne postoji | `application/problem+json` |
| `409` / `422` | dokument nije u stanju za štampu (npr. nekompletan) | `application/problem+json` |
| `500` | greška pri generisanju PDF-a | `application/problem+json` |

#### Zaglavlja odgovora (200)
```
Content-Type: application/pdf
Content-Disposition: inline; filename="{tip}-{sifra}.pdf"
Cache-Control: no-store          // za nezaključane (svež svaki put)
```
`{sifra}` = šifra/broj dokumenta iz baze (npr. broj otpremnice). Ako dokument nema šifru, koristiti `{tip}-{id}.pdf`.

#### Format greške (`application/problem+json`, RFC 7807)
```json
{
  "type": "about:blank",
  "title": "Forbidden",
  "status": 403,
  "detail": "Korisnik nema pravo pristupa dokumentu otpremnica/123."
}
```
`detail` se prikazuje korisniku.

### (Opciono, korisno) `GET /api/v1/print/types`
Vraća listu podržanih `{tip}` slug-ova — self-dokumentacija i provera kod pozivaoca.
```json
["otpremnica", "faktura", "prijemnica", "paletni-list", "platni-list", "otkupni-list"]
```

---

## 4. Registar tipova dokumenata

Jedinstveni izvor istine `{tip} → generator`. Dogovoriti tačne slug-ove; predlog:

| `{tip}` | Dokument | Napomena |
|---|---|---|
| `otpremnica` | Otpremnica | prioritet za prvu fazu |
| `faktura` | Faktura / račun | prioritet |
| `platni-list` | Platni list | štampa se iz otvorene prijemnice/otpremnice — prioritet |
| `prijemnica` | Prijemnica | |
| `paletni-list` | Paletni list | |
| `otkupni-list` | Otkupni list | |

Dodavanje novog dokumenta = novi slug u registru + generator na web strani. **Pozivalac se ne menja**
(zove isti endpoint sa novim `{tip}`).

---


## 5. Šema baze

Web koristi dve tabele.

### 5.1 `WebPrintToken` — web samo ČITA
```sql
CREATE TABLE `WebPrintToken` (
  `Id`         BIGINT      NOT NULL AUTO_INCREMENT PRIMARY KEY,
  `KorisnikId` BIGINT      NOT NULL,
  `Token`      VARCHAR(36) NOT NULL,          -- GUID
  `Istek`      DATETIME    NOT NULL,          -- lokalno vreme
  `Kreirano`   DATETIME    NOT NULL,
  `Azurirano`  DATETIME    NOT NULL,
  `Version`    INT         NOT NULL DEFAULT 0,
  CONSTRAINT `UX_WebPrintToken_Korisnik` UNIQUE (`KorisnikId`),   -- jedan token po korisniku
  CONSTRAINT `UX_WebPrintToken_Token`    UNIQUE (`Token`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
```
Web nad ovom tabelom radi samo `SELECT` iz §2. **Ne piše u nju** (popunjava je pozivaoc).

### 5.2 `DokumentPdf` — web piše i čita
```sql
CREATE TABLE `DokumentPdf` (
  `Id`         BIGINT        NOT NULL AUTO_INCREMENT PRIMARY KEY,
  `TipDok`     VARCHAR(32)   NOT NULL,        -- slug tipa: otpremnica, faktura, ...
  `DokId`      BIGINT        NOT NULL,        -- ID dokumenta u bazi
  `Putanja`    VARCHAR(1000) NOT NULL,        -- UNC do DiskStation-a (interno webu)
  `KorisnikId` BIGINT        NULL,            -- ko je generisao (iz tokena)
  `Kreirano`   DATETIME      NOT NULL,
  `Azurirano`  DATETIME      NOT NULL,
  `Version`    INT           NOT NULL DEFAULT 0,
  CONSTRAINT `UX_DokumentPdf_Tip_Dok` UNIQUE (`TipDok`, `DokId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
```
- Web upisuje red kada prvi put snimi finalni (zaključani) PDF.
- `Putanja` se **nikad ne vraća klijentu** — služi samo webu da pročita fajl.

### 5.3 Popunjavanje polja `Kreirano`/`Azurirano`/`Version`
Ove kolone nemaju DB default — web ih postavlja **eksplicitno** u SQL-u:

- **`Kreirano`** — `NOW()` samo pri **INSERT**; kasnije se ne menja.
- **`Azurirano`** — `NOW()` pri **INSERT i pri svakom UPDATE**.
- **`Version`** — `0` pri INSERT; pri UPDATE `Version = Version + 1`.
- Vremena su **lokalna** (`NOW()`), bez UTC konverzije (isto važi za `Istek > NOW()` u §2).

INSERT (snimanje PDF-a; `ON DUPLICATE KEY` pokriva regeneraciju/ponovno snimanje istog dokumenta):
```sql
INSERT INTO `DokumentPdf`
  (`TipDok`, `DokId`, `Putanja`, `KorisnikId`, `Kreirano`, `Azurirano`, `Version`)
VALUES
  (?, ?, ?, ?, NOW(), NOW(), 0)
ON DUPLICATE KEY UPDATE
  `Putanja`   = VALUES(`Putanja`),
  `KorisnikId`= VALUES(`KorisnikId`),
  `Azurirano` = NOW(),
  `Version`   = `Version` + 1;
```

---

## 6. Sigurnost i ne-funkcionalni zahtevi

- Poziv ide preko LAN-a; token je u header-u (ne u URL-u).
- Ne izlagati DiskStation putanju u odgovoru; PDF servisati kroz endpoint (nema path traversal-a).
- Dokument koji nije zaključan je uvek svež — `Cache-Control: no-store`.
- Verzija u putanji (`/api/v1/`) — buduće promene ugovora idu kroz `/api/v2/`.
- Izlaz isključivo PDF.

---

## 7. Kontrolna lista implementacije (web strana)

- [ ] `[ApiController]` sa rutom `GET /api/v1/print/{tip}/{id}` (van Blazor interaktivnog dela).
- [ ] Validacija `X-Print-Token` protiv `WebPrintToken` → 401.
- [ ] Autorizacija po dokumentu za `KorisnikId` → 403.
- [ ] Dispečer `{tip}` → generator (registar iz §4); nepoznat tip/id → 404.
- [ ] Logika po statusu: nije Zaključen → generiši (no-store); Zaključen → sačuvan ili generiši+snimi+upiši.
- [ ] Snimanje PDF-a na DiskStation + upsert `DokumentPdf` (INSERT ... ON DUPLICATE KEY, §5.3).
- [ ] Pri pisanju `DokumentPdf` postaviti audit kolone: `Kreirano=NOW()` (insert), `Azurirano=NOW()` (insert/update), `Version` 0→`+1` (§5.3).
- [ ] `application/problem+json` za sve ne-200.
- [ ] (Opciono) `GET /api/v1/print/types`.
- [ ] `WebPrintToken` se samo čita (web ne piše u nju).

## END
