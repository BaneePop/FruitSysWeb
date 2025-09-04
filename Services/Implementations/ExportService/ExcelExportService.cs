using ClosedXML.Excel;
using FruitSysWeb.Services.Interfaces;
using System.Reflection;

namespace FruitSysWeb.Services.Implementations.ExportService
{
    public class ExcelExportService : IExportService
    {
        public byte[] ExportToExcel<T>(List<T> data)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Podaci");

            var properties = typeof(T).GetProperties();

            // Add headers
            for (int i = 0; i < properties.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = properties[i].Name;
            }

            // Add data
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
            // Za sada vraćamo prazan niz, PDF implementacija može kasnije
            return new byte[0];
        }
    }
}
/* 
        public byte[] ExportToExcel(List<Dictionary<string, object>> data)
        {
            if (data == null || !data.Any())
                return new byte[0];

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Izveštaj");

            // Add headers
            var columns = data.First().Keys.ToList();
            for (int i = 0; i < columns.Count; i++)
            {
                worksheet.Cell(1, i + 1).Value = columns[i];
            }

            // Add data
            for (int row = 0; row < data.Count; row++)
            {
                for (int col = 0; col < columns.Count; col++)
                {
                    var value = data[row][columns[col]];
                    worksheet.Cell(row + 2, col + 1).Value = value?.ToString();
                }

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                return stream.ToArray();

            }
        }
    } 
} */