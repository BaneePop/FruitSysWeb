namespace FruitSysWeb.Services.Interfaces
{
    public interface IExportService
    {
        byte[] ExportToExcel<T>(IEnumerable<T> data);
        byte[] ExportToPdf<T>(IEnumerable<T> data);
        
        // DODATO: Nove metode
        byte[] ExportToCsv<T>(IEnumerable<T> data);
        bool TestPdfGeneration();
    }
}
