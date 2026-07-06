using FruitSysWeb.Models;
using FruitSysWeb.Models.Print;
using FruitSysWeb.Services;
using FruitSysWeb.Services.Implementations.ExportService;
using FruitSysWeb.Services.Interfaces;

namespace FruitSysWeb.Services.Print
{
    public interface IPrintDocumentService
    {
        Task<PrintPdfResult> GenerisiAsync(string tip, long id, long korisnikId, CancellationToken ct = default);
    }

    public class PrintDocumentService : IPrintDocumentService
    {
        private const int StatusZakljucen = (int)DokumentStatus.Zakljucen;
        private const int StatusStorno = 4;

        private readonly DatabaseService _db;
        private readonly IDokumentPdfStore _pdfStore;
        private readonly IIzvodDokumenataService _izvodService;
        private readonly IFakturaService _fakturaService;
        private readonly OtpremnicaPdfService _otpremnicaPdf;
        private readonly PrijemnicaPdfService _prijemnicaPdf;
        private readonly PaletniListPdfService _paletniListPdf;
        private readonly OtkupniListPdfService _otkupniListPdf;
        private readonly FakturaPdfService _fakturaPdf;
        private readonly ILogger<PrintDocumentService> _logger;

        public PrintDocumentService(
            DatabaseService db,
            IDokumentPdfStore pdfStore,
            IIzvodDokumenataService izvodService,
            IFakturaService fakturaService,
            OtpremnicaPdfService otpremnicaPdf,
            PrijemnicaPdfService prijemnicaPdf,
            PaletniListPdfService paletniListPdf,
            OtkupniListPdfService otkupniListPdf,
            FakturaPdfService fakturaPdf,
            ILogger<PrintDocumentService> logger)
        {
            _db = db;
            _pdfStore = pdfStore;
            _izvodService = izvodService;
            _fakturaService = fakturaService;
            _otpremnicaPdf = otpremnicaPdf;
            _prijemnicaPdf = prijemnicaPdf;
            _paletniListPdf = paletniListPdf;
            _otkupniListPdf = otkupniListPdf;
            _fakturaPdf = fakturaPdf;
            _logger = logger;
        }

        public async Task<PrintPdfResult> GenerisiAsync(string tip, long id, long korisnikId, CancellationToken ct = default)
        {
            tip = tip.Trim().ToLowerInvariant();

            if (!PrintDocumentRegistry.IsSupported(tip))
                throw new PrintServiceException(404, "Not Found", $"Nepoznat tip dokumenta '{tip}'.");

            if (tip == PrintDocumentRegistry.PlatniList)
                throw new PrintServiceException(422, "Unprocessable Entity", "Platni list još nije implementiran na web strani.");

            var meta = await UcitajMetaAsync(tip, id);
            if (meta == null)
                throw new PrintServiceException(404, "Not Found", $"Dokument {tip}/{id} ne postoji.");

            if (meta.DokumentStatus == StatusStorno)
                throw new PrintServiceException(422, "Unprocessable Entity", $"Dokument {tip}/{id} je storniran.");

            var fileName = BuildFileName(tip, meta.Sifra, id);
            var isFinal = meta.DokumentStatus == StatusZakljucen;

            if (!isFinal)
            {
                var fresh = await GenerisiPdfAsync(tip, id, meta);
                return new PrintPdfResult
                {
                    PdfBytes = fresh,
                    FileName = fileName,
                    FromCache = false,
                    IsFinal = false
                };
            }

            var cached = await _pdfStore.UcitajAsync(tip, id, ct);
            if (cached != null)
            {
                var bytes = await _pdfStore.ProcitajSaDiskaAsync(cached.Putanja, ct);
                if (bytes != null)
                {
                    return new PrintPdfResult
                    {
                        PdfBytes = bytes,
                        FileName = fileName,
                        FromCache = true,
                        IsFinal = true
                    };
                }

                _logger.LogWarning("PDF zapis postoji u bazi ali fajl nedostaje: {Putanja}", cached.Putanja);
            }

            var generated = await GenerisiPdfAsync(tip, id, meta);
            await _pdfStore.SnimiAsync(tip, id, korisnikId, generated, meta.Sifra, ct);

            return new PrintPdfResult
            {
                PdfBytes = generated,
                FileName = fileName,
                FromCache = false,
                IsFinal = true
            };
        }

