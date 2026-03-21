using ClosedXML.Excel;
using FruitSysWeb.Models.Sledljivost;

namespace FruitSysWeb.Services.Implementations.IzvestajService
{
    public class SledljivostExcelService
    {
        public byte[] GenerisiUpstreamExcel(SledljivostModel sledljivost)
        {
            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Sledljivost - UPSTREAM");
            KreirajTabelu(sheet, sledljivost, "UPSTREAM");
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public byte[] GenerisiDownstreamExcel(SledljivostModel sledljivost)
        {
            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Sledljivost - DOWNSTREAM");
            KreirajTabelu(sheet, sledljivost, "DOWNSTREAM");
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        private void KreirajTabelu(IXLWorksheet sheet, SledljivostModel sledljivost, string tip)
        {
            var rn = sledljivost.RadniNalog;
            var sviPl = sledljivost.PaletniListoviUlaz.Concat(sledljivost.PaletniListoviIzlaz).ToList();

            // ── Info header ──────────────────────────────────────────
            Naslov(sheet, "A1", $"SLEDLJIVOST {tip} — ODETTA DOO", 16, XLColor.FromArgb(26, 54, 93), XLColor.White);
            sheet.Range("A1:J1").Merge();

            if (rn != null)
            {
                sheet.Cell("A2").Value = $"Radni Nalog: {rn.Sifra}";
                sheet.Cell("A2").Style.Font.Bold = true;
                if (!string.IsNullOrEmpty(rn.LotNaloga))
                    sheet.Cell("D2").Value = $"LOT: {rn.LotNaloga}";
                if (!string.IsNullOrEmpty(rn.KomitentNaziv))
                    sheet.Cell("G2").Value = $"Kupac: {rn.KomitentNaziv}";
                if (rn.Datum.HasValue)
                    sheet.Cell("A3").Value = $"Datum RN: {rn.Datum.Value:dd.MM.yyyy}";
                if (rn.Kolicina.HasValue)
                    sheet.Cell("D3").Value = $"Kolicina: {rn.Kolicina.Value:N2} kg";
                if (rn.BrojPakovanja.HasValue)
                    sheet.Cell("G3").Value = $"Br. pakovanja: {rn.BrojPakovanja.Value}";
            }

            sheet.Cell("A4").Value = $"Generisano: {DateTime.Now:dd.MM.yyyy HH:mm}";
            sheet.Cell("A4").Style.Font.Italic = true;
            sheet.Cell("A4").Style.Font.FontColor = XLColor.Gray;

            // ── Zaglavlje tabele ─────────────────────────────────────
            int hRow = 6;
            var cols = new[] { "Faza", "Dokument", "Sifra", "Datum", "Komitent / Dobavljac", "Paletni List", "Artikal", "Tezina (kg)", "Pakovanje", "Veze / Napomene" };
            for (int i = 0; i < cols.Length; i++)
            {
                var cell = sheet.Cell(hRow, i + 1);
                cell.Value = cols[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromArgb(45, 106, 79);
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            int row = hRow + 1;

            // ── FAZA 1: OTPREMNICE ───────────────────────────────────
            var bOtpr = XLColor.FromArgb(209, 250, 229);
            if (sledljivost.Otpremnice.Any())
            {
                foreach (var o in sledljivost.Otpremnice)
                {
                    // Red: Otpremnica
                    ZapisiRed(sheet, row, "1. PRODAJA", "Otpremnica", o.Sifra,
                        o.Datum?.ToString("dd.MM.yyyy"), o.KomitentNaziv,
                        "", "", "", o.Vozilo ?? "", bOtpr, bold: true);
                    row++;

                    // Stavke otpremnice
                    if (o.Stavke.Any())
                    {
                        foreach (var s in o.Stavke)
                        {
                            ZapisiRed(sheet, row, "", "  Stavka", o.Sifra,
                                "", "", "", s.ArtikalNaziv,
                                s.Kolicina.HasValue ? s.Kolicina.Value.ToString("N2") : "", "", bOtpr);
                            row++;
                        }
                    }

                    // PL izlaza — samo sa pakovanjem
                    var plZaOtp = sledljivost.PaletniListoviIzlaz
                        .Where(pl => !string.IsNullOrEmpty(pl.OtpremnicaSifra) && pl.OtpremnicaSifra == o.Sifra
                                     && pl.ArtikalMagacinID == 6 && !string.IsNullOrEmpty(pl.PakovanjeNaziv))
                        .ToList();
                    if (!plZaOtp.Any())
                        plZaOtp = sledljivost.PaletniListoviIzlaz
                            .Where(pl => pl.ArtikalMagacinID == 6 && !string.IsNullOrEmpty(pl.PakovanjeNaziv))
                            .ToList();

                    foreach (var pl in plZaOtp)
                    {
                        var veze = GradiVeze(pl, sviPl);
                        ZapisiRed(sheet, row, "", "  Paletni List", o.Sifra,
                            pl.DatumKreiranja?.ToString("dd.MM.yyyy") ?? "", pl.KomitentNaziv ?? "",
                            pl.Sifra, pl.ArtikalNaziv ?? "", pl.Tezina.HasValue ? pl.Tezina.Value.ToString("N2") : "",
                            veze, bOtpr);
                        sheet.Cell(row, 9).Value = pl.PakovanjeNaziv;
                        row++;
                    }
                }
            }

            // ── FAZA 2: EVIDENCIJE RADA ──────────────────────────────
            var bEvid = XLColor.FromArgb(224, 231, 255);
            if (sledljivost.EvidencijeRada.Any())
            {
                foreach (var e in sledljivost.EvidencijeRada)
                {
                    var evidMeta = $"Smena {e.Smena?.ToString() ?? "—"}{(e.BrojRadnihSati.HasValue ? $" | {e.BrojRadnihSati.Value:N1}h" : "")}{(!string.IsNullOrEmpty(e.SmenskiIzvestajSifra) ? $" | SI: {e.SmenskiIzvestajSifra}" : "")}";
                    ZapisiRed(sheet, row, "2. PROIZVODNJA", "Evidencija Rada", e.Sifra,
                        e.Datum?.ToString("dd.MM.yyyy"), "",
                        "", "", "", evidMeta, bEvid, bold: true);
                    row++;

                    // Utrošeni PL
                    var plZaEv = sledljivost.PaletniListoviUlaz
                        .Where(pl => !string.IsNullOrEmpty(pl.EvidencijaRadaSifra) && pl.EvidencijaRadaSifra == e.Sifra)
                        .ToList();
                    var plKor = sledljivost.PaletniListoviUlaz
                        .Where(pl => pl.KoriscenUEvidencijama?.Contains(e.Sifra) == true && !plZaEv.Contains(pl))
                        .ToList();
                    plZaEv.AddRange(plKor);
                    if (e.UtroseniPaletniListoviIDs?.Any() == true)
                    {
                        var plUtr = sledljivost.PaletniListoviUlaz
                            .Where(pl => e.UtroseniPaletniListoviIDs.Contains(pl.ID) && !plZaEv.Contains(pl))
                            .ToList();
                        plZaEv.AddRange(plUtr);
                    }

                    foreach (var pl in plZaEv)
                    {
                        var veze = GradiVeze(pl, sviPl);
                        ZapisiRed(sheet, row, "", "  Utroseni PL", e.Sifra,
                            pl.DatumKreiranja?.ToString("dd.MM.yyyy") ?? "", pl.KomitentNaziv ?? "",
                            pl.Sifra, pl.ArtikalNaziv ?? "", pl.Tezina.HasValue ? pl.Tezina.Value.ToString("N2") : "",
                            veze, bEvid);
                        row++;
                    }
                }
            }

            // ── FAZA 3: PRIJEMNICE ───────────────────────────────────
            var bPrij = XLColor.FromArgb(254, 235, 200);
            if (sledljivost.Prijemnice.Any())
            {
                foreach (var p in sledljivost.Prijemnice)
                {
                    ZapisiRed(sheet, row, "3. NABAVKA", "Prijemnica", p.Sifra,
                        p.Datum?.ToString("dd.MM.yyyy"), p.KomitentNaziv ?? "",
                        "", "", "", p.Vozilo ?? "", bPrij, bold: true);
                    row++;

                    // Stavke
                    if (p.Stavke.Any())
                    {
                        foreach (var s in p.Stavke)
                        {
                            ZapisiRed(sheet, row, "", "  Stavka", p.Sifra,
                                "", "", "", s.ArtikalNaziv ?? "",
                                s.Kolicina.HasValue ? s.Kolicina.Value.ToString("N2") : "", "", bPrij);
                            row++;
                        }
                    }

                    // PL ulaza (bez gotove robe)
                    var plZaPrij = sledljivost.PaletniListoviUlaz
                        .Where(pl => !string.IsNullOrEmpty(pl.PrijemnicaSifra) && pl.PrijemnicaSifra == p.Sifra
                                     && pl.ArtikalMagacinID != 6)
                        .ToList();

                    foreach (var pl in plZaPrij)
                    {
                        var veze = GradiVeze(pl, sviPl);
                        ZapisiRed(sheet, row, "", "  Paletni List", p.Sifra,
                            pl.DatumKreiranja?.ToString("dd.MM.yyyy") ?? "", pl.KomitentNaziv ?? "",
                            pl.Sifra, pl.ArtikalNaziv ?? "", pl.Tezina.HasValue ? pl.Tezina.Value.ToString("N2") : "",
                            veze, bPrij);
                        row++;
                    }
                }
            }

            // ── Tabela i formatiranje ────────────────────────────────
            if (row > hRow + 1)
            {
                var dataRange = sheet.Range(hRow, 1, row - 1, 10);
                var table = dataRange.CreateTable("SledljivostTabela");
                table.Theme = XLTableTheme.TableStyleMedium2;
                table.ShowAutoFilter = true;
            }

            sheet.ColumnsUsed().AdjustToContents();
            sheet.Column(10).Width = 50; // Veze kolona — može biti duga
            sheet.SheetView.FreezeRows(hRow);
        }

        // ── Helpers ──────────────────────────────────────────────────

        private void ZapisiRed(IXLWorksheet sheet, int row,
            string faza, string tip, string sifra, string datum, string komitent,
            string pl, string artikal, string tezina, string napomena,
            XLColor boja, bool bold = false)
        {
            var vals = new[] { faza, tip, sifra, datum, komitent, pl, artikal, tezina, "", napomena };
            for (int i = 0; i < vals.Length; i++)
            {
                var cell = sheet.Cell(row, i + 1);
                if (!string.IsNullOrEmpty(vals[i]))
                    cell.Value = vals[i];
                cell.Style.Fill.BackgroundColor = boja;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Hair;
                if (bold) cell.Style.Font.Bold = true;
            }
            // Tezina kao broj
            if (decimal.TryParse(tezina, out var tez))
            {
                sheet.Cell(row, 8).Value = tez;
                sheet.Cell(row, 8).Style.NumberFormat.Format = "#,##0.00";
            }
        }

        private string GradiVeze(PaletniListDetalji pl, List<PaletniListDetalji> sviPl)
        {
            var delovi = new List<string>();

            if (!string.IsNullOrEmpty(pl.PrijemnicaSifra))
                delovi.Add($"Prijemnica: {pl.PrijemnicaSifra}");
            if (!string.IsNullOrEmpty(pl.LotDobavljaca))
                delovi.Add($"LOT: {pl.LotDobavljaca}");
            if (!string.IsNullOrEmpty(pl.EvidencijaRadaSifra))
                delovi.Add($"Evidencija: {pl.EvidencijaRadaSifra}");
            if (!string.IsNullOrEmpty(pl.SmenskiIzvestajSifra))
                delovi.Add($"SI: {pl.SmenskiIzvestajSifra}");
            if (!string.IsNullOrEmpty(pl.RadniNalogSifra))
                delovi.Add($"RN: {pl.RadniNalogSifra}");
            if (!string.IsNullOrEmpty(pl.OtpremnicaSifra))
                delovi.Add($"Otpremnica: {pl.OtpremnicaSifra}");

            // Povezani PL — sa artiklom i komitentom
            if (pl.PovezaniPaletniListoviSifre?.Any() == true)
            {
                var povParts = new List<string>();
                foreach (var plSifra in pl.PovezaniPaletniListoviSifre)
                {
                    var povPl = sviPl.FirstOrDefault(x => x.Sifra == plSifra);
                    if (povPl != null)
                        povParts.Add($"{plSifra}{(string.IsNullOrEmpty(povPl.ArtikalNaziv) ? "" : $" ({povPl.ArtikalNaziv}{(string.IsNullOrEmpty(povPl.KomitentNaziv) ? "" : $", {povPl.KomitentNaziv}")})")}");
                    else
                        povParts.Add(plSifra);
                }
                delovi.Add($"Povezani PL: {string.Join("; ", povParts)}");
            }

            return string.Join(" | ", delovi);
        }

        private void Naslov(IXLWorksheet sheet, string adresa, string tekst, int fontSize, XLColor bg, XLColor fg)
        {
            var cell = sheet.Cell(adresa);
            cell.Value = tekst;
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontSize = fontSize;
            cell.Style.Fill.BackgroundColor = bg;
            cell.Style.Font.FontColor = fg;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        }
    }
}
