using Dapper;
using FruitSysWeb.Models;
using FruitSysWeb.Services.Core;
using FruitSysWeb.Services.Interfaces;
using FruitSysWeb.Services.Models.Requests;
using Microsoft.Extensions.Logging;

namespace FruitSysWeb.Services.Implementations.IzvestajService;

/// <summary>
/// Servis za Brzi Pregled funkcionalnost - OPTIMIZOVAN
/// </summary>
public class BrziPregledService : IBrziPregledService
{
    private readonly DatabaseService _db;
        private readonly ILogger<BrziPregledService> _logger;

    public BrziPregledService(DatabaseService db,
            ILogger<BrziPregledService> logger)
    {
        _db = db;
            _logger = logger;
    }

    public Task SacuvajKonfiguraciju(BrziPregledKonfiguracija config)
    {
        // localStorage handling je na client side preko JS
        return Task.CompletedTask;
    }

    public Task<BrziPregledKonfiguracija> UcitajKonfiguraciju()
    {
        // localStorage handling je na client side preko JS
        return Task.FromResult(new BrziPregledKonfiguracija());
    }

    /// <summary>
    /// Učitava brzi pregled za dobavljače (KL-, IS- dokumenti)
    /// </summary>
    public async Task<List<BrziPregledStavka>> UcitajBrziPregledDobavljaca(
        List<int> komitentIds, FilterRequest filter)
    {
        try
        {
            if (komitentIds == null || !komitentIds.Any())
            {
                _logger.LogInformation("⚠️ UcitajBrziPregledDobavljaca: Nema izabranih komitenata");
                return new List<BrziPregledStavka>();
            }

            var odDatum = filter.OdDatum ?? new DateTime(2025, 6, 1);
            var doDatum = filter.DoDatum ?? DateTime.Now;

            _logger.LogInformation($"🔍 UcitajBrziPregledDobavljaca: {komitentIds.Count} komitenata");

            var sql = @"
                SELECT 
                    k.ID as KomitentID,
                    k.Naziv,
                    COALESCE(SUM(CASE 
                        WHEN fm.Dokument LIKE 'KL-%'
                            AND fm.Datum >= @OdDatum 
                            AND fm.Datum <= @DoDatum 
                            AND fm.DokumentStatus != 2 AND fm.DokumentStatus != 4
                        THEN fm.Potrazuje 
                        ELSE 0 
                    END), 0) as VrednostRobe,
                    COALESCE(SUM(CASE 
                        WHEN fm.Dokument LIKE 'IS-%'
                            AND fm.Datum >= @OdDatum 
                            AND fm.Datum <= @DoDatum 
                            AND fm.DokumentStatus != 2 AND fm.DokumentStatus != 4
                        THEN fm.Duguje 
                        ELSE 0 
                    END), 0) as Isplata
                FROM Komitent k
                LEFT JOIN vPrometFinansijev9 fm ON k.ID = fm.KomitentID
                WHERE k.ID IN @KomitentIds
                GROUP BY k.ID, k.Naziv
                ORDER BY k.Naziv";

            var parameters = new DynamicParameters();
            parameters.Add("@KomitentIds", komitentIds);
            parameters.Add("@OdDatum", odDatum);
            parameters.Add("@DoDatum", doDatum);

            var rezultat = await _db.QueryAsync<BrziPregledStavka>(sql, parameters);
            var lista = rezultat?.ToList() ?? new List<BrziPregledStavka>();

            _logger.LogInformation($"✅ Učitano {lista.Count} dobavljača");

            return lista;
        }
        catch (Exception ex)
        {
            _logger.LogInformation($"❌ Greška u UcitajBrziPregledDobavljaca: {ex.Message}");
            return new List<BrziPregledStavka>();
        }
    }

