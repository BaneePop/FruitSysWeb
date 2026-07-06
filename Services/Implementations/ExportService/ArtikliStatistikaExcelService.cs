using ClosedXML.Excel;
using FruitSysWeb.Models;
using System.Globalization;

namespace FruitSysWeb.Services.Implementations.ExportService
{
    public class ArtikliStatistikaExcelService
    {
        private static readonly CultureInfo SrFormat = new("sr-Latn-RS");
        private static readonly XLColor HeaderBoja = XLColor.FromArgb(13, 71, 161);
        private static readonly XLColor SubHeaderBoja = XLColor.FromArgb(30, 136, 229);
        private static readonly XLColor FooterBoja = XLColor.FromArgb(21, 101, 192);
        private static readonly XLColor AltRed = XLColor.FromArgb(232, 240, 254);

        public byte[] Generisi(List<ArtikliStatistikaGrupa> grupe, string naslovTaba, ArtikliStatistikaFilter filter)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(naslovTaba.Length > 31 ? naslovTaba[..31] : naslovTaba);
            ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
            ws.PageSetup.PaperSize = XLPaperSize.A4Paper;
            ws.PageSetup.FitToPages(1, 0);

            int row = 1;

            // Naslov
            ws.Cell(row, 1).Value = $"{naslovTaba.ToUpper()} — ODETTA DOO";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Font.FontSize = 13;
            ws.Range(row, 1, row, 4).Merge();
            row++;

            ws.Cell(row, 1).Value = $"Period: {filter.OdDatum:dd.MM.yyyy} – {filter.DoDatum:dd.MM.yyyy}    Generisano: {DateTime.Now:dd.MM.yyyy HH:mm}";
            ws.Cell(row, 1).Style.Font.FontColor = XLColor.Gray;
            ws.Range(row, 1, row, 4).Merge();
            row += 2;

            foreach (var grupa in grupe)
            {
                // Naziv grupe
                ws.Cell(row, 1).Value = grupa.NazivGrupe.ToUpper();
                ws.Cell(row, 1).Style.Font.Bold = true;
                ws.Cell(row, 1).Style.Font.FontColor = XLColor.White;
                ws.Cell(row, 1).Style.Font.FontSize = 11;
                ws.Range(row, 1, row, 4).Merge();
                ws.Range(row, 1, row, 4).Style.Fill.BackgroundColor = SubHeaderBoja;
                row++;

                // Header kolona
                var headers = new[] { "Artikal", "Količina (kg)", "Prosečna cena", "Vrednost (RSD)" };
                for (int c = 0; c < headers.Length; c++)
                {
                    var cell = ws.Cell(row, c + 1);
                    cell.Value = headers[c];
                    cell.Style.Font.Bold = true;
                    cell.Style.Font.FontColor = XLColor.White;
                    cell.Style.Fill.BackgroundColor = HeaderBoja;
                    cell.Style.Alignment.Horizontal = c == 0
                        ? XLAlignmentHorizontalValues.Left
                        : XLAlignmentHorizontalValues.Right;
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }
                row++;

                // Stavke
                for (int i = 0; i < grupa.Stavke.Count; i++)
                {
                    var s = grupa.Stavke[i];
                    var bg = i % 2 == 0 ? AltRed : XLColor.White;

                    ws.Cell(row, 1).Value = s.Artikal;
                    ws.Cell(row, 2).Value = s.Kolicina;
                    ws.Cell(row, 3).Value = s.ProsecnaCena;
                    ws.Cell(row, 4).Value = s.Vrednost;

                    for (int c = 1; c <= 4; c++)
                    {
                        ws.Cell(row, c).Style.Fill.BackgroundColor = bg;
                        ws.Cell(row, c).Style.Border.OutsideBorder = XLBorderStyleValues.Hair;
                    }
                    ws.Cell(row, 2).Style.NumberFormat.Format = "#,##0.00";
                    ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
                    ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
                    ws.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Cell(row, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    row++;
                }

                // Suma red
                ws.Cell(row, 1).Value = "UKUPNO";
                ws.Cell(row, 2).Value = grupa.UkupnoKolicina;
                ws.Cell(row, 3).Value = grupa.ProsecnaCenaUkupno;
                ws.Cell(row, 4).Value = grupa.UkupnoVrednost;

                for (int c = 1; c <= 4; c++)
                {
                    ws.Cell(row, c).Style.Font.Bold = true;
                    ws.Cell(row, c).Style.Font.FontColor = XLColor.White;
                    ws.Cell(row, c).Style.Fill.BackgroundColor = FooterBoja;
                    ws.Cell(row, c).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }
                ws.Cell(row, 2).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(row, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                row += 2;
            }

            // Širine kolona
            ws.Column(1).Width = 40;
            ws.Column(2).Width = 16;
            ws.Column(3).Width = 16;
            ws.Column(4).Width = 18;

            using var stream = new MemoryStream();
            wb.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
