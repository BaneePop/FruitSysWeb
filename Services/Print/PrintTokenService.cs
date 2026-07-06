using FruitSysWeb.Models;
using FruitSysWeb.Services;

namespace FruitSysWeb.Services.Print
{
    public class PrintTokenService : IPrintTokenService
    {
        private const string PrintPageUrl = "/izvoz-dokumenata";

        private readonly DatabaseService _db;

        public PrintTokenService(DatabaseService db)
        {
            _db = db;
        }

        public async Task<long?> ValidateTokenAsync(string? token, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            const string sql = @"
                SELECT KorisnikId
                FROM WebPrintToken
                WHERE Token = @Token
                  AND Istek > NOW()
                LIMIT 1";

            return await _db.QueryFirstOrDefaultAsync<long?>(sql, new { Token = token.Trim() });
        }

        public async Task<KorisnikModel?> UcitajKorisnikaAsync(long korisnikId, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT
                    k.ID, k.Ime, k.Lozinka, k.Seed, k.Administrator, k.Kreirano, k.Azurirano,
                    k.Version, k.RadnikID, k.GrupaKorisnikaID, k.KomitentID,
                    gk.Naziv AS GrupaNaziv
                FROM Korisnik k
                LEFT JOIN GrupaKorisnika gk ON k.GrupaKorisnikaID = gk.ID
                WHERE k.ID = @Id
                LIMIT 1";

            return await _db.QueryFirstOrDefaultAsync<KorisnikModel>(sql, new { Id = korisnikId });
        }

        public bool ImaPravoNaStampu(KorisnikModel korisnik) =>
            GrupaKorisnikaHelper.ImaPristupStranici(korisnik.Ime, PrintPageUrl);
    }
}