    /// <summary>
    /// Učitava brzi pregled za kupce (FK-, UP- dokumenti)
    /// </summary>
    public async Task<List<BrziPregledStavka>> UcitajBrziPregledKupaca(
        List<int> komitentIds, FilterRequest filter)
    {
        try
        {
            if (komitentIds == null || !komitentIds.Any())
            {
                _logger.LogInformation("⚠️ UcitajBrziPregledKupaca: Nema izabranih komitenata");
                return new List<BrziPregledStavka>();
            }

            var odDatum = filter.OdDatum ?? new DateTime(2025, 6, 1);
            var doDatum = filter.DoDatum ?? DateTime.Now;

            _logger.LogInformation($"🔍 UcitajBrziPregledKupaca: {komitentIds.Count} komitenata");

            var sql = @"
                SELECT 
                    k.ID as KomitentID,
                    k.Naziv,
                    COALESCE(SUM(CASE 
                        WHEN fm.Dokument LIKE 'FK-%'
                            AND fm.Datum >= @OdDatum 
                            AND fm.Datum <= @DoDatum 
                            AND fm.DokumentStatus != 2 AND fm.DokumentStatus != 4
                        THEN fm.Duguje 
                        ELSE 0 
                    END), 0) as VrednostRobe,
                    COALESCE(SUM(CASE 
                        WHEN fm.Dokument LIKE 'UP-%'
                            AND fm.Datum >= @OdDatum 
                            AND fm.Datum <= @DoDatum 
                            AND fm.DokumentStatus != 2 AND fm.DokumentStatus != 4
                        THEN fm.Potrazuje 
                        ELSE 0 
                    END), 0) as Isplata
                FROM Komitent k
                LEFT JOIN vPrometFinansijev9 fm ON k.ID = fm.KomitentID
                WHERE k.ID IN @KomitentIds
                GROUP BY k.ID, k.Naziv
                ORDER BY k.Naziv";

            var parameters = new DynamicParameters();
            parameters.Add("@KomitentIds", komitentIds);
            parameters.Add("@OdDatum", odDatum);
            parameters.Add("@DoDatum", doDatum);

            var rezultat = await _db.QueryAsync<BrziPregledStavka>(sql, parameters);
            var lista = rezultat?.ToList() ?? new List<BrziPregledStavka>();

            _logger.LogInformation($"✅ Učitano {lista.Count} kupaca");

            return lista;
        }
        catch (Exception ex)
        {
            _logger.LogInformation($"❌ Greška u UcitajBrziPregledKupaca: {ex.Message}");
            return new List<BrziPregledStavka>();
        }
    }

