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

        // NOVO: Metode sa custom kolonama - omogućava export samo vidljivih kolona
        byte[] ExportToExcelWithColumns<T>(IEnumerable<T> data, Dictionary<string, string> columns);
        byte[] ExportToPdfWithColumns<T>(IEnumerable<T> data, Dictionary<string, string> columns);

        // NOVO: Metode sa custom kolonama i totalima
        byte[] ExportToExcelWithColumns<T>(IEnumerable<T> data, Dictionary<string, string> columns, Dictionary<string, object>? totals);
        byte[] ExportToPdfWithColumns<T>(IEnumerable<T> data, Dictionary<string, string> columns, Dictionary<string, object>? totals);
    }
}
