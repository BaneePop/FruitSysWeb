# Štampa dokumenata — Plan modula

**Datum:** 4. jul 2026.  
**Projekat:** FruitSysWeb  
**Specifikacija:** `Dokumentacija/1011_web_pdf_api_spec.md` (dogovor sa desktop programerom)  
**Status:** 🟢 API implementiran — spreman za test sa desktop programom

---

## Cilj

FruitSys **desktop** poziva FruitSysWeb preko HTTP API-ja i dobija PDF po tipu i ID-u dokumenta. Web drži celu logiku (generisanje, keš na DiskStation-u, status Zaključen).

```
Desktop  →  GET /api/v1/print/{tip}/{id}  +  X-Print-Token
Web      →  PDF (application/pdf)
```

---

## API — rezime specifikacije

### Endpoint
| | |
|---|---|
| **URL** | `GET /api/v1/print/{tip}/{id}` |
| **Auth** | Header `X-Print-Token: <guid>` (tabela `WebPrintToken`, web samo čita) |
| **Izlaz** | `application/pdf` |
| **Greške** | `application/problem+json` (RFC 7807) |

### Logika po statusu dokumenta
| Status | Ponašanje |
|--------|-----------|
| **Nije Zaključen** (≠ 3) | Generiši svež PDF iz baze, `Cache-Control: no-store`, **ne snimaj** |
| **Zaključen** (3) + postoji u `DokumentPdf` | Vrati sačuvani fajl sa DiskStation-a |
| **Zaključen** (3) + nema zapisa | Generiši → snimi na DiskStation → upsert `DokumentPdf` → vrati |

`DokumentStatusEnum.Zakljucen = 3` — već postoji u `Models/Enums.cs`.

### Registar tipova (`{tip}` slug)
| Slug | Dokument | PDF servis u web-u | Prioritet |
|------|----------|-------------------|-----------|
| `otpremnica` | Otpremnica | `OtpremnicaPdfService` ✅ | **1** |
| `faktura` | Faktura | `FakturaPdfService` ✅ | **1** |
| `platni-list` | Platni list | ❌ **nema još** | **1** |
| `prijemnica` | Prijemnica | `PrijemnicaPdfService` ✅ | 2 |
| `paletni-list` | Paletni list | `PaletniListPdfService` ✅ (+ barkod) | 2 |
| `otkupni-list` | Otkupni list | `OtkupniListPdfService` ✅ | 2 |

Opciono: `GET /api/v1/print/types` → JSON lista slug-ova.

### Baza (web koristi)
- **`WebPrintToken`** — desktop piše, web **samo SELECT** (validacija tokena + `Istek > NOW()`)
- **`DokumentPdf`** — web piše/čita (`TipDok`, `DokId`, `Putanja` UNC DiskStation)

SQL migracije: vidi §5 u `1011_web_pdf_api_spec.md`.

---

## Mapiranje na postojeći kod

| Šta spec traži | Gde u FruitSysWeb |
|----------------|-------------------|
| PDF generatori | `Services/Implementations/ExportService/*PdfService.cs` |
| Učitavanje dokumenata | `IIzvodDokumenataService`, `IzvodDokumenataService` |
| Postojeća UI stranica | `/izvoz-dokumenata` — **ne dira se**, API je paralelan put |
| API kontroleri (primer) | `Controllers/PreradaController.cs` — postoje ali **`MapControllers` nije u `Program.cs`** ⚠️ |
| DI registracija | `Extensions/ServiceCollectionExtensions.cs` |

---

## Kontrolna lista implementacije

- [x] SQL: tabele `WebPrintToken`, `DokumentPdf` (programer)
- [x] Config: `PrintApi:StorageRoot` u `appsettings.json` (`Data/print-pdf` lokalno)
- [x] `Program.cs`: `AddControllers()` + `MapControllers()`
- [x] `PrintApiController` — `GET /api/v1/print/{tip}/{id}`
- [x] `PrintTokenService` — validacija `X-Print-Token`
- [x] `PrintDocumentService` — dispečer tip → generator, logika statusa
- [x] `DokumentPdfStore` — upsert sa `ON DUPLICATE KEY UPDATE`
- [x] Autorizacija — pristup stranici `/izvoz-dokumenata`
- [x] `application/problem+json` za greške
- [x] `GET /api/v1/print/types`
- [ ] **Platni list** — vraća 422 (nije implementiran)
- [ ] Test sa desktop timom
- [ ] Produkcija: UNC putanja DiskStation u config-u

---

## Predložene faze rada

### Faza B — Skelet API-ja
1. `MapControllers` u `Program.cs`
2. Controller + token validacija + problem+json
3. Jedan tip (`otpremnica`) end-to-end bez DiskStation keša

### Faza C — Keš + svi tipovi
1. DiskStation snimanje + `DokumentPdf`
2. Ostali slug-ovi iz registra
3. Platni list generator

### Faza D — Produkcija
1. Deploy, LAN pristup, test sa desktop-om
2. Dozvole po grupi korisnika

---

## Testiranje (Bane / programer)

### Pokretanje
```bash
cd /Users/Bane/FruitSysWeb
dotnet run
```

### URL-ovi
| Okruženje | Baza URL |
|-----------|----------|
| Lokalno (isti računar) | `http://localhost:5073` |
| LAN (drugi PC u mreži) | `http://<IP-ovog-Mac-a>:5073` |
| Produkcija | `https://fruitsys.rs` |

### Primer poziva
```bash
curl -v \
  -H "X-Print-Token: <GUID-iz-WebPrintToken>" \
  "http://localhost:5073/api/v1/print/otpremnica/123" \
  --output test.pdf
```

Lista tipova:
```bash
curl http://localhost:5073/api/v1/print/types
```

Token za test: u tabeli `WebPrintToken` (korisnici `bane` i `programer`, istek 1 mesec).

### Novi fajlovi
```
Controllers/PrintApiController.cs
Services/Print/PrintTokenService.cs
Services/Print/PrintDocumentService.cs
Services/Print/DokumentPdfStore.cs
Services/Print/PrintDocumentRegistry.cs
Models/Print/PrintModels.cs
```

---

## Povezani fajlovi

- `Dokumentacija/1011_web_pdf_api_spec.md` — pun tekst specifikacije
- `Dokumentacija/Solar-Stanje-Razvoja.md` — solar pauziran
- `Components/Pages/IzvodDokumenata.razor`
- `Services/Implementations/ExportService/PaletniListPdfService.cs`
