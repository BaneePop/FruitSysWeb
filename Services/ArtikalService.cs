using FruitSysWeb.Models;
using FruitSysWeb.Services.Interfaces;

namespace FruitSysWeb.Services
{
    public class ArtikalService : IArtikalService
    {
        private readonly DatabaseService _databaseService;

        public ArtikalService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
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
                Console.WriteLine($"Greška pri učitavanju artikala: {ex.Message}");
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
                Console.WriteLine($"Greška pri učitavanju artikla: {ex.Message}");
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
                Console.WriteLine($"Greška pri učitavanju artikala po tipu: {ex.Message}");
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
                Console.WriteLine($"Greška pri pretrazi artikala: {ex.Message}");
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
                Console.WriteLine($"Greška pri učitavanju artikala po pretrazi i tipu: {ex.Message}");
                return new List<Artikal>();
            }
        }
    }
}
