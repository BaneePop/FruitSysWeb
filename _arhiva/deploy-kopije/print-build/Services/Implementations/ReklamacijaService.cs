using System.Text.Json;
using FruitSysWeb.Models;
using FruitSysWeb.Services.Interfaces;

namespace FruitSysWeb.Services.Implementations
{
    /// <summary>
    /// Servis za upravljanje reklamacijama.
    /// Podaci se čuvaju lokalno u JSON fajlovima na serveru u Data/Reklamacije/.
    /// Dokumenti se čuvaju u Data/Reklamacije/{reklamacijaId}/.
    /// </summary>
    public class ReklamacijaService : IReklamacijaService
    {
        private readonly string _dataFolder;
        private readonly string _jsonPath;
        private readonly ILogger<ReklamacijaService> _logger;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public ReklamacijaService(ILogger<ReklamacijaService> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _dataFolder = Path.Combine(env.ContentRootPath, "Data", "Reklamacije");
            _jsonPath = Path.Combine(_dataFolder, "reklamacije.json");

            if (!Directory.Exists(_dataFolder))
                Directory.CreateDirectory(_dataFolder);

            if (!File.Exists(_jsonPath))
                File.WriteAllText(_jsonPath, "[]");
        }

        // ============================================================
        // CRUD OPERACIJE
        // ============================================================

