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
    private readonly CacheService _cacheService;

    public BrziPregledService(DatabaseService db,
        ILogger<BrziPregledService> logger,
        CacheService cacheService)
    {
        _db = db;
        _logger = logger;
        _cacheService = cacheService;
    }

    /// <summary>
    /// Čuva globalnu konfiguraciju u JSON fajl (deli se između svih korisnika)
    /// </summary>
    public async Task SacuvajKonfiguraciju(BrziPregledKonfiguracija config)
    {
        try
        {
            // Putanja do config foldera u publish direktorijumu
            var configPath = Path.Combine(AppContext.BaseDirectory, "config", "brzi_pregled_config.json");

            // Kreiraj direktorijum ako ne postoji
            var configDir = Path.GetDirectoryName(configPath);
            if (!string.IsNullOrEmpty(configDir) && !Directory.Exists(configDir))
            {
                Directory.CreateDirectory(configDir);
            }

            // Serializuj konfiguraciju u JSON
            var json = System.Text.Json.JsonSerializer.Serialize(config, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });

            // Sačuvaj u fajl
            await File.WriteAllTextAsync(configPath, json);

            _logger.LogInformation($"✅ Konfiguracija sačuvana u {configPath}: {config.IzabraniDobavljaci.Count} dobavljača, {config.IzabraniKupci.Count} kupaca");
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Greška pri čuvanju konfiguracije: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Učitava globalnu konfiguraciju iz JSON fajla (deli se između svih korisnika)
    /// </summary>
    public async Task<BrziPregledKonfiguracija> UcitajKonfiguraciju()
    {
        try
        {
            // Putanja do config foldera u publish direktorijumu
            var configPath = Path.Combine(AppContext.BaseDirectory, "config", "brzi_pregled_config.json");

            // Ako fajl ne postoji, vrati praznu konfiguraciju
            if (!File.Exists(configPath))
            {
                _logger.LogWarning($"⚠️ Konfiguracija ne postoji na putanji {configPath}, vraćam praznu");
                return new BrziPregledKonfiguracija();
            }

            // Učitaj JSON iz fajla
            var json = await File.ReadAllTextAsync(configPath);

            // Deserializuj JSON
            var config = System.Text.Json.JsonSerializer.Deserialize<BrziPregledKonfiguracija>(json);

            if (config == null)
            {
                _logger.LogWarning("⚠️ Deserijalizacija nije uspela, vraćam praznu konfiguraciju");
                return new BrziPregledKonfiguracija();
            }

            _logger.LogInformation($"✅ Konfiguracija učitana iz {configPath}: {config.IzabraniDobavljaci.Count} dobavljača, {config.IzabraniKupci.Count} kupaca");

            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Greška pri učitavanju konfiguracije: {ex.Message}");
            return new BrziPregledKonfiguracija();
        }
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
    /// Učitava nabavku po danima i po vrsti voća
    /// Filtrira artikle sa MagacinID (2, 3, 5, 6)
    /// </summary>
    public async Task<Dictionary<string, Dictionary<string, decimal>>> UcitajNabavkuPoDanimaPoVociAsync(DateTime? odDatum = null, DateTime? doDatum = null)
    {
        try
        {
            var endDatum = doDatum ?? DateTime.Now;
            var startDatum = odDatum ?? endDatum.AddDays(-30); // Default: poslednjih 30 dana

            _logger.LogInformation($"🔍 UcitajNabavkuPoDanimaPoVoci: {startDatum:yyyy-MM-dd} - {endDatum:yyyy-MM-dd}");

            // SQL query: Nabavka (Ulaz > 0) grupisan po datumu i vrsti artikla
            var sql = @"
                SELECT
                    DATE(fm.Datum) as Datum,
                    COALESCE(fm.ArtikalPrvaKlasifikacijaID, 0) as KlasifikacijaID,
                    COALESCE(SUM(fm.Ulaz), 0) as Kolicina
                FROM vPrometRobav6 fm
                WHERE fm.Ulaz > 0
                  AND fm.Datum >= @OdDatum
                  AND fm.Datum <= @DoDatum
                  AND fm.DokumentStatus = 3
                  AND fm.ArtikalPrvaKlasifikacijaID IN (6, 10, 11, 15, 28, 34, 39)
                GROUP BY DATE(fm.Datum), fm.ArtikalPrvaKlasifikacijaID
                ORDER BY Datum ASC";

            var parameters = new DynamicParameters();
            parameters.Add("@OdDatum", startDatum);
            parameters.Add("@DoDatum", endDatum);

            var rezultat = await _db.QueryAsync<dynamic>(sql, parameters);

            var rezultatDict = new Dictionary<string, Dictionary<string, decimal>>();

            foreach (var red in rezultat)
            {
                int klasifikacijaId = (int)red.KlasifikacijaID;
                string vrstaVoca = FruitSysWeb.Components.Charts.ChartDataHelper.MapKlasifikacijaToFruitType(klasifikacijaId);
                string datumStr = ((DateTime)red.Datum).ToString("dd.MM.yy");
                decimal kolicina = red.Kolicina;

                if (!rezultatDict.ContainsKey(vrstaVoca))
                    rezultatDict[vrstaVoca] = new Dictionary<string, decimal>();

                if (rezultatDict[vrstaVoca].ContainsKey(datumStr))
                    rezultatDict[vrstaVoca][datumStr] += kolicina;
                else
                    rezultatDict[vrstaVoca][datumStr] = kolicina;
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
    /// Učitava prodaju po danima i po vrsti voća
    /// Filtrira artikle sa MagacinID (2, 3, 5, 6)
    /// </summary>
    public async Task<Dictionary<string, Dictionary<string, decimal>>> UcitajProdajuPoDanimaPoVociAsync(DateTime? odDatum = null, DateTime? doDatum = null)
    {
        try
        {
            var endDatum = doDatum ?? DateTime.Now;
            var startDatum = odDatum ?? endDatum.AddDays(-30); // Default: poslednjih 30 dana

            _logger.LogInformation($"🔍 UcitajProdajuPoDanimaPoVoci: {startDatum:yyyy-MM-dd} - {endDatum:yyyy-MM-dd}");

            // SQL query: Prodaja (Izlaz > 0) grupisan po datumu i vrsti artikla
            var sql = @"
                SELECT
                    DATE(fm.Datum) as Datum,
                    COALESCE(fm.ArtikalPrvaKlasifikacijaID, 0) as KlasifikacijaID,
                    COALESCE(SUM(fm.Izlaz), 0) as Kolicina
                FROM vPrometRobav6 fm
                WHERE fm.Izlaz > 0
                  AND fm.Datum >= @OdDatum
                  AND fm.Datum <= @DoDatum
                  AND fm.DokumentStatus = 3
                  AND fm.ArtikalPrvaKlasifikacijaID IN (6, 10, 11, 15, 28, 34, 39)
                GROUP BY DATE(fm.Datum), fm.ArtikalPrvaKlasifikacijaID
                ORDER BY Datum ASC";

            var parameters = new DynamicParameters();
            parameters.Add("@OdDatum", startDatum);
            parameters.Add("@DoDatum", endDatum);

            var rezultat = await _db.QueryAsync<dynamic>(sql, parameters);

            var rezultatDict = new Dictionary<string, Dictionary<string, decimal>>();

            foreach (var red in rezultat)
            {
                int klasifikacijaId = (int)red.KlasifikacijaID;
                string vrstaVoca = FruitSysWeb.Components.Charts.ChartDataHelper.MapKlasifikacijaToFruitType(klasifikacijaId);
                string datumStr = ((DateTime)red.Datum).ToString("dd.MM.yy");
                decimal kolicina = red.Kolicina;

                if (!rezultatDict.ContainsKey(vrstaVoca))
                    rezultatDict[vrstaVoca] = new Dictionary<string, decimal>();

                if (rezultatDict[vrstaVoca].ContainsKey(datumStr))
                    rezultatDict[vrstaVoca][datumStr] += kolicina;
                else
                    rezultatDict[vrstaVoca][datumStr] = kolicina;
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
    public async Task<Dictionary<string, (decimal Kolicina, decimal Vrednost)>> UcitajUkupneVrednostiNabavkeAsync(DateTime? odDatum = null, DateTime? doDatum = null)
    {
        try
        {
            var endDatum = doDatum ?? DateTime.Now;
            var startDatum = odDatum ?? endDatum.AddDays(-30);

            var sql = @"
                SELECT
                    COALESCE(fm.ArtikalPrvaKlasifikacijaID, 0) as KlasifikacijaID,
                    COALESCE(SUM(CASE WHEN LEFT(fm.Dokument, 2) = 'PR' THEN fm.Ulaz ELSE 0 END), 0) as Kolicina,
                    0 as Vrednost
                FROM vPrometRobav6 fm
                WHERE fm.DokumentStatus = 3
                  AND fm.Datum >= @OdDatum
                  AND fm.Datum <= @DoDatum
                  AND fm.ArtikalPrvaKlasifikacijaID IN (6, 10, 11, 15, 28, 34, 39)
                  AND fm.Ulaz > 0
                GROUP BY fm.ArtikalPrvaKlasifikacijaID";

            var parameters = new DynamicParameters();
            parameters.Add("@OdDatum", startDatum);
            parameters.Add("@DoDatum", endDatum);

            var rezultat = await _db.QueryAsync<dynamic>(sql, parameters);

            var ukupnoPoVoci = new Dictionary<string, (decimal Kolicina, decimal Vrednost)>();

            foreach (var red in rezultat)
            {
                int klasifikacijaId = (int)red.KlasifikacijaID;
                string vrstaVoca = FruitSysWeb.Components.Charts.ChartDataHelper.MapKlasifikacijaToFruitType(klasifikacijaId);
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

    public async Task<Dictionary<string, (decimal Kolicina, decimal Vrednost)>> UcitajUkupneVrednostiProdajeAsync(DateTime? odDatum = null, DateTime? doDatum = null)
    {
        try
        {
            var endDatum = doDatum ?? DateTime.Now;
            var startDatum = odDatum ?? endDatum.AddDays(-30);

            var sql = @"
                SELECT
                    COALESCE(fm.ArtikalPrvaKlasifikacijaID, 0) as KlasifikacijaID,
                    COALESCE(SUM(CASE WHEN LEFT(fm.Dokument, 2) = 'OT' THEN fm.Izlaz ELSE 0 END), 0) as Kolicina,
                    0 as Vrednost
                FROM vPrometRobav6 fm
                WHERE fm.DokumentStatus = 3
                  AND fm.Datum >= @OdDatum
                  AND fm.Datum <= @DoDatum
                  AND fm.ArtikalPrvaKlasifikacijaID IN (6, 10, 11, 15, 28, 34, 39)
                  AND fm.Izlaz > 0
                GROUP BY fm.ArtikalPrvaKlasifikacijaID";

            var parameters = new DynamicParameters();
            parameters.Add("@OdDatum", startDatum);
            parameters.Add("@DoDatum", endDatum);

            var rezultat = await _db.QueryAsync<dynamic>(sql, parameters);

            var ukupnoPoVoci = new Dictionary<string, (decimal Kolicina, decimal Vrednost)>();

            foreach (var red in rezultat)
            {
                int klasifikacijaId = (int)red.KlasifikacijaID;
                string vrstaVoca = FruitSysWeb.Components.Charts.ChartDataHelper.MapKlasifikacijaToFruitType(klasifikacijaId);
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

    private static readonly int[] PraceneKlasifikacije = { 6, 10, 11, 15, 28, 34, 39 };

    /// <summary>
    /// Ucitava promet iz baze od datumOd do datumDo i vraca neto (Ulaz-Izlaz) po KlasifikacijaID i po danu.
    /// Koristi LEFT(Dokument,2)='PR' za ulaz, 'OT' za izlaz.
    /// </summary>
    private async Task<Dictionary<int, Dictionary<DateTime, decimal>>> UcitajPrometPoKlasifikacijiAsync(DateTime datumOd, DateTime datumDo)
    {
        var sql = @"
            SELECT
                DATE(fm.Datum) as Datum,
                fm.ArtikalPrvaKlasifikacijaID as KlasifikacijaID,
                SUM(CASE WHEN LEFT(fm.Dokument, 2) = 'PR' THEN COALESCE(fm.Ulaz, 0) ELSE 0 END) as Ulaz,
                SUM(CASE WHEN LEFT(fm.Dokument, 2) = 'OT' THEN COALESCE(fm.Izlaz, 0) ELSE 0 END) as Izlaz
            FROM vPrometRobav6 fm
            WHERE fm.DokumentStatus = 3
              AND fm.ArtikalPrvaKlasifikacijaID IN (6, 10, 11, 15, 28, 34, 39)
              AND fm.Datum >= @DatumOd
              AND fm.Datum <= @DatumDo
              AND (fm.Ulaz > 0 OR fm.Izlaz > 0)
            GROUP BY DATE(fm.Datum), fm.ArtikalPrvaKlasifikacijaID
            ORDER BY Datum ASC";

        var parameters = new DynamicParameters();
        parameters.Add("@DatumOd", datumOd);
        parameters.Add("@DatumDo", datumDo);

        var rezultat = await _db.QueryAsync<dynamic>(sql, parameters);

        var dict = new Dictionary<int, Dictionary<DateTime, decimal>>();
        foreach (var red in rezultat)
        {
            int klasId = (int)red.KlasifikacijaID;
            DateTime datum = ((DateTime)red.Datum).Date;
            decimal neto = (decimal)red.Ulaz - (decimal)red.Izlaz;

            if (!dict.ContainsKey(klasId))
                dict[klasId] = new Dictionary<DateTime, decimal>();

            dict[klasId][datum] = dict[klasId].TryGetValue(datum, out var existing) ? existing + neto : neto;
        }
        return dict;
    }

    /// <summary>
    /// Izracunava pocetno stanje sezone:
    /// PocetnoStanje = TrenutnoStanje(MagacinLager) - NetoPromet(pocetakSezone -> danas)
    /// </summary>
    private async Task<Dictionary<int, decimal>> IzracunajPocetnoStanjeSezonAsync(DateTime pocetakSezone)
    {
        // 1. Trenutno stanje iz MagacinLager JOIN Artikal, grupisano po PrvaKlasifikacijaID
        var sqlLager = @"
            SELECT
                a.PrvaKlasifikacijaID as KlasifikacijaID,
                SUM(ml.Kolicina) as Kolicina
            FROM MagacinLager ml
            JOIN ArtikalInstanca ai ON ml.ArtikalInstancaID = ai.ID
            JOIN Artikal a ON ai.ArtikalID = a.ID
            WHERE a.PrvaKlasifikacijaID IN (6, 10, 11, 15, 28, 34, 39)
            GROUP BY a.PrvaKlasifikacijaID";

        var lagerRezultat = await _db.QueryAsync<dynamic>(sqlLager);
        var trenutnoStanje = new Dictionary<int, decimal>();
        foreach (var red in lagerRezultat)
            trenutnoStanje[(int)red.KlasifikacijaID] = (decimal)red.Kolicina;

        // 2. Neto promet od pocetka sezone do danas
        var danas = DateTime.Today;
        var promet = await UcitajPrometPoKlasifikacijiAsync(pocetakSezone, danas);

        // 3. PocetnoStanje = TrenutnoStanje - NetoPrometOdPocetka
        var pocetnoStanje = new Dictionary<int, decimal>();
        foreach (var klasId in PraceneKlasifikacije)
        {
            decimal trenutno = trenutnoStanje.TryGetValue(klasId, out var t) ? t : 0m;
            decimal neto = promet.TryGetValue(klasId, out var dp) ? dp.Values.Sum() : 0m;
            pocetnoStanje[klasId] = Math.Max(0, trenutno - neto);
        }

        return pocetnoStanje;
    }

    /// <summary>
    /// Ucitava kretanje stanja lagera po vrsti voca kroz vreme.
    /// Pocinje od tacnog pocetnog stanja sezone (izracunatog iz baze od 01.06.2023).
    /// interval: "dnevno", "nedeljno", "mesecno"
    /// </summary>
    public async Task<Dictionary<string, Dictionary<string, decimal>>> UcitajLagerKretanjePoVociAsync(DateTime odDatum, DateTime doDatum, string interval = "dnevno")
    {
        try
        {
            _logger.LogInformation($"UcitajLagerKretanje: {odDatum:yyyy-MM-dd} - {doDatum:yyyy-MM-dd}, interval={interval}");

            // Odredi sezonu (godina u kojoj pocinje sezona = godina od datuma, ali ako je pre 01.06 onda prethodna)
            int sezona = odDatum.Month >= 6 ? odDatum.Year : odDatum.Year - 1;
            var pocetakSezone = new DateTime(sezona, 6, 1);

            // 1. Izracunaj pocetno stanje na pocetku sezone
            var pocetnoStanje = await IzracunajPocetnoStanjeSezonAsync(pocetakSezone);

            // 2. Ucitaj sav promet od pocetka sezone do kraja trazenog perioda
            var promet = await UcitajPrometPoKlasifikacijiAsync(pocetakSezone, doDatum);

            // 3. Za svaki klasifikacijski ID izracunaj kumulativ po danima
            var intervalDatumi = GenerisiIntervalDatume(odDatum, doDatum, interval);
            var rezultatDict = new Dictionary<string, Dictionary<string, decimal>>();

            foreach (var klasId in PraceneKlasifikacije)
            {
                string vrstaVoca = FruitSysWeb.Components.Charts.ChartDataHelper.MapKlasifikacijaToFruitType(klasId);

                // Kumulativ od pocetka sezone do pocetka prikaza
                decimal kumulativ = pocetnoStanje.TryGetValue(klasId, out var ps) ? ps : 0m;

                if (promet.TryGetValue(klasId, out var dnevniPromet))
                {
                    // Dodaj promet od pocetka sezone do pocetka prikaza (pre-load)
                    kumulativ += dnevniPromet
                        .Where(x => x.Key < odDatum.Date)
                        .Sum(x => x.Value);
                }

                var serija = new Dictionary<string, decimal>();

                foreach (var (intervalStart, intervalEnd, intervalLabel) in intervalDatumi)
                {
                    if (promet.TryGetValue(klasId, out var dp))
                    {
                        kumulativ += dp
                            .Where(x => x.Key >= intervalStart && x.Key <= intervalEnd)
                            .Sum(x => x.Value);
                    }

                    serija[intervalLabel] = Math.Max(0, kumulativ);
                }

                // Dodaj samo ako ima podataka > 0
                if (serija.Values.Any(v => v > 0))
                    rezultatDict[vrstaVoca] = serija;
            }

            _logger.LogInformation($"Lager kretanje ucitano: {rezultatDict.Count} vrsta voca, sezona={sezona}, pocetnoStanje={string.Join(", ", pocetnoStanje.Select(x => $"{x.Key}:{x.Value:N0}"))}");
            return rezultatDict;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Greška u UcitajLagerKretanje: {ex.Message}");
            return new Dictionary<string, Dictionary<string, decimal>>();
        }
    }

    /// <summary>
    /// Stanje lagera za jednu vrstu voca (klasifikacijaId), razdeljeno po tipu proizvoda (MagacinID).
    /// Tipovi: Sirovina(2+3), Poluproizvod(5), Gotova roba(6), Kalo i Rastur(7)
    /// Boje su nijanse boje te vrste voca.
    /// </summary>
    public async Task<Dictionary<string, Dictionary<string, decimal>>> UcitajLagerPoTipuProizvodaAsync(int klasifikacijaId, DateTime odDatum, DateTime doDatum, string interval = "dnevno")
    {
        try
        {
            int sezona = odDatum.Month >= 6 ? odDatum.Year : odDatum.Year - 1;
            var pocetakSezone = new DateTime(sezona, 6, 1);

            // Pocetno stanje po MagacinID za ovu klasifikaciju
            var sqlLager = @"
                SELECT
                    a.MagacinID,
                    SUM(ml.Kolicina) as Kolicina
                FROM MagacinLager ml
                JOIN ArtikalInstanca ai ON ml.ArtikalInstancaID = ai.ID
                JOIN Artikal a ON ai.ArtikalID = a.ID
                WHERE a.PrvaKlasifikacijaID = @KlasifikacijaId
                  AND a.MagacinID IN (2, 3, 5, 6, 7)
                GROUP BY a.MagacinID";

            var lagerParams = new DynamicParameters();
            lagerParams.Add("@KlasifikacijaId", klasifikacijaId);
            var lagerRezultat = await _db.QueryAsync<dynamic>(sqlLager, lagerParams);

            var trenutnoPoTipu = new Dictionary<int, decimal>();
            foreach (var red in lagerRezultat)
                trenutnoPoTipu[(int)red.MagacinID] = (decimal)red.Kolicina;

            // Spoji MagacinID 2 i 3 u Sirovina
            decimal sirovinaKolicina = (trenutnoPoTipu.TryGetValue(2, out var m2) ? m2 : 0)
                                     + (trenutnoPoTipu.TryGetValue(3, out var m3) ? m3 : 0);
            var trenutno = new Dictionary<int, decimal>
            {
                { 23, sirovinaKolicina },
                { 5,  trenutnoPoTipu.TryGetValue(5, out var m5) ? m5 : 0 },
                { 6,  trenutnoPoTipu.TryGetValue(6, out var m6) ? m6 : 0 },
                { 7,  trenutnoPoTipu.TryGetValue(7, out var m7) ? m7 : 0 },
            };

            // Promet od pocetka sezone do danas po MagacinID
            var sqlPromet = @"
                SELECT
                    DATE(fm.Datum) as Datum,
                    a.MagacinID,
                    SUM(CASE WHEN LEFT(fm.Dokument, 2) = 'PR' THEN COALESCE(fm.Ulaz, 0) ELSE 0 END) as Ulaz,
                    SUM(CASE WHEN LEFT(fm.Dokument, 2) = 'OT' THEN COALESCE(fm.Izlaz, 0) ELSE 0 END) as Izlaz
                FROM vPrometRobav6 fm
                JOIN Artikal a ON fm.ArtikalID = a.ID
                WHERE fm.DokumentStatus = 3
                  AND fm.ArtikalPrvaKlasifikacijaID = @KlasifikacijaId
                  AND a.MagacinID IN (2, 3, 5, 6, 7)
                  AND fm.Datum >= @PocetakSezone
                  AND fm.Datum <= @DoDatum
                  AND (fm.Ulaz > 0 OR fm.Izlaz > 0)
                GROUP BY DATE(fm.Datum), a.MagacinID
                ORDER BY Datum ASC";

            var prometParams = new DynamicParameters();
            prometParams.Add("@KlasifikacijaId", klasifikacijaId);
            prometParams.Add("@PocetakSezone", pocetakSezone);
            prometParams.Add("@DoDatum", doDatum);
            var prometRezultat = await _db.QueryAsync<dynamic>(sqlPromet, prometParams);

            // Grupisanje prometa: key=tip(23=Sirovina,5,6,7), value=Dictionary<datum, neto>
            var prometPoTipu = new Dictionary<int, Dictionary<DateTime, decimal>>();
            foreach (var red in prometRezultat)
            {
                int magId = (int)red.MagacinID;
                int tip = (magId == 2 || magId == 3) ? 23 : magId; // spoji 2+3 u 23
                DateTime datum = ((DateTime)red.Datum).Date;
                decimal neto = (decimal)red.Ulaz - (decimal)red.Izlaz;

                if (!prometPoTipu.ContainsKey(tip))
                    prometPoTipu[tip] = new Dictionary<DateTime, decimal>();
                prometPoTipu[tip][datum] = prometPoTipu[tip].TryGetValue(datum, out var ex) ? ex + neto : neto;
            }

            // Pocetno stanje = trenutno - neto promet od pocetka sezone do danas
            var danas = DateTime.Today;
            var pocetnoStanje = new Dictionary<int, decimal>();
            foreach (var tip in trenutno.Keys)
            {
                decimal neto = prometPoTipu.TryGetValue(tip, out var dp)
                    ? dp.Where(x => x.Key <= danas).Sum(x => x.Value)
                    : 0;
                pocetnoStanje[tip] = Math.Max(0, trenutno[tip] - neto);
            }

            // Nazivi i boje tipova — nijanse boje vrste voca
            string baseColor = ChartDataHelper.GetFruitColor(ChartDataHelper.MapKlasifikacijaToFruitType(klasifikacijaId));
            var tipInfo = new Dictionary<int, (string Naziv, string Boja)>
            {
                { 23, ("Sirovina",     AdjustColorBrightness(baseColor, 0.9m))  },
                { 5,  ("Poluproizvod", AdjustColorBrightness(baseColor, 0.6m))  },
                { 6,  ("Gotova roba",  AdjustColorBrightness(baseColor, 0.3m))  },
                { 7,  ("Kalo i Rastur",AdjustColorBrightness(baseColor, -0.3m)) },
            };

            var intervalDatumi = GenerisiIntervalDatume(odDatum, doDatum, interval);
            var rezultatDict = new Dictionary<string, Dictionary<string, decimal>>();

            foreach (var (tip, info) in tipInfo)
            {
                decimal kumulativ = pocetnoStanje.TryGetValue(tip, out var ps) ? ps : 0m;

                if (prometPoTipu.TryGetValue(tip, out var dp2))
                    kumulativ += dp2.Where(x => x.Key < odDatum.Date).Sum(x => x.Value);

                var serija = new Dictionary<string, decimal>();
                foreach (var (intervalStart, intervalEnd, label) in intervalDatumi)
                {
                    if (prometPoTipu.TryGetValue(tip, out var dp3))
                        kumulativ += dp3.Where(x => x.Key >= intervalStart && x.Key <= intervalEnd).Sum(x => x.Value);
                    serija[label] = Math.Max(0, kumulativ);
                }

                if (serija.Values.Any(v => v > 0))
                    rezultatDict[$"{info.Naziv}|{info.Boja}"] = serija;
            }

            return rezultatDict;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Greška u UcitajLagerPoTipuProizvoda: {ex.Message}");
            return new Dictionary<string, Dictionary<string, decimal>>();
        }
    }

    /// Tamni ili posvetli hex boju. factor: 0.9=skoro ista, 0.3=tamna, -0.3=jos tamnija
    private static string AdjustColorBrightness(string hex, decimal factor)
    {
        hex = hex.TrimStart('#');
        if (hex.Length != 6) return "#" + hex;
        int r = Convert.ToInt32(hex[..2], 16);
        int g = Convert.ToInt32(hex[2..4], 16);
        int b = Convert.ToInt32(hex[4..6], 16);

        if (factor >= 0)
        {
            // Posvetli ka beloj
            r = (int)(r + (255 - r) * (double)(1 - factor));
            g = (int)(g + (255 - g) * (double)(1 - factor));
            b = (int)(b + (255 - b) * (double)(1 - factor));
        }
        else
        {
            // Potamni ka crnoj
            double f = 1 + (double)factor;
            r = (int)(r * f);
            g = (int)(g * f);
            b = (int)(b * f);
        }

        r = Math.Clamp(r, 0, 255);
        g = Math.Clamp(g, 0, 255);
        b = Math.Clamp(b, 0, 255);
        return $"#{r:X2}{g:X2}{b:X2}";
    }

    private List<(DateTime Start, DateTime End, string Label)> GenerisiIntervalDatume(DateTime odDatum, DateTime doDatum, string interval)
    {
        var lista = new List<(DateTime, DateTime, string)>();

        if (interval == "mesecno")
        {
            var current = new DateTime(odDatum.Year, odDatum.Month, 1);
            while (current <= doDatum)
            {
                var end = new DateTime(current.Year, current.Month, DateTime.DaysInMonth(current.Year, current.Month));
                if (end > doDatum) end = doDatum;
                lista.Add((current, end, current.ToString("MM.yyyy")));
                current = current.AddMonths(1);
            }
        }
        else if (interval == "nedeljno")
        {
            // Pocetak od ponedeljka
            int diff = (int)odDatum.DayOfWeek - (int)DayOfWeek.Monday;
            if (diff < 0) diff += 7;
            var current = odDatum.AddDays(-diff).Date;
            while (current <= doDatum)
            {
                var end = current.AddDays(6);
                if (end > doDatum) end = doDatum;
                lista.Add((current, end, current.ToString("dd.MM.yy")));
                current = current.AddDays(7);
            }
        }
        else // dnevno
        {
            var current = odDatum.Date;
            while (current <= doDatum.Date)
            {
                lista.Add((current, current, current.ToString("dd.MM.yy")));
                current = current.AddDays(1);
            }
        }

        return lista;
    }

    public async Task<Dictionary<string, decimal>> UcitajProdajuGotovihProizvodaAsync(DateTime? odDatum = null, DateTime? doDatum = null)
    {
        try
        {
            var endDatum = doDatum ?? DateTime.Now;
            var startDatum = odDatum ?? endDatum.AddDays(-30);

            var sql = @"
                SELECT
                    COALESCE(a.Naziv, 'Nepoznato') as ArtikalNaziv,
                    COALESCE(SUM(ABS(fm.Kolicina)), 0) as Kolicina
                FROM vPrometFinansijev9 fm
                INNER JOIN Artikal a ON fm.ArtikalID = a.ID
                WHERE fm.Dokument LIKE 'FK-%'
                  AND fm.Datum >= @OdDatum
                  AND fm.Datum <= @DoDatum
                  AND fm.DokumentStatus != 2
                  AND fm.DokumentStatus != 4
                  AND a.MagacinID = 6
                GROUP BY a.Naziv
                ORDER BY Kolicina DESC";

            var parameters = new DynamicParameters();
            parameters.Add("@OdDatum", startDatum);
            parameters.Add("@DoDatum", endDatum);

            var rezultat = await _db.QueryAsync<dynamic>(sql, parameters);

            return rezultat.ToDictionary(
                x => (string)(x.ArtikalNaziv ?? "Nepoznato"),
                x => (decimal)x.Kolicina
            );
        }
        catch (Exception ex)
        {
            _logger.LogInformation($"❌ Greška u UcitajProdajuGotovihProizvoda: {ex.Message}");
            return new Dictionary<string, decimal>();
        }
    }
}
