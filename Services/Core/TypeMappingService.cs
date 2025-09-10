using FruitSysWeb.Constants;

namespace FruitSysWeb.Services.Core
{
    public interface ITypeMappingService
    {
        // Magacin mapping
        string GetMagacinBadgeClass(int magacinId);
        string GetMagacinDisplayName(int magacinId);
        List<DropdownOption> GetMagacinDropdownOptions();
        List<QuickFilter> GetMagacinQuickFilters();

        // Document Status mapping
        string GetStatusBadgeClass(int status);
        string GetStatusDisplayName(int status);
        string GetStatusIcon(int status);
        (string displayName, string badgeClass, string icon) GetDocumentStatus(int status);

        // Quantity status
        (string status, string badgeClass, string icon) GetQuantityStatus(decimal kolicina, decimal minimum = 10);

        // Formatting utilities
        string FormatDecimal(decimal value, int decimals = 2);
        string FormatDate(DateTime date);
        string GenerateExportFileName(string prefix, string extension);

        // Generic dropdown builder
        List<DropdownOption> BuildDropdown<T>(IEnumerable<T> items, Func<T, string> valueSelector, Func<T, string> textSelector, string defaultText = "");
    }

    public class TypeMappingService : ITypeMappingService
    {
        /// <summary>
        /// Dobija Bootstrap badge klasu za MagacinID
        /// </summary>
        public string GetMagacinBadgeClass(int magacinId)
        {
            return MagacinTypes.GetBadgeClass(magacinId);
        }

        /// <summary>
        /// Dobija display naziv za MagacinID
        /// </summary>
        public string GetMagacinDisplayName(int magacinId)
        {
            return MagacinTypes.GetDisplayName(magacinId);
        }

        /// <summary>
        /// Kreira dropdown opcije za MagacinID
        /// </summary>
        public List<DropdownOption> GetMagacinDropdownOptions()
        {
            var options = new List<DropdownOption>
            {
                new DropdownOption("", "Svi tipovi")
            };

            foreach (var magacinId in MagacinTypes.ValidIds)
            {
                options.Add(new DropdownOption(
                    magacinId.ToString(),
                    MagacinTypes.GetDisplayName(magacinId)
                ));
            }

            return options;
        }

        /// <summary>
        /// Kreira brze filtere za MagacinID
        /// </summary>
        public List<QuickFilter> GetMagacinQuickFilters()
        {
            return new List<QuickFilter>
            {
                new QuickFilter
                {
                    Label = "Gotove robe",
                    MagacinId = MagacinTypes.GOTOVA_ROBA,
                    Icon = "bi-box-seam",
                    IsActive = false
                },
                new QuickFilter
                {
                    Label = "Sirovine",
                    MagacinId = MagacinTypes.SIROVINE,
                    Icon = "bi-snow",
                    IsActive = false
                },
                new QuickFilter
                {
                    Label = "Ambalaze",
                    MagacinId = MagacinTypes.AMBALAZA,
                    Icon = "bi-box",
                    IsActive = false
                }
            };
        }

        /// <summary>
        /// Dobija Bootstrap badge klasu za DocumentStatus
        /// </summary>
        public string GetStatusBadgeClass(int status)
        {
            return DocumentStatus.GetBadgeClass(status);
        }

        /// <summary>
        /// Dobija display naziv za DocumentStatus
        /// </summary>
        public string GetStatusDisplayName(int status)
        {
            return DocumentStatus.GetDisplayName(status);
        }

        /// <summary>
        /// Dobija ikonu za DocumentStatus
        /// </summary>
        public string GetStatusIcon(int status)
        {
            return DocumentStatus.GetIcon(status);
        }

        /// <summary>
        /// Dobija kompletne informacije o DocumentStatus
        /// </summary>
        public (string displayName, string badgeClass, string icon) GetDocumentStatus(int status)
        {
            return (
                DocumentStatus.GetDisplayName(status),
                DocumentStatus.GetBadgeClass(status),
                DocumentStatus.GetIcon(status)
            );
        }

        /// <summary>
        /// Dobija status količine sa badge class i ikonom
        /// </summary>
        public (string status, string badgeClass, string icon) GetQuantityStatus(decimal kolicina, decimal minimum = 10)
        {
            if (kolicina < minimum)
                return ("Ispod minimuma", "bg-danger", "bi-exclamation-triangle");
            if (kolicina < minimum * 2)
                return ("Ograničeno", "bg-warning", "bi-exclamation-circle");
            return ("Dostupno", "bg-success", "bi-check-circle");
        }

        /// <summary>
        /// Formatira decimal sa određenim brojem decimala
        /// </summary>
        public string FormatDecimal(decimal value, int decimals = 2)
        {
            return value.ToString($"N{decimals}");
        }

        /// <summary>
        /// Formatira datum u srpskom formatu
        /// </summary>
        public string FormatDate(DateTime date)
        {
            return date == DateTime.MinValue ? "" : date.ToString("dd.MM.yyyy");
        }

        /// <summary>
        /// Generiše ime fajla za export sa timestamp
        /// </summary>
        public string GenerateExportFileName(string prefix, string extension)
        {
            return $"{prefix}_{DateTime.Now:yyyyMMdd_HHmmss}{extension}";
        }

        /// <summary>
        /// Generiše dropdown opcije iz bilo koje kolekcije
        /// </summary>
        public List<DropdownOption> BuildDropdown<T>(IEnumerable<T> items, Func<T, string> valueSelector, Func<T, string> textSelector, string defaultText = "")
        {
            var options = new List<DropdownOption>();
            
            if (!string.IsNullOrEmpty(defaultText))
            {
                options.Add(new DropdownOption("", defaultText));
            }

            foreach (var item in items)
            {
                options.Add(new DropdownOption(
                    valueSelector(item),
                    textSelector(item)
                ));
            }

            return options;
        }
    }

    /// <summary>
    /// Dropdown opcija model
    /// </summary>
    public class DropdownOption
    {
        public string Value { get; set; }
        public string Text { get; set; }

        public DropdownOption(string value, string text)
        {
            Value = value;
            Text = text;
        }
    }

    /// <summary>
    /// Quick filter model za brze filtere u UI
    /// </summary>
    public class QuickFilter
    {
        public string Label { get; set; } = "";
        public int MagacinId { get; set; }
        public string Icon { get; set; } = "";
        public bool IsActive { get; set; }
    }
}
