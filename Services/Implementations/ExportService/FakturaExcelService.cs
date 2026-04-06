using ClosedXML.Excel;
using FruitSysWeb.Models;
using System.Globalization;

namespace FruitSysWeb.Services.Implementations.ExportService
{
    /// <summary>
    /// Generiše Excel exportove za fakture:
    /// - Lista faktura (jedan sheet sa svim filterovanim fakturama)
    /// - Detalji jedne fakture (dva sheeta: header + stavke)
    /// </summary>
    public class FakturaExcelService
    {
        private static readonly CultureInfo SrFormat = new CultureInfo("sr-Latn-RS");

        // ═══════════════════════════════════════════════════════════════
        // Lista faktura — za export iz tabele na stranici
        // ═══════════════════════════════════════════════════════════════

        public byte[] GenerisiListuFaktura(List<FakturaModel> fakture, bool naEngleskom = false)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(naEngleskom ? "Invoices" : "Fakture");

            // Zaglavlje firme
            var naslov = naEngleskom ? "INVOICE LIST — ODETTA DOO" : "LISTA FAKTURA — ODETTA DOO";
            ws.Cell(1, 1).Value = naslov;
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 13;
            ws.Range(1, 1, 1, 9).Merge();

            ws.Cell(2, 1).Value = $"{(naEngleskom ? "Generated" : "Generisano")}: {DateTime.Now:dd.MM.yyyy HH:mm}";
            ws.Cell(2, 1).Style.Font.FontColor = XLColor.Gray;
            ws.Range(2, 1, 2, 9).Merge();

            // Header red
            var headers = naEngleskom
                ? new[] { "No.", "Invoice No", "Date", "Customer", "Net RSD", "VAT RSD", "Total RSD", "Total EUR", "Status" }
                : new[] { "Rb.", "Šifra", "Datum", "Kupac", "Neto RSD", "PDV RSD", "Bruto RSD", "Bruto EUR", "Status" };

