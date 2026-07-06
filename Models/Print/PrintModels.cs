namespace FruitSysWeb.Models.Print
{
    public class PrintDocumentMeta
    {
        public long Id { get; set; }
        public string Sifra { get; set; } = string.Empty;
        public int DokumentStatus { get; set; }
    }

    public class DokumentPdfRecord
    {
        public long Id { get; set; }
        public string TipDok { get; set; } = string.Empty;
        public long DokId { get; set; }
        public string Putanja { get; set; } = string.Empty;
        public long? KorisnikId { get; set; }
    }

    public class PrintPdfResult
    {
        public required byte[] PdfBytes { get; init; }
        public required string FileName { get; init; }
        public bool FromCache { get; init; }
        public bool IsFinal { get; init; }
    }

    public class PrintServiceException : Exception
    {
        public int StatusCode { get; }
        public string Title { get; }

        public PrintServiceException(int statusCode, string title, string detail)
            : base(detail)
        {
            StatusCode = statusCode;
            Title = title;
        }
    }
}
