using FruitSysWeb.Models;
using FruitSysWeb.Services.Interfaces;
using FruitSysWeb.Services.Models.Requests;
using FruitSysWeb.Services.Core;
using Dapper;
using System.Text;
using System.Data.SqlTypes;

namespace FruitSysWeb.Services.Implementations.IzvestajService
{
    public class PaletniListService : BaseService, IPaletniListService
    {
        private readonly DatabaseService _databaseService;
        private readonly ILogger<PaletniListService> _logger;

        public PaletniListService(
            DatabaseService databaseService,
            ILogger<PaletniListService> logger)
        {
            _databaseService = databaseService;
            _logger = logger;
        }

        /// <summary>
        /// Učitava sve prijeme za današnji dan (Sifra počinje sa 'N-')
        /// </summary>
        public async Task<List<PaletniListModel>> UcitajDanasnjePrijeme()
        {
            var filterRequest = new FilterRequest
            {
                OdDatum = DateTime.Today,
                DoDatum = DateTime.Today
            };
            return await UcitajPrijemeZaPeriod(filterRequest);
        }

        /// <summary>
        /// Učitava prijeme za period
        /// ✅ REFACTORED: Using BaseService helper methods
        /// </summary>
        public async Task<List<PaletniListModel>> UcitajPrijemeZaPeriod(FilterRequest filterRequest)
        {
            try
            {
                var sql = CreateSqlBuilder(@"
                    SELECT
                        pl.ID,
                        pl.Sifra,
                        pl.Tezina,
                        pl.Aktivan,
                        pl.DatumKreiranja,
                        pl.DatumZatvaranja,
                        pl.BrojAmbalaze,
                        pl.DokumentStatus,
                        pl.BrutoTezina,
                        pl.ArtikalID,
                        pl.KomitentID,
                        pl.PaletniListTip,
                        a.Naziv as Artikal,
                        k.Naziv as Komitent
                    FROM PaletniList pl
                    LEFT JOIN Artikal a ON pl.ArtikalID = a.ID
                    LEFT JOIN Komitent k ON pl.KomitentID = k.ID
                    WHERE pl.Sifra LIKE 'N-%'
                      AND a.MagacinID IN (2, 3, 5)
                ");

                var parameters = CreateParameters();

                // Apply date filter with 4-hour offset for working day
                ApplyDateFilterWithOffset(sql, parameters, filterRequest,
                    dateColumnName: "pl.DatumKreiranja",
                    odDatumHourOffset: 4,
                    doDatumHourOffset: 4,
                    doDatumDayOffset: 1);

                sql.Append(" ORDER BY pl.DatumKreiranja DESC");

                var result = await _databaseService.QueryAsync<PaletniListModel>(
                    sql.ToString(),
                    parameters
                );

                _logger.LogInformation($"Učitano {result.Count()} prijema za period {filterRequest.OdDatum:dd.MM.yyyy} - {filterRequest.DoDatum:dd.MM.yyyy}");
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Greška pri učitavanju prijema za period {filterRequest.OdDatum:dd.MM.yyyy} - {filterRequest.DoDatum:dd.MM.yyyy}");
                return new List<PaletniListModel>();
            }
        }

        /// <summary>
        /// Učitava ukupne količine prijema po vrstama voća za današnji dan
        /// </summary>
        public async Task<List<PrijemStatistikaModel>> UcitajStatistikuPrijemaPoVocu()
        {
            var filterRequest = new FilterRequest
            {
                OdDatum = DateTime.Today,
                DoDatum = DateTime.Today
            };
            return await UcitajStatistikuPrijemaPoVocuPeriod(filterRequest);
        }

        /// <summary>
        /// Učitava statistiku prijema po voću u periodu
        /// ✅ REFACTORED: Using BaseService helper methods
        /// </summary>
        public async Task<List<PrijemStatistikaModel>> UcitajStatistikuPrijemaPoVocuPeriod(FilterRequest filterRequest)
        {
            try
            {
                var sql = CreateSqlBuilder(@"
                    SELECT
                        a.Naziv as Voce,
                        SUM(pl.Tezina) as UkupnaKolicina,
                        COUNT(DISTINCT pl.ID) as BrojPrijema
                    FROM PaletniList pl
                    LEFT JOIN Artikal a ON pl.ArtikalID = a.ID
                    WHERE pl.Sifra LIKE 'N-%'
                      AND a.MagacinID IN (2, 3, 5)
                ");

                var parameters = CreateParameters();

                // Apply date filter with 4-hour offset
                ApplyDateFilterWithOffset(sql, parameters, filterRequest,
                    dateColumnName: "pl.DatumKreiranja",
                    odDatumHourOffset: 4,
                    doDatumHourOffset: 4,
                    doDatumDayOffset: 1);

                sql.Append(@"
                    GROUP BY a.ID, a.Naziv
                    HAVING SUM(pl.Tezina) > 0
                    ORDER BY UkupnaKolicina DESC
                ");

                var result = await _databaseService.QueryAsync<PrijemStatistikaModel>(
                    sql.ToString(),
                    parameters
                );

                var statistike = result.ToList();

                // Za svaku vrstu voća, učitaj dobavljače
                foreach (var stat in statistike)
                {
                    stat.Dobavljaci = await UcitajDobavljacePoVocuPeriod(stat.Voce, filterRequest);
                }

                _logger.LogInformation($"Učitano {statistike.Count} vrsta voća sa statistikama za period");
                return statistike;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju statistike prijema po voću za period");
                return new List<PrijemStatistikaModel>();
            }
        }

        /// <summary>
        /// Helper metoda - učitava dobavljače za specifičnu vrstu voća
        /// </summary>
        private async Task<List<PrijemPoDobavljacuModel>> UcitajDobavljacePoVocu(string voce)
        {
            var filterRequest = new FilterRequest
            {
                OdDatum = DateTime.Today,
                DoDatum = DateTime.Today
            };
            return await UcitajDobavljacePoVocuPeriod(voce, filterRequest);
        }

        /// <summary>
        /// Helper metoda - učitava dobavljače za specifičnu klasifikaciju voća (Malina, Kupina, itd.)
        /// </summary>
        private async Task<List<PrijemPoDobavljacuModel>> UcitajDobavljacePoKlasifikaciji(string klasifikacija)
        {
            try
            {
                var sql = CreateSqlBuilder();
                sql.Append(@"
                    SELECT
                        COALESCE(k.Naziv, 'Nepoznat dobavljač') as Dobavljac,
                        SUM(pl.Tezina) as Kolicina,
                        GROUP_CONCAT(DISTINCT a.Naziv ORDER BY a.Naziv SEPARATOR ', ') as Artikli
                    FROM PaletniList pl
                    LEFT JOIN Artikal a ON pl.ArtikalID = a.ID
                    LEFT JOIN ArtikalKlasifikacija ak ON a.PrvaKlasifikacijaID = ak.ID
                    LEFT JOIN Komitent k ON pl.KomitentID = k.ID
                    WHERE pl.Sifra LIKE 'N-%'
                      AND DATE(pl.DatumKreiranja) = @Danas
                      AND ak.Naziv = @Klasifikacija
                      AND a.MagacinID IN (2, 3, 5)
                    GROUP BY k.ID, k.Naziv
                    HAVING SUM(pl.Tezina) > 0
                    ORDER BY Kolicina DESC
                ");

                var result = await _databaseService.QueryAsync<PrijemPoDobavljacuModel>(
                    sql.ToString(),
                    new {
                        Klasifikacija = klasifikacija,
                        Danas = DateTime.Today
                    }
                );

                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Greška pri učitavanju dobavljača za klasifikaciju: {klasifikacija}");
                return new List<PrijemPoDobavljacuModel>();
            }
        }

        /// <summary>
        /// Helper metoda - učitava dobavljače za specifičnu klasifikaciju voća u periodu
        /// </summary>
        private async Task<List<PrijemPoDobavljacuModel>> UcitajDobavljacePoKlasifikacijiPeriod(string klasifikacija, FilterRequest filterRequest)
        {
            try
            {
                var sql = CreateSqlBuilder();
                sql.Append(@"
                    SELECT
                        COALESCE(k.Naziv, 'Nepoznat dobavljač') as Dobavljac,
                        SUM(pl.Tezina) as Kolicina,
                        GROUP_CONCAT(DISTINCT a.Naziv ORDER BY a.Naziv SEPARATOR ', ') as Artikli
                    FROM PaletniList pl
                    LEFT JOIN Artikal a ON pl.ArtikalID = a.ID
                    LEFT JOIN ArtikalKlasifikacija ak ON a.PrvaKlasifikacijaID = ak.ID
                    LEFT JOIN Komitent k ON pl.KomitentID = k.ID
                    WHERE pl.Sifra LIKE 'N-%'
                      AND ak.Naziv = @Klasifikacija
                      AND a.MagacinID IN (2, 3, 5)
                ");

                object? parameters = null;

                if (filterRequest.OdDatum.HasValue && filterRequest.DoDatum.HasValue)
                {
                    var adjustedOdDatum = filterRequest.OdDatum.Value.AddHours(4);
                    var adjustedDoDatum = filterRequest.DoDatum.Value.AddDays(1).AddHours(4);

                    sql.Append(" AND pl.DatumKreiranja >= @OdDatum");
                    sql.Append(" AND pl.DatumKreiranja < @DoDatum");

                    parameters = new {
                        Klasifikacija = klasifikacija,
                        OdDatum = adjustedOdDatum,
                        DoDatum = adjustedDoDatum
                    };
                }
                else if (filterRequest.OdDatum.HasValue)
                {
                    var adjustedOdDatum = filterRequest.OdDatum.Value.AddHours(4);
                    sql.Append(" AND pl.DatumKreiranja >= @OdDatum");

                    parameters = new {
                        Klasifikacija = klasifikacija,
                        OdDatum = adjustedOdDatum
                    };
                }
                else if (filterRequest.DoDatum.HasValue)
                {
                    var adjustedDoDatum = filterRequest.DoDatum.Value.AddDays(1).AddHours(4);
                    sql.Append(" AND pl.DatumKreiranja < @DoDatum");

                    parameters = new {
                        Klasifikacija = klasifikacija,
                        DoDatum = adjustedDoDatum
                    };
                }
                else
                {
                    parameters = new { Klasifikacija = klasifikacija };
                }

                sql.Append(@"
                    GROUP BY k.ID, k.Naziv
                    HAVING SUM(pl.Tezina) > 0
                    ORDER BY Kolicina DESC
                ");

                var result = await _databaseService.QueryAsync<PrijemPoDobavljacuModel>(
                    sql.ToString(),
                    parameters
                );

                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Greška pri učitavanju dobavljača za klasifikaciju u periodu: {klasifikacija}");
                return new List<PrijemPoDobavljacuModel>();
            }
        }

        /// <summary>
        /// Helper metoda - učitava dobavljače za specifičnu vrstu voća u periodu
        /// ✅ ISPRAVKA: Anonymous object umesto Dictionary
        /// </summary>
        private async Task<List<PrijemPoDobavljacuModel>> UcitajDobavljacePoVocuPeriod(string voce, FilterRequest filterRequest)
        {
            try
            {
                var sql = CreateSqlBuilder();
                sql.Append(@"
                    SELECT 
                        COALESCE(k.Naziv, 'Nepoznat dobavljač') as Dobavljac,
                        SUM(pl.Tezina) as Kolicina
                    FROM PaletniList pl
                    LEFT JOIN Artikal a ON pl.ArtikalID = a.ID
                    LEFT JOIN Komitent k ON pl.KomitentID = k.ID
                    WHERE pl.Sifra LIKE 'N-%'
                      AND a.Naziv = @Voce
                      AND a.MagacinID IN (2, 3, 5)
                ");

                // ✅ ISPRAVKA: Dynamic object
                object? parameters = null;

                if (filterRequest.OdDatum.HasValue && filterRequest.DoDatum.HasValue)
                {
                    var adjustedOdDatum = filterRequest.OdDatum.Value.AddHours(4);
                    var adjustedDoDatum = filterRequest.DoDatum.Value.AddDays(1).AddHours(4);
                    
                    sql.Append(" AND pl.DatumKreiranja >= @OdDatum");
                    sql.Append(" AND pl.DatumKreiranja < @DoDatum");
                    
                    parameters = new { 
                        Voce = voce,
                        OdDatum = adjustedOdDatum,
                        DoDatum = adjustedDoDatum
                    };
                }
                else if (filterRequest.OdDatum.HasValue)
                {
                    var adjustedOdDatum = filterRequest.OdDatum.Value.AddHours(4);
                    sql.Append(" AND pl.DatumKreiranja >= @OdDatum");
                    
                    parameters = new { 
                        Voce = voce,
                        OdDatum = adjustedOdDatum 
                    };
                }
                else if (filterRequest.DoDatum.HasValue)
                {
                    var adjustedDoDatum = filterRequest.DoDatum.Value.AddDays(1).AddHours(4);
                    sql.Append(" AND pl.DatumKreiranja < @DoDatum");
                    
                    parameters = new { 
                        Voce = voce,
                        DoDatum = adjustedDoDatum 
                    };
                }
                else
                {
                    parameters = new { Voce = voce };
                }

                sql.Append(@"
                    GROUP BY k.ID, k.Naziv
                    HAVING SUM(pl.Tezina) > 0
                    ORDER BY Kolicina DESC
                ");

                var result = await _databaseService.QueryAsync<PrijemPoDobavljacuModel>(
                    sql.ToString(),
                    parameters
                );

                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Greška pri učitavanju dobavljača za voće: {voce} u periodu");
                return new List<PrijemPoDobavljacuModel>();
            }
        }

        /// <summary>
        /// Učitava ukupnu težinu prijema u periodu
        /// ✅ ISPRAVKA: Anonymous object umesto Dictionary
        /// </summary>
        public async Task<decimal> UcitajUkupnuTezinuPeriod(FilterRequest filterRequest)
        {
            try
            {
                var sql = CreateSqlBuilder();
                sql.Append(@"
                    SELECT COALESCE(SUM(pl.Tezina), 0)
                    FROM PaletniList pl
                    LEFT JOIN Artikal a ON pl.ArtikalID = a.ID
                    WHERE pl.Sifra LIKE 'N-%'
                      AND a.MagacinID IN (2, 3, 5)
                ");

                var parameters = CreateParameters();

                // Apply date filter with 4-hour offset
                ApplyDateFilterWithOffset(sql, parameters, filterRequest,
                    dateColumnName: "pl.DatumKreiranja",
                    odDatumHourOffset: 4,
                    doDatumHourOffset: 4,
                    doDatumDayOffset: 1);

                return await _databaseService.ExecuteScalarAsync<decimal>(
                    sql.ToString(),
                    parameters
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju ukupne težine u periodu");
                return 0;
            }
        }

        /// <summary>
        /// Učitava prijem po voću i dobavljačima u periodu
        /// ✅ ISPRAVKA: Anonymous object umesto Dictionary
        /// </summary>
        public async Task<Dictionary<string, List<PrijemPoDobavljacuModel>>> UcitajPrijemPoVocuIDobavljacimaPeriod(FilterRequest filterRequest)
        {
            try
            {
                // Grupisanje po PrvaKlasifikacijaID (vrsta voća: Malina, Kupina, Višnja, itd.)
                var sqlVoce = new StringBuilder();
                sqlVoce.Append(@"
                    SELECT DISTINCT ak.Naziv as Voce
                    FROM PaletniList pl
                    LEFT JOIN Artikal a ON pl.ArtikalID = a.ID
                    LEFT JOIN ArtikalKlasifikacija ak ON a.PrvaKlasifikacijaID = ak.ID
                    LEFT JOIN Komitent k ON pl.KomitentID = k.ID
                    WHERE pl.Sifra LIKE 'N-%'
                      AND a.MagacinID IN (2, 3, 5)
                      AND ak.Naziv IS NOT NULL
                ");

                // ✅ ISPRAVKA: Dynamic object
                object? parameters = null;

                if (filterRequest.OdDatum.HasValue && filterRequest.DoDatum.HasValue)
                {
                    var adjustedOdDatum = filterRequest.OdDatum.Value.AddHours(4);
                    var adjustedDoDatum = filterRequest.DoDatum.Value.AddDays(1).AddHours(4);
                    
                    sqlVoce.Append(" AND pl.DatumKreiranja >= @OdDatum");
                    sqlVoce.Append(" AND pl.DatumKreiranja < @DoDatum");
                    
                    parameters = new { 
                        OdDatum = adjustedOdDatum,
                        DoDatum = adjustedDoDatum
                    };
                }
                else if (filterRequest.OdDatum.HasValue)
                {
                    var adjustedOdDatum = filterRequest.OdDatum.Value.AddHours(4);
                    sqlVoce.Append(" AND pl.DatumKreiranja >= @OdDatum");
                    
                    parameters = new { OdDatum = adjustedOdDatum };
                }
                else if (filterRequest.DoDatum.HasValue)
                {
                    var adjustedDoDatum = filterRequest.DoDatum.Value.AddDays(1).AddHours(4);
                    sqlVoce.Append(" AND pl.DatumKreiranja < @DoDatum");
                    
                    parameters = new { DoDatum = adjustedDoDatum };
                }

                sqlVoce.Append(@"
                    ORDER BY
                        CASE
                            WHEN ak.Naziv LIKE '%Malina%' THEN 1
                            WHEN ak.Naziv LIKE '%Kupina%' THEN 2
                            WHEN ak.Naziv LIKE '%Višnja%' OR ak.Naziv LIKE '%Visnja%' THEN 3
                            WHEN ak.Naziv LIKE '%Šljiva%' OR ak.Naziv LIKE '%Sljiva%' THEN 4
                            WHEN ak.Naziv LIKE '%Borovnica%' THEN 5
                            WHEN ak.Naziv LIKE '%Kajsija%' THEN 6
                            WHEN ak.Naziv LIKE '%Jagoda%' THEN 7
                            ELSE 8
                        END, ak.Naziv");

                var voceList = await _databaseService.QueryAsync<string>(
                    sqlVoce.ToString(),
                    parameters
                );

                var rezultat = new Dictionary<string, List<PrijemPoDobavljacuModel>>();

                foreach (var voce in voceList)
                {
                    var dobavljaci = await UcitajDobavljacePoKlasifikacijiPeriod(voce, filterRequest);
                    if (dobavljaci.Any())
                    {
                        rezultat[voce] = dobavljaci;
                    }
                }

                _logger.LogInformation($"Učitano {rezultat.Count} vrsta voća sa dobavljačima za period");
                return rezultat;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju prijema po voću i dobavljačima za period");
                return new Dictionary<string, List<PrijemPoDobavljacuModel>>();
            }
        }

        public Task<List<PaletniListModel>> UcitajPrijemePoDatumu(DateTime datum)
        {
            throw new NotImplementedException();
        }

        public async Task<Dictionary<string, List<PrijemPoDobavljacuModel>>> UcitajPrijemPoVocuIDobavljacima()
        {
            try
            {
                // Grupisanje po PrvaKlasifikacijaID (vrsta voća: Malina, Kupina, Višnja, itd.)
                var sqlVoce = @"
                    SELECT DISTINCT ak.Naziv as Voce
                    FROM PaletniList pl
                    LEFT JOIN Artikal a ON pl.ArtikalID = a.ID
                    LEFT JOIN ArtikalKlasifikacija ak ON a.PrvaKlasifikacijaID = ak.ID
                    WHERE pl.Sifra LIKE 'N-%'
                      AND DATE(pl.DatumKreiranja) = @Danas
                      AND ak.Naziv IS NOT NULL
                      AND a.MagacinID IN (2, 3, 5)
                    ORDER BY
                        CASE
                            WHEN ak.Naziv LIKE '%Malina%' THEN 1
                            WHEN ak.Naziv LIKE '%Kupina%' THEN 2
                            WHEN ak.Naziv LIKE '%Višnja%' OR ak.Naziv LIKE '%Visnja%' THEN 3
                            WHEN ak.Naziv LIKE '%Šljiva%' OR ak.Naziv LIKE '%Sljiva%' THEN 4
                            WHEN ak.Naziv LIKE '%Borovnica%' THEN 5
                            WHEN ak.Naziv LIKE '%Kajsija%' THEN 6
                            WHEN ak.Naziv LIKE '%Jagoda%' THEN 7
                            ELSE 8
                        END, ak.Naziv";

                var voceList = await _databaseService.QueryAsync<string>(
                    sqlVoce,
                    new { Danas = DateTime.Today }
                );

                var rezultat = new Dictionary<string, List<PrijemPoDobavljacuModel>>();

                foreach (var voce in voceList)
                {
                    var dobavljaci = await UcitajDobavljacePoKlasifikaciji(voce);
                    if (dobavljaci.Any())
                    {
                        rezultat[voce] = dobavljaci;
                    }
                }

                _logger.LogInformation($"Učitano {rezultat.Count} vrsta voća sa dobavljačima");
                return rezultat;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju prijema po voću i dobavljačima");
                return new Dictionary<string, List<PrijemPoDobavljacuModel>>();
            }
        }

        public async Task<int> UcitajBrojAktivnihPrijema()
        {
            try
            {
                var sql = @"
                    SELECT COUNT(*)
                    FROM PaletniList pl
                    LEFT JOIN Artikal a ON pl.ArtikalID = a.ID
                    WHERE pl.Sifra LIKE 'N-%'
                      AND DATE(pl.DatumKreiranja) = @Danas
                      AND a.MagacinID IN (2, 3, 5)
                    ";

                return await _databaseService.ExecuteScalarAsync<int>(
                    sql,
                    new { Danas = DateTime.Today }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri brojanju aktivnih prijema");
                return 0;
            }
        }

        public async Task<decimal> UcitajUkupnuTezinoZaDan()
        {
            try
            {
                var sql = @"
                    SELECT COALESCE(SUM(pl.Tezina), 0)
                    FROM PaletniList pl
                    LEFT JOIN Artikal a ON pl.ArtikalID = a.ID
                    WHERE pl.Sifra LIKE 'N-%'
                      AND DATE(pl.DatumKreiranja) = @Danas
                      AND a.MagacinID IN (2, 3, 5)
                    ";

                return await _databaseService.ExecuteScalarAsync<decimal>(
                    sql,
                    new { Danas = DateTime.Today }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju ukupne težine");
                return 0;
            }
        }

        /// <summary>
        /// ✅ ISPRAVKA: Anonymous object umesto Dictionary
        /// </summary>
        public async Task<int> UcitajBrojAktivnihPrijemaPeriod(FilterRequest filterRequest)
        {
            try
            {
                var sql = CreateSqlBuilder();
                sql.Append(@"
                    SELECT COUNT(*)
                    FROM PaletniList pl
                    LEFT JOIN Artikal a ON pl.ArtikalID = a.ID
                    WHERE pl.Sifra LIKE 'N-%'
                      AND a.MagacinID IN (2, 3, 5)
                ");

                var parameters = CreateParameters();

                // Apply date filter with 4-hour offset
                ApplyDateFilterWithOffset(sql, parameters, filterRequest,
                    dateColumnName: "pl.DatumKreiranja",
                    odDatumHourOffset: 4,
                    doDatumHourOffset: 4,
                    doDatumDayOffset: 1);

                return await _databaseService.ExecuteScalarAsync<int>(
                    sql.ToString(),
                    parameters
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri brojanju aktivnih prijema u periodu");
                return 0;
            }
        }

        /// <summary>
        /// ✅ ISPRAVKA: Implementacija sa DateTime parametrima
        /// </summary>
        public async Task<List<PrijemStatistikaModel>> UcitajStatistikuPrijemaPoVocuPeriod(DateTime? odDatum, DateTime? doDatum)
        {
            var filterRequest = new FilterRequest
            {
                OdDatum = odDatum,
                DoDatum = doDatum
            };
            return await UcitajStatistikuPrijemaPoVocuPeriod(filterRequest);
        }

        /// <summary>
        /// ✅ ISPRAVKA: Implementacija sa DateTime parametrima
        /// </summary>
        public async Task<decimal> UcitajUkupnuTezinuPeriod(DateTime? odDatum, DateTime? doDatum)
        {
            var filterRequest = new FilterRequest
            {
                OdDatum = odDatum,
                DoDatum = doDatum
            };
            return await UcitajUkupnuTezinuPeriod(filterRequest);
        }

        /// <summary>
        /// ✅ ISPRAVKA: Implementacija sa DateTime parametrima
        /// </summary>
        public async Task<Dictionary<string, List<PrijemPoDobavljacuModel>>> UcitajPrijemPoVocuIDobavljacimaPeriod(DateTime? odDatum, DateTime? doDatum)
        {
            var filterRequest = new FilterRequest
            {
                OdDatum = odDatum,
                DoDatum = doDatum
            };
            return await UcitajPrijemPoVocuIDobavljacimaPeriod(filterRequest);
        }
    }
}
