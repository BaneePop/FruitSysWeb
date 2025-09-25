namespace FruitSysWeb.Services.Interfaces
{
    public interface IExportService
    {
        byte[] ExportToExcel<T>(IEnumerable<T> data);
        byte[] ExportToPdf<T>(IEnumerable<T> data);
        
        // DODATO: Async verzije
        Task<byte[]> ExportToExcel<T>(IEnumerable<T> data, string title);
        Task<byte[]> ExportToPdf<T>(IEnumerable<T> data, string title);
        
        // DODATO: Nove metode
        byte[] ExportToCsv<T>(IEnumerable<T> data);
        bool TestPdfGeneration();
    }
}
