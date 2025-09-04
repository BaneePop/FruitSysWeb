namespace FruitSysWeb.Services.Interfaces
{
    public interface IExportService
    {
        byte[] ExportToExcel<T>(List<T> data);
        byte[] ExportToPdf<T>(List<T> data);
    }
}