using FruitSysWeb.Models.Print;
using FruitSysWeb.Services.Print;
using Microsoft.AspNetCore.Mvc;

namespace FruitSysWeb.Controllers
{
    [ApiController]
    [Route("api/v1/print")]
    public class PrintApiController : ControllerBase
    {
        private readonly IPrintTokenService _tokenService;
        private readonly IPrintDocumentService _printService;
        private readonly ILogger<PrintApiController> _logger;

        public PrintApiController(
            IPrintTokenService tokenService,
            IPrintDocumentService printService,
            ILogger<PrintApiController> logger)
        {
            _tokenService = tokenService;
            _printService = printService;
            _logger = logger;
        }

        [HttpGet("types")]
        public IActionResult GetSupportedTypes() => Ok(PrintDocumentRegistry.SupportedTypes);

        [HttpGet("{tip}/{id:long}")]
        public async Task<IActionResult> Print(string tip, long id, CancellationToken ct)
        {
            if (!Request.Headers.TryGetValue("X-Print-Token", out var tokenValues))
                return ProblemDetails(401, "Unauthorized", "Nedostaje header X-Print-Token.");

            var token = tokenValues.FirstOrDefault();
            var korisnikId = await _tokenService.ValidateTokenAsync(token, ct);
            if (!korisnikId.HasValue)
                return ProblemDetails(401, "Unauthorized", "Token je nevažeći ili je istekao.");

            var korisnik = await _tokenService.UcitajKorisnikaAsync(korisnikId.Value, ct);
            if (korisnik == null)
                return ProblemDetails(401, "Unauthorized", "Korisnik za token nije pronađen.");

            if (!_tokenService.ImaPravoNaStampu(korisnik))
                return ProblemDetails(403, "Forbidden",
                    $"Korisnik '{korisnik.Ime}' nema pravo pristupa dokumentu {tip}/{id}.");

            try
            {
                var result = await _printService.GenerisiAsync(tip, id, korisnik.ID, ct);

                Response.Headers.CacheControl = result.IsFinal ? "private" : "no-store";
                Response.Headers.ContentDisposition = $"inline; filename=\"{result.FileName}\"";

                return File(result.PdfBytes, "application/pdf");
            }
            catch (PrintServiceException ex)
            {
                _logger.LogWarning("Print API {Status} za {Tip}/{Id}: {Detail}", ex.StatusCode, tip, id, ex.Message);
                return ProblemDetails(ex.StatusCode, ex.Title, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Print API greška za {Tip}/{Id}", tip, id);
                return ProblemDetails(500, "Internal Server Error", "Greška pri generisanju PDF-a.");
            }
        }

        private ObjectResult ProblemDetails(int status, string title, string detail) =>
            new(new
            {
                type = "about:blank",
                title,
                status,
                detail
            })
            {
                StatusCode = status,
                ContentTypes = { "application/problem+json" }
            };
    }
}