    /// <summary>
    /// ✨ OPTIMIZOVANO: Učitava robu na zalihama - BATCH SQL umesto loop-a
    /// </summary>
    public async Task<List<RobaZaliheStavka>> UcitajRobaNaZalihama(
        Dictionary<string, List<long>> artikliPoVrstama,
        FilterRequest filter)
    {
        try
        {
            if (artikliPoVrstama == null || !artikliPoVrstama.Any())
            {
                _logger.LogInformation("⚠️ UcitajRobaNaZalihama: Nema izabranih artikala");
                return new List<RobaZaliheStavka>();
            }

            var odDatum = filter.OdDatum ?? new DateTime(2025, 6, 1);
            var doDatum = filter.DoDatum ?? DateTime.Now;

            _logger.LogInformation($"🔍 UcitajRobaNaZalihama: {artikliPoVrstama.Count} vrsta voća");

            var rezultat = new List<RobaZaliheStavka>();

            // Mapa cena
            var cenaIdMap = new Dictionary<string, int>
            {
                { "Malina", 152 },
                { "Kupina", 154 },
                { "Šljiva", 155 },
                { "Kajsija", 153 },
                { "Višnja", 156 },
                { "Usluga", 160 }
            };

            // ✨ OPTIMIZACIJA: Grupisanje svih artikala za batch SQL
            var sviArtikliIds = artikliPoVrstama
                .Where(x => x.Value.Any())
                .SelectMany(x => x.Value)
                .Distinct()
                .ToList();

            if (!sviArtikliIds.Any())
            {
                _logger.LogInformation("⚠️ Nema artikala za učitavanje");
                return rezultat;
            }

            _logger.LogInformation($"📦 Batch učitavanje za {sviArtikliIds.Count} ukupno artikala");

            // ✨ BATCH SQL 1: Nabavka za SVE artikle odjednom
            var sqlNabavka = @"
                SELECT 
                    fm.ArtikalID,
                    COALESCE(SUM(ABS(fm.Kolicina)), 0) as Kolicina,
                    COALESCE(SUM(fm.Potrazuje), 0) as Vrednost
                FROM vPrometFinansijev9 fm
                WHERE fm.Dokument LIKE 'KL-%'
                  AND fm.Datum >= @OdDatum
                  AND fm.Datum <= @DoDatum
                  AND fm.DokumentStatus != 2 AND fm.DokumentStatus != 4
                  AND fm.ArtikalID IN @ArtikalIds
                GROUP BY fm.ArtikalID";

            var parametersNabavka = new DynamicParameters();
            parametersNabavka.Add("@OdDatum", odDatum);
            parametersNabavka.Add("@DoDatum", doDatum);
            parametersNabavka.Add("@ArtikalIds", sviArtikliIds);

            // ✨ BATCH SQL 2: Prodaja za SVE artikle odjednom
            var sqlProdaja = @"
                SELECT 
                    fm.ArtikalID,
                    COALESCE(SUM(ABS(fm.Kolicina)), 0) as Kolicina,
                    COALESCE(SUM(fm.Duguje), 0) as Vrednost
                FROM vPrometFinansijev9 fm
                WHERE fm.Dokument LIKE 'FK-%'
                  AND fm.Datum >= @OdDatum
                  AND fm.Datum <= @DoDatum
                  AND fm.DokumentStatus != 2 AND fm.DokumentStatus != 4
                  AND fm.ArtikalID IN @ArtikalIds
                GROUP BY fm.ArtikalID";

            var parametersProdaja = new DynamicParameters();
            parametersProdaja.Add("@OdDatum", odDatum);
            parametersProdaja.Add("@DoDatum", doDatum);
            parametersProdaja.Add("@ArtikalIds", sviArtikliIds);

            // ✨ BATCH SQL 3: Lager za SVE artikle odjednom
            var sqlLager = @"
                SELECT 
                    ml.ArtikalID,
                    COALESCE(SUM(ml.Kolicina), 0) as Kolicina
                FROM vwMagacinLager ml
                WHERE ml.ArtikalID IN @ArtikalIds
                  AND ml.Kolicina > 0
                GROUP BY ml.ArtikalID";

            var parametersLager = new DynamicParameters();
            parametersLager.Add("@ArtikalIds", sviArtikliIds);

            // ✨ PARALELNO izvršavanje sva tri SQL upita
            var nabavkaTask = _db.QueryAsync<dynamic>(sqlNabavka, parametersNabavka);
            var prodajaTask = _db.QueryAsync<dynamic>(sqlProdaja, parametersProdaja);
            var lagerTask = _db.QueryAsync<dynamic>(sqlLager, parametersLager);

            await Task.WhenAll(nabavkaTask, prodajaTask, lagerTask);

            // Konvertuj rezultate u dictionary za brz pristup
            var nabavkaDict = nabavkaTask.Result
                .ToDictionary(x => (long)x.ArtikalID, x => new { Kolicina = (decimal)x.Kolicina, Vrednost = (decimal)x.Vrednost });

            var prodajaDict = prodajaTask.Result
                .ToDictionary(x => (long)x.ArtikalID, x => new { Kolicina = (decimal)x.Kolicina, Vrednost = (decimal)x.Vrednost });

            var lagerDict = lagerTask.Result
                .ToDictionary(x => (long)x.ArtikalID, x => (decimal)x.Kolicina);

            _logger.LogInformation($"✅ Batch podaci učitani: Nabavka={nabavkaDict.Count}, Prodaja={prodajaDict.Count}, Lager={lagerDict.Count}");

            // Grupisanje po vrstama voća
            foreach (var vrsta in artikliPoVrstama.Where(x => x.Value.Any()))
            {
                var stavka = new RobaZaliheStavka
                {
                    VrstaProizvoda = vrsta.Key
                };

                // Saberi vrednosti za sve artikle ove vrste
                foreach (var artikalId in vrsta.Value)
                {
                    if (nabavkaDict.ContainsKey(artikalId))
                    {
                        stavka.NabavkaKg += nabavkaDict[artikalId].Kolicina;
                        stavka.NabavnaVrednost += nabavkaDict[artikalId].Vrednost;
                    }

                    if (prodajaDict.ContainsKey(artikalId))
                    {
                        stavka.ProdajaKg += prodajaDict[artikalId].Kolicina;
                        stavka.ProdajaVrednost += prodajaDict[artikalId].Vrednost;
                    }

                    if (lagerDict.ContainsKey(artikalId))
                    {
                        stavka.LagerKg += lagerDict[artikalId];
                    }
                }

                // Lager vrednost (iz KalkulacijaArtikalCena)
                if (stavka.LagerKg > 0 && cenaIdMap.ContainsKey(vrsta.Key))
                {
                    var cenaId = cenaIdMap[vrsta.Key];
                    var cena = await UcitajCenuIzKalkulacije(cenaId);
                    stavka.LagerVrednost = stavka.LagerKg * cena;
                }

                _logger.LogInformation($"  ✅ {vrsta.Key}: Nabavka={stavka.NabavkaKg:N2}kg, Prodaja={stavka.ProdajaKg:N2}kg, Lager={stavka.LagerKg:N2}kg");

                rezultat.Add(stavka);
            }

            _logger.LogInformation($"✅ Ukupno učitano {rezultat.Count} vrsta voća");

            return rezultat;
        }
        catch (Exception ex)
        {
            _logger.LogInformation($"❌ Greška u UcitajRobaNaZalihama: {ex.Message}");
            _logger.LogInformation($"Stack trace: {ex.StackTrace}");
            return new List<RobaZaliheStavka>();
        }
    }

