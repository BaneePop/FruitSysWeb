namespace FruitSysWeb.Models
{
    public class RadnikModel
    {
        public int ID { get; set; }
        public string ImePrezime { get; set; } = string.Empty;
        public bool JePoslovodja { get; set; }
        public bool JeTehnolog { get; set; }
        public bool Aktivan { get; set; } = true;
        public DateTime? DatumZaposlenja { get; set; }
        public DateTime? DatumPrestankaRadnogOdnosa { get; set; }

        // Helper properties
        public string TipRadnika => JePoslovodja ? "Poslovođa" : JeTehnolog ? "Tehnolog" : "Radnik";

        public string StatusBadgeClass => Aktivan ? "bg-success" : "bg-secondary";
    }
}
