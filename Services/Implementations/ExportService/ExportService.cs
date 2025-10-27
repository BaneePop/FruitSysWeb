using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ClosedXML.Excel;
using FruitSysWeb.Services.Interfaces;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace FruitSysWeb.Services.Implementations.ExportService
{
    public class SimpleExportService : IExportService
    {
        private readonly ILogger<SimpleExportService> _logger;

        // Srpska kultura za formatiranje brojeva (1.000.987,56)
        private static readonly CultureInfo SrpskiFormat = new CultureInfo("sr-Latn-RS");

        // Putanja do logo fajla
        private static readonly string LogoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logo.png");

        public SimpleExportService(ILogger<SimpleExportService> logger)
        {
            _logger = logger;
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
                _logger.LogError(ex, "Greška pri exportu u Excel");
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

                var dataList = data.ToList();
                int rowIndex = 0;

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Landscape());
                        page.Margin(1.5f, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Arial));

                        // HEADER SA LOGOM I ODETTA PODACIMA - Svetlo plavi sa zaobljenim uglovima
                        page.Header()
                            .Padding(10)
                            .Column(col =>
                            {
                                col.Item()
                                    .Background(Colors.Blue.Lighten2) // Svetlo plavi
                                    .Padding(15)
                                    .Row(row =>
                                    {
                                        // Logo (levo)
                                        if (File.Exists(LogoPath))
                                        {
                                            row.ConstantItem(80).Column(logoCol =>
                                            {
                                                logoCol.Item().Image(LogoPath).FitArea();
                                                logoCol.Item().AlignCenter().Text("ODETTA DOO")
                                                    .FontSize(8)
                                                    .Bold()
                                                    .FontColor(Colors.White);
                                            });
                                        }

                                        // Podaci o kompaniji (centar-levo)
                                        row.RelativeItem().PaddingLeft(10).Column(companyCol =>
                                        {
                                            companyCol.Item().Text("ODETTA DOO")
                                                .FontSize(14)
                                                .Bold()
                                                .FontColor(Colors.White);
                                            companyCol.Item().Text("Kralja Dragutina 5, 7/31,")
                                                .FontSize(9)
                                                .FontColor(Colors.White);
                                            companyCol.Item().Text("15000 Šabac, Srbija")
                                                .FontSize(9)
                                                .FontColor(Colors.White);
                                        });

                                        // Info o izveštaju (desno)
                                        row.ConstantItem(140).AlignRight().Column(infoCol =>
                                        {
                                            infoCol.Item().AlignRight().Text($"Datum: {DateTime.Now.ToString("dd.MM.yyyy", SrpskiFormat)}")
                                                .FontSize(9)
                                                .FontColor(Colors.White);
                                            infoCol.Item().AlignRight().Text($"Vreme: {DateTime.Now.ToString("HH:mm", SrpskiFormat)}")
                                                .FontSize(9)
                                                .FontColor(Colors.White);
                                            infoCol.Item().AlignRight().Text($"Ukupno: {dataList.Count} stavki")
                                                .FontSize(10)
                                                .SemiBold()
                                                .FontColor(Colors.Yellow.Darken1);
                                        });
                                    });
                            });

                        // CONTENT
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

                                // MODERNI HEADER - Svetlo plavi sa zaobljenim uglovima
                                table.Header(header =>
                                {
                                    foreach (var property in properties)
                                    {
                                        header.Cell()
                                            .Background(Colors.Blue.Lighten2) // Svetlo plavi
                                            .PaddingVertical(8)
                                            .PaddingHorizontal(10)
                                            .Text(GetDisplayName(property))
                                            .FontSize(10)
                                            .Bold()
                                            .FontColor(Colors.White);
                                    }
                                });

                                // DATA ROWS sa zebra stripama
                                foreach (var item in dataList)
                                {
                                    var isEvenRow = rowIndex % 2 == 0;
                                    var backgroundColor = isEvenRow ? Colors.Grey.Lighten4 : Colors.White;

                                    foreach (var property in properties)
                                    {
                                        var value = property.GetValue(item);
                                        var formattedValue = FormatValue(value);
                                        var isNumber = value is decimal or double or float or int or long;

                                        var cell = table.Cell()
                                            .Background(backgroundColor)
                                            .BorderBottom(0.5f)
                                            .BorderColor(Colors.Grey.Lighten2)
                                            .PaddingVertical(6)
                                            .PaddingHorizontal(10)
                                            .AlignMiddle();

                                        if (isNumber)
                                        {
                                            cell.AlignRight()
                                                .Text(formattedValue)
                                                .FontSize(9)
                                                .FontColor(Colors.Grey.Darken2);
                                        }
                                        else
                                        {
                                            cell.Text(formattedValue)
                                                .FontSize(9)
                                                .FontColor(Colors.Grey.Darken2);
                                        }
                                    }
                                    rowIndex++;
                                }
                            });

                        // FOOTER SA ODETTA PODACIMA
                        page.Footer()
                            .Background(Colors.Grey.Lighten3)
                            .Padding(10)
                            .Row(row =>
                            {
                                row.RelativeItem().AlignLeft().AlignMiddle().Text("ODETTA DOO, Kralja Dragutina 5, 7/31, 15000 Šabac, www.odetta.rs")
                                    .FontSize(8)
                                    .FontColor(Colors.Grey.Darken1);

                                row.ConstantItem(100)
                                    .AlignRight()
                                    .AlignMiddle()
                                    .Text(x =>
                                    {
                                        x.Span("Stranica ").FontSize(9).SemiBold().FontColor(Colors.Blue.Darken2);
                                        x.CurrentPageNumber().FontSize(9).SemiBold().FontColor(Colors.Blue.Darken2);
                                        x.Span(" od ").FontSize(9).SemiBold().FontColor(Colors.Blue.Darken2);
                                        x.TotalPages().FontSize(9).SemiBold().FontColor(Colors.Blue.Darken2);
                                    });
                            });
                    });
                });

                using var stream = new MemoryStream();
                document.GeneratePdf(stream);
                var pdfBytes = stream.ToArray();

                if (pdfBytes.Length < 100)
                {
                    throw new Exception("PDF fajl je prekratak - možda je greška u generisanju");
                }

                if (pdfBytes.Length >= 4 && Encoding.ASCII.GetString(pdfBytes, 0, 4) != "%PDF")
                {
                    throw new Exception("Generisani fajl nije valjan PDF");
                }

                return pdfBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri exportu u PDF");
                _logger.LogInformation($"Stack trace: {ex.StackTrace}");
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
                _logger.LogError(ex, "Greška pri exportu u CSV");
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
                DateTime dt => dt.ToString("dd.MM.yyyy", SrpskiFormat),
                decimal d => d.ToString("N2", SrpskiFormat),
                double d => d.ToString("N2", SrpskiFormat),
                float f => f.ToString("N2", SrpskiFormat),
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
                _logger.LogInformation($"PDF test failed: {ex.Message}");
                return false;
            }
        }

        // DODATO: Async verzije sa title parametrom
        public async Task<byte[]> ExportToExcel<T>(IEnumerable<T> data, string title)
        {
            return await Task.FromResult(ExportToExcel(data));
        }

        public async Task<byte[]> ExportToPdf<T>(IEnumerable<T> data, string title)
        {
            return await Task.FromResult(ExportToPdf(data));
        }

        // NOVO: Export sa custom kolonama - omogućava export samo vidljivih kolona
        // Dictionary key = property name, value = display name
        public byte[] ExportToExcelWithColumns<T>(IEnumerable<T> data, Dictionary<string, string> columns)
        {
            return ExportToExcelWithColumns(data, columns, null);
        }

        // NOVO: Export sa custom kolonama i totalima
        public byte[] ExportToExcelWithColumns<T>(IEnumerable<T> data, Dictionary<string, string> columns, Dictionary<string, object>? totals)
        {
            try
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Export");

                if (data?.Any() == true && columns?.Any() == true)
                {
                    var properties = typeof(T).GetProperties()
                        .Where(p => columns.ContainsKey(p.Name))
                        .ToList();

                    // Headers
                    for (int i = 0; i < properties.Count; i++)
                    {
                        var displayName = columns[properties[i].Name];
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

                    // Totals row (ako postoje totali)
                    if (totals?.Any() == true)
                    {
                        for (int col = 0; col < properties.Count; col++)
                        {
                            var propName = properties[col].Name;
                            if (totals.ContainsKey(propName))
                            {
                                var cell = worksheet.Cell(row, col + 1);
                                cell.Value = FormatValue(totals[propName]);
                                cell.Style.Font.Bold = true;
                                cell.Style.Fill.BackgroundColor = XLColor.LightYellow;
                            }
                        }
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
                _logger.LogError(ex, "Greška pri exportu u Excel sa custom kolonama");
                throw new Exception($"Greška pri kreiranju Excel fajla: {ex.Message}");
            }
        }

        public byte[] ExportToPdfWithColumns<T>(IEnumerable<T> data, Dictionary<string, string> columns)
        {
            return ExportToPdfWithColumns(data, columns, null);
        }

        // NOVO: Export sa custom kolonama i totalima
        public byte[] ExportToPdfWithColumns<T>(IEnumerable<T> data, Dictionary<string, string> columns, Dictionary<string, object>? totals)
        {
            try
            {
                if (data?.Any() != true)
                {
                    throw new ArgumentException("Nema podataka za export");
                }

                if (columns?.Any() != true)
                {
                    throw new ArgumentException("Nisu definisane kolone za export");
                }

                var properties = typeof(T).GetProperties()
                    .Where(p => columns.ContainsKey(p.Name))
                    .ToList();

                var dataList = data.ToList();
                int rowIndex = 0;

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Landscape());
                        page.Margin(1.5f, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Arial));

                        // HEADER SA LOGOM I ODETTA PODACIMA - Svetlo plavi sa zaobljenim uglovima
                        page.Header()
                            .Padding(10)
                            .Column(col =>
                            {
                                col.Item()
                                    .Background(Colors.Blue.Lighten2) // Svetlo plavi
                                    .Padding(15)
                                    .Row(row =>
                                    {
                                        // Logo (levo)
                                        if (File.Exists(LogoPath))
                                        {
                                            row.ConstantItem(80).Column(logoCol =>
                                            {
                                                logoCol.Item().Image(LogoPath).FitArea();
                                                logoCol.Item().AlignCenter().Text("ODETTA DOO")
                                                    .FontSize(8)
                                                    .Bold()
                                                    .FontColor(Colors.White);
                                            });
                                        }

                                        // Podaci o kompaniji (centar-levo)
                                        row.RelativeItem().PaddingLeft(10).Column(companyCol =>
                                        {
                                            companyCol.Item().Text("ODETTA DOO")
                                                .FontSize(14)
                                                .Bold()
                                                .FontColor(Colors.White);
                                            companyCol.Item().Text("Kralja Dragutina 5, 7/31,")
                                                .FontSize(9)
                                                .FontColor(Colors.White);
                                            companyCol.Item().Text("15000 Šabac, Srbija")
                                                .FontSize(9)
                                                .FontColor(Colors.White);
                                        });

                                        // Info o izveštaju (desno)
                                        row.ConstantItem(140).AlignRight().Column(infoCol =>
                                        {
                                            infoCol.Item().AlignRight().Text($"Datum: {DateTime.Now.ToString("dd.MM.yyyy", SrpskiFormat)}")
                                                .FontSize(9)
                                                .FontColor(Colors.White);
                                            infoCol.Item().AlignRight().Text($"Vreme: {DateTime.Now.ToString("HH:mm", SrpskiFormat)}")
                                                .FontSize(9)
                                                .FontColor(Colors.White);
                                            infoCol.Item().AlignRight().Text($"Ukupno: {dataList.Count} stavki")
                                                .FontSize(10)
                                                .SemiBold()
                                                .FontColor(Colors.Yellow.Darken1);
                                        });
                                    });
                            });

                        // CONTENT SA MODERNOM TABELOM
                        page.Content()
                            .PaddingVertical(1, Unit.Centimetre)
                            .Table(table =>
                            {
                                // Define columns
                                table.ColumnsDefinition(cols =>
                                {
                                    for (int i = 0; i < properties.Count; i++)
                                    {
                                        cols.RelativeColumn();
                                    }
                                });

                                // MODERNI HEADER - Svetlo plavi sa zaobljenim uglovima
                                table.Header(header =>
                                {
                                    foreach (var property in properties)
                                    {
                                        var displayName = columns[property.Name];
                                        header.Cell()
                                            .Background(Colors.Blue.Lighten2) // Svetlo plavi
                                            .PaddingVertical(8)
                                            .PaddingHorizontal(10)
                                            .Text(displayName)
                                            .FontSize(10)
                                            .Bold()
                                            .FontColor(Colors.White);
                                    }
                                });

                                // DATA ROWS sa zebra stripama i hover efektom
                                foreach (var item in dataList)
                                {
                                    var isEvenRow = rowIndex % 2 == 0;
                                    var backgroundColor = isEvenRow ? Colors.Grey.Lighten4 : Colors.White;

                                    foreach (var property in properties)
                                    {
                                        var value = property.GetValue(item);
                                        var formattedValue = FormatValue(value);

                                        // Desno poravnanje za brojeve
                                        var isNumber = value is decimal or double or float or int or long;

                                        var cell = table.Cell()
                                            .Background(backgroundColor)
                                            .BorderBottom(0.5f)
                                            .BorderColor(Colors.Grey.Lighten2)
                                            .PaddingVertical(6)
                                            .PaddingHorizontal(10)
                                            .AlignMiddle();

                                        if (isNumber)
                                        {
                                            cell.AlignRight()
                                                .Text(formattedValue)
                                                .FontSize(9)
                                                .FontColor(Colors.Grey.Darken2);
                                        }
                                        else
                                        {
                                            cell.Text(formattedValue)
                                                .FontSize(9)
                                                .FontColor(Colors.Grey.Darken2);
                                        }
                                    }
                                    rowIndex++;
                                }

                                // TOTALS ROW (ako postoje totali)
                                if (totals?.Any() == true)
                                {
                                    foreach (var property in properties)
                                    {
                                        var propName = property.Name;
                                        var hasTotal = totals.ContainsKey(propName);
                                        var value = hasTotal ? totals[propName] : null;
                                        var formattedValue = FormatValue(value);
                                        var isNumber = value is decimal or double or float or int or long;

                                        var cell = table.Cell()
                                            .Background(Colors.Yellow.Lighten3)
                                            .BorderTop(1.5f)
                                            .BorderColor(Colors.Grey.Darken1)
                                            .PaddingVertical(8)
                                            .PaddingHorizontal(10)
                                            .AlignMiddle();

                                        if (isNumber)
                                        {
                                            cell.AlignRight()
                                                .Text(formattedValue)
                                                .FontSize(10)
                                                .Bold()
                                                .FontColor(Colors.Grey.Darken3);
                                        }
                                        else
                                        {
                                            cell.Text(formattedValue)
                                                .FontSize(10)
                                                .Bold()
                                                .FontColor(Colors.Grey.Darken3);
                                        }
                                    }
                                }
                            });

                        // FOOTER SA ODETTA PODACIMA
                        page.Footer()
                            .Background(Colors.Grey.Lighten3)
                            .Padding(10)
                            .Row(row =>
                            {
                                row.RelativeItem().AlignLeft().AlignMiddle().Text("ODETTA DOO, Kralja Dragutina 5, 7/31, 15000 Šabac, www.odetta.rs")
                                    .FontSize(8)
                                    .FontColor(Colors.Grey.Darken1);

                                row.ConstantItem(100)
                                    .AlignRight()
                                    .AlignMiddle()
                                    .Text(x =>
                                    {
                                        x.Span("Stranica ").FontSize(9).SemiBold().FontColor(Colors.Blue.Darken2);
                                        x.CurrentPageNumber().FontSize(9).SemiBold().FontColor(Colors.Blue.Darken2);
                                        x.Span(" od ").FontSize(9).SemiBold().FontColor(Colors.Blue.Darken2);
                                        x.TotalPages().FontSize(9).SemiBold().FontColor(Colors.Blue.Darken2);
                                    });
                            });
                    });
                });

                using var stream = new MemoryStream();
                document.GeneratePdf(stream);
                var pdfBytes = stream.ToArray();

                if (pdfBytes.Length < 100)
                {
                    throw new Exception("PDF fajl je prekratak - možda je greška u generisanju");
                }

                if (pdfBytes.Length >= 4 && Encoding.ASCII.GetString(pdfBytes, 0, 4) != "%PDF")
                {
                    throw new Exception("Generisani fajl nije valjan PDF");
                }

                return pdfBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri exportu u PDF sa custom kolonama");
                _logger.LogInformation($"Stack trace: {ex.StackTrace}");
                throw new Exception($"Greška pri kreiranju PDF fajla: {ex.Message}");
            }
        }
    }
}