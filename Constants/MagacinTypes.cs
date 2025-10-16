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
        public const int KALO_I_RASTUR = 7;              // Kalo i Rastur (koristiće se kasnije)
        public const int USL_MLEKO = 8;                  // Uslužni Lager Mlečni
        public const int REPROMATERIJAL = 9;              // Lepljiva traka itd
        public const int DJUBRIVA = 10;
        public const int USL_VOCE = 11;                   // Uslužni Lager Voće i Povrće  
        public const int USL_MESO = 12;                      // Uslužni Lager Meso

        /// <summary>
        /// Lista svih validnih MagacinID vrednosti (osim 1, uključujući 7 za buduće korišćenje)
        /// </summary>
        public static readonly int[] ValidIds = { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };

        /// <summary>
        /// MagacinID vrednosti koje se trenutno NE računaju u kalkulacije
        /// </summary>
        public static readonly int[] ExcludedIds = { 7 }; // Kalo i Rastur (će se koristiti kasnije)

        /// <summary>
        /// Mapiranje MagacinID -> Display Name
        /// </summary>
        public static readonly Dictionary<int, string> DisplayNames = new()
        {
            { SVEZA_ROBA, "Sveza Roba" },
            { SIROVINE, "Sirovine" },
            { AMBALAZA, "Ambalaza" },
            { POLUPROIZVODI, "Polu Proizvod" },           // ✅ ISPRAVKA: "Polu Proizvod"
            { GOTOVA_ROBA, "Gotov Proizvod" },
            { KALO_I_RASTUR, "Kalo i Rastur" },           // ✅ DODANO: ID 7
            { USL_MLEKO, "Usl.Mleko" },                   // ✅ ISPRAVKA: "Usl.Mleko"
            { REPROMATERIJAL, "Repromaterijal" },
            { DJUBRIVA, "Đubriva" },
            { USL_VOCE, "Usl. Voće" },
            { USL_MESO, "Usl. Meso" }
        };

        /// <summary>
        /// Mapiranje MagacinID -> Bootstrap Badge Class
        /// ✅ ISPRAVKE BOJA prema zahtevima
        /// </summary>
        public static readonly Dictionary<int, string> BadgeClasses = new()
        {
            { SVEZA_ROBA, "bg-danger text-white" },       // ✅ CRVENA - Sveza Roba
            { SIROVINE, "bg-primary text-white" },        // Plava - Sirovine
            { AMBALAZA, "bg-warning text-dark" },         // Žuta - Ambalaza
            { POLUPROIZVODI, "bg-secondary text-white" }, // Siva - Polu Proizvod
            { GOTOVA_ROBA, "bg-success text-white" },     // ✅ ZELENA - Gotov Proizvod
            { KALO_I_RASTUR, "bg-dark text-white" },     // Crna - Kalo i Rastur
            { USL_MLEKO, "bg-info text-white" },          // Svetlo plava - Usl.Mleko
            { REPROMATERIJAL, "bg-warning text-dark" },   // ✅ BRAON-ISH - Repromaterijal (koristi warning kao braon)
            { DJUBRIVA, "bg-success text-white" },        // Zelena - Đubriva
            { USL_VOCE, "bg-warning text-dark" },         // Žuta - Usl. Voće
            { USL_MESO, "bg-danger text-white" }          // Crvena - Usl. Meso
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
            { KALO_I_RASTUR, "bi-trash" },               // ✅ DODANO: Ikona za Kalo i Rastur
            { USL_MLEKO, "bi-cup" },
            { REPROMATERIJAL, "bi-tools" },
            { DJUBRIVA, "bi-flower1" },
            { USL_VOCE, "bi-basket" },
            { USL_MESO, "bi-basket3" }
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