namespace FruitSysWeb.Services.Print
{
    public class PrintApiOptions
    {
        public const string SectionName = "PrintApi";

        /// <summary>Koren za čuvanje zaključenih PDF-ova (lokalna putanja ili UNC).</summary>
        public string StorageRoot { get; set; } = "Data/print-pdf";
    }
}
