namespace FruitSysWeb.Utils
{
    public static class StatusHelper
    {
        public static readonly Dictionary<int, string> DokumentStatusi = new()
        {
            { 1, "Kreiran" },
            { 2, "Otvoren" },   // Prema dokumentu
            { 3, "Zaključen" }, // Prema dokumentu
            { 4, "Odustano" }
        };

        public static string GetStatusNaziv(int status)
        {
            return DokumentStatusi.TryGetValue(status, out var naziv) ? naziv : $"Status {status}";
        }

        public static string GetStatusBadgeClass(int status)
        {
            return status switch
            {
                1 => "bg-info",      // Kreiran - plava
                2 => "bg-success",   // Otvoren - zelena
                3 => "bg-secondary", // Zaključen - siva
                4 => "bg-danger",    // Odustano - crvena
                _ => "bg-light text-dark"
            };
        }
    }
}