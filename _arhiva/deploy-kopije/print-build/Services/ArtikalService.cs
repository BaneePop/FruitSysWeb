using FruitSysWeb.Models;
using FruitSysWeb.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace FruitSysWeb.Services
{
    public class ArtikalService : IArtikalService
    {
        private readonly DatabaseService _databaseService;
        private readonly ILogger<ArtikalService> _logger;

        public ArtikalService(DatabaseService databaseService, ILogger<ArtikalService> logger)
        {
            _databaseService = databaseService;
            _logger = logger;
        }

        public async Task<List<Artikal>> UcitajSveArtikle()
        {
            try
            {
                var sql = @"
                    SELECT 
                        ID as Id, Naziv, Tip, JedinicaMereID, Kreirano
                    FROM Artikal
                    WHERE Aktivno = 1
                    ORDER BY Naziv
                ";

                var rezultat = await _databaseService.QueryAsync<Artikal>(sql);
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju artikala");
                return new List<Artikal>();
            }
        }

        public async Task<Artikal?> UcitajArtikal(long id)
        {
            try
            {
                var sql = @"
                    SELECT 
                        ID as Id, Naziv, Tip, JedinicaMereID, Kreirano
                    FROM Artikal
                    WHERE ID = @Id
                ";

                var rezultat = await _databaseService.QueryFirstOrDefaultAsync<Artikal>(sql, new { Id = id });
                return rezultat;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju artikla");
                return null;
            }
        }

        public async Task<List<Artikal>> UcitajArtiklePoTipu(int tip)
        {
            try
            {
                var sql = @"
                    SELECT 
                        ID as Id, Naziv, Tip, JedinicaMereID, Kreirano
                    FROM Artikal
                    WHERE Aktivno = 1 AND Tip = @Tip
                    ORDER BY Naziv
                ";

                var rezultat = await _databaseService.QueryAsync<Artikal>(sql, new { Tip = tip });
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju artikala po tipu");
                return new List<Artikal>();
            }
        }

        public async Task<List<Artikal>> UcitajArtiklePoPretezi(string pretraga)
        {
            try
            {
                var sql = @"
                    SELECT 
                        ID as Id, Naziv, Tip, JedinicaMereID, Kreirano
                    FROM Artikal
                    WHERE Aktivno = 1 
                      AND Naziv LIKE @Pretraga
                    ORDER BY Naziv
                ";

                var rezultat = await _databaseService.QueryAsync<Artikal>(sql, new { Pretraga = $"%{pretraga}%" });
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri pretrazi artikala");
                return new List<Artikal>();
            }
        }

        // NOVO: Metoda za kaskadno filtriranje
        public async Task<List<Artikal>> UcitajArtiklePoPretragaITipu(string pretraga = "", int? tip = null)
        {
            try
            {
                var whereClause = "Aktivno = 1";
                var parameters = new Dictionary<string, object>();

                if (!string.IsNullOrEmpty(pretraga))
                {
                    whereClause += " AND Naziv LIKE @Pretraga";
                    parameters.Add("@Pretraga", $"%{pretraga}%");
                }

                if (tip.HasValue)
                {
                    whereClause += " AND Tip = @Tip";
                    parameters.Add("@Tip", tip.Value);
                }

                var sql = $@"
                    SELECT 
                        ID as Id, Naziv, Tip, JedinicaMereID, Kreirano
                    FROM Artikal
                    WHERE {whereClause}
                    ORDER BY Naziv
                ";

                var rezultat = await _databaseService.QueryAsync<Artikal>(sql, parameters);
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju artikala po pretrazi i tipu");
                return new List<Artikal>();
            }

        }
        public async Task<List<Artikal>> UcitajAmbalazuPoTipu(int tip)
        {
            try
            {
                var sql = @"
                    SELECT 
                        ArtikalTip as Tip, Naziv, Tip, JedinicaMereID, Kreirano
                    FROM Artikal
                    WHERE Aktivno = 1 AND Tip = @Tip
                    ORDER BY Naziv
                ";

                var rezultat = await _databaseService.QueryAsync<Artikal>(sql, new { Tip = tip });
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju Ambalaze po tipu");
                return new List<Artikal>();
            }
        }
    }
}
