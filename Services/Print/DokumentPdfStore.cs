using FruitSysWeb.Models.Print;
using FruitSysWeb.Services;
using Microsoft.Extensions.Options;

namespace FruitSysWeb.Services.Print
{
    public interface IDokumentPdfStore
    {
        Task<DokumentPdfRecord?> UcitajAsync(string tipDok, long dokId, CancellationToken ct = default);
        Task<string> SnimiAsync(string tipDok, long dokId, long korisnikId, byte[] pdfBytes, string sifra, CancellationToken ct = default);
        Task<byte[]?> ProcitajSaDiskaAsync(string putanja, CancellationToken ct = default);
    }

    public class DokumentPdfStore : IDokumentPdfStore
    {
        private readonly DatabaseService _db;
        private readonly PrintApiOptions _options;
        private readonly ILogger<DokumentPdfStore> _logger;

        public DokumentPdfStore(
            DatabaseService db,
            IOptions<PrintApiOptions> options,
            ILogger<DokumentPdfStore> logger)
        {
            _db = db;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<DokumentPdfRecord?> UcitajAsync(string tipDok, long dokId, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT Id, TipDok, DokId, Putanja, KorisnikId
                FROM DokumentPdf
                WHERE TipDok = @TipDok AND DokId = @DokId
                LIMIT 1";

            return await _db.QueryFirstOrDefaultAsync<DokumentPdfRecord>(sql, new { TipDok = tipDok, DokId = dokId });
        }

        public async Task<string> SnimiAsync(
            string tipDok,
            long dokId,
            long korisnikId,
            byte[] pdfBytes,
            string sifra,
            CancellationToken ct = default)
        {
            var root = _options.StorageRoot;
            if (string.IsNullOrWhiteSpace(root))
                throw new InvalidOperationException("PrintApi:StorageRoot nije konfigurisan.");

            var folder = Path.Combine(root, tipDok);
            Directory.CreateDirectory(folder);

            var safeSifra = SanitizeFileName(string.IsNullOrWhiteSpace(sifra) ? dokId.ToString() : sifra);
            var fileName = $"{dokId}_{safeSifra}.pdf";
            var fullPath = Path.GetFullPath(Path.Combine(folder, fileName));

            await File.WriteAllBytesAsync(fullPath, pdfBytes, ct);

            const string sql = @"
                INSERT INTO DokumentPdf
                    (TipDok, DokId, Putanja, KorisnikId, Kreirano, Azurirano, Version)
                VALUES
                    (@TipDok, @DokId, @Putanja, @KorisnikId, NOW(), NOW(), 0)
                ON DUPLICATE KEY UPDATE
                    Putanja    = VALUES(Putanja),
                    KorisnikId = VALUES(KorisnikId),
                    Azurirano  = NOW(),
                    Version    = Version + 1";

            await _db.ExecuteAsync(sql, new
            {
                TipDok = tipDok,
                DokId = dokId,
                Putanja = fullPath,
                KorisnikId = korisnikId
            });

            _logger.LogInformation("Sačuvan PDF {Tip}/{Id} na {Putanja}", tipDok, dokId, fullPath);
            return fullPath;
        }

        public Task<byte[]?> ProcitajSaDiskaAsync(string putanja, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(putanja) || !File.Exists(putanja))
                return Task.FromResult<byte[]?>(null);

            return File.ReadAllBytesAsync(putanja, ct).ContinueWith(t => (byte[]?)t.Result, ct);
        }

        private static string SanitizeFileName(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name.Replace('/', '_').Replace('\\', '_');
        }
    }
}
