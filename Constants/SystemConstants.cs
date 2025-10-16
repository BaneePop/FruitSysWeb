namespace FruitSysWeb.Constants
{
    /// <summary>
    /// Sistemske konstante i podešavanja
    /// SOURCE OF TRUTH za system-wide konfiguraciju
    /// </summary>
    public static class SystemConstants
    {
        // LAGER KONSTANTE
        public static class Lager
        {
            public const decimal DEFAULT_MIN_KOLICINA = 10.0m;
            public const decimal CRITICAL_MIN_KOLICINA = 5.0m;
            public const decimal WARNING_MIN_KOLICINA = 20.0m;

            public const int MAX_ITEMS_PER_PAGE = 100;
            public const int DEFAULT_ITEMS_PER_PAGE = 50;

            // Status indikatori za količine
            public static readonly Dictionary<string, (decimal Threshold, string BadgeClass, string Icon)> QuantityStatus = new()
            {
                { "KRITIČNO", (5.0m, "bg-danger", "bi-exclamation-triangle-fill") },
                { "UPOZORENJE", (20.0m, "bg-warning text-dark", "bi-exclamation-triangle") },
                { "DOSTUPNO", (decimal.MaxValue, "bg-success", "bi-check-circle") }
            };
        }

        // EXPORT KONSTANTE
        public static class Export
        {
            public const int MAX_EXPORT_RECORDS = 10000;
            public const string DEFAULT_DATE_FORMAT = "dd.MM.yyyy";
            public const string DEFAULT_DECIMAL_FORMAT = "N2";

            // File format extensions
            public const string EXCEL_EXTENSION = ".xlsx";
            public const string PDF_EXTENSION = ".pdf";
            public const string CSV_EXTENSION = ".csv";

            // MIME types
            public const string EXCEL_MIME_TYPE = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            public const string PDF_MIME_TYPE = "application/pdf";
            public const string CSV_MIME_TYPE = "text/csv";
        }

        // KOMITENT TIPOVI - centralizovano
        public static readonly Dictionary<string, string> KomitentTipovi = new()
        {
            { "kupac", "Kupac" },
            { "dobavljac", "Dobavljač" },
            { "proizvodjac", "Proizvođač" },
            { "otkupljivac", "Otkupljivač" }
        };

        // SHORTHAND PROPERTIES za često korišćene konstante
        public static string ExcelMimeType => Export.EXCEL_MIME_TYPE;
        public static string PdfMimeType => Export.PDF_MIME_TYPE;

        // UI KONSTANTE
        public static class UI
        {
            public const int TOAST_DURATION_MS = 5000;
            public const int LOADING_DELAY_MS = 500;
            public const int REFRESH_INTERVAL_MS = 30000; // 30 sekundi

            // Progress bar settings
            public const int PROGRESS_ANIMATION_DURATION = 300;

            // Table settings
            public const int MAX_TABLE_ROWS = 200;
            public const int PAGINATION_SIZE = 25;

            // Chart settings
            public const int MAX_CHART_ITEMS = 10;
            public const int DEFAULT_CHART_HEIGHT = 350;

            // Default colors for charts
            public static readonly List<string> ChartColors = new()
            {
                "#28a745", "#dc3545", "#007bff", "#ffc107", "#6f42c1",
                "#20c997", "#fd7e14", "#e83e8c", "#6c757d", "#17a2b8",
                "#ff6384", "#36a2eb", "#ffcd56", "#4bc0c0", "#9966ff"
            };
        }

        // BUSINESS LOGIC KONSTANTE
        public static class Business
        {
            // Datumski opsezi
            public const int DEFAULT_DATE_RANGE_DAYS = 7;
            public const int MAX_DATE_RANGE_DAYS = 365;

            // Finansije
            public const decimal MIN_TRANSACTION_AMOUNT = 0.01m;
            public const decimal MAX_TRANSACTION_AMOUNT = 999999999.99m;

            // Proizvodnja
            public const decimal MIN_PRODUCTION_QUANTITY = 0.001m;
            public const int MAX_WORK_ORDER_DAYS = 365;

            // Validacija
            public const int MIN_NAME_LENGTH = 2;
            public const int MAX_NAME_LENGTH = 255;
            public const int MAX_DESCRIPTION_LENGTH = 1000;
        }

        // DATABASE KONSTANTE
        public static class Database
        {
            public const int COMMAND_TIMEOUT_SECONDS = 30;
            public const int MAX_QUERY_RESULTS = 10000;
            public const string DEFAULT_SORT_ORDER = "DESC";

            // Connection settings
            public const int CONNECTION_POOL_SIZE = 50;
            public const bool USE_CONNECTION_POOLING = true;
        }

        // CACHE KONSTANTE
        public static class Cache
        {
            public const int DEFAULT_CACHE_DURATION_MINUTES = 15;
            public const int DROPDOWN_CACHE_DURATION_MINUTES = 60;
            public const int STATS_CACHE_DURATION_MINUTES = 5;

            // Cache keys
            public const string ARTIKLI_CACHE_KEY = "artikli_all";
            public const string KOMITENTI_CACHE_KEY = "komitenti_all";
            public const string LAGER_STATS_CACHE_KEY = "lager_stats";
        }

        // FORMATTING KONSTANTE
        public static class Formatting
        {
            // Brojevi
            public const string DECIMAL_FORMAT = "N2";
            public const string INTEGER_FORMAT = "N0";
            public const string CURRENCY_FORMAT = "C2";
            public const string PERCENTAGE_FORMAT = "P1";

            // Datumi
            public const string DATE_FORMAT = "dd.MM.yyyy";
            public const string DATETIME_FORMAT = "dd.MM.yyyy HH:mm";
            public const string TIME_FORMAT = "HH:mm";
            public const string MONTH_YEAR_FORMAT = "MM/yyyy";

            // File naming
            public const string EXPORT_TIMESTAMP_FORMAT = "yyyyMMdd_HHmmss";
        }

        // VALIDACIJA REGEX PATERNI
        public static class Validation
        {
            public const string EMAIL_PATTERN = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            public const string PHONE_PATTERN = @"^[\d\s\-\+\(\)]+$";
            public const string POSTAL_CODE_PATTERN = @"^\d{5}$";
            public const string TAX_ID_PATTERN = @"^\d{8,9}$";

            // Numerička validacija
            public const decimal MIN_DECIMAL_VALUE = -999999999.99m;
            public const decimal MAX_DECIMAL_VALUE = 999999999.99m;
        }

        // ERROR MESSAGES
        public static class ErrorMessages
        {
            public const string GENERIC_ERROR = "Došlo je do greške. Pokušajte ponovo.";
            public const string NETWORK_ERROR = "Problem sa mrežom. Proverite internetsku vezu.";
            public const string VALIDATION_ERROR = "Podaci nisu validni. Proverite unos.";
            public const string PERMISSION_ERROR = "Nemate dozvolu za ovu akciju.";
            public const string NOT_FOUND_ERROR = "Traženi podaci nisu pronađeni.";

            public const string EXPORT_TOO_LARGE = "Previše podataka za export. Filtrirajte rezultate.";
            public const string INVALID_DATE_RANGE = "Nevaljan datumski opseg.";
            public const string DUPLICATE_ENTRY = "Ovaj unos već postoji.";
        }

        // SUCCESS MESSAGES
        public static class SuccessMessages
        {
            public const string SAVE_SUCCESS = "Podaci su uspešno sačuvani.";
            public const string DELETE_SUCCESS = "Podaci su uspešno obrisani.";
            public const string EXPORT_SUCCESS = "Export je uspešno kreiran.";
            public const string UPDATE_SUCCESS = "Podaci su uspešno ažurirani.";
            public const string IMPORT_SUCCESS = "Import je uspešno završen.";
        }

        // HELPER METODE

        /// <summary>
        /// Formatira decimal vrednost
        /// </summary>
        public static string FormatDecimal(decimal value)
        {
            return value.ToString(Formatting.DECIMAL_FORMAT);
        }

        /// <summary>
        /// Formatira datum
        /// </summary>
        public static string FormatDate(DateTime date)
        {
            return date.ToString(Formatting.DATE_FORMAT);
        }

        /// <summary>
        /// Generiše timestamp za export fajlove
        /// </summary>
        public static string GenerateExportTimestamp()
        {
            return DateTime.Now.ToString(Formatting.EXPORT_TIMESTAMP_FORMAT);
        }

        /// <summary>
        /// Vraća quantity status info na osnovu količine
        /// </summary>
        public static (string Status, string BadgeClass, string Icon) GetQuantityStatus(decimal quantity)
        {
            foreach (var (status, (threshold, badgeClass, icon)) in Lager.QuantityStatus)
            {
                if (quantity <= threshold)
                {
                    return (status, badgeClass, icon);
                }
            }
            return ("DOSTUPNO", "bg-success", "bi-check-circle");
        }

        /// <summary>
        /// Kreira default datumski opseg (poslednji mesec)
        /// </summary>
        public static (DateTime startDate, DateTime endDate) GetDefaultDateRange()
        {
            var today = DateTime.Today;
            var fromDate = today.AddDays(-Business.DEFAULT_DATE_RANGE_DAYS);
            return (fromDate, today);
        }

        /// <summary>
        /// Validira datumski opseg
        /// </summary>
        public static bool IsValidDateRange(DateTime? odDatum, DateTime? doDatum)
        {
            if (!odDatum.HasValue || !doDatum.HasValue)
                return true; // Null values are OK

            if (odDatum > doDatum)
                return false;

            var daysDiff = (doDatum.Value - odDatum.Value).TotalDays;
            return daysDiff <= Business.MAX_DATE_RANGE_DAYS;
        }

        /// <summary>
        /// Generiše file name za export
        /// </summary>
        public static string GenerateExportFileName(string baseName, string extension)
        {
            var timestamp = GenerateExportTimestamp();
            return $"{baseName}_{timestamp}{extension}";
        }
    }
}
