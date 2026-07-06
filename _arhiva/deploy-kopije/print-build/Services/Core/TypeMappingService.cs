using FruitSysWeb.Constants;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

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
        List<DropdownOption> GetStatusDropdownOptions();
        (string displayName, string badgeClass, string icon) GetDocumentStatus(int status);

        // Quantity status
        (string status, string badgeClass, string icon) GetQuantityStatus(decimal kolicina, decimal minimum = 10);
        string GetQuantityStatusClass(string status);

        // Formatting utilities - ✅ IZMENJENI SRPSKI FORMATI
        string FormatDecimal(decimal value, int decimals = 2);
        string FormatInteger(int value);
        string FormatDate(DateTime date);
        string FormatDate(DateTime? date);
        string FormatDateTime(DateTime dateTime);
        string FormatCurrency(decimal value);              // ✅ SRPSKI FORMAT
        string FormatWeight(decimal value);                // ✅ NOVI - kg format
        string GenerateExportFileName(string prefix, string extension);

        // UI Helpers
        string GetSaldoBadgeClass(decimal saldo);
        MarkupString BuildDropdown(List<DropdownOption> options, string defaultText = "", string? selectedValue = null);
        List<DropdownOption> GetFilteredMagacinOptions(params int[] excludeIds);

        // Generic dropdown builder
        List<DropdownOption> BuildDropdown<T>(IEnumerable<T> items, Func<T, string> valueSelector, Func<T, string> textSelector, string defaultText = "");

        // Additional methods needed by components
        bool IsActiveStatus(int status);
        List<string> GetChartColors(int count);
    }

    public class TypeMappingService : ITypeMappingService
    {
        // ✅ SRPSKA KULTURA za formatiranje brojeva
        private static readonly CultureInfo SrpskaCultura = new CultureInfo("sr-Latn-RS");

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
                // ✅ Ne prikazuj Kalo i Rastur u dropdown-u (ID 7)
                if (magacinId == MagacinTypes.KALO_I_RASTUR) continue;

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
                return ("Ispod minimuma", "bg-warning", "bi-exclamation-triangle");
            if (kolicina < minimum * 2)
                return ("Ograničeno", "bg-warning", "bi-exclamation-circle");
            return ("Dostupno", "bg-success", "bi-check-circle");
        }

        /// <summary>
        /// ✅ FORMATIRA DECIMAL SA SRPSKIM FORMATOM (zarez za decimale, tačka za hiljade)
        /// Format: 10.456,90
        /// </summary>
        public string FormatDecimal(decimal value, int decimals = 2)
        {
            return value.ToString($"N{decimals}", SrpskaCultura);
        }

        /// <summary>
        /// ✅ FORMATIRA DATUM U SRPSKOM FORMATU: 19.09.2025
        /// </summary>
        public string FormatDate(DateTime date)
        {
            return date == DateTime.MinValue ? "" : date.ToString("dd.MM.yyyy", SrpskaCultura);
        }

        /// <summary>
        /// ✅ FORMATIRA DATUM SA VREMENOM: 19.09.2025 14:30:15
        /// </summary>
        public string FormatDateTime(DateTime dateTime)
        {
            return dateTime.ToString("dd.MM.yyyy HH:mm:ss", SrpskaCultura);
        }

        /// <summary>
        /// ✅ FORMATIRA OPCIONI DATUM
        /// </summary>
        public string FormatDate(DateTime? date)
        {
            return date?.ToString("dd.MM.yyyy", SrpskaCultura) ?? "";
        }

        /// <summary>
        /// ✅ FORMATIRA INTEGER SA SRPSKIM FORMATOM
        /// </summary>
        public string FormatInteger(int value)
        {
            return value.ToString("N0", SrpskaCultura);
        }

        /// <summary>
        /// ✅ FORMATIRA NOVČANE VREDNOSTI: 1.245.455,88 RSD
        /// </summary>
        public string FormatCurrency(decimal value)
        {
            return $"{value.ToString("N2", SrpskaCultura)} RSD";
        }

        /// <summary>
        /// ✅ NOVI - FORMATIRA TEŽINU: 10.456,90 kg
        /// </summary>
        public string FormatWeight(decimal value)
        {
            return $"{value.ToString("N2", SrpskaCultura)} kg";
        }

        /// <summary>
        /// Dobija status dropdown opcije
        /// </summary>
        public List<DropdownOption> GetStatusDropdownOptions()
        {
            return DocumentStatus.DropdownOptions;
        }

        /// <summary>
        /// Dobija CSS klasu za količinu status
        /// </summary>
        public string GetQuantityStatusClass(string status)
        {
            return status.ToLower() switch
            {
                "positive" => "text-success",
                "negative" => "text-danger",
                "warning" => "text-warning",
                "info" => "text-info",
                _ => "text-muted"
            };
        }

        /// <summary>
        /// Dobija saldo badge CSS klasu
        /// </summary>
        public string GetSaldoBadgeClass(decimal saldo)
        {
            if (saldo > 0) return "bg-success";
            if (saldo < 0) return "";
            return "bg-secondary";
        }

        /// <summary>
        /// Kreira HTML dropdown sa opcijama
        /// </summary>
        public MarkupString BuildDropdown(List<DropdownOption> options, string defaultText = "", string? selectedValue = null)
        {
            var html = "";

            if (!string.IsNullOrEmpty(defaultText))
            {
                html += $"<option value=\"\">{defaultText}</option>";
            }

            foreach (var option in options)
            {
                var selected = option.Value == selectedValue ? "selected" : "";
                html += $"<option value=\"{option.Value}\" {selected}>{option.Text}</option>";
            }

            return new MarkupString(html);
        }

        /// <summary>
        /// Dobija filtrirane magacin opcije
        /// </summary>
        public List<DropdownOption> GetFilteredMagacinOptions(params int[] excludeIds)
        {
            var allOptions = GetMagacinDropdownOptions();
            return allOptions.Where(o => !excludeIds.Contains(int.TryParse(o.Value, out int id) ? id : 0)).ToList();
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

        /// <summary>
        /// Proverava da li je status aktivan
        /// </summary>
        public bool IsActiveStatus(int status)
        {
            return DocumentStatus.IsActiveStatus(status);
        }

        /// <summary>
        /// Vraća chart boje za grafike - DARK THEME OPTIMIZOVANO
        /// Boje su usklađene sa modernom tamnom temom
        /// </summary>
        public List<string> GetChartColors(int count)
        {
            var colors = new List<string>
            {
                "#60a5fa",  // Plava (Chart primary)
                "#f87171",  // Crvena (Chart danger)
                "#fb923c",  // Narandžasta (Chart warning)
                "#2dd4bf",  // Tirkiz (Chart info)
                "#a78bfa",  // Ljubičasta (Chart purple)
                "#34d399",  // Zelena (Chart success)
                "#fbbf24",  // Žuta (Chart yellow)
                "#f472b6",  // Pink (Chart pink)
                "#93c5fd",  // Svetlo plava
                "#fca5a5"   // Svetlo crvena
            };

            var result = new List<string>();
            for (int i = 0; i < count; i++)
            {
                result.Add(colors[i % colors.Count]);
            }

            return result;
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