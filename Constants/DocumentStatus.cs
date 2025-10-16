using FruitSysWeb.Services.Core;

namespace FruitSysWeb.Constants
{
    /// <summary>
    /// Centralizovane konstante za statuse dokumenata
    /// SOURCE OF TRUTH za sve document statuse u sistemu
    /// </summary>
    public static class DocumentStatus
    {
        // DOCUMENT STATUS KONSTANTE (1-4)
        public const int KREIRAN = 1;      // Kreiran
        public const int OTVOREN = 2;      // Otvoren (aktivan)
        public const int ZATVOREN = 3;     // Zatvoren/Završen
        public const int STORNO = 4;       // ✅ ISPRAVKA: Storno umesto Odustano

        // DISPLAY NAZIVI - Kako se prikazuju u UI
        public static readonly Dictionary<int, string> DisplayNames = new()
        {
            { KREIRAN, "Kreiran" },
            { OTVOREN, "Otvoren" },
            { ZATVOREN, "Zatvoren" },
            { STORNO, "Storno" }            // ✅ ISPRAVKA: "Storno"
        };

        // BADGE CSS KLASE - Bootstrap badge stilovi
        public static readonly Dictionary<int, string> BadgeClasses = new()
        {
            { KREIRAN, "bg-info text-white" },       // Plava - novi dokument
            { OTVOREN, "bg-success text-white" },    // Zelena - aktivan
            { ZATVOREN, "bg-secondary text-white" }, // Siva - završen
            { STORNO, "bg-danger text-white" }       // ✅ Crvena - storno
        };

        // IKONE - Bootstrap ikone za statuse
        public static readonly Dictionary<int, string> Icons = new()
        {
            { KREIRAN, "bi-file-plus" },      // Novi fajl
            { OTVOREN, "bi-folder2-open" },   // Otvoren folder
            { ZATVOREN, "bi-check-circle" },  // Čekirana
            { STORNO, "bi-x-circle" }         // ✅ X krug za storno
        };

        // DROPDOWN OPCIJE - Za select elementi
        public static readonly List<DropdownOption> DropdownOptions = new()
        {
            new DropdownOption("", "Svi statusi"), // Default opcija
            new DropdownOption(KREIRAN.ToString(), DisplayNames[KREIRAN]),
            new DropdownOption(OTVOREN.ToString(), DisplayNames[OTVOREN]),
            new DropdownOption(ZATVOREN.ToString(), DisplayNames[ZATVOREN]),
            new DropdownOption(STORNO.ToString(), DisplayNames[STORNO])   // ✅ Storno
        };

        // STATUSИ GRUPA - Za filtriranje
        public static readonly Dictionary<string, List<int>> StatusGroups = new()
        {
            { "Aktivni", new List<int> { KREIRAN, OTVOREN } },
            { "Završeni", new List<int> { ZATVOREN, STORNO } },    // ✅ STORNO umesto ODUSTANO
            { "U toku", new List<int> { KREIRAN, OTVOREN } },
            { "Finalni", new List<int> { ZATVOREN, STORNO } }      // ✅ STORNO umesto ODUSTANO
        };

        // HELPER METODE

        /// <summary>
        /// Vraća display naziv za status
        /// </summary>
        public static string GetDisplayName(int status)
        {
            return DisplayNames.TryGetValue(status, out var name) ? name : $"Status {status}";
        }

        /// <summary>
        /// Vraća CSS badge klasu za status
        /// </summary>
        public static string GetBadgeClass(int status)
        {
            return BadgeClasses.TryGetValue(status, out var badgeClass) ? badgeClass : "bg-light text-dark";
        }

        /// <summary>
        /// Vraća ikonu za status
        /// </summary>
        public static string GetIcon(int status)
        {
            return Icons.TryGetValue(status, out var icon) ? icon : "bi-question-circle";
        }

        /// <summary>
        /// Vraća da li je status valjan
        /// </summary>
        public static bool IsValidStatus(int status)
        {
            return DisplayNames.ContainsKey(status);
        }

        /// <summary>
        /// Vraća da li je status aktivan (KREIRAN ili OTVOREN)
        /// </summary>
        public static bool IsActiveStatus(int status)
        {
            return status == KREIRAN || status == OTVOREN;
        }

        /// <summary>
        /// Vraća da li je status završen (ZATVOREN ili STORNO)
        /// </summary>
        public static bool IsClosedStatus(int status)
        {
            return status == ZATVOREN || status == STORNO;    // ✅ STORNO umesto ODUSTANO
        }

        /// <summary>
        /// Vraća sledeći logični status
        /// </summary>
        public static int? GetNextStatus(int currentStatus)
        {
            return currentStatus switch
            {
                KREIRAN => OTVOREN,
                OTVOREN => ZATVOREN,
                ZATVOREN => null,     // Završen - nema sledeći
                STORNO => null,       // ✅ Storno - nema sledeći
                _ => null
            };
        }

        /// <summary>
        /// Vraća prethodni status
        /// </summary>
        public static int? GetPreviousStatus(int currentStatus)
        {
            return currentStatus switch
            {
                KREIRAN => null,      // Prvi status
                OTVOREN => KREIRAN,
                ZATVOREN => OTVOREN,
                STORNO => OTVOREN,    // ✅ Može se storni iz OTVOREN
                _ => null
            };
        }

        /// <summary>
        /// Vraća dropdown opcije samo za aktivne statuse
        /// </summary>
        public static List<DropdownOption> GetActiveStatusDropdown()
        {
            return DropdownOptions.Where(opt =>
                string.IsNullOrEmpty(opt.Value) || // Zadržи "Svi statusi"
                IsActiveStatus(int.Parse(opt.Value))
            ).ToList();
        }

        /// <summary>
        /// Formatira status sa ikonom
        /// </summary>
        public static string FormatStatusWithIcon(int status)
        {
            var icon = GetIcon(status);
            var name = GetDisplayName(status);
            return $"<i class=\"{icon}\"></i> {name}";
        }

        /// <summary>
        /// Vraća sve statuse osim određenih
        /// </summary>
        public static List<DropdownOption> GetFilteredDropdownOptions(params int[] excludeStatuses)
        {
            var excludeSet = new HashSet<int>(excludeStatuses);
            return DropdownOptions.Where(opt =>
                string.IsNullOrEmpty(opt.Value) || // Zadržи "Svi statusi"
                !excludeSet.Contains(int.Parse(opt.Value))
            ).ToList();
        }
    }
}