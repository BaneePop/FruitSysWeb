namespace FruitSysWeb.Utils
{
    public static class ArtikalHelper
    {
        public static readonly Dictionary<int, string> TipoviArtikala = new()
        {
            { 1, "Sirovina" },
            { 2, "Ambalaza" },
            { 3, "Potrosni materijal" },
            { 4, "Gotova roba" },
            { 5, "Oprema" }
        };

        public static string GetTipNaziv(int tip)
        {
            return TipoviArtikala.TryGetValue(tip, out var naziv) ? naziv : $"Tip {tip}";
        }

        public static string GetTipNaziv(string? tip)
        {
            if (int.TryParse(tip, out int tipInt))
            {
                return GetTipNaziv(tipInt);
            }
            return tip ?? "Nepoznato";
        }

        public static List<(int Value, string Text)> GetAllTypes()
        {
            return TipoviArtikala.Select(kvp => (kvp.Key, kvp.Value)).ToList();
        }

        public static string GetBadgeClass(int tip)
        {
            return tip switch
            {
                1 => "bg-primary",      // Sirovina - plava
                2 => "bg-warning text-dark", // Ambalaza - žuta
                3 => "bg-secondary",    // Potrosni materijal - siva
                4 => "bg-success",      // Gotova roba - zelena
                5 => "bg-danger",       // Oprema - crvena
                _ => "bg-light text-dark"
            };
        }
    }
}
