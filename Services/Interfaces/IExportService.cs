using FruitSysWeb.Models;
using FruitSysWeb.Services.Models.Requests;
using FruitSysWeb.Services.Implementations.IzvestajService;
using FruitSysWeb.Services.Interfaces;


namespace FruitSysWeb.Services.Interfaces

{
    /// <summary>
    /// Interface za export funkcionalnosti u Excel i PDF formate
    /// </summary>
    public interface IExportService
    {
        /// <summary>
        /// Eksportuje listu podataka u Excel format
        /// </summary>
        /// <typeparam name="T">Tip podataka za export</typeparam>
        /// <param name="data">Lista podataka</param>
        /// <returns>Byte array Excel fajla</returns>
        byte[] ExportToExcel<T>(List<T> data);

        /// <summary>
        /// Eksportuje listu podataka u PDF format
        /// </summary>
        /// <typeparam name="T">Tip podataka za export</typeparam>
        /// <param name="data">Lista podataka</param>
        /// <returns>Byte array PDF fajla</returns>
        byte[] ExportToPdf<T>(List<T> data);
    }
    /* 
    public interface IExportService
    {
        byte[] ExportToExcel<T>(List<T> data);
        byte[] ExportToPdf<T>(List<T> data);
    
    } */
}