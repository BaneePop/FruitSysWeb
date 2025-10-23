namespace FruitSysWeb.Models
{
    /// <summary>
    /// Helper klasa za rad sa pristupom korisnika
    /// VAŽNO: Pristup se određuje na osnovu IMENA korisnika, NE na osnovu GrupaKorisnikaID
    /// (jer se GrupaKorisnikaID menja u bazi)
    /// </summary>
    public static class GrupaKorisnikaHelper
    {
        /// <summary>
        /// Korisnici sa OGRANIČENIM pristupom (samo Prerada, Lager, delovi Troškovi i Promet)
        /// </summary>
        private static readonly HashSet<string> OgraniceniKorisnici = new(StringComparer.OrdinalIgnoreCase)
        {
            "zoran",
            "jelena",
            "pedja",
            "radmila",
            "masinska",
            "BaneT"
        };

        /// <summary>
        /// Provera da li korisnik ima puni pristup (SVI osim ograničenih)
        /// </summary>
        public static bool ImaPuniPristup(string korisnickoIme)
        {
            if (string.IsNullOrWhiteSpace(korisnickoIme))
                return false;

            return !OgraniceniKorisnici.Contains(korisnickoIme);
        }

        /// <summary>
        /// Provera da li korisnik ima ograničeni pristup
        /// </summary>
        public static bool ImaOgraniceniPristup(string korisnickoIme)
        {
            if (string.IsNullOrWhiteSpace(korisnickoIme))
                return false;

            return OgraniceniKorisnici.Contains(korisnickoIme);
        }

        /// <summary>
        /// Provera pristupa specifičnoj stranici
        /// </summary>
        public static bool ImaPristupStranici(string korisnickoIme, string url)
        {
            // Admin (puni pristup) uvek ima pristup
            if (ImaPuniPristup(korisnickoIme))
                return true;

            // Ograničeni pristup
            if (ImaOgraniceniPristup(korisnickoIme))
            {
                var urlLower = url.ToLower().TrimEnd('/');

                // ❌ ZABRANJENO: Home (Dashboard) - početna stranica
                if (urlLower == "" || urlLower == "/")
                    return false;

                // ✅ DOZVOLJENO: Home za ograničene korisnike
                if (urlLower.Contains("/home-ograniceni"))
                    return true;

                // ✅ DOZVOLJENO: Prerada
                if (urlLower.Contains("/prerada") ||
                    urlLower.Contains("/proizvodnja") ||
                    urlLower.Contains("/radni-nalozi") ||
                    urlLower.Contains("/smenski-izvestaji") ||
                    urlLower.Contains("/sledljivost") ||
                    urlLower.Contains("/lager-proizvodnje"))
                    return true;

                // ✅ DOZVOLJENO: Lager
                if (urlLower.Contains("/lager") ||
                    urlLower.Contains("/ambalaza") ||
                    urlLower.Contains("/roba"))
                    return true;

                // ✅ DOZVOLJENO: Troškovi (samo proizvodnje i radne snage)
                if (urlLower.Contains("/troskovi-proizvodnje") ||
                    urlLower.Contains("/troskovi-radne-snage") ||
                    urlLower.Contains("/troskovihome"))
                    return true;

                // ✅ DOZVOLJENO: Promet (samo izveštaji prijem)
                if (urlLower.Contains("/izvestaj-prijem") ||
                    urlLower.Contains("/promethome"))
                    return true;

                // ❌ ZABRANJENO: Sve ostalo
                return false;
            }

            // Default: nema pristup
            return false;
        }

        /// <summary>
        /// Početna stranica za korisnika
        /// </summary>
        public static string PocetnaStranica(string korisnickoIme)
        {
            Console.WriteLine($"🔍 PocetnaStranica - Korisnik: {korisnickoIme}");
            Console.WriteLine($"   - ImaPuniPristup: {ImaPuniPristup(korisnickoIme)}");
            Console.WriteLine($"   - ImaOgraniceniPristup: {ImaOgraniceniPristup(korisnickoIme)}");

            if (ImaPuniPristup(korisnickoIme))
            {
                Console.WriteLine($"   ✅ Rezultat: / (puni pristup)");
                return "/"; // Dashboard
            }

            if (ImaOgraniceniPristup(korisnickoIme))
            {
                Console.WriteLine($"   ✅ Rezultat: /home-ograniceni (ograničeni pristup)");
                return "/home-ograniceni"; // Home za ograničene korisnike (bez Nabavka/Prodaja chartova)
            }

            Console.WriteLine($"   ❌ Rezultat: /zabranjen-pristup (nema pristup)");
            return "/zabranjen-pristup";
        }
    }
}