    /// <summary>
    /// Učitava cenu iz KalkulacijaArtikalCena tabele
    /// </summary>
    private async Task<decimal> UcitajCenuIzKalkulacije(int kalkulacijaId)
    {
        try
        {
            var sql = @"
                SELECT BrutoCena
                FROM KalkulacijaArtikalCena
                WHERE ID = @KalkulacijaId
                  AND BrutoCena > 0
                LIMIT 1";

            var parameters = new DynamicParameters();
            parameters.Add("@KalkulacijaId", kalkulacijaId);

            var cena = await _db.ExecuteScalarAsync<decimal?>(sql, parameters);
            return cena ?? 0;
        }
        catch (Exception ex)
        {
            _logger.LogInformation($"❌ Greška u UcitajCenuIzKalkulacije: {ex.Message}");
            return 0;
        }
    }

    /// <summary>
    /// Učitava nabavku po danima i po vrsti voća za zadnjih 30 dana
    /// Filtrira artikle sa MagacinID (2, 3, 5, 6)
    /// </summary>
    public async Task<Dictionary<string, Dictionary<string, decimal>>> UcitajNabavkuPoDanimaPoVociAsync()
    {
        try
        {
            var doDatum = DateTime.Now;
            var odDatum = doDatum.AddDays(-30);

            _logger.LogInformation($"🔍 UcitajNabavkuPoDanimaPoVoci: {odDatum:yyyy-MM-dd} - {doDatum:yyyy-MM-dd}");

            // SQL query: Nabavka (KL- dokumenti) grupisan po datumu i vrsti artikla
            var sql = @"
                SELECT
                    DATE(fm.Datum) as Datum,
                    COALESCE(a.Naziv, 'Nepoznato') as VrstaVoca,
                    COALESCE(SUM(ABS(fm.Kolicina)), 0) as Kolicina,
                    COALESCE(SUM(fm.Potrazuje), 0) as Vrednost
                FROM vPrometFinansijev9 fm
                INNER JOIN Artikal a ON fm.ArtikalID = a.ID
                WHERE fm.Dokument LIKE 'KL-%'
                  AND fm.Datum >= @OdDatum
                  AND fm.Datum <= @DoDatum
                  AND fm.DokumentStatus != 2
                  AND fm.DokumentStatus != 4
                  AND a.MagacinID IN (2, 3, 5, 6)
                GROUP BY DATE(fm.Datum), a.Naziv
                ORDER BY Datum ASC, a.Naziv";

            var parameters = new DynamicParameters();
            parameters.Add("@OdDatum", odDatum);
            parameters.Add("@DoDatum", doDatum);

            var rezultat = await _db.QueryAsync<dynamic>(sql, parameters);

            // Grupisanje po vrsti voća koristeći MapToFruitType
            // Dictionary<VrstaVoca, Dictionary<Datum, (Kolicina, Vrednost)>>
            var podaciPoVoci = new Dictionary<string, Dictionary<string, (decimal Kolicina, decimal Vrednost)>>();

            foreach (var red in rezultat)
            {
                string artikalNaziv = red.VrstaVoca ?? "Nepoznato";
                string vrstaVoca = FruitSysWeb.Components.Charts.ChartDataHelper.MapToFruitType(artikalNaziv);
                string datum = ((DateTime)red.Datum).ToString("dd.MM");
                decimal kolicina = red.Kolicina;
                decimal vrednost = red.Vrednost;

                if (!podaciPoVoci.ContainsKey(vrstaVoca))
                {
                    podaciPoVoci[vrstaVoca] = new Dictionary<string, (decimal, decimal)>();
                }

                // Saberi vrednosti ako već postoji taj datum
                if (podaciPoVoci[vrstaVoca].ContainsKey(datum))
                {
                    var postojeci = podaciPoVoci[vrstaVoca][datum];
                    podaciPoVoci[vrstaVoca][datum] = (postojeci.Kolicina + kolicina, postojeci.Vrednost + vrednost);
                }
                else
                {
                    podaciPoVoci[vrstaVoca][datum] = (kolicina, vrednost);
                }
            }

            // Konvertuj u stari format (samo količina za chart)
            var rezultatDict = new Dictionary<string, Dictionary<string, decimal>>();
            foreach (var voce in podaciPoVoci)
            {
                rezultatDict[voce.Key] = new Dictionary<string, decimal>();
                foreach (var datum in voce.Value)
                {
                    rezultatDict[voce.Key][datum.Key] = datum.Value.Kolicina; // Vraćamo količinu, ne vrednost
                }
            }

            _logger.LogInformation($"✅ Učitano {rezultatDict.Count} vrsta voća za nabavku");

            return rezultatDict;
        }
        catch (Exception ex)
        {
            _logger.LogInformation($"❌ Greška u UcitajNabavkuPoDanimaPoVoci: {ex.Message}");
            return new Dictionary<string, Dictionary<string, decimal>>();
        }
    }

