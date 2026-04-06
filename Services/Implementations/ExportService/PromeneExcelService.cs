using ClosedXML.Excel;
using FruitSysWeb.Models;
using System.Globalization;

namespace FruitSysWeb.Services.Implementations.ExportService
{
    public class PromeneExcelService
    {
        private static readonly CultureInfo SrFormat = new CultureInfo("sr-Latn-RS");
        private static readonly XLColor HeaderBoja = XLColor.FromArgb(13, 71, 161);
        private static readonly XLColor UlazBoja = XLColor.FromArgb(21, 101, 192);
        private static readonly XLColor IzlazBoja = XLColor.FromArgb(183, 28, 28);
        private static readonly XLColor FinansijeBoja = XLColor.FromArgb(27, 94, 32);
        private static readonly XLColor SubHeaderBoja = XLColor.FromArgb(30, 136, 229);
        private static readonly XLColor AltRed = XLColor.FromArgb(232, 240, 254);

        public byte[] Generisi(PromenPeriodData data, string naslovPerioda, string periodOpis)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Promene");
            ws.PageSetup.PageOrientation = XLPageOrientation.Landscape;
            ws.PageSetup.PaperSize = XLPaperSize.A4Paper;
            ws.PageSetup.FitToPages(1, 0);

            int row = 1;

            // ── Naslov ──────────────────────────────────────────────────
            ws.Cell(row, 1).Value = $"PROMENE — {naslovPerioda.ToUpper()} — ODETTA DOO";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Font.FontSize = 14;
            ws.Range(row, 1, row, 7).Merge();
            row++;

            ws.Cell(row, 1).Value = $"Period: {periodOpis}    Generisano: {DateTime.Now:dd.MM.yyyy HH:mm}";
            ws.Cell(row, 1).Style.Font.FontColor = XLColor.Gray;
            ws.Range(row, 1, row, 7).Merge();
            row += 2;

            // ════════════════════════════════════════
            // BLOK: ULAZ
            // ════════════════════════════════════════
            row = DodajBlokNaslov(ws, row, "ULAZ", UlazBoja);

            decimal ukupnoUlaz = 0;

            // Sirovine i proizvodi
            decimal sumaSirovine = DodajUlazTabelu(ws, ref row, "Sirovine i Proizvodi",
                data.UlazSirovineProizvodi, SubHeaderBoja);
            ukupnoUlaz += sumaSirovine;
            row++;

            // Ambalaza
            decimal sumaAmbalaza = DodajUlazTabelu(ws, ref row, "Ambalaza (nepovratna)",
                data.UlazAmbalaza, SubHeaderBoja);
            ukupnoUlaz += sumaAmbalaza;
            row++;

            // Repromaterijal
            decimal sumaRepro = DodajUlazTabelu(ws, ref row, "Repromaterijal",
                data.UlazRepromaterijal, SubHeaderBoja);
            ukupnoUlaz += sumaRepro;
            row++;

            // Ukupno ulaz
            DodajUkupnoRed(ws, ref row, "UKUPNO ULAZ", ukupnoUlaz, UlazBoja);
            row += 2;

            // ════════════════════════════════════════
            // BLOK: IZLAZ
            // ════════════════════════════════════════
            row = DodajBlokNaslov(ws, row, "IZLAZ", IzlazBoja);

            decimal ukupnoIzlaz = 0;

            decimal sumaGotova = DodajIzlazTabelu(ws, ref row, "Gotova Roba i Sirovine",
                data.IzlazGotovaRobaSirovine, SubHeaderBoja);
            ukupnoIzlaz += sumaGotova;
            row++;

            decimal sumaIzlazAmb = DodajIzlazTabelu(ws, ref row, "Ambalaza",
                data.IzlazAmbalaza, SubHeaderBoja);
            ukupnoIzlaz += sumaIzlazAmb;
            row++;

            decimal sumaIzlazRepro = DodajIzlazTabelu(ws, ref row, "Repromaterijal",
                data.IzlazRepromaterijal, SubHeaderBoja);
            ukupnoIzlaz += sumaIzlazRepro;
            row++;

            DodajUkupnoRed(ws, ref row, "UKUPNO IZLAZ", ukupnoIzlaz, IzlazBoja);
            row += 2;

            // ════════════════════════════════════════
            // BLOK: FINANSIJE
            // ════════════════════════════════════════
            row = DodajBlokNaslov(ws, row, "FINANSIJE", FinansijeBoja);
            DodajFinansijeTabelu(ws, ref row, data.Finansije);
            row += 2;

            // Podesi širine kolona
            ws.Columns().AdjustToContents();
            ws.Column(1).Width = 8;
            ws.Column(2).Width = 12;
            ws.Column(3).Width = 12;
            ws.Column(4).Width = 28;
            ws.Column(5).Width = 28;
            ws.Column(6).Width = 14;
            ws.Column(7).Width = 16;

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        private int DodajBlokNaslov(IXLWorksheet ws, int row, string naziv, XLColor boja)
        {
            var cell = ws.Cell(row, 1);
            cell.Value = $"▶  {naziv}";
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontSize = 12;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Fill.BackgroundColor = boja;
            ws.Range(row, 1, row, 7).Merge();
            ws.Range(row, 1, row, 7).Style.Fill.BackgroundColor = boja;
            return row + 1;
        }