            int hr = 4;
            for (int c = 0; c < headers.Length; c++)
            {
                var cell = ws.Cell(hr, c + 1);
                cell.Value = headers[c];
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Fill.BackgroundColor = XLColor.FromArgb(13, 71, 161); // Blue.Darken2
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            // Podaci
            decimal sumNeto = 0, sumPorez = 0, sumBruto = 0, sumEur = 0;
            for (int i = 0; i < fakture.Count; i++)
            {
                var f = fakture[i];
                int row = hr + 1 + i;
                var bg = i % 2 == 0 ? XLColor.FromArgb(232, 240, 254) : XLColor.White;

                ws.Cell(row, 1).Value = i + 1;
                ws.Cell(row, 2).Value = f.Sifra;
                ws.Cell(row, 3).Value = f.Datum.ToString("dd.MM.yyyy", SrFormat);
                ws.Cell(row, 4).Value = f.Komitent ?? "";
                ws.Cell(row, 5).Value = f.Neto;
                ws.Cell(row, 6).Value = f.Porez;
                ws.Cell(row, 7).Value = f.Bruto;
                ws.Cell(row, 8).Value = f.BrutoEur ?? 0;
                ws.Cell(row, 9).Value = f.Status;

                // Formatiranje
                for (int c = 1; c <= 9; c++)
                {
                    var cell = ws.Cell(row, c);
                    cell.Style.Fill.BackgroundColor = bg;
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Hair;
                }

                // Numerički format
                ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, 8).Style.NumberFormat.Format = "#,##0.00";

                ws.Cell(row, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(row, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(row, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(row, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                sumNeto += f.Neto;
                sumPorez += f.Porez;
                sumBruto += f.Bruto;
                sumEur += f.BrutoEur ?? 0;
            }

            // Ukupno red
            int totalRow = hr + 1 + fakture.Count;
            ws.Cell(totalRow, 4).Value = naEngleskom ? "TOTAL:" : "UKUPNO:";
            ws.Cell(totalRow, 4).Style.Font.Bold = true;
            ws.Cell(totalRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Cell(totalRow, 5).Value = sumNeto;
            ws.Cell(totalRow, 6).Value = sumPorez;
            ws.Cell(totalRow, 7).Value = sumBruto;
            ws.Cell(totalRow, 8).Value = sumEur;
            for (int c = 4; c <= 8; c++)
            {
                ws.Cell(totalRow, c).Style.Fill.BackgroundColor = XLColor.FromArgb(197, 217, 241);
                ws.Cell(totalRow, c).Style.Font.Bold = true;
                if (c >= 5) ws.Cell(totalRow, c).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(totalRow, c).Style.Border.TopBorder = XLBorderStyleValues.Medium;
            }

            ws.Columns().AdjustToContents();
            ws.Column(4).Width = Math.Max(ws.Column(4).Width, 35);

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        // ═══════════════════════════════════════════════════════════════
        // Detalji jedne fakture — 2 sheeta: Info + Stavke
        // ═══════════════════════════════════════════════════════════════

        public byte[] GenerisiDetalje(FakturaDetaljiModel model, bool naEngleskom = false)
        {
            using var wb = new XLWorkbook();
            DodajSheetInfo(wb, model, naEngleskom);
            DodajSheetStavke(wb, model, naEngleskom);

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        private void DodajSheetInfo(XLWorkbook wb, FakturaDetaljiModel model, bool en)
        {
            var ws = wb.Worksheets.Add(en ? "Invoice" : "Faktura");
            var naslov = en ? "INVOICE" : "FAKTURA";

            // Naslov
            ws.Cell(1, 1).Value = $"{naslov}: {model.Faktura.Sifra}";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Range(1, 1, 1, 4).Merge();

            ws.Cell(2, 1).Value = "ODETTA DOO | Kralja Dragutina 5, 15000 Šabac";
            ws.Cell(2, 1).Style.Font.FontColor = XLColor.Gray;
            ws.Range(2, 1, 2, 4).Merge();

            // Podaci fakture
            int row = 4;
            InfoRow(ws, ref row, en ? "Invoice No" : "Šifra", model.Faktura.Sifra);
            InfoRow(ws, ref row, en ? "Date" : "Datum", model.Faktura.Datum.ToString("dd.MM.yyyy", SrFormat));
            InfoRow(ws, ref row, en ? "Status" : "Status", model.Faktura.Status);
            InfoRow(ws, ref row, en ? "Currency" : "Valuta", model.ImaEurIznose ? "EUR" : "RSD");
            if (model.ImaEurIznose && model.Faktura.KursEur.HasValue)
                InfoRow(ws, ref row, en ? "Exchange Rate" : "Kurs EUR",
                    $"1 EUR = {model.Faktura.KursEur.Value:N4} RSD");

            row++;
            // Kupac
            if (model.Kupac != null)
            {
                InfoRow(ws, ref row, en ? "Customer" : "Kupac", model.Kupac.Naziv);
                InfoRow(ws, ref row, en ? "Address" : "Adresa", model.Kupac.PunaAdresa);
                if (!string.IsNullOrEmpty(model.Kupac.PoreskiBroj))
                    InfoRow(ws, ref row, en ? "VAT No" : "PIB", model.Kupac.PoreskiBroj);
                if (!string.IsNullOrEmpty(model.Kupac.MaticniBroj))
                    InfoRow(ws, ref row, en ? "Reg. No" : "MB", model.Kupac.MaticniBroj);
            }

            row++;
            if (model.Otpremnica != null)
                InfoRow(ws, ref row, en ? "Delivery Note" : "Otpremnica",
                    $"{model.Otpremnica.Sifra} ({model.Otpremnica.Datum:dd.MM.yyyy})");

            if (model.Ugovor != null)
            {
                InfoRow(ws, ref row, en ? "Contract No" : "Br. ugovora", model.Ugovor.BrojUgovora);
                if (!string.IsNullOrEmpty(model.Ugovor.Paritet))
                    InfoRow(ws, ref row, en ? "Delivery Terms" : "Paritet", model.Ugovor.Paritet);
                if (!string.IsNullOrEmpty(model.Ugovor.Placanje))
                    InfoRow(ws, ref row, en ? "Payment Terms" : "Uslovi plaćanja", model.Ugovor.Placanje);
            }

            row++;
            // Totali
            var totalLabel = en ? "Net Base" : "Neto osnova";
            InfoRow(ws, ref row, totalLabel, $"{model.UkupnoNetoFormatted} RSD");
            if (model.ImaEurIznose)
                ws.Cell(row - 1, 3).Value = $"{model.UkupnoNetoEurFormatted} EUR";

            InfoRow(ws, ref row, en ? $"VAT ({model.PdvStopa:N0}%)" : $"PDV ({model.PdvStopa:N0}%)",
                $"{model.UkupnoPorezFormatted} RSD");

            var totalRow = row;
            InfoRow(ws, ref row, en ? "TOTAL" : "UKUPNO", $"{model.UkupnoBrutoFormatted} RSD");
            if (model.ImaEurIznose)
                ws.Cell(totalRow, 3).Value = $"{model.UkupnoBrutoEurFormatted} EUR";

            ws.Cell(totalRow, 1).Style.Font.Bold = true;
            ws.Cell(totalRow, 2).Style.Font.Bold = true;
            ws.Cell(totalRow, 1).Style.Fill.BackgroundColor = XLColor.FromArgb(197, 217, 241);
            ws.Cell(totalRow, 2).Style.Fill.BackgroundColor = XLColor.FromArgb(197, 217, 241);

            ws.Columns().AdjustToContents();
            ws.Column(2).Width = Math.Max(ws.Column(2).Width, 25);
        }

        private void DodajSheetStavke(XLWorkbook wb, FakturaDetaljiModel model, bool en)
        {
            var ws = wb.Worksheets.Add(en ? "Line Items" : "Stavke");

            var headers = en
                ? new[] { "No.", "Description", "Lot", "Packaging", "Qty (kg)", "Pcs", "Unit Price RSD", "Unit Price EUR", "Net RSD", "Net EUR", "VAT%", "VAT RSD", "Total RSD", "Total EUR" }
                : new[] { "Rb.", "Artikal", "Lot", "Pakovanje", "Kol. (kg)", "Kom.", "Jed. Cena RSD", "Jed. Cena EUR", "Neto RSD", "Neto EUR", "PDV%", "PDV RSD", "Bruto RSD", "Bruto EUR" };

            // Naslov
            ws.Cell(1, 1).Value = $"{(en ? "Invoice" : "Faktura")}: {model.Faktura.Sifra} — {(en ? "Line Items" : "Stavke")}";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 12;
            ws.Range(1, 1, 1, headers.Length).Merge();

            // Header
            for (int c = 0; c < headers.Length; c++)
            {
                var cell = ws.Cell(2, c + 1);
                cell.Value = headers[c];
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Fill.BackgroundColor = XLColor.FromArgb(13, 71, 161);
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Alignment.WrapText = true;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            // Stavke
            for (int i = 0; i < model.Stavke.Count; i++)
            {
                var s = model.Stavke[i];
                int row = 3 + i;
                var bg = i % 2 == 0 ? XLColor.FromArgb(232, 240, 254) : XLColor.White;

                ws.Cell(row, 1).Value = i + 1;
                ws.Cell(row, 2).Value = s.ArtikalNaziv;
                ws.Cell(row, 3).Value = s.Lot ?? "";
                ws.Cell(row, 4).Value = s.Pakovanje ?? "";
                ws.Cell(row, 5).Value = s.Kolicina;
                ws.Cell(row, 6).Value = s.BrojPakovanja;
                ws.Cell(row, 7).Value = s.JedinicnaCena;
                ws.Cell(row, 8).Value = s.JedinicnaCenaEur;
                ws.Cell(row, 9).Value = s.NetoIznos;
                ws.Cell(row, 10).Value = s.NetoIznosEur;
                ws.Cell(row, 11).Value = s.PorezStopa;
                ws.Cell(row, 12).Value = s.PorezIznos;
                ws.Cell(row, 13).Value = s.BrutoIznos;
                ws.Cell(row, 14).Value = s.BrutoIznosEur;

                for (int c = 1; c <= 14; c++)
                {
                    ws.Cell(row, c).Style.Fill.BackgroundColor = bg;
                    ws.Cell(row, c).Style.Border.OutsideBorder = XLBorderStyleValues.Hair;
                }

                // Numerički format
                foreach (var c in new[] { 5, 7, 8, 9, 10, 12, 13, 14 })
                    ws.Cell(row, c).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, 11).Style.NumberFormat.Format = "#,##0.00";
            }

            // Ukupno
            int totalRow = 3 + model.Stavke.Count;
            ws.Cell(totalRow, 4).Value = en ? "TOTAL:" : "UKUPNO:";
            ws.Cell(totalRow, 4).Style.Font.Bold = true;
            ws.Cell(totalRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Cell(totalRow, 9).Value = model.UkupnoNeto;
            ws.Cell(totalRow, 10).Value = model.UkupnoNetoEur;
            ws.Cell(totalRow, 12).Value = model.UkupnoPorez;
            ws.Cell(totalRow, 13).Value = model.UkupnoBruto;
            ws.Cell(totalRow, 14).Value = model.UkupnoBrutoEur;

            foreach (var c in new[] { 9, 10, 12, 13, 14 })
            {
                ws.Cell(totalRow, c).Style.Fill.BackgroundColor = XLColor.FromArgb(197, 217, 241);
                ws.Cell(totalRow, c).Style.Font.Bold = true;
                ws.Cell(totalRow, c).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(totalRow, c).Style.Border.TopBorder = XLBorderStyleValues.Medium;
            }

            ws.Columns().AdjustToContents();
            ws.Column(2).Width = Math.Max(ws.Column(2).Width, 35);
            ws.Row(2).Height = 30;
        }

        private static void InfoRow(IXLWorksheet ws, ref int row, string label, string value)
        {
            ws.Cell(row, 1).Value = label;
            ws.Cell(row, 1).Style.Font.FontColor = XLColor.Gray;
            ws.Cell(row, 2).Value = value;
            row++;
        }
    }
}