    /// <summary>
    /// Učitava prodaju po danima i po vrsti voća za zadnjih 30 dana
    /// Filtrira artikle sa MagacinID (2, 3, 5, 6)
    /// </summary>
    public async Task<Dictionary<string, Dictionary<string, decimal>>> UcitajProdajuPoDanimaPoVociAsync()
    {
        try
        {
            var doDatum = DateTime.Now;
            var odDatum = doDatum.AddDays(-30);

            _logger.LogInformation($"🔍 UcitajProdajuPoDanimaPoVoci: {odDatum:yyyy-MM-dd} - {doDatum:yyyy-MM-dd}");

            // SQL query: Prodaja (FK- dokumenti) grupisan po datumu i vrsti artikla
            var sql = @"
                SELECT
                    DATE(fm.Datum) as Datum,
                    COALESCE(a.Naziv, 'Nepoznato') as VrstaVoca,
                    COALESCE(SUM(ABS(fm.Kolicina)), 0) as Kolicina,
                    COALESCE(SUM(fm.Duguje), 0) as Vrednost
                FROM vPrometFinansijev9 fm
                INNER JOIN Artikal a ON fm.ArtikalID = a.ID
                WHERE fm.Dokument LIKE 'FK-%'
                  AND fm.Datum >= @OdDatum
                  AND fm.Datum <= @DoDatum
                  AND fm.DokumentStatus != 2
                  AND fm.DokumentStatus != 4
                  AND a.MagacinID IN (2, 3, 5, 6)
                GROUP BY DATE(fm.Datum), a.Naziv
                ORDER BY Datum ASC, a.Naziv";

            var parameters = new DynamicParameters();
            parameters.Add("@OdDatum", odDatum);
            parameters.Add("@DoDatum", doDatum);

            var rezultat = await _db.QueryAsync<dynamic>(sql, parameters);

            // Grupisanje po vrsti voća koristeći MapToFruitType
            var podaciPoVoci = new Dictionary<string, Dictionary<string, (decimal Kolicina, decimal Vrednost)>>();

            foreach (var red in rezultat)
            {
                string artikalNaziv = red.VrstaVoca ?? "Nepoznato";
                string vrstaVoca = FruitSysWeb.Components.Charts.ChartDataHelper.MapToFruitType(artikalNaziv);
                string datum = ((DateTime)red.Datum).ToString("dd.MM");
                decimal kolicina = red.Kolicina;
                decimal vrednost = red.Vrednost;

                if (!podaciPoVoci.ContainsKey(vrstaVoca))
                {
                    podaciPoVoci[vrstaVoca] = new Dictionary<string, (decimal, decimal)>();
                }

                // Saberi vrednosti ako već postoji taj datum
                if (podaciPoVoci[vrstaVoca].ContainsKey(datum))
                {
                    var postojeci = podaciPoVoci[vrstaVoca][datum];
                    podaciPoVoci[vrstaVoca][datum] = (postojeci.Kolicina + kolicina, postojeci.Vrednost + vrednost);
                }
                else
                {
                    podaciPoVoci[vrstaVoca][datum] = (kolicina, vrednost);
                }
            }

            // Konvertuj u stari format (samo količina za chart)
            var rezultatDict = new Dictionary<string, Dictionary<string, decimal>>();
            foreach (var voce in podaciPoVoci)
            {
                rezultatDict[voce.Key] = new Dictionary<string, decimal>();
                foreach (var datum in voce.Value)
                {
                    rezultatDict[voce.Key][datum.Key] = datum.Value.Kolicina; // Vraćamo količinu, ne vrednost
                }
            }

            _logger.LogInformation($"✅ Učitano {rezultatDict.Count} vrsta voća za prodaju");

            return rezultatDict;
        }
        catch (Exception ex)
        {
            _logger.LogInformation($"❌ Greška u UcitajProdajuPoDanimaPoVoci: {ex.Message}");
            return new Dictionary<string, Dictionary<string, decimal>>();
        }
    }

