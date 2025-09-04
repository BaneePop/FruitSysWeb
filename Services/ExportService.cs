using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FruitSysWeb.Services
{
    public class ExportService
    {
        public ExportService()
        {
            // POSTAVI COMMUNITY LICENCU - BESPLATNO ZA OPEN SOURCE
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] ExportToExcel<T>(List<T> data)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Podaci");

            var properties = typeof(T).GetProperties();
            for (int i = 0; i < properties.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = properties[i].Name;
            }

            for (int row = 0; row < data.Count; row++)
            {
                for (int col = 0; col < properties.Length; col++)
                {
                    var value = properties[col].GetValue(data[row]);
                    worksheet.Cell(row + 2, col + 1).Value = value?.ToString();
                }
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public byte[] ExportToPdf<T>(List<T> data)
        {
            // Community licenca je već postavljena u konstruktoru
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    // Header
                    page.Header()
                        .AlignCenter()
                        .Text("Izveštaj - Proizvodnja")
                        .Bold().FontSize(16).FontColor(Colors.Blue.Darken2);

                    page.Content()
                        .PaddingVertical(0.5f, Unit.Centimetre)
                        .Table(table =>
                        {
                            var properties = typeof(T).GetProperties();
                            
                            table.ColumnsDefinition(columns =>
                            {
                                foreach (var prop in properties)
                                {
                                    columns.RelativeColumn();
                                }
                            });

                            // Table header
                            table.Header(header =>
                            {
                                foreach (var prop in properties)
                                {
                                    header.Cell()
                                        .Background(Colors.Grey.Lighten3)
                                        .Padding(5)
                                        .AlignCenter()
                                        .Text(prop.Name)
                                        .Bold();
                                }
                            });

                            // Table content
                            foreach (var item in data)
                            {
                                foreach (var prop in properties)
                                {
                                    var value = prop.GetValue(item)?.ToString() ?? "";
                                    
                                    table.Cell()
                                        .BorderBottom(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .Padding(5)
                                        .Text(value);
                                }
                            }
                        });

                    // Footer
                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Generisano: ");
                            x.Span(DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
                        });
                });
            }).GeneratePdf();
        }
    }
}