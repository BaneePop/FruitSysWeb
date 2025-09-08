using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ClosedXML.Excel;
using FruitSysWeb.Services.Interfaces;
using System.Text;

namespace FruitSysWeb.Services.Implementations.ExportService
{
    public class SimpleExportService : IExportService
    {
        public SimpleExportService()
        {
            // Licenciranje za QuestPDF - IMPORTANT FIX
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] ExportToExcel<T>(IEnumerable<T> data)
        {
            try
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Export");

                if (data?.Any() == true)
                {
                    var properties = typeof(T).GetProperties()
                        .Where(p => p.CanRead && IsSimpleType(p.PropertyType))
                        .ToList();

                    // Headers
                    for (int i = 0; i < properties.Count; i++)
                    {
                        var displayName = GetDisplayName(properties[i]);
                        worksheet.Cell(1, i + 1).Value = displayName;
                        worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                        worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                    }

                    // Data
                    int row = 2;
                    foreach (var item in data)
                    {
                        for (int col = 0; col < properties.Count; col++)
                        {
                            var value = properties[col].GetValue(item);
                            worksheet.Cell(row, col + 1).Value = FormatValue(value);
                        }
                        row++;
                    }

                    // Auto-fit columns
                    worksheet.ColumnsUsed().AdjustToContents();
                }

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri exportu u Excel: {ex.Message}");
                throw new Exception($"Greška pri kreiranju Excel fajla: {ex.Message}");
            }
        }

        public byte[] ExportToPdf<T>(IEnumerable<T> data)
        {
            try
            {
                if (data?.Any() != true)
                {
                    throw new ArgumentException("Nema podataka za export");
                }

                var properties = typeof(T).GetProperties()
                    .Where(p => p.CanRead && IsSimpleType(p.PropertyType))
                    .Take(8) // Ograniči na 8 kolona da stane na stranicu
                    .ToList();

                // ISPRAVKA: Proper PDF document creation
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Landscape()); // Landscape za više kolona
                        page.Margin(1, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Arial));

                        page.Header()
                            .Text($"Export podataka - {DateTime.Now:dd.MM.yyyy HH:mm}")
                            .SemiBold().FontSize(14).FontColor(Colors.Blue.Medium);

                        page.Content()
                            .PaddingVertical(1, Unit.Centimetre)
                            .Table(table =>
                            {
                                // Define columns
                                table.ColumnsDefinition(columns =>
                                {
                                    for (int i = 0; i < properties.Count; i++)
                                    {
                                        columns.RelativeColumn();
                                    }
                                });

                                // Header
                                table.Header(header =>
                                {
                                    foreach (var property in properties)
                                    {
                                        header.Cell()
                                            .Element(CellStyle)
                                            .Text(GetDisplayName(property))
                                            .SemiBold()
                                            .FontColor(Colors.White);
                                    }

                                    static IContainer CellStyle(IContainer container)
                                    {
                                        return container
                                            .DefaultTextStyle(x => x.SemiBold())
                                            .PaddingVertical(5)
                                            .PaddingHorizontal(10)
                                            .BorderBottom(1)
                                            .BorderColor(Colors.Black)
                                            .Background(Colors.Blue.Medium);
                                    }
                                });

                                // Data rows
                                foreach (var item in data)
                                {
                                    foreach (var property in properties)
                                    {
                                        var value = property.GetValue(item);
                                        table.Cell()
                                            .Element(CellStyle)
                                            .Text(FormatValue(value))
                                            .FontSize(8);
                                    }

                                    static IContainer CellStyle(IContainer container)
                                    {
                                        return container
                                            .BorderBottom(1)
                                            .BorderColor(Colors.Grey.Lighten2)
                                            .PaddingVertical(3)
                                            .PaddingHorizontal(8);
                                    }
                                }
                            });

                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.Span("Stranica ");
                                x.CurrentPageNumber();
                                x.Span(" od ");
                                x.TotalPages();
                            });
                    });
                });

                // ISPRAVKA: Generate PDF to byte array properly
                using var stream = new MemoryStream();
                document.GeneratePdf(stream);
                var pdfBytes = stream.ToArray();

                // VALIDACIJA: Proveri da li je PDF valjan
                if (pdfBytes.Length < 100) // PDF mora biti veći od 100 bytes
                {
                    throw new Exception("PDF fajl je prekratak - možda je greška u generisanju");
                }

                // Proveri PDF header
                if (pdfBytes.Length >= 4 && Encoding.ASCII.GetString(pdfBytes, 0, 4) != "%PDF")
                {
                    throw new Exception("Generisani fajl nije valjan PDF");
                }

                return pdfBytes;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri exportu u PDF: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw new Exception($"Greška pri kreiranju PDF fajla: {ex.Message}");
            }
        }

        // DODATO: Export CSV kao alternativa
        public byte[] ExportToCsv<T>(IEnumerable<T> data)
        {
            try
            {
                var csv = new StringBuilder();

                if (data?.Any() == true)
                {
                    var properties = typeof(T).GetProperties()
                        .Where(p => p.CanRead && IsSimpleType(p.PropertyType))
                        .ToList();

                    // Headers
                    csv.AppendLine(string.Join(",", properties.Select(p => $"\"{GetDisplayName(p)}\"")));

                    // Data
                    foreach (var item in data)
                    {
                        var values = properties.Select(p =>
                        {
                            var value = p.GetValue(item);
                            var formatted = FormatValue(value);
                            // Escape quotes in CSV
                            return $"\"{formatted?.Replace("\"", "\"\"")}";
                        });
                        csv.AppendLine(string.Join(",", values));
                    }
                }

                return Encoding.UTF8.GetBytes(csv.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri exportu u CSV: {ex.Message}");
                throw new Exception($"Greška pri kreiranju CSV fajla: {ex.Message}");
            }
        }

        private static bool IsSimpleType(Type type)
        {
            var nullableType = Nullable.GetUnderlyingType(type);
            if (nullableType != null)
                type = nullableType;

            return type.IsPrimitive ||
                   type == typeof(string) ||
                   type == typeof(DateTime) ||
                   type == typeof(decimal) ||
                   type == typeof(Guid) ||
                   type.IsEnum;
        }

        private static string GetDisplayName(System.Reflection.PropertyInfo property)
        {
            var displayAttribute = property.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.DisplayAttribute), false)
                .FirstOrDefault() as System.ComponentModel.DataAnnotations.DisplayAttribute;

            return displayAttribute?.Name ?? property.Name;
        }

        private static string FormatValue(object? value)
        {
            if (value == null) return "";

            return value switch
            {
                DateTime dt => dt.ToString("dd.MM.yyyy"),
                decimal d => d.ToString("N2"),
                double d => d.ToString("N2"),
                float f => f.ToString("N2"),
                bool b => b ? "Da" : "Ne",
                _ => value.ToString() ?? ""
            };
        }

        // DODATO: Metoda za testiranje PDF generisanja
        public bool TestPdfGeneration()
        {
            try
            {
                var testData = new[]
                {
                    new { Naziv = "Test 1", Vrednost = 123.45m, Datum = DateTime.Now },
                    new { Naziv = "Test 2", Vrednost = 678.90m, Datum = DateTime.Now.AddDays(-1) }
                };

                var pdf = ExportToPdf(testData);
                return pdf.Length > 100 && Encoding.ASCII.GetString(pdf, 0, 4) == "%PDF";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PDF test failed: {ex.Message}");
                return false;
            }
        }
    }
}