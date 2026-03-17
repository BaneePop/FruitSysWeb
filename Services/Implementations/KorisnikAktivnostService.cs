using System.Text.Json;
using FruitSysWeb.Models;
using FruitSysWeb.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace FruitSysWeb.Services.Implementations
{
    public class KorisnikAktivnostService : IKorisnikAktivnostService
    {
        private readonly string _filePath;
        private readonly ILogger<KorisnikAktivnostService> _logger;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public KorisnikAktivnostService(ILogger<KorisnikAktivnostService> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _filePath = Path.Combine(env.ContentRootPath, "Data", "korisnik-aktivnost.json");

            var dataFolder = Path.GetDirectoryName(_filePath);
            if (!Directory.Exists(dataFolder))
                Directory.CreateDirectory(dataFolder!);

            if (!File.Exists(_filePath))
                File.WriteAllText(_filePath, "[]");
        }

        public async Task<long> ZabeležiLogin(string korisnikIme, string? ipAdresa)
        {
            await _semaphore.WaitAsync();
            try
            {
                var lista = await UcitajFajl();
                var id = lista.Count > 0 ? lista.Max(x => x.ID) + 1 : 1;
                lista.Add(new KorisnikAktivnostModel
                {
                    ID = id,
                    KorisnikIme = korisnikIme,
                    IpAdresa = ipAdresa,
                    VremeLogina = DateTime.Now
                });
                await SacuvajFajl(lista);
                _logger.LogInformation("Login zabeležen: {Korisnik} sa IP {IP}", korisnikIme, ipAdresa);
                return id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri beleženju logina za {Korisnik}", korisnikIme);
                return 0;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task ZabeležiLogout(long aktivnostId, string korisnikIme)
        {
            if (aktivnostId <= 0) return;
            await _semaphore.WaitAsync();
            try
            {
                var lista = await UcitajFajl();
                var zapis = lista.FirstOrDefault(x => x.ID == aktivnostId && x.KorisnikIme == korisnikIme);
                if (zapis != null)
                {
                    zapis.VremeLogauta = DateTime.Now;
                    zapis.TrajanjeSekundi = (int)(zapis.VremeLogauta.Value - zapis.VremeLogina).TotalSeconds;
                    await SacuvajFajl(lista);
                    _logger.LogInformation("Logout zabeležen: {Korisnik}, ID={ID}", korisnikIme, aktivnostId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri beleženju logouta za {Korisnik}", korisnikIme);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<List<KorisnikAktivnostModel>> UcitajAktivnosti(KorisnikAktivnostFilter filter)
        {
            try
            {
                var lista = await UcitajFajl();

                var upit = lista.AsEnumerable();

                if (filter.DatumOd.HasValue)
                    upit = upit.Where(x => x.VremeLogina >= filter.DatumOd.Value.Date);
                if (filter.DatumDo.HasValue)
                    upit = upit.Where(x => x.VremeLogina < filter.DatumDo.Value.Date.AddDays(1));
                if (!string.IsNullOrEmpty(filter.KorisnikIme))
                    upit = upit.Where(x => x.KorisnikIme == filter.KorisnikIme);

                return upit.OrderByDescending(x => x.VremeLogina).Take(1000).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju aktivnosti korisnika");
                return new List<KorisnikAktivnostModel>();
            }
        }

        public async Task<List<string>> UcitajSveKorisnike()
        {
            try
            {
                var lista = await UcitajFajl();
                return lista.Select(x => x.KorisnikIme).Distinct().OrderBy(x => x).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju liste korisnika");
                return new List<string>();
            }
        }

        private async Task<List<KorisnikAktivnostModel>> UcitajFajl()
        {
            if (!File.Exists(_filePath)) return new List<KorisnikAktivnostModel>();
            var json = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<List<KorisnikAktivnostModel>>(json) ?? new List<KorisnikAktivnostModel>();
        }

        private async Task SacuvajFajl(List<KorisnikAktivnostModel> lista)
        {
            // Čuvamo max 10.000 zapisa — brišemo najstarije
            if (lista.Count > 10000)
                lista = lista.OrderByDescending(x => x.VremeLogina).Take(10000).ToList();

            var json = JsonSerializer.Serialize(lista, new JsonSerializerOptions { WriteIndented = false });
            await File.WriteAllTextAsync(_filePath, json);
        }
    }
}
