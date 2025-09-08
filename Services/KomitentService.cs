using FruitSysWeb.Models;

namespace FruitSysWeb.Services
{
    public class KomitentService
    {
        private readonly DatabaseService _databaseService;

        public KomitentService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<List<Komitent>> UcitajSveKomitente()
        {
            try
            {
                var sql = @"
                    SELECT 
                        ID,
                        Sifra,
                        Naziv,
                        FizickoLice,
                        JeDobavljac,
                        JeKupac,
                        JeProizvodjac,
                        JeOtkupljivac,
                        JePrevoznik,
                        Aktivno
                    FROM Komitent 
                    WHERE Aktivno = 1 
                    ORDER BY Naziv
                ";

                var rezultat = await _databaseService.QueryAsync<Komitent>(sql);
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajSveKomitente: {ex.Message}");
                return new List<Komitent>();
            }
        }

        public async Task<Komitent?> UcitajKomitentaPoId(long id)
        {
            try
            {
                var sql = @"
                    SELECT 
                        ID,
                        Sifra,
                        Naziv,
                        FizickoLice,
                        JeDobavljac,
                        JeKupac,
                        JeProizvodjac,
                        JeOtkupljivac,
                        JePrevoznik,
                        Aktivno
                    FROM Komitent 
                    WHERE ID = @Id AND Aktivno = 1
                ";

                return await _databaseService.QueryFirstOrDefaultAsync<Komitent>(sql, new { Id = id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajKomitentaPoId: {ex.Message}");
                return null;
            }
        }

        public async Task<List<Komitent>> UcitajKupce()
        {
            try
            {
                var sql = @"
                    SELECT 
                        ID,
                        Sifra,
                        Naziv,
                        FizickoLice,
                        JeDobavljac,
                        JeKupac,
                        JeProizvodjac,
                        JeOtkupljivac,
                        JePrevoznik,
                        Aktivno
                    FROM Komitent 
                    WHERE Aktivno = 1 AND JeKupac = 1
                    ORDER BY Naziv
                ";

                var rezultat = await _databaseService.QueryAsync<Komitent>(sql);
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajKupce: {ex.Message}");
                return new List<Komitent>();
            }
        }

        public async Task<List<Komitent>> UcitajDobavljace()
        {
            try
            {
                var sql = @"
                    SELECT 
                        ID,
                        Sifra,
                        Naziv,
                        FizickoLice,
                        JeDobavljac,
                        JeKupac,
                        JeProizvodjac,
                        JeOtkupljivac,
                        JePrevoznik,
                        Aktivno
                    FROM Komitent 
                    WHERE Aktivno = 1 AND JeDobavljac = 1
                    ORDER BY Naziv
                ";

                var rezultat = await _databaseService.QueryAsync<Komitent>(sql);
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajDobavljace: {ex.Message}");
                return new List<Komitent>();
            }
        }

        public async Task<List<Komitent>> UcitajProizvodjace()
        {
            try
            {
                var sql = @"
                    SELECT 
                        ID,
                        Sifra,
                        Naziv,
                        FizickoLice,
                        JeDobavljac,
                        JeKupac,
                        JeProizvodjac,
                        JeOtkupljivac,
                        JePrevoznik,
                        Aktivno
                    FROM Komitent 
                    WHERE Aktivno = 1 AND JeProizvodjac = 1
                    ORDER BY Naziv
                ";

                var rezultat = await _databaseService.QueryAsync<Komitent>(sql);
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajProizvodjace: {ex.Message}");
                return new List<Komitent>();
            }
        }
    }
}