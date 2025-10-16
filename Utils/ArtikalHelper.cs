using System.ComponentModel;

namespace FruitSysWeb.Utils
{
    /// <summary>
    /// DEPRECATED: This helper class is being phased out in favor of centralized TypeMappingService.
    /// Use FruitSysWeb.Services.Core.ITypeMappingService instead for new development.
    /// </summary>
    [Obsolete("Use FruitSysWeb.Services.Core.ITypeMappingService instead. This will be removed in future versions.")]
    public static class ArtikalHelper
    {
        /// <summary>
        /// DEPRECATED: Use MagacinTypes.DisplayNames instead
        /// </summary>
        [Obsolete("Use MagacinTypes.DisplayNames from Constants instead")]
        public static readonly Dictionary<int, string> TipoviArtikala = new()
        {
            { 1, "Ne Postoji" },
            { 2, "Sveza Roba" },
            { 3, "Sirovine" },
            { 4, "Ambalaza" },
            { 5, "PoluProizvodi" },
            { 6, "Gotovi Proizvodi" },
            { 7, "Kalo i Rastur" },
            { 8, "Uluzni Lager Mleko" },
            { 9, "Repromaterijal" },
            { 10, "Djubriva" },
            { 11, "Uluzni Lager Voce" },
            { 12, "Usluzni Lager Meso" }

        };

        /// <summary>
        /// DEPRECATED: Use TypeMappingService.GetMagacinDisplayName(int magacinId) instead
        /// </summary>
        [Obsolete("Use ITypeMappingService.GetMagacinDisplayName() instead")]
        public static string GetTipNaziv(int tip)
        {
            return TipoviArtikala.TryGetValue(tip, out var naziv) ? naziv : $"Tip {tip}";
        }

        /// <summary>
        /// DEPRECATED: Use TypeMappingService.GetMagacinDisplayName() instead
        /// </summary>
        [Obsolete("Use ITypeMappingService.GetMagacinDisplayName() instead")]
        public static string GetTipNaziv(string? tip)
        {
            if (int.TryParse(tip, out int tipInt))
            {
                return GetTipNaziv(tipInt);
            }
            return tip ?? "Nepoznato";
        }

        /// <summary>
        /// DEPRECATED: Use TypeMappingService.GetMagacinDropdownOptions() instead
        /// </summary>
        [Obsolete("Use ITypeMappingService.GetMagacinDropdownOptions() instead")]
        public static List<(int Value, string Text)> GetAllTypes()
        {
            return TipoviArtikala.Select(kvp => (kvp.Key, kvp.Value)).ToList();
        }

        /// <summary>
        /// DEPRECATED: Use TypeMappingService.GetMagacinBadgeClass(int magacinId) instead
        /// </summary>
        [Obsolete("Use ITypeMappingService.GetMagacinBadgeClass() instead")]
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