    /// <summary>
    /// Učitava ukupne vrednosti (količina i vrednost) za nabavku po vrsti voća za zadnjih 30 dana
    /// </summary>
    public async Task<Dictionary<string, (decimal Kolicina, decimal Vrednost)>> UcitajUkupneVrednostiNabavkeAsync()
    {
        try
        {
            var doDatum = DateTime.Now;
            var odDatum = doDatum.AddDays(-30);

            var sql = @"
                SELECT
                    COALESCE(a.Naziv, 'Nepoznato') as ArtikalNaziv,
                    COALESCE(SUM(ABS(fm.Kolicina)), 0) as Kolicina,
                    COALESCE(SUM(fm.Potrazuje), 0) as Vrednost
                FROM vPrometFinansijev9 fm
                INNER JOIN Artikal a ON fm.ArtikalID = a.ID
                WHERE fm.Dokument LIKE 'KL-%'
                  AND fm.Datum >= @OdDatum
                  AND fm.Datum <= @DoDatum
                  AND fm.DokumentStatus != 2
                  AND fm.DokumentStatus != 4
                  AND a.MagacinID IN (2, 3, 5, 6)
                GROUP BY a.Naziv";

            var parameters = new DynamicParameters();
            parameters.Add("@OdDatum", odDatum);
            parameters.Add("@DoDatum", doDatum);

            var rezultat = await _db.QueryAsync<dynamic>(sql, parameters);

            var ukupnoPoVoci = new Dictionary<string, (decimal Kolicina, decimal Vrednost)>();

            foreach (var red in rezultat)
            {
                string artikalNaziv = red.ArtikalNaziv ?? "Nepoznato";
                string vrstaVoca = FruitSysWeb.Components.Charts.ChartDataHelper.MapToFruitType(artikalNaziv);
                decimal kolicina = red.Kolicina;
                decimal vrednost = red.Vrednost;

                if (ukupnoPoVoci.ContainsKey(vrstaVoca))
                {
                    var postojeci = ukupnoPoVoci[vrstaVoca];
                    ukupnoPoVoci[vrstaVoca] = (postojeci.Kolicina + kolicina, postojeci.Vrednost + vrednost);
                }
                else
                {
                    ukupnoPoVoci[vrstaVoca] = (kolicina, vrednost);
                }
            }

            return ukupnoPoVoci;
        }
        catch (Exception ex)
        {
            _logger.LogInformation($"❌ Greška u UcitajUkupneVrednostiNabavke: {ex.Message}");
            return new Dictionary<string, (decimal, decimal)>();
        }
    }

