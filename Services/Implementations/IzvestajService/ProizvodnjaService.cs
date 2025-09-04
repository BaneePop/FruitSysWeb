using Dapper;
using MySqlConnector;
using FruitSysWeb.Services.Interfaces;
using FruitSysWeb.Services.Models.Requests;
using FruitSysWeb.Models;
using Microsoft.Extensions.Configuration;

namespace FruitSysWeb.Services.Implementations.IzvestajService
{
    public class ProizvodnjaService : IProizvodnjaService
    {
        private readonly string _connectionString;

        public ProizvodnjaService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string not found.");
        }

        public async Task<List<ProizvodnjaModel>> UcitajIzvestajProizvodnje(FilterRequest filter)
        {
            using var connection = new MySqlConnection(_connectionString);

            var query = @"
                SELECT 
                    vpp.RadniNalogID,
                    vpp.RadniNalog,
                    vpp.Artikal,
                    vpp.Komitent,
                    vpp.ArtikalPrvaKlasifikacija AS Klasifikacija,
                    vpp.Kolicina,
                    rn.DatumPocetka AS Datum,
                    vpp.RpArtikalTip AS TipArtikla
                FROM vPreradaPregled vpp
                LEFT JOIN RadniNalog rn ON vpp.RadniNalogID = rn.ID
                WHERE 1=1";

            var parameters = new DynamicParameters();

            if (filter.OdDatum.HasValue)
            {
                query += " AND rn.DatumIsporuke >= @OdDatum";
                parameters.Add("OdDatum", filter.OdDatum.Value);
            }

            if (filter.DoDatum.HasValue)
            {
                query += " AND rn.DatumIsporuke <= @DoDatum";
                parameters.Add("DoDatum", filter.DoDatum.Value);
            }

            if (!string.IsNullOrEmpty(filter.RadniNalog))
            {
                query += " AND vpp.RadniNalog LIKE @RadniNalog";
                parameters.Add("RadniNalog", $"%{filter.RadniNalog}%");
            }

            if (filter.KomitentId.HasValue)
            {
                query += " AND EXISTS (SELECT 1 FROM Komitent k WHERE k.ID = @KomitentId)";
                parameters.Add("KomitentId", filter.KomitentId.Value);
            }

            if (!string.IsNullOrEmpty(filter.KomitentTip))
            {
                query += " AND EXISTS (SELECT 1 FROM Komitent k WHERE ";
                switch (filter.KomitentTip)
                {
                    case "Kupac": query += "k.Tip = 1"; break;
                    case "Dobavljac": query += "k.Tip = 2"; break;
                    case "Proizvodjac": query += "k.Tip = 3"; break;
                    case "Otkupljivac": query += "k.Tip = 4"; break;
                }
                query += ")";
            }

            query += " ORDER BY rn.DatumIsporuke DESC, vpp.RadniNalog";

            try
            {
                var result = await connection.QueryAsync<ProizvodnjaModel>(query, parameters);
                return result.AsList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju podataka proizvodnje: {ex.Message}");
                return new List<ProizvodnjaModel>();
            }
        }

        public async Task<List<ProizvodnjaModel>> GetProizvodnjaAsync(FilterRequest filter)
        {
            return await Task.Run(() =>
            {
                // CPU-intensive synchronous code
                var result = new List<ProizvodnjaModel>();
                // ... your logic here
                return result;
            });
        }

        public async Task<decimal> UcitajUkupnuProizvodnju(FilterRequest filter)
        {
            using var connection = new MySqlConnection(_connectionString);

            var query = @"
                SELECT SUM(vpp.Kolicina)
                FROM vPreradaPregled vpp
                LEFT JOIN RadniNalog rn ON vpp.RadniNalogID = rn.ID
                WHERE 1=1";

            var parameters = new DynamicParameters();

            if (filter.OdDatum.HasValue)
            {
                query += " AND rn.DatumIsporuke >= @OdDatum";
                parameters.Add("OdDatum", filter.OdDatum.Value);
            }

            if (filter.DoDatum.HasValue)
            {
                query += " AND rn.DatumIsporuke <= @DoDatum";
                parameters.Add("DoDatum", filter.DoDatum.Value);
            }

            try
            {
                var result = await connection.ExecuteScalarAsync<decimal?>(query, parameters);
                return result ?? 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju ukupne proizvodnje: {ex.Message}");
                return 0;
            }
        }

        public async Task<int> UcitajBrojAktivnihNaloga(FilterRequest filter)
        {
            using var connection = new MySqlConnection(_connectionString);

            var query = @"
                    SELECT COUNT(*) 
                    FROM RadniNalog 
                    WHERE (Status = 'Aktivan' OR Status = 'U toku')
                    AND (@OdDatum IS NULL OR DatumPocetka >= @OdDatum)
                    AND (@DoDatum IS NULL OR DatumPocetka <= @DoDatum)";

            var parameters = new
            {
                OdDatum = filter.OdDatum,
                DoDatum = filter.DoDatum
            };

            try
            {
                var result = await connection.ExecuteScalarAsync<int?>(query, parameters);
                return result ?? 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju broja aktivnih naloga: {ex.Message}");
                return 0;
            }
        }

        public async Task<Dictionary<string, decimal>> UcitajProizvodnjuPoArtiklima(FilterRequest filter)
        {
            using var connection = new MySqlConnection(_connectionString);

            var query = @"
                SELECT 
                    vpp.Artikal,
                    SUM(vpp.Kolicina) as UkupnaKolicina
                FROM vPreradaPregled vpp
                LEFT JOIN RadniNalog rn ON vpp.RadniNalogID = rn.ID
                WHERE 1=1";

            var parameters = new DynamicParameters();

            if (filter.OdDatum.HasValue)
            {
                query += " AND rn.DatumIsporuke >= @OdDatum";
                parameters.Add("OdDatum", filter.OdDatum.Value);
            }

            if (filter.DoDatum.HasValue)
            {
                query += " AND rn.DatumIsporuke <= @DoDatum";
                parameters.Add("DoDatum", filter.DoDatum.Value);
            }

            query += " GROUP BY vpp.Artikal ORDER BY UkupnaKolicina DESC LIMIT 10";

            try
            {
                var result = await connection.QueryAsync<(string Artikal, decimal UkupnaKolicina)>(query, parameters);
                return result.ToDictionary(x => x.Artikal, x => x.UkupnaKolicina);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju proizvodnje po artiklima: {ex.Message}");
                return new Dictionary<string, decimal>();
            }
        }

        // Implementacija ostalih metoda iz IIzvestajService
        public Task<List<FinansijeModel>> UcitajFinansijskiIzvestaj(FilterRequest filter)
        {
            // Ovo će biti implementirano u FinansijeService
            return Task.FromResult(new List<FinansijeModel>());
        }

        public async Task<decimal> UcitajUkupnoSaldo(FilterRequest filter)
        {
            // Privremena implementacija - vrati 0 dok ne dodate pravu logiku
            return await Task.FromResult(0m);
        }

    }
}
