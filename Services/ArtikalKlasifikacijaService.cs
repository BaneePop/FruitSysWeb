using FruitSysWeb.Models;
using FruitSysWeb.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace FruitSysWeb.Services
{
    public class ArtikalKlasifikacijaService : IArtikalKlasifikacijaService
    {
        private readonly DatabaseService _databaseService;
        private readonly ILogger<ArtikalKlasifikacijaService> _logger;

        public ArtikalKlasifikacijaService(DatabaseService databaseService, ILogger<ArtikalKlasifikacijaService> logger)
        {
            _databaseService = databaseService;
            _logger = logger;
        }

        public async Task<List<ArtikalKlasifikacija>> UcitajSveKlasifikacije()
        {
            try
            {
                // Prvo pokušaj da učitaš osnovne voćne klasifikacije iz tabele
                var sql = @"
                    SELECT 
                        ID as Id, 
                        Naziv,
                        '' as Opis,
                        COALESCE(Aktivno, 1) as Aktivno,
                        COALESCE(Kreirano, NOW()) as Kreirano
                    FROM ArtikalKlasifikacija
                    WHERE COALESCE(Aktivno, 1) = 1
                      AND (Naziv LIKE '%Malina%' OR Naziv LIKE '%Kupina%' OR Naziv LIKE '%Sljiva%' OR 
                           Naziv LIKE '%Visnja%' OR Naziv LIKE '%Borovnica%' OR Naziv LIKE '%Kajsija%' OR 
                           Naziv LIKE '%Jagoda%' OR Naziv = 'Malina' OR Naziv = 'Kupina' OR 
                           Naziv = 'Sljiva' OR Naziv = 'Visnja' OR Naziv = 'Borovnica' OR 
                           Naziv = 'Kajsija' OR Naziv = 'Jagoda')
                      AND Naziv NOT LIKE '%Klasa%' AND Naziv NOT LIKE '%klasa%' 
                      AND Naziv NOT LIKE '%Klase%' AND Naziv NOT LIKE '%klase%'
                    ORDER BY 
                        CASE 
                            WHEN Naziv = 'Malina' OR Naziv LIKE 'Malina %' THEN 1
                            WHEN Naziv = 'Kupina' OR Naziv LIKE 'Kupina %' THEN 2
                            WHEN Naziv = 'Sljiva' OR Naziv LIKE 'Sljiva %' THEN 3
                            WHEN Naziv = 'Visnja' OR Naziv LIKE 'Visnja %' THEN 4
                            WHEN Naziv = 'Borovnica' OR Naziv LIKE 'Borovnica %' THEN 5
                            WHEN Naziv = 'Kajsija' OR Naziv LIKE 'Kajsija %' THEN 6
                            WHEN Naziv = 'Jagoda' OR Naziv LIKE 'Jagoda %' THEN 7
                            ELSE 8
                        END, Naziv
                ";

                var rezultat = await _databaseService.QueryAsync<ArtikalKlasifikacija>(sql);
                _logger.LogInformation($"Učitano {rezultat.Count()} voćnih klasifikacija iz baze");

                if (rezultat.Any())
                {
                    foreach (var klasifikacija in rezultat)
                    {
                        _logger.LogInformation($"- {klasifikacija.Id}: {klasifikacija.Naziv}");
                    }
                    return rezultat.ToList();
                }
                else
                {
                    // Fallback - vrati hardkodovane voćne klasifikacije
                    _logger.LogInformation("Nema voćnih podataka u tabeli, koristim fallback");
                    return GetFallbackVocneKlasifikacije();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju klasifikacija");
                _logger.LogInformation("Koristim fallback voćne klasifikacije");
                return GetFallbackVocneKlasifikacije();
            }
        }

        private List<ArtikalKlasifikacija> GetFallbackVocneKlasifikacije()
        {
            return new List<ArtikalKlasifikacija>
            {
                new ArtikalKlasifikacija { Id = 6, Naziv = "Malina", Opis = "Malina", Aktivno = true, Kreirano = DateTime.Now },
                new ArtikalKlasifikacija { Id = 10, Naziv = "Kupina", Opis = "Kupina", Aktivno = true, Kreirano = DateTime.Now },
                new ArtikalKlasifikacija { Id = 15, Naziv = "Sljiva", Opis = "Sljiva", Aktivno = true, Kreirano = DateTime.Now },
                new ArtikalKlasifikacija { Id = 11, Naziv = "Visnja", Opis = "Visnja", Aktivno = true, Kreirano = DateTime.Now },
                new ArtikalKlasifikacija { Id = 39, Naziv = "Borovnica", Opis = "Borovnica", Aktivno = true, Kreirano = DateTime.Now },
                new ArtikalKlasifikacija { Id = 28, Naziv = "Kajsija", Opis = "Kajsija", Aktivno = true, Kreirano = DateTime.Now },
                new ArtikalKlasifikacija { Id = 34, Naziv = "Jagoda", Opis = "Jagoda", Aktivno = true, Kreirano = DateTime.Now }
            };
        }

        private List<ArtikalKlasifikacija> GetFallbackKlasifikacije()
        {
            return GetFallbackVocneKlasifikacije();
        }

        public async Task<ArtikalKlasifikacija?> UcitajKlasifikaciju(long id)
        {
            try
            {
                var sql = @"
                    SELECT 
                        ID as Id, 
                        Naziv,
                        '' as Opis,
                        COALESCE(Aktivno, 1) as Aktivno,
                        COALESCE(Kreirano, NOW()) as Kreirano
                    FROM ArtikalKlasifikacija
                    WHERE ID = @Id
                ";

                var rezultat = await _databaseService.QueryFirstOrDefaultAsync<ArtikalKlasifikacija>(sql, new { Id = id });
                return rezultat;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju klasifikacije");
                return null;
            }
        }

        public async Task<List<ArtikalKlasifikacija>> UcitajKlasifikacijePoPretezi(string pretraga)
        {
            try
            {
                var sql = @"
                    SELECT 
                        ID as Id, 
                        Naziv,
                        '' as Opis,
                        COALESCE(Aktivno, 1) as Aktivno,
                        COALESCE(Kreirano, NOW()) as Kreirano
                    FROM ArtikalKlasifikacija
                    WHERE COALESCE(Aktivno, 1) = 1 
                      AND Naziv LIKE @Pretraga
                    ORDER BY Naziv
                ";

                var rezultat = await _databaseService.QueryAsync<ArtikalKlasifikacija>(sql, new { Pretraga = $"%{pretraga}%" });
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri pretrazi klasifikacija");
                return new List<ArtikalKlasifikacija>();
            }
        }
    }
}
