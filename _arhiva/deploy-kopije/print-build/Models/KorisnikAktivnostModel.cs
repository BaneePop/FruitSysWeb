namespace FruitSysWeb.Models
{
    public class KorisnikAktivnostModel
    {
        public long ID { get; set; }
        public string KorisnikIme { get; set; } = string.Empty;
        public string? IpAdresa { get; set; }
        public DateTime VremeLogina { get; set; }
        public DateTime? VremeLogauta { get; set; }
        public int? TrajanjeSekundi { get; set; }

        public string TrajanjePrikaz => TrajanjeSekundi.HasValue
            ? FormatTrajanje(TrajanjeSekundi.Value)
            : VremeLogauta.HasValue ? FormatTrajanje((int)(VremeLogauta.Value - VremeLogina).TotalSeconds) : "Aktivan";

        private static string FormatTrajanje(int sekundi)
        {
            if (sekundi < 60) return $"{sekundi}s";
            if (sekundi < 3600) return $"{sekundi / 60}m {sekundi % 60}s";
            return $"{sekundi / 3600}h {(sekundi % 3600) / 60}m";
        }
    }

    public class KorisnikAktivnostFilter
    {
        public DateTime? DatumOd { get; set; }
        public DateTime? DatumDo { get; set; }
        public string? KorisnikIme { get; set; }
    }
}
