using System.Text.Json;
using FruitSysWeb.Models;
using FruitSysWeb.Services.Models.Requests;
using FruitSysWeb.Services.Core;
using FruitSysWeb.Services.Interfaces;

namespace FruitSysWeb.Services
{
    public class BrziPregledService : IBrziPregledService
    {
        private readonly DatabaseService _db;
        
        public BrziPregledService(DatabaseService db)
        {
            _db = db;
        }

        public async Task SacuvajKonfiguraciju(BrziPregledKonfiguracija config)
        {
            var json = JsonSerializer.Serialize(config);
            Console.WriteLine($"Čuvam konfiguraciju: {json}");
            await Task.CompletedTask;
        }

        public async Task<BrziPregledKonfiguracija> UcitajKonfiguraciju()
        {
            return await Task.FromResult(new BrziPregledKonfiguracija());
        }

        public async Task<List<BrziPregledStavka>> UcitajBrziPregledDobavljaca(
            List<int> komitentIds, FilterRequest filter)
        {
            if (komitentIds == null || !komitentIds.Any())
                return new List<BrziPregledStavka>();

            try
            {
                // DOBAVLJAČI: 
                // Vrednost Robe = KL- (Potrazuje)
                // Isplata = IS- (Duguje)
                var sql = @"
                    SELECT 
                        k.ID as KomitentID,
                        k.Naziv,
                        COALESCE(SUM(CASE 
                            WHEN vpf.Dokument LIKE 'KL-%' AND vpf.PCenaUkupno > 0 
                            THEN vpf.Potrazuje 
                            ELSE 0 
                        END), 0) as VrednostRobe,
                        COALESCE(SUM(CASE 
                            WHEN vpf.Dokument LIKE 'IS-%' 
                            THEN vpf.Duguje
                            ELSE 0 
                        END), 0) as Isplata
                    FROM Komitent k
                    LEFT JOIN vPrometFinansijev9 vpf ON k.ID = vpf.KomitentID
                        AND vpf.Datum BETWEEN @OdDatum AND @DoDatum
                        AND (vpf.Dokument LIKE 'KL-%' OR vpf.Dokument LIKE 'IS-%')
                        AND vpf.DokumentStatus != 4
                    WHERE k.ID IN @KomitentIds
                    GROUP BY k.ID, k.Naziv
                    ORDER BY k.Naziv";

                var rezultat = await _db.QueryAsync<BrziPregledStavka>(sql, new
                {
                    KomitentIds = komitentIds,
                    OdDatum = filter.OdDatum,
                    DoDatum = filter.DoDatum
                });
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajBrziPregledDobavljaca: {ex.Message}");
                return new List<BrziPregledStavka>();
            }
        }

        public async Task<List<BrziPregledStavka>> UcitajBrziPregledKupaca(
            List<int> komitentIds, FilterRequest filter)
        {
            if (komitentIds == null || !komitentIds.Any())
                return new List<BrziPregledStavka>();

            try
            {
                // KUPCI:
                // Vrednost Robe = FK- (Duguje)
                // Uplata = UP- (Potrazuje)
                var sql = @"
                    SELECT 
                        k.ID as KomitentID,
                        k.Naziv,
                        COALESCE(SUM(CASE 
                            WHEN vpf.Dokument LIKE 'FK-%' AND vpf.PCenaUkupno > 0 
                            THEN vpf.Duguje 
                            ELSE 0 
                        END), 0) as VrednostRobe,
                        COALESCE(SUM(CASE 
                            WHEN vpf.Dokument LIKE 'UP-%' 
                            THEN vpf.Potrazuje
                            ELSE 0 
                        END), 0) as Isplata
                    FROM Komitent k
                    LEFT JOIN vPrometFinansijev9 vpf ON k.ID = vpf.KomitentID
                        AND vpf.Datum BETWEEN @OdDatum AND @DoDatum
                        AND (vpf.Dokument LIKE 'FK-%' OR vpf.Dokument LIKE 'UP-%')
                        AND vpf.DokumentStatus != 4
                    WHERE k.ID IN @KomitentIds
                    GROUP BY k.ID, k.Naziv
                    ORDER BY k.Naziv";

                var rezultat = await _db.QueryAsync<BrziPregledStavka>(sql, new
                {
                    KomitentIds = komitentIds,
                    OdDatum = filter.OdDatum,
                    DoDatum = filter.DoDatum
                });
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajBrziPregledKupaca: {ex.Message}");
                return new List<BrziPregledStavka>();
            }
        }
    }
}
