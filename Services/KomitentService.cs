using FruitSysWeb.Models;
using FruitSysWeb.Services.Interfaces;
using FruitSysWeb.Services;
using FruitSysWeb.Services.Models.Requests;
using FruitSysWeb.Components.Shared.Filters;


namespace FruitSysWeb.Services
{
    public class KomitentService : IKomitentService
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
                        ID as Id, Naziv, PoreskiBroj, MaticniBroj, Adresa, Mesto, Telefon,
                        JeKupac, JeDobavljac, JeProizvodjac, JeOtkupljivac, FizickoLice, Aktivno,
                        Kreirano
                    FROM Komitent
                    WHERE Aktivno = 1
                    ORDER BY Naziv
                ";

                var rezultat = await _databaseService.QueryAsync<Komitent>(sql);
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju komitenata: {ex.Message}");
                return new List<Komitent>();
            }
        }

        public async Task<Komitent?> UcitajKomitenta(long id)
        {
            try
            {
                var sql = @"
                    SELECT 
                        ID as Id, Naziv, PoreskiBroj, MaticniBroj, Adresa, Mesto, Telefon,
                        JeKupac, JeDobavljac, JeProizvodjac, JeOtkupljivac, FizickoLice, Aktivno,
                        Kreirano
                    FROM Komitent
                    WHERE ID = @Id
                ";

                var rezultat = await _databaseService.QueryFirstOrDefaultAsync<Komitent>(sql, new { Id = id });
                return rezultat;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju komitenta: {ex.Message}");
                return null;
            }
        }

        public async Task<List<Komitent>> UcitajKomitentePoPretezi(string pretraga)
        {
            try
            {
                var sql = @"
                    SELECT 
                        ID as Id, Naziv, PoreskiBroj, MaticniBroj, Adresa, Mesto, Telefon,
                        JeKupac, JeDobavljac, JeProizvodjac, JeOtkupljivac, FizickoLice, Aktivno,
                        Kreirano
                    FROM Komitent
                    WHERE Aktivno = 1 
                      AND (Naziv LIKE @Pretraga OR PoreskiBroj LIKE @Pretraga)
                    ORDER BY Naziv
                ";

                var rezultat = await _databaseService.QueryAsync<Komitent>(sql, new { Pretraga = $"%{pretraga}%" });
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri pretrazi komitenata: {ex.Message}");
                return new List<Komitent>();
            }
        }

        public async Task<List<Komitent>> UcitajKomitentePoTipu(string tip)
        {
            try
            {
                var whereClause = tip.ToLower() switch
                {
                    "kupac" => "JeKupac = 1",
                    "dobavljac" => "JeDobavljac = 1", 
                    "proizvodjac" => "JeProizvodjac = 1",
                    "otkupljivac" => "JeOtkupljivac = 1",
                    _ => "1=1"
                };

                var sql = $@"
                    SELECT 
                        ID as Id, Naziv, PoreskiBroj, MaticniBroj, Adresa, Mesto, Telefon,
                        JeKupac, JeDobavljac, JeProizvodjac, JeOtkupljivac, FizickoLice, Aktivno,
                        Kreirano
                    FROM Komitent
                    WHERE Aktivno = 1 AND {whereClause}
                    ORDER BY Naziv
                ";

                var rezultat = await _databaseService.QueryAsync<Komitent>(sql);
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju komitenata po tipu: {ex.Message}");
                return new List<Komitent>();
            }
        }

        // NOVO: Metoda za kaskadno filtriranje
        public async Task<List<Komitent>> UcitajKomitentePoPretragaITipu(string pretraga = "", string? tip = null)
        {
            try
            {
                var whereClause = "Aktivno = 1";
                var parameters = new Dictionary<string, object>();
                
                if (!string.IsNullOrEmpty(pretraga))
                {
                    whereClause += " AND (Naziv LIKE @Pretraga OR PoreskiBroj LIKE @Pretraga)";
                    parameters.Add("@Pretraga", $"%{pretraga}%");
                }
                
                if (!string.IsNullOrEmpty(tip))
                {
                    whereClause += tip.ToLower() switch
                    {
                        "kupac" => " AND JeKupac = 1",
                        "dobavljac" => " AND JeDobavljac = 1", 
                        "proizvodjac" => " AND JeProizvodjac = 1",
                        "otkupljivac" => " AND JeOtkupljivac = 1",
                        _ => ""
                    };
                }

                var sql = $@"
                    SELECT 
                        ID as Id, Naziv, PoreskiBroj, MaticniBroj, Adresa, Mesto, Telefon,
                        JeKupac, JeDobavljac, JeProizvodjac, JeOtkupljivac, FizickoLice, Aktivno,
                        Kreirano
                    FROM Komitent
                    WHERE {whereClause}
                    ORDER BY Naziv
                ";

                var rezultat = await _databaseService.QueryAsync<Komitent>(sql, parameters);
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju komitenata po pretrazi i tipu: {ex.Message}");
                return new List<Komitent>();
            }
        }

        // NOVO: Metoda za komitente koji postoje u RadniNalog tabeli
        public async Task<List<Komitent>> UcitajKomitenteKojiImajuRadneNaloge()
        {
            try
            {
                var sql = @"
                    SELECT DISTINCT
                        k.ID as Id, k.Naziv, k.PoreskiBroj, k.MaticniBroj, k.Adresa, k.Mesto, k.Telefon,
                        k.JeKupac, k.JeDobavljac, k.JeProizvodjac, k.JeOtkupljivac, k.FizickoLice, k.Aktivno,
                        k.Kreirano
                    FROM Komitent k
                    INNER JOIN RadniNalog rn ON k.ID = rn.KomitentID
                    WHERE k.Aktivno = 1 AND rn.Aktivno = 1
                    ORDER BY k.Naziv
                ";

                var rezultat = await _databaseService.QueryAsync<Komitent>(sql);
                Console.WriteLine($"DEBUG: Učitano {rezultat.Count()} komitenata koji imaju radne naloge");
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju komitenata sa radnim nalozima: {ex.Message}");
                return new List<Komitent>();
            }
        }
    }
}