    /// <summary>
    /// Učitava ukupne vrednosti (količina i vrednost) za prodaju po vrsti voća za zadnjih 30 dana
    /// </summary>
    public async Task<Dictionary<string, (decimal Kolicina, decimal Vrednost)>> UcitajUkupneVrednostiProdajeAsync()
    {
        try
        {
            var doDatum = DateTime.Now;
            var odDatum = doDatum.AddDays(-30);

            var sql = @"
                SELECT
                    COALESCE(a.Naziv, 'Nepoznato') as ArtikalNaziv,
                    COALESCE(SUM(ABS(fm.Kolicina)), 0) as Kolicina,
                    COALESCE(SUM(fm.Duguje), 0) as Vrednost
                FROM vPrometFinansijev9 fm
                INNER JOIN Artikal a ON fm.ArtikalID = a.ID
                WHERE fm.Dokument LIKE 'FK-%'
                  AND fm.Datum >= @OdDatum
                  AND fm.Datum <= @DoDatum
                  AND fm.DokumentStatus != 2
                  AND fm.DokumentStatus != 4
                  AND a.MagacinID IN (2, 3, 5, 6)
                GROUP BY a.Naziv";

            var parameters = new DynamicParameters();
            parameters.Add("@OdDatum", odDatum);
            parameters.Add("@DoDatum", doDatum);

            var rezultat = await _db.QueryAsync<dynamic>(sql, parameters);

            var ukupnoPoVoci = new Dictionary<string, (decimal Kolicina, decimal Vrednost)>();

            foreach (var red in rezultat)
            {
                string artikalNaziv = red.ArtikalNaziv ?? "Nepoznato";
                string vrstaVoca = FruitSysWeb.Components.Charts.ChartDataHelper.MapToFruitType(artikalNaziv);
                decimal kolicina = red.Kolicina;
                decimal vrednost = red.Vrednost;

                if (ukupnoPoVoci.ContainsKey(vrstaVoca))
                {
                    var postojeci = ukupnoPoVoci[vrstaVoca];
                    ukupnoPoVoci[vrstaVoca] = (postojeci.Kolicina + kolicina, postojeci.Vrednost + vrednost);
                }
                else
                {
                    ukupnoPoVoci[vrstaVoca] = (kolicina, vrednost);
                }
            }

            return ukupnoPoVoci;
        }
        catch (Exception ex)
        {
            _logger.LogInformation($"❌ Greška u UcitajUkupneVrednostiProdaje: {ex.Message}");
            return new Dictionary<string, (decimal, decimal)>();
        }
    }
}
