namespace FruitSysWeb.Constants
{
    /// <summary>
    /// Konstante za rad sa sezonom proizvodnje.
    /// Sezona počinje 01.06. i traje do 30.05. sledeće godine.
    /// </summary>
    public static class SezonaConstants
    {
        /// <summary>
        /// Dan u mesecu kada počinje sezona (1. juni)
        /// </summary>
        public const int PocetakSezoneDan = 1;

        /// <summary>
        /// Mesec kada počinje sezona (juni = 6)
        /// </summary>
        public const int PocetakSezonaMesec = 6;

        /// <summary>
        /// Dan u mesecu kada se završava sezona (30. maj)
        /// </summary>
        public const int KrajSezoneDan = 30;

        /// <summary>
        /// Mesec kada se završava sezona (maj = 5)
        /// </summary>
        public const int KrajSezonaMesec = 5;

        /// <summary>
        /// Vraća datum početka trenutne sezone.
        /// Ako smo posle 01.06, to je 01.06. ove godine.
        /// Ako smo pre 01.06, to je 01.06. prošle godine.
        /// </summary>
        public static DateTime GetPocetakTrenutneSezone()
        {
            var danas = DateTime.Now;

            // Ako smo u junu ili kasnije (juni-decembar), sezona je počela 01.06. ove godine
            if (danas.Month >= PocetakSezonaMesec)
            {
                return new DateTime(danas.Year, PocetakSezonaMesec, PocetakSezoneDan);
            }
            // Ako smo u januaru-maju, sezona je počela 01.06. prošle godine
            else
            {
                return new DateTime(danas.Year - 1, PocetakSezonaMesec, PocetakSezoneDan);
            }
        }

        /// <summary>
        /// Vraća datum kraja trenutne sezone.
        /// Ako smo posle 01.06, to je 30.05. sledeće godine.
        /// Ako smo pre 01.06, to je 30.05. ove godine.
        /// </summary>
        public static DateTime GetKrajTrenutneSezone()
        {
            var danas = DateTime.Now;

            // Ako smo u junu ili kasnije (juni-decembar), kraj sezone je 30.05. sledeće godine
            if (danas.Month >= PocetakSezonaMesec)
            {
                return new DateTime(danas.Year + 1, KrajSezonaMesec, KrajSezoneDan);
            }
            // Ako smo u januaru-maju, kraj sezone je 30.05. ove godine
            else
            {
                return new DateTime(danas.Year, KrajSezonaMesec, KrajSezoneDan);
            }
        }

        /// <summary>
        /// Vraća da li je dati datum u trenutnoj sezoni
        /// </summary>
        public static bool JeUTrenutnojSezoni(DateTime datum)
        {
            var pocetakSezone = GetPocetakTrenutneSezone();
            var krajSezone = GetKrajTrenutneSezone();

            return datum >= pocetakSezone && datum <= krajSezone;
        }

        /// <summary>
        /// Vraća početak sezone za dati datum
        /// </summary>
        public static DateTime GetPocetakSezoneZaDatum(DateTime datum)
        {
            // Ako je datum u junu ili kasnije, sezona je počela 01.06. te godine
            if (datum.Month >= PocetakSezonaMesec)
            {
                return new DateTime(datum.Year, PocetakSezonaMesec, PocetakSezoneDan);
            }
            // Ako je datum u januaru-maju, sezona je počela 01.06. prošle godine
            else
            {
                return new DateTime(datum.Year - 1, PocetakSezonaMesec, PocetakSezoneDan);
            }
        }

        /// <summary>
        /// Vraća kraj sezone za dati datum
        /// </summary>
        public static DateTime GetKrajSezoneZaDatum(DateTime datum)
        {
            // Ako je datum u junu ili kasnije, kraj sezone je 30.05. sledeće godine
            if (datum.Month >= PocetakSezonaMesec)
            {
                return new DateTime(datum.Year + 1, KrajSezonaMesec, KrajSezoneDan);
            }
            // Ako je datum u januaru-maju, kraj sezone je 30.05. te godine
            else
            {
                return new DateTime(datum.Year, KrajSezonaMesec, KrajSezoneDan);
            }
        }

        /// <summary>
        /// Vraća naziv sezone za dati datum (npr. "2024/2025")
        /// </summary>
        public static string GetNazivSezone(DateTime datum)
        {
            var pocetakSezone = GetPocetakSezoneZaDatum(datum);
            var krajSezone = GetKrajSezoneZaDatum(datum);

            return $"{pocetakSezone.Year}/{krajSezone.Year}";
        }

        /// <summary>
        /// Vraća naziv trenutne sezone (npr. "2024/2025")
        /// </summary>
        public static string GetNazivTrenutneSezone()
        {
            return GetNazivSezone(DateTime.Now);
        }

        /// <summary>
        /// Vraća broj dana do kraja sezone
        /// </summary>
        public static int GetBrojDanaDoKrajaSezone()
        {
            var krajSezone = GetKrajTrenutneSezone();
            var danas = DateTime.Now;

            return (int)(krajSezone - danas).TotalDays;
        }

        /// <summary>
        /// Vraća broj dana od početka sezone
        /// </summary>
        public static int GetBrojDanaOdPocetkaSezone()
        {
            var pocetakSezone = GetPocetakTrenutneSezone();
            var danas = DateTime.Now;

            return (int)(danas - pocetakSezone).TotalDays;
        }
    }
}
