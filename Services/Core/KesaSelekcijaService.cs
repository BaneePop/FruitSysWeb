using System.Text.Json;
using FruitSysWeb.Models;

namespace FruitSysWeb.Services.Core
{
    /// <summary>
    /// Servis za čuvanje i učitavanje globalne selekcije artikala
    /// za Stanje Kesa - Altiva izveštaj
    /// </summary>
    public class KesaSelekcijaService
    {
        private readonly string _filePath;
        private readonly ILogger<KesaSelekcijaService> _logger;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public KesaSelekcijaService(ILogger<KesaSelekcijaService> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _filePath = Path.Combine(env.ContentRootPath, "Data", "stanje-kese-selekcija.json");

            // Kreiraj Data folder ako ne postoji
            var dataFolder = Path.GetDirectoryName(_filePath);
            if (!Directory.Exists(dataFolder))
            {
                Directory.CreateDirectory(dataFolder!);
            }

            // Kreiraj fajl ako ne postoji
            if (!File.Exists(_filePath))
            {
                var praznaSelekcija = new StanjeKeseSelekcija();
                SacuvajSelekciju(praznaSelekcija).Wait();
            }
        }

        /// <summary>
        /// Učitava sačuvanu selekciju artikala
        /// </summary>
        public async Task<StanjeKeseSelekcija> UcitajSelekciju()
        {
            await _semaphore.WaitAsync();
            try
            {
                if (!File.Exists(_filePath))
                {
                    _logger.LogWarning("Fajl sa selekcijom ne postoji, vraćam praznu selekciju");
                    return new StanjeKeseSelekcija();
                }

                var json = await File.ReadAllTextAsync(_filePath);
                var selekcija = JsonSerializer.Deserialize<StanjeKeseSelekcija>(json);

                _logger.LogInformation($"✅ Učitana selekcija sa {selekcija?.SelektovaniArtikli.Count ?? 0} artikala");

                return selekcija ?? new StanjeKeseSelekcija();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju selekcije");
                return new StanjeKeseSelekcija();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Čuva selekciju artikala
        /// </summary>
        public async Task SacuvajSelekciju(StanjeKeseSelekcija selekcija)
        {
            await _semaphore.WaitAsync();
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                var json = JsonSerializer.Serialize(selekcija, options);
                await File.WriteAllTextAsync(_filePath, json);

                _logger.LogInformation($"💾 Sačuvana selekcija sa {selekcija.SelektovaniArtikli.Count} artikala");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri čuvanju selekcije");
                throw;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Čuva listu ID-jeva selektovanih artikala
        /// </summary>
        public async Task SacuvajSelektovaneArtikle(List<long> artikalIds)
        {
            var selekcija = new StanjeKeseSelekcija
            {
                SelektovaniArtikli = artikalIds
            };
            await SacuvajSelekciju(selekcija);
        }

        /// <summary>
        /// Vraća listu ID-jeva selektovanih artikala
        /// </summary>
        public async Task<List<long>> UcitajSelektovaneArtikle()
        {
            var selekcija = await UcitajSelekciju();
            return selekcija.SelektovaniArtikli;
        }
    }
}