        public async Task<List<ReklamacijaModel>> UcitajSveReklamacije()
        {
            await _semaphore.WaitAsync();
            try
            {
                return await UcitajIzFajla();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<List<ReklamacijaModel>> FilterirajReklamacije(ReklamacijaFilterModel filter)
        {
            var sve = await UcitajSveReklamacije();
            var query = sve.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(filter.KupacNaziv))
                query = query.Where(r => r.KupacNaziv.Contains(filter.KupacNaziv, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(filter.VrstaVoca))
                query = query.Where(r => r.VrstaVoca.Contains(filter.VrstaVoca, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(filter.TipReklamacije))
                query = query.Where(r => r.TipReklamacije == filter.TipReklamacije);

            if (!string.IsNullOrWhiteSpace(filter.PodtipReklamacije))
                query = query.Where(r => r.PodtipReklamacije == filter.PodtipReklamacije);

            if (!string.IsNullOrWhiteSpace(filter.Rezultat))
                query = query.Where(r => r.Rezultat == filter.Rezultat);

            if (filter.OdDatum.HasValue)
                query = query.Where(r => r.DatumReklamacije >= filter.OdDatum.Value);

            if (filter.DoDatum.HasValue)
                query = query.Where(r => r.DatumReklamacije <= filter.DoDatum.Value);

            if (!string.IsNullOrWhiteSpace(filter.TraziTekst))
            {
                var tekst = filter.TraziTekst.ToLower();
                query = query.Where(r =>
                    r.RadniNalogSifra.ToLower().Contains(tekst) ||
                    r.KupacNaziv.ToLower().Contains(tekst) ||
                    r.OpisReklamacije.ToLower().Contains(tekst) ||
                    (r.LotNaloga ?? "").ToLower().Contains(tekst));
            }

            return query.OrderByDescending(r => r.DatumReklamacije).ToList();
        }

        public async Task<ReklamacijaModel?> UcitajReklamaciju(string id)
        {
            var sve = await UcitajSveReklamacije();
            return sve.FirstOrDefault(r => r.Id == id);
        }

        public async Task<ReklamacijaModel> SacuvajReklamaciju(ReklamacijaModel reklamacija)
        {
            await _semaphore.WaitAsync();
            try
            {
                var sve = await UcitajIzFajla();
                var postojeci = sve.FindIndex(r => r.Id == reklamacija.Id);

                reklamacija.DatumIzmene = DateTime.Now.ToString("dd.MM.yyyy HH:mm");

                if (postojeci >= 0)
                    sve[postojeci] = reklamacija;
                else
                {
                    reklamacija.DatumKreiranja = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
                    sve.Add(reklamacija);
                }

                await SacuvajUFajl(sve);
                _logger.LogInformation("Reklamacija {Id} sačuvana (RN: {RN})", reklamacija.Id, reklamacija.RadniNalogSifra);
                return reklamacija;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task ObrisiReklamaciju(string id)
        {
            await _semaphore.WaitAsync();
            try
            {
                var sve = await UcitajIzFajla();
                var reklamacija = sve.FirstOrDefault(r => r.Id == id);
                if (reklamacija == null) return;

                // Obriši fajlove
                var folder = Path.Combine(_dataFolder, id);
                if (Directory.Exists(folder))
                    Directory.Delete(folder, recursive: true);

                sve.RemoveAll(r => r.Id == id);
                await SacuvajUFajl(sve);
                _logger.LogInformation("Reklamacija {Id} obrisana", id);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        // ============================================================
        // DOKUMENTI
        // ============================================================

        public async Task<ReklamacijaDokumentModel> DodajDokument(
            string reklamacijaId, string tipDokumenta,
            string naziv, Stream sadrzaj, string ekstenzija)
        {
            var dokFolder = Path.Combine(_dataFolder, reklamacijaId);
            if (!Directory.Exists(dokFolder))
                Directory.CreateDirectory(dokFolder);

            var dokId = Guid.NewGuid().ToString();
            var fajlNaziv = $"{dokId}{ekstenzija}";
            var fajlPutanja = Path.Combine(dokFolder, fajlNaziv);

            await using var fs = File.Create(fajlPutanja);
            await sadrzaj.CopyToAsync(fs);
            var velicina = fs.Length;

            var dokument = new ReklamacijaDokumentModel
            {
                Id = dokId,
                Naziv = naziv,
                TipDokumenta = tipDokumenta,
                PutanjaFajla = Path.Combine(reklamacijaId, fajlNaziv),
                Ekstenzija = ekstenzija,
                VelicinaBajta = velicina,
                DatumDodavanja = DateTime.Now.ToString("dd.MM.yyyy HH:mm")
            };

            // Ažuriraj JSON
            await _semaphore.WaitAsync();
            try
            {
                var sve = await UcitajIzFajla();
                var reklamacija = sve.FirstOrDefault(r => r.Id == reklamacijaId);
                if (reklamacija != null)
                {
                    if (tipDokumenta == "KupacDokument")
                        reklamacija.DokumentiKupca.Add(dokument);
                    else
                        reklamacija.NasiOdgovori.Add(dokument);

                    reklamacija.DatumIzmene = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
                    await SacuvajUFajl(sve);
                }
            }
            finally
            {
                _semaphore.Release();
            }

            _logger.LogInformation("Dokument {Id} dodat za reklamaciju {RekId}", dokId, reklamacijaId);
            return dokument;
        }

        public async Task ObrisiDokument(string reklamacijaId, string dokumentId)
        {
            await _semaphore.WaitAsync();
            try
            {
                var sve = await UcitajIzFajla();
                var reklamacija = sve.FirstOrDefault(r => r.Id == reklamacijaId);
                if (reklamacija == null) return;

                ReklamacijaDokumentModel? dokument =
                    reklamacija.DokumentiKupca.FirstOrDefault(d => d.Id == dokumentId) ??
                    reklamacija.NasiOdgovori.FirstOrDefault(d => d.Id == dokumentId);

                if (dokument == null) return;

                // Obriši fizički fajl
                var fajlPutanja = Path.Combine(_dataFolder, dokument.PutanjaFajla);
                if (File.Exists(fajlPutanja))
                    File.Delete(fajlPutanja);

                reklamacija.DokumentiKupca.RemoveAll(d => d.Id == dokumentId);
                reklamacija.NasiOdgovori.RemoveAll(d => d.Id == dokumentId);
                reklamacija.DatumIzmene = DateTime.Now.ToString("dd.MM.yyyy HH:mm");

                await SacuvajUFajl(sve);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<(Stream Sadrzaj, string Naziv, string ContentType)> PreuzmiDokument(
            string reklamacijaId, string dokumentId)
        {
            var reklamacija = await UcitajReklamaciju(reklamacijaId);
            if (reklamacija == null)
                throw new FileNotFoundException($"Reklamacija {reklamacijaId} nije pronađena.");

            var dokument =
                reklamacija.DokumentiKupca.FirstOrDefault(d => d.Id == dokumentId) ??
                reklamacija.NasiOdgovori.FirstOrDefault(d => d.Id == dokumentId);

            if (dokument == null)
                throw new FileNotFoundException($"Dokument {dokumentId} nije pronađen.");

            var fajlPutanja = Path.Combine(_dataFolder, dokument.PutanjaFajla);
            if (!File.Exists(fajlPutanja))
                throw new FileNotFoundException($"Fajl nije pronađen: {fajlPutanja}");

            var stream = File.OpenRead(fajlPutanja);
            var contentType = GetContentType(dokument.Ekstenzija);
            return (stream, dokument.Naziv + dokument.Ekstenzija, contentType);
        }

        // ============================================================
        // HELPER METODE ZA FILTERE
        // ============================================================

        public async Task<List<string>> UcitajSveKupce()
        {
            var sve = await UcitajSveReklamacije();
            return sve.Select(r => r.KupacNaziv)
                .Where(k => !string.IsNullOrEmpty(k))
                .Distinct()
                .OrderBy(k => k)
                .ToList();
        }

        public async Task<List<string>> UcitajSveVrsteVoca()
        {
            var sve = await UcitajSveReklamacije();
            return sve.Select(r => r.VrstaVoca)
                .Where(v => !string.IsNullOrEmpty(v))
                .Distinct()
                .OrderBy(v => v)
                .ToList();
        }

        // ============================================================
        // PRIVATE HELPERS
        // ============================================================

        private async Task<List<ReklamacijaModel>> UcitajIzFajla()
        {
            if (!File.Exists(_jsonPath))
                return new List<ReklamacijaModel>();

            var json = await File.ReadAllTextAsync(_jsonPath);
            if (string.IsNullOrWhiteSpace(json)) return new List<ReklamacijaModel>();

            return JsonSerializer.Deserialize<List<ReklamacijaModel>>(json, _jsonOptions)
                   ?? new List<ReklamacijaModel>();
        }

        private async Task SacuvajUFajl(List<ReklamacijaModel> lista)
        {
            var json = JsonSerializer.Serialize(lista, _jsonOptions);
            await File.WriteAllTextAsync(_jsonPath, json);
        }

        private static string GetContentType(string ekstenzija) => ekstenzija.ToLower() switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".doc" => "application/msword",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".xls" => "application/vnd.ms-excel",
            _ => "application/octet-stream"
        };
    }
}