        private async Task<PrintDocumentMeta?> UcitajMetaAsync(string tip, long id)
        {
            var sql = tip switch
            {
                PrintDocumentRegistry.Otpremnica => @"
                    SELECT o.ID AS Id, o.Sifra, o.DokumentStatus
                    FROM Otpremnica o
                    WHERE o.ID = @Id AND o.Aktivno = 1",
                PrintDocumentRegistry.Prijemnica => @"
                    SELECT p.ID AS Id, p.Sifra, p.DokumentStatus
                    FROM Prijemnica p
                    WHERE p.ID = @Id AND p.Aktivno = 1",
                PrintDocumentRegistry.Faktura => @"
                    SELECT f.ID AS Id, f.Sifra, f.DokumentStatus
                    FROM Faktura f
                    WHERE f.ID = @Id AND f.Aktivno = 1",
                PrintDocumentRegistry.PaletniList => @"
                    SELECT pl.ID AS Id, pl.Sifra, COALESCE(pl.DokumentStatus, 0) AS DokumentStatus
                    FROM PaletniList pl
                    WHERE pl.ID = @Id",
                PrintDocumentRegistry.OtkupniList => @"
                    SELECT ol.ID AS Id, ol.Sifra, ol.DokumentStatus
                    FROM OtkupniList ol
                    WHERE ol.ID = @Id",
                _ => null
            };

            if (sql == null)
                return null;

            return await _db.QueryFirstOrDefaultAsync<PrintDocumentMeta>(sql, new { Id = id });
        }

        private async Task<byte[]> GenerisiPdfAsync(string tip, long id, PrintDocumentMeta meta)
        {
            try
            {
                return tip switch
                {
                    PrintDocumentRegistry.Otpremnica => await GenerisiOtpremnicuAsync(id),
                    PrintDocumentRegistry.Prijemnica => await GenerisiPrijemnicuAsync(id),
                    PrintDocumentRegistry.Faktura => await GenerisiFakturuAsync(id, meta.DokumentStatus),
                    PrintDocumentRegistry.PaletniList => await GenerisiPaletniListAsync(id),
                    PrintDocumentRegistry.OtkupniList => await GenerisiOtkupniListAsync(id),
                    _ => throw new PrintServiceException(422, "Unprocessable Entity", $"Tip '{tip}' nije podržan za generisanje.")
                };
            }
            catch (PrintServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri generisanju PDF-a {Tip}/{Id}", tip, id);
                throw new PrintServiceException(500, "Internal Server Error", $"Greška pri generisanju PDF-a za {tip}/{id}.");
            }
        }

        private async Task<byte[]> GenerisiOtpremnicuAsync(long id)
        {
            var model = await _izvodService.UcitajOtpremnicuDetalji(id);
            if (model == null)
                throw new PrintServiceException(404, "Not Found", $"Otpremnica {id} nije pronađena.");
            return _otpremnicaPdf.Generisi(model);
        }

        private async Task<byte[]> GenerisiPrijemnicuAsync(long id)
        {
            var model = await _izvodService.UcitajPrijemnicuDetalji(id);
            if (model == null)
                throw new PrintServiceException(404, "Not Found", $"Prijemnica {id} nije pronađena.");
            return _prijemnicaPdf.Generisi(model);
        }

        private async Task<byte[]> GenerisiPaletniListAsync(long id)
        {
            var model = await _izvodService.UcitajPaletniListNabavkaDetalji(id);
            if (model == null)
                throw new PrintServiceException(404, "Not Found", $"Paletni list {id} nije pronađen.");
            return _paletniListPdf.Generisi(model);
        }

        private async Task<byte[]> GenerisiOtkupniListAsync(long id)
        {
            var model = await _izvodService.UcitajOtkupniListDetalji(id);
            if (model == null)
                throw new PrintServiceException(404, "Not Found", $"Otkupni list {id} nije pronađen.");
            return _otkupniListPdf.Generisi(model);
        }

        private async Task<byte[]> GenerisiFakturuAsync(long id, int dokumentStatus)
        {
            var detalji = await _fakturaService.UcitajDetalje(id);
            if (detalji?.Faktura == null)
                throw new PrintServiceException(404, "Not Found", $"Faktura {id} nije pronađena.");

            var naEngleskom = detalji.Kupac?.Ino == true;
            var isProfaktura = dokumentStatus != StatusZakljucen;

            return isProfaktura
                ? _fakturaPdf.GenerisiProfakturu(detalji, naEngleskom)
                : _fakturaPdf.GenerisiFakturu(detalji, naEngleskom);
        }

        private static string BuildFileName(string tip, string sifra, long id) =>
            string.IsNullOrWhiteSpace(sifra) ? $"{tip}-{id}.pdf" : $"{tip}-{sifra}.pdf";
    }
}
