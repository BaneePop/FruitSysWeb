using System.Text.Json;
using FruitSysWeb.Models;

namespace FruitSysWeb.Services.Core
{
    /// <summary>
    /// Globalna konfiguracija za stranicu Kalkulacija.
    /// Čuva: izabrani period, izbor dobavljača za Obračun Otkupa,
    /// cene prerade po kg, formule iskorišćenja, prodajne cene.
    /// Pattern identičan KesaSelekcijaService — JSON u Data/ folder, Singleton.
    /// </summary>
    public class KalkulacijaKonfiguracijaService
    {
        private readonly string _filePath;
        private readonly ILogger<KalkulacijaKonfiguracijaService> _logger;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public KalkulacijaKonfiguracijaService(
            ILogger<KalkulacijaKonfiguracijaService> logger,
            IWebHostEnvironment env)
        {
            _logger = logger;
            _filePath = Path.Combine(env.ContentRootPath, "Data", "kalkulacija-konfiguracija.json");

            var dataFolder = Path.GetDirectoryName(_filePath)!;
            if (!Directory.Exists(dataFolder))
                Directory.CreateDirectory(dataFolder);

            if (!File.Exists(_filePath))
                SacuvajKonfiguraciju(new KalkulacijaKonfiguracija()).Wait();
        }

        public async Task<KalkulacijaKonfiguracija> UcitajKonfiguraciju()
        {
            await _semaphore.WaitAsync();
            try
            {
                if (!File.Exists(_filePath))
                    return new KalkulacijaKonfiguracija();

                var json = await File.ReadAllTextAsync(_filePath);
                return JsonSerializer.Deserialize<KalkulacijaKonfiguracija>(json)
                       ?? new KalkulacijaKonfiguracija();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju kalkulacija konfiguracije");
                return new KalkulacijaKonfiguracija();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task SacuvajKonfiguraciju(KalkulacijaKonfiguracija konfiguracija)
        {
            await _semaphore.WaitAsync();
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(konfiguracija, options);
                await File.WriteAllTextAsync(_filePath, json);
                _logger.LogInformation("Kalkulacija konfiguracija sačuvana");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri čuvanju kalkulacija konfiguracije");
                throw;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }

    // ─────────────────────────────────────────────────────────────
    // Model koji se serijalizuje u JSON
    // ─────────────────────────────────────────────────────────────

    public class KalkulacijaKonfiguracija
    {
        // Izabrani period (čuva se da se ne gubi između poseta)
        public DateTime? OdDatum { get; set; }
        public DateTime? DoDatum { get; set; }

        // Faza 2: izabrani dobavljači za Obračun Otkupa (max 10), KomitentID lista
        public List<int> IzabraniDobavljaci { get; set; } = new();

        // Faza 2: početno stanje pre sezone po dobavljaču (KomitentID → saldo)
        // Negativno = mi dugujemo dobavljaču, pozitivno = dobavljač duguje nama
        public Dictionary<int, decimal> PocetnoStanjePoDobaveljacu { get; set; } = new();

        // Zadržano zbog kompatibilnosti sa starim JSON fajlovima
        public Dictionary<int, Dictionary<string, decimal>> MarzaPoKgDobavljac { get; set; } = new();

        // Faza 3: ručno unete nabavne cene po kg — ključ = ArtikalID (string), vrednost = din/kg
        // Koristi se kada baza nema nabavke za dati artikal (poluproizvodi, stari lager)
        public Dictionary<string, decimal> CeneNabavke { get; set; } = new();

        // Faza 3: cena prerade po kg sirovine — ključ = naziv vrste voća (npr. "Malina")
        public Dictionary<string, decimal> CenaPrerade { get; set; } = new();

        // Faza 4: formula iskorišćenja — ključ = naziv sirovine
        public Dictionary<string, List<FormulaIskoriscenjaStavka>> FormulaIskoriscenja { get; set; } = new();

        // Faza 4: prodajne cene po kg, ključ = ArtikalID gotovog proizvoda (string)
        public Dictionary<string, decimal> ProdajneCene { get; set; } = new();

        // Sekcija 5: ArtikalID-evi koji se isključuju iz Prenos Zaliha kalkulacije
        public List<int> IskluceniArtikliPrenosZaliha { get; set; } = new();
    }

    public class FormulaIskoriscenjaStavka
    {
        public int ArtikalID { get; set; }              // ID gotovog proizvoda (MagacinID=6)
        public string Naziv { get; set; } = string.Empty; // cached za prikaz
        public decimal Procenat { get; set; }           // npr. 85.0 za 85%
    }
}
