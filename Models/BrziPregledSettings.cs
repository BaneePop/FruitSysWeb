namespace FruitSysWeb.Models
{
    public class BrziPregledSettings
    {
        // Filter postavke
        public DateTime? OdDatum { get; set; }
        public DateTime? DoDatum { get; set; }
        public long? KomitentId { get; set; }
        public long? ArtikalId { get; set; }
        public int? MagacinId { get; set; }
        public int? DokumentStatus { get; set; }
        public string? RadniNalog { get; set; }
        public string? Tip { get; set; }
        public string? Pakovanje { get; set; }
        
        // Boolean filteri
        public bool SamoGotoveRobe { get; set; }
        public bool SamoSirovine { get; set; }
        public bool SamoAmbalaze { get; set; }
        
        // UI postavke
        public string ActiveTab { get; set; } = "proizvodnja";
        
        // Timestamp poslednje izmene
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}