        private decimal DodajUlazTabelu(IXLWorksheet ws, ref int row,
            string podNaziv, List<PromenUlazModel> stavke, XLColor subHeaderBoja)
        {
            // Sub-header
            var subCell = ws.Cell(row, 1);
            subCell.Value = podNaziv;
            subCell.Style.Font.Bold = true;
            subCell.Style.Font.FontColor = XLColor.White;
            subCell.Style.Fill.BackgroundColor = subHeaderBoja;
            ws.Range(row, 1, row, 7).Merge();
            ws.Range(row, 1, row, 7).Style.Fill.BackgroundColor = subHeaderBoja;
            row++;

            // Header kolona
            var headers = new[] { "Rb.", "Dokument", "Datum", "Vrsta Proizvoda", "Dobavljač", "Količina", "Vrednost (RSD)" };
            DodajKoloneHeader(ws, row, headers);
            row++;

            if (!stavke.Any())
            {
                ws.Cell(row, 1).Value = "— nema podataka —";
                ws.Cell(row, 1).Style.Font.FontColor = XLColor.Gray;
                ws.Cell(row, 1).Style.Font.Italic = true;
                ws.Range(row, 1, row, 7).Merge();
                row++;
                return 0;
            }

            decimal suma = 0;
            for (int i = 0; i < stavke.Count; i++)
            {
                var s = stavke[i];
                int r = row + i;
                var bg = i % 2 == 0 ? AltRed : XLColor.White;

                ws.Cell(r, 1).Value = i + 1;
                ws.Cell(r, 2).Value = s.Dokument;
                ws.Cell(r, 3).Value = s.Datum.ToString("dd.MM.yyyy", SrFormat);
                ws.Cell(r, 4).Value = s.VrstaProizvoda;
                ws.Cell(r, 5).Value = s.Dobavljac;
                ws.Cell(r, 6).Value = s.Kolicina;
                ws.Cell(r, 7).Value = s.Vrednost;

                ws.Cell(r, 6).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(r, 7).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(r, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(r, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                for (int c = 1; c <= 7; c++)
                {
                    ws.Cell(r, c).Style.Fill.BackgroundColor = bg;
                    ws.Cell(r, c).Style.Border.OutsideBorder = XLBorderStyleValues.Hair;
                }

                suma += s.Vrednost;
            }
            row += stavke.Count;

            // Suma red
            var sumaCell = ws.Cell(row, 6);
            ws.Cell(row, 5).Value = "Ukupno:";
            ws.Cell(row, 5).Style.Font.Bold = true;
            ws.Cell(row, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            sumaCell.Value = $"Σ {suma:N2}";
            ws.Cell(row, 7).Value = suma;
            ws.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 7).Style.Font.Bold = true;
            ws.Cell(row, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Range(row, 1, row, 7).Style.Fill.BackgroundColor = XLColor.FromArgb(200, 220, 255);
            row++;

            return suma;
        }

        private decimal DodajIzlazTabelu(IXLWorksheet ws, ref int row,
            string podNaziv, List<PromenIzlazModel> stavke, XLColor subHeaderBoja)
        {
            var subCell = ws.Cell(row, 1);
            subCell.Value = podNaziv;
            subCell.Style.Font.Bold = true;
            subCell.Style.Font.FontColor = XLColor.White;
            subCell.Style.Fill.BackgroundColor = XLColor.FromArgb(229, 57, 53);
            ws.Range(row, 1, row, 7).Merge();
            ws.Range(row, 1, row, 7).Style.Fill.BackgroundColor = XLColor.FromArgb(229, 57, 53);
            row++;

            var headers = new[] { "Rb.", "Dokument", "Datum", "Vrsta Proizvoda", "Kupac", "Količina", "Vrednost (RSD)" };
            DodajKoloneHeader(ws, row, headers);
            row++;

            if (!stavke.Any())
            {
                ws.Cell(row, 1).Value = "— nema podataka —";
                ws.Cell(row, 1).Style.Font.FontColor = XLColor.Gray;
                ws.Cell(row, 1).Style.Font.Italic = true;
                ws.Range(row, 1, row, 7).Merge();
                row++;
                return 0;
            }

            decimal suma = 0;
            for (int i = 0; i < stavke.Count; i++)
            {
                var s = stavke[i];
                int r = row + i;
                var bg = i % 2 == 0 ? XLColor.FromArgb(255, 235, 235) : XLColor.White;

                ws.Cell(r, 1).Value = i + 1;
                ws.Cell(r, 2).Value = s.Dokument;
                ws.Cell(r, 3).Value = s.Datum.ToString("dd.MM.yyyy", SrFormat);
                ws.Cell(r, 4).Value = s.VrstaProizvoda;
                ws.Cell(r, 5).Value = s.Kupac;
                ws.Cell(r, 6).Value = s.Kolicina;
                ws.Cell(r, 7).Value = s.Vrednost;

                ws.Cell(r, 6).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(r, 7).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(r, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(r, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                for (int c = 1; c <= 7; c++)
                {
                    ws.Cell(r, c).Style.Fill.BackgroundColor = bg;
                    ws.Cell(r, c).Style.Border.OutsideBorder = XLBorderStyleValues.Hair;
                }

                suma += s.Vrednost;
            }
            row += stavke.Count;

            ws.Cell(row, 5).Value = "Ukupno:";
            ws.Cell(row, 5).Style.Font.Bold = true;
            ws.Cell(row, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Cell(row, 7).Value = suma;
            ws.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 7).Style.Font.Bold = true;
            ws.Cell(row, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Range(row, 1, row, 7).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 205, 210);
            row++;

            return suma;
        }

        private void DodajFinansijeTabelu(IXLWorksheet ws, ref int row, List<PromenFinansijeModel> stavke)
        {
            var headers = new[] { "Rb.", "Dokument", "Datum", "Tip Prometa", "Komitent", "Uplata (RSD)", "Isplata (RSD)" };
            DodajKoloneHeader(ws, row, headers, XLColor.FromArgb(46, 125, 50));
            row++;

            if (!stavke.Any())
            {
                ws.Cell(row, 1).Value = "— nema podataka —";
                ws.Cell(row, 1).Style.Font.FontColor = XLColor.Gray;
                ws.Cell(row, 1).Style.Font.Italic = true;
                ws.Range(row, 1, row, 7).Merge();
                row++;
                return;
            }

            decimal sumaUplata = 0, sumaIsplata = 0;
            for (int i = 0; i < stavke.Count; i++)
            {
                var s = stavke[i];
                int r = row + i;
                var bg = i % 2 == 0 ? XLColor.FromArgb(232, 245, 233) : XLColor.White;

                ws.Cell(r, 1).Value = i + 1;
                ws.Cell(r, 2).Value = s.Dokument;
                ws.Cell(r, 3).Value = s.Datum.ToString("dd.MM.yyyy", SrFormat);
                ws.Cell(r, 4).Value = s.TipPrometa;
                ws.Cell(r, 5).Value = s.Komitent;
                ws.Cell(r, 6).Value = s.Uplata > 0 ? s.Uplata : (decimal?)null;
                ws.Cell(r, 7).Value = s.Isplata > 0 ? s.Isplata : (decimal?)null;

                ws.Cell(r, 6).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(r, 7).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(r, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(r, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                for (int c = 1; c <= 7; c++)
                {
                    ws.Cell(r, c).Style.Fill.BackgroundColor = bg;
                    ws.Cell(r, c).Style.Border.OutsideBorder = XLBorderStyleValues.Hair;
                }

                sumaUplata += s.Uplata;
                sumaIsplata += s.Isplata;
            }
            row += stavke.Count;

            ws.Cell(row, 5).Value = "Ukupno:";
            ws.Cell(row, 5).Style.Font.Bold = true;
            ws.Cell(row, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Cell(row, 6).Value = sumaUplata;
            ws.Cell(row, 7).Value = sumaIsplata;
            ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 6).Style.Font.Bold = true;
            ws.Cell(row, 7).Style.Font.Bold = true;
            ws.Cell(row, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Cell(row, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Range(row, 1, row, 7).Style.Fill.BackgroundColor = XLColor.FromArgb(200, 230, 201);
            row++;
        }

        private void DodajKoloneHeader(IXLWorksheet ws, int row, string[] headers,
            XLColor? boja = null)
        {
            var headerBoja = boja ?? HeaderBoja;
            for (int c = 0; c < headers.Length; c++)
            {
                var cell = ws.Cell(row, c + 1);
                cell.Value = headers[c];
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Fill.BackgroundColor = headerBoja;
                cell.Style.Alignment.Horizontal = c >= 5
                    ? XLAlignmentHorizontalValues.Right
                    : XLAlignmentHorizontalValues.Left;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }
        }

        private void DodajUkupnoRed(IXLWorksheet ws, ref int row, string naziv, decimal vrednost, XLColor boja)
        {
            ws.Cell(row, 1).Value = naziv;
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Font.FontColor = XLColor.White;
            ws.Cell(row, 1).Style.Font.FontSize = 11;
            ws.Cell(row, 7).Value = vrednost;
            ws.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 7).Style.Font.Bold = true;
            ws.Cell(row, 7).Style.Font.FontColor = XLColor.White;
            ws.Cell(row, 7).Style.Font.FontSize = 11;
            ws.Cell(row, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Range(row, 1, row, 7).Merge();
            ws.Range(row, 1, row, 7).Style.Fill.BackgroundColor = boja;
            // Unmerge value cell
            ws.Range(row, 1, row, 6).Merge();
            ws.Cell(row, 7).Value = vrednost;
            ws.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 7).Style.Font.Bold = true;
            ws.Cell(row, 7).Style.Font.FontColor = XLColor.White;
            ws.Cell(row, 7).Style.Font.FontSize = 11;
            ws.Cell(row, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Cell(row, 7).Style.Fill.BackgroundColor = boja;
            row++;
        }
    }
}
