using FruitSysWeb.Models;

namespace FruitSysWeb.Services
{
    public class ArtikalService
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
                        ID,
                        Sifra,
                        Naziv,
                        Tip,
                        Aktivan,
                        Aktivno
                    FROM Artikal 
                    WHERE Aktivno = 1 
                    ORDER BY Naziv
                ";

                var rezultat = await _databaseService.QueryAsync<Artikal>(sql);
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajSveArtikle: {ex.Message}");
                return new List<Artikal>();
            }
        }

        public async Task<Artikal?> UcitajArtikalPoId(long id)
        {
            try
            {
                var sql = @"
                    SELECT 
                        ID,
                        Sifra,
                        Naziv,
                        Tip,
                        Aktivan,
                        Aktivno
                    FROM Artikal 
                    WHERE ID = @Id AND Aktivno = 1
                ";

                return await _databaseService.QueryFirstOrDefaultAsync<Artikal>(sql, new { Id = id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajArtikalPoId: {ex.Message}");
                return null;
            }
        }

        public async Task<List<Artikal>> UcitajArtiklePoTipu(int tip)
        {
            try
            {
                var sql = @"
                    SELECT 
                        ID,
                        Sifra,
                        Naziv,
                        Tip,
                        Aktivan,
                        Aktivno
                    FROM Artikal 
                    WHERE Aktivno = 1 AND Tip = @Tip
                    ORDER BY Naziv
                ";

                var rezultat = await _databaseService.QueryAsync<Artikal>(sql, new { Tip = tip });
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajArtiklePoTipu: {ex.Message}");
                return new List<Artikal>();
            }
        }
    }
}