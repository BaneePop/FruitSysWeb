namespace FruitSysWeb.Constants
{
    /// <summary>
    /// MagacinID vrednosti iz Artikal.MagacinID kolone u bazi
    /// NAPOMENA: ID 1 ne postoji, ID 7 (Kalo i Rastur) se ne računa!
    /// </summary>
    public static class MagacinTypes 
    {
        // ❌ ID 1 = Ne postoji
        public const int SVEZA_ROBA = 2;
        public const int SIROVINE = 3;                    // Zamrznuta malina, kupina, šljiva
        public const int AMBALAZA = 4;
        public const int POLUPROIZVODI = 5;               // Odbačena roba prilikom prerade
        public const int GOTOVA_ROBA = 6;
        // ❌ ID 7 = Kalo i Rastur (NE RAČUNAJU SE!)
        public const int POTROSNI_MATERIJAL = 8;          // Uslužni Lager Mlečni
        public const int REPROMATERIJAL = 9;              // Lepljiva traka itd
        public const int DJUBRIVA = 10;
        public const int TRGOVINA = 11;                   // Uslužni Lager Voće i Povrće  
        public const int KLASE = 12;                      // Uslužni Lager Meso

        /// <summary>
        /// Lista svih validnih MagacinID vrednosti (osim 1 i 7)
        /// </summary>
        public static readonly int[] ValidIds = { 2, 3, 4, 5, 6, 8, 9, 10, 11, 12 };

        /// <summary>
        /// MagacinID vrednosti koje se NE računaju u kalkulacije
        /// </summary>
        public static readonly int[] ExcludedIds = { 7 }; // Kalo i Rastur

        /// <summary>
        /// Mapiranje MagacinID -> Display Name
        /// </summary>
        public static readonly Dictionary<int, string> DisplayNames = new()
        {
            { SVEZA_ROBA, "Sveza Roba" },
            { SIROVINE, "Sirovine" },
            { AMBALAZA, "Ambalaza" },
            { POLUPROIZVODI, "PoluProizvodi" },
            { GOTOVA_ROBA, "Gotov Proizvod" },
            { POTROSNI_MATERIJAL, "Usl. Mlečni" },
            { REPROMATERIJAL, "Repromaterijal" },
            { DJUBRIVA, "Đubriva" },
            { TRGOVINA, "Usl. Voće" },
            { KLASE, "Usl. Meso" }
        };

        /// <summary>
        /// Mapiranje MagacinID -> Bootstrap Badge Class
        /// </summary>
        public static readonly Dictionary<int, string> BadgeClasses = new()
        {
            { SVEZA_ROBA, "bg-success" },
            { SIROVINE, "bg-primary" },
            { AMBALAZA, "bg-warning text-dark" },
            { POLUPROIZVODI, "bg-secondary" },
            { GOTOVA_ROBA, "bg-danger" },
            { POTROSNI_MATERIJAL, "bg-info" },
            { REPROMATERIJAL, "bg-light text-dark" },
            { DJUBRIVA, "bg-success text-white" },
            { TRGOVINA, "bg-warning" },
            { KLASE, "bg-danger text-white" }
        };

        /// <summary>
        /// Bootstrap ikone za tipove magacina
        /// </summary>
        public static readonly Dictionary<int, string> Icons = new()
        {
            { SVEZA_ROBA, "bi-leaf" },
            { SIROVINE, "bi-snow" },
            { AMBALAZA, "bi-box" },
            { POLUPROIZVODI, "bi-recycle" },
            { GOTOVA_ROBA, "bi-box-seam" },
            { POTROSNI_MATERIJAL, "bi-cup" },
            { REPROMATERIJAL, "bi-tools" },
            { DJUBRIVA, "bi-flower1" },
            { TRGOVINA, "bi-shop" },
            { KLASE, "bi-basket" }
        };

        /// <summary>
        /// Proverava da li je MagacinID validan
        /// </summary>
        public static bool IsValid(int magacinId) => ValidIds.Contains(magacinId);

        /// <summary>
        /// Proverava da li MagacinID treba da bude isključen iz kalkulacija
        /// </summary>
        public static bool ShouldExclude(int magacinId) => ExcludedIds.Contains(magacinId);

        /// <summary>
        /// Dobija display name za MagacinID
        /// </summary>
        public static string GetDisplayName(int magacinId) => 
            DisplayNames.TryGetValue(magacinId, out var name) ? name : $"MagacinID {magacinId}";

        /// <summary>
        /// Dobija badge class za MagacinID
        /// </summary>
        public static string GetBadgeClass(int magacinId) => 
            BadgeClasses.TryGetValue(magacinId, out var badgeClass) ? badgeClass : "bg-light text-dark";

        /// <summary>
        /// Dobija ikonu za MagacinID
        /// </summary>
        public static string GetIcon(int magacinId) => 
            Icons.TryGetValue(magacinId, out var icon) ? icon : "bi-question-circle";
    }
}
