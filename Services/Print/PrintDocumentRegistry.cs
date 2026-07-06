namespace FruitSysWeb.Services.Print
{
    public static class PrintDocumentRegistry
    {
        public const string Otpremnica = "otpremnica";
        public const string Faktura = "faktura";
        public const string PlatniList = "platni-list";
        public const string Prijemnica = "prijemnica";
        public const string PaletniList = "paletni-list";
        public const string OtkupniList = "otkupni-list";

        private static readonly HashSet<string> Supported = new(StringComparer.OrdinalIgnoreCase)
        {
            Otpremnica, Faktura, PlatniList, Prijemnica, PaletniList, OtkupniList
        };

        public static IReadOnlyList<string> SupportedTypes { get; } = Supported.OrderBy(x => x).ToList();

        public static bool IsSupported(string tip) => Supported.Contains(tip);
    }
}
