using FruitSysWeb.Models;
using FruitSysWeb.Services.Interfaces;
using FruitSysWeb.Services.Core;

namespace FruitSysWeb.Services.Implementations.IzvestajService
{
    public class ArtikliStatistikaService : IArtikliStatistikaService
    {
        private readonly DatabaseService _db;

        // Voće: ArtikalPrvaKlasifikacijaID → naziv
        private static readonly Dictionary<int, string> VoceGrupe = new()
        {
            { 6,  "Malina" },
            { 10, "Kupina" },
            { 11, "Višnja" },
            { 15, "Šljiva" },
            { 28, "Kajsija" },
            { 34, "Jagoda" },
            { 39, "Borovnica" },
        };

        // Ambalaža: AmbalazaTip → naziv (2=Kesa, 3=Kutija, 4=Paleta)
        private static readonly Dictionary<int, string> AmbalazaGrupe = new()
        {
            { 2, "Kesa" },
            { 3, "Kutija" },
            { 4, "Paleta" },
        };

        public ArtikliStatistikaService(DatabaseService db)
        {
            _db = db;
        }

        public async Task<List<ArtikliStatistikaGrupa>> UcitajNabavkuVoce(ArtikliStatistikaFilter filter)
        {
            const string sql = @"
                SELECT
                    vp.ArtikalID,
                    vp.Artikal,
                    vp.ArtikalPrvaKlasifikacijaID AS GrupaID,
                    SUM(ABS(vp.Kolicina)) AS Kolicina,
                    SUM(ABS(vp.Potrazuje)) AS Vrednost
                FROM vPrometFinansijev9 vp
                WHERE vp.Dokument LIKE 'KL-%'
                  AND vp.DokumentStatus NOT IN (2, 4)
                  AND vp.ArtikalPrvaKlasifikacijaID IN (6, 10, 11, 15, 28, 34, 39)
                  AND vp.Kolicina <> 0
                  AND vp.Potrazuje <> 0
                  AND DATE(vp.Datum) >= @OdDatum
                  AND DATE(vp.Datum) <= @DoDatum
                GROUP BY vp.ArtikalID, vp.Artikal, vp.ArtikalPrvaKlasifikacijaID
                ORDER BY vp.ArtikalPrvaKlasifikacijaID, Kolicina DESC";

            var rows = await _db.QueryAsync<dynamic>(sql, new
            {
                OdDatum = filter.OdDatum.ToString("yyyy-MM-dd"),
                DoDatum = filter.DoDatum.ToString("yyyy-MM-dd")
            });

            return GrupisajVoce(rows);
        }

        public async Task<List<ArtikliStatistikaGrupa>> UcitajNabavkuAmbalaza(ArtikliStatistikaFilter filter)
        {
            const string sql = @"
                SELECT
                    vp.ArtikalID,
                    vp.Artikal,
                    a.AmbalazaTip AS GrupaID,
                    SUM(ABS(vp.Kolicina)) AS Kolicina,
                    SUM(ABS(vp.Potrazuje)) AS Vrednost
                FROM vPrometFinansijev9 vp
                INNER JOIN Artikal a ON a.ID = vp.ArtikalID
                WHERE vp.Dokument LIKE 'KL-%'
                  AND vp.DokumentStatus NOT IN (2, 4)
                  AND a.MagacinID = 4
                  AND a.AmbalazaTip IN (2, 3, 4)
                  AND vp.Kolicina <> 0
                  AND vp.Potrazuje <> 0
                  AND DATE(vp.Datum) >= @OdDatum
                  AND DATE(vp.Datum) <= @DoDatum
                GROUP BY vp.ArtikalID, vp.Artikal, a.AmbalazaTip
                ORDER BY a.AmbalazaTip, Kolicina DESC";

            var rows = await _db.QueryAsync<dynamic>(sql, new
            {
                OdDatum = filter.OdDatum.ToString("yyyy-MM-dd"),
                DoDatum = filter.DoDatum.ToString("yyyy-MM-dd")
            });

            return GrupisajAmbalazu(rows);
        }

        public async Task<List<ArtikliStatistikaGrupa>> UcitajProdajuVoce(ArtikliStatistikaFilter filter)
        {
            const string sql = @"
                SELECT
                    vp.ArtikalID,
                    vp.Artikal,
                    vp.ArtikalPrvaKlasifikacijaID AS GrupaID,
                    SUM(ABS(vp.Kolicina)) AS Kolicina,
                    SUM(ABS(vp.Duguje)) AS Vrednost
                FROM vPrometFinansijev9 vp
                WHERE vp.Dokument LIKE 'FK-%'
                  AND vp.DokumentStatus NOT IN (2, 4)
                  AND vp.ArtikalPrvaKlasifikacijaID IN (6, 10, 11, 15, 28, 34, 39)
                  AND vp.Kolicina <> 0
                  AND vp.Duguje <> 0
                  AND vp.Artikal NOT IN ('D/Z Malina - ALTIVA', 'D/Z Malina - FRIKOS', 'Kupina Rollend Šumska')
                  AND DATE(vp.Datum) >= @OdDatum
                  AND DATE(vp.Datum) <= @DoDatum
                GROUP BY vp.ArtikalID, vp.Artikal, vp.ArtikalPrvaKlasifikacijaID
                ORDER BY vp.ArtikalPrvaKlasifikacijaID, Kolicina DESC";

            var rows = await _db.QueryAsync<dynamic>(sql, new
            {
                OdDatum = filter.OdDatum.ToString("yyyy-MM-dd"),
                DoDatum = filter.DoDatum.ToString("yyyy-MM-dd")
            });

            return GrupisajVoce(rows);
        }

        private List<ArtikliStatistikaGrupa> GrupisajVoce(IEnumerable<dynamic> rows)
        {
            var materijalized = rows.ToList();
            var result = new List<ArtikliStatistikaGrupa>();

            foreach (var (grupaId, naziv) in VoceGrupe)
            {
                var stavke = materijalized
                    .Where(r => Convert.ToInt32(r.GrupaID) == grupaId)
                    .Select(r => (ArtikliStatistikaRow)MapRow(r))
                    .ToList();

                if (stavke.Count == 0) continue;

                result.Add(new ArtikliStatistikaGrupa
                {
                    NazivGrupe = naziv,
                    Stavke = stavke
                });
            }

            return result;
        }

        private List<ArtikliStatistikaGrupa> GrupisajAmbalazu(IEnumerable<dynamic> rows)
        {
            var materijalized = rows.ToList();
            var result = new List<ArtikliStatistikaGrupa>();

            foreach (var (grupaId, naziv) in AmbalazaGrupe)
            {
                var stavke = materijalized
                    .Where(r => Convert.ToInt32(r.GrupaID) == grupaId)
                    .Select(r => (ArtikliStatistikaRow)MapRow(r))
                    .ToList();

                if (stavke.Count == 0) continue;

                result.Add(new ArtikliStatistikaGrupa
                {
                    NazivGrupe = naziv,
                    Stavke = stavke
                });
            }

            return result;
        }

        private static ArtikliStatistikaRow MapRow(dynamic r)
        {
            decimal kolicina = Convert.ToDecimal(r.Kolicina);
            decimal vrednost = Convert.ToDecimal(r.Vrednost);
            return new ArtikliStatistikaRow
            {
                ArtikalID = Convert.ToInt32(r.ArtikalID),
                Artikal = (string)r.Artikal,
                Kolicina = kolicina,
                Vrednost = vrednost,
                ProsecnaCena = kolicina > 0 ? vrednost / kolicina : 0
            };
        }
    }
}
