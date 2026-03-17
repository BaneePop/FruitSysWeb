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

        // ══════════════════════════════════════════════════════════════════════
        // PALETNI LIST PREGLED - 4 TABA
        // ══════════════════════════════════════════════════════════════════════

        public async Task<List<PaletniListPregledRow>> UcitajNabavkuPaletniListova(PaletniListPregledFilter filter)
        {
            try
            {
                var sql = new StringBuilder(@"
                    SELECT
                        pl.ID,
                        pl.Sifra,
                        pl.Kreirano AS Datum,
                        pl.PaletniListTip,
                        pl.Tezina AS Kolicina,
                        pl.BrojAmbalaze,
                        COALESCE(k.Naziv, '') AS Komitent,
                        COALESCE(a.Naziv, '') AS Artikal,
                        COALESCE(pak.Naziv, '') AS Ambalaza,
                        pr.Sifra AS PrijemnicaSifra,
                        NULL AS OtpremnicaSifra,
                        NULL AS RadniNalogSifra,
                        NULL AS EvidencijaRadaSifra
                    FROM PaletniList pl
                    LEFT JOIN Artikal a ON pl.ArtikalID = a.ID
                    LEFT JOIN Komitent k ON pl.KomitentID = k.ID
                    LEFT JOIN Pakovanje pak ON pl.PakovanjeID = pak.ID
                    LEFT JOIN PrijemnicaStavka prs ON pl.PrijemnicaStavkaID = prs.ID
                    LEFT JOIN Prijemnica pr ON prs.PrijemnicaID = pr.ID
                    WHERE pl.PaletniListTip = 1
                ");

                var p = new DynamicParameters();
                ApplyPaletniListFilter(sql, p, filter);
                if (!string.IsNullOrWhiteSpace(filter.TipArtikla))
                {
                    sql.Append(" AND a.MagacinID = @TipArtikla");
                    p.Add("@TipArtikla", filter.TipArtikla.Trim());
                }
                if (!string.IsNullOrWhiteSpace(filter.Prijemnica))
                {
                    sql.Append(" AND pr.Sifra LIKE @Prijemnica");
                    p.Add("@Prijemnica", $"%{filter.Prijemnica.Trim()}%");
                }
                sql.Append(" ORDER BY pl.Kreirano DESC LIMIT 2000");

                var result = await _databaseService.QueryAsync<PaletniListPregledRow>(sql.ToString(), p);
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju Nabavke paletnih listova");
                return new();
            }
        }

        public async Task<List<PaletniListPregledRow>> UcitajProdajuPaletniListova(PaletniListPregledFilter filter)
        {
            try
            {
                var sql = new StringBuilder(@"
                    SELECT
                        pl.ID,
                        pl.Sifra,
                        pl.Kreirano AS Datum,
                        pl.PaletniListTip,
                        pl.Tezina AS Kolicina,
                        pl.BrojAmbalaze,
                        COALESCE(k.Naziv, '') AS Komitent,
                        COALESCE(a.Naziv, '') AS Artikal,
                        COALESCE(pak.Naziv, '') AS Ambalaza,
                        NULL AS PrijemnicaSifra,
                        otp.Sifra AS OtpremnicaSifra,
                        NULL AS RadniNalogSifra,
                        NULL AS EvidencijaRadaSifra,
                        NULL AS Kvalitet
                    FROM PaletniList pl
                    LEFT JOIN Artikal a ON pl.ArtikalID = a.ID
                    LEFT JOIN Komitent k ON pl.KomitentID = k.ID
                    LEFT JOIN Pakovanje pak ON pl.PakovanjeID = pak.ID
                    LEFT JOIN OtpremnicaStavka otps ON pl.OtpremnicaStavkaID = otps.ID
                    LEFT JOIN Otpremnica otp ON otps.OtpremnicaID = otp.ID
                    WHERE pl.PaletniListTip = 3
                ");

                var p = new DynamicParameters();
                ApplyPaletniListFilter(sql, p, filter);
                if (!string.IsNullOrWhiteSpace(filter.Otpremnica))
                {
                    sql.Append(" AND otp.Sifra LIKE @Otpremnica");
                    p.Add("@Otpremnica", $"%{filter.Otpremnica.Trim()}%");
                }
                sql.Append(" ORDER BY pl.Kreirano DESC LIMIT 2000");

                var result = await _databaseService.QueryAsync<PaletniListPregledRow>(sql.ToString(), p);
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju Prodaje paletnih listova");
                return new();
            }
        }

        public async Task<List<PaletniListPregledRow>> UcitajProizvodnjuPaletniListova(PaletniListPregledFilter filter)
        {
            try
            {
                var sql = new StringBuilder(@"
                    SELECT
                        pl.ID,
                        pl.Sifra,
                        pl.Kreirano AS Datum,
                        pl.PaletniListTip,
                        pl.Tezina AS Kolicina,
                        pl.BrojAmbalaze,
                        COALESCE(k.Naziv, '') AS Komitent,
                        COALESCE(a.Naziv, '') AS Artikal,
                        COALESCE(pak.Naziv, '') AS Ambalaza,
                        NULL AS PrijemnicaSifra,
                        NULL AS OtpremnicaSifra,
                        rn.Sifra AS RadniNalogSifra,
                        er.Sifra AS EvidencijaRadaSifra,
                        NULL AS Kvalitet
                    FROM PaletniList pl
                    LEFT JOIN Artikal a ON pl.ArtikalID = a.ID
                    LEFT JOIN Komitent k ON pl.KomitentID = k.ID
                    LEFT JOIN Pakovanje pak ON pl.PakovanjeID = pak.ID
                    LEFT JOIN RadniNalog rn ON pl.RadniNalogID = rn.ID
                    LEFT JOIN EvidencijaRada er ON pl.EvidencijaRadaID = er.ID
                    WHERE pl.PaletniListTip = 2
                ");

                var p = new DynamicParameters();
                ApplyPaletniListFilter(sql, p, filter);
                if (!string.IsNullOrWhiteSpace(filter.RadniNalog))
                {
                    sql.Append(" AND rn.Sifra LIKE @RadniNalog");
                    p.Add("@RadniNalog", $"%{filter.RadniNalog.Trim()}%");
                }
                if (!string.IsNullOrWhiteSpace(filter.EvidencijaRada))
                {
                    sql.Append(" AND er.Sifra LIKE @EvidencijaRada");
                    p.Add("@EvidencijaRada", $"%{filter.EvidencijaRada.Trim()}%");
                }
                sql.Append(" ORDER BY pl.Kreirano DESC LIMIT 2000");

                var result = await _databaseService.QueryAsync<PaletniListPregledRow>(sql.ToString(), p);
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju Proizvodnje paletnih listova");
                return new();
            }
        }

        public async Task<PaletniListPovezanostResult> UcitajPovezanostPaletniListova(string sifraPL)
        {
            try
            {
                // Nađi izvorni paletni list
                var sqlIzvorni = @"
                    SELECT pl.ID AS PaletniListID, pl.Sifra AS PaletniListSifra, pl.Kreirano AS Datum, pl.PaletniListTip,
                           pl.Tezina AS Kolicina, COALESCE(k.Naziv,'') AS Komitent,
                           COALESCE(a.Naziv,'') AS Artikal,
                           pr.Sifra AS PrijemnicaSifra,
                           otp.Sifra AS OtpremnicaSifra,
                           rn.Sifra AS RadniNalogSifra,
                           er.Sifra AS EvidencijaRadaSifra
                    FROM PaletniList pl
                    LEFT JOIN Artikal a ON pl.ArtikalID = a.ID
                    LEFT JOIN Komitent k ON pl.KomitentID = k.ID
                    LEFT JOIN PrijemnicaStavka prs ON pl.PrijemnicaStavkaID = prs.ID
                    LEFT JOIN Prijemnica pr ON prs.PrijemnicaID = pr.ID
                    LEFT JOIN OtpremnicaStavka otps ON pl.OtpremnicaStavkaID = otps.ID
                    LEFT JOIN Otpremnica otp ON otps.OtpremnicaID = otp.ID
                    LEFT JOIN RadniNalog rn ON pl.RadniNalogID = rn.ID
                    LEFT JOIN EvidencijaRada er ON pl.EvidencijaRadaID = er.ID
                    WHERE pl.Sifra = @Sifra
                    LIMIT 1";

                var izvorni = await _databaseService.QueryFirstOrDefaultAsync<PaletniListVezaModel>(
                    sqlIzvorni, new { Sifra = sifraPL });

                if (izvorni == null)
                    return new PaletniListPovezanostResult();

                // Nađi sve TPaletniListID koji su u PaletniListoviPracenje vezani za ovaj PaletniListID
                var sqlPovezani = @"
                    SELECT pl.ID AS PaletniListID, pl.Sifra AS PaletniListSifra, pl.Kreirano AS Datum, pl.PaletniListTip,
                           pl.Tezina AS Kolicina, COALESCE(k.Naziv,'') AS Komitent,
                           COALESCE(a.Naziv,'') AS Artikal,
                           pr.Sifra AS PrijemnicaSifra,
                           otp.Sifra AS OtpremnicaSifra,
                           rn.Sifra AS RadniNalogSifra,
                           er.Sifra AS EvidencijaRadaSifra
                    FROM PaletniListoviPracenje plp
                    JOIN PaletniList pl ON plp.TPaletniListID = pl.ID
                    LEFT JOIN Artikal a ON pl.ArtikalID = a.ID
                    LEFT JOIN Komitent k ON pl.KomitentID = k.ID
                    LEFT JOIN PrijemnicaStavka prs ON pl.PrijemnicaStavkaID = prs.ID
                    LEFT JOIN Prijemnica pr ON prs.PrijemnicaID = pr.ID
                    LEFT JOIN OtpremnicaStavka otps ON pl.OtpremnicaStavkaID = otps.ID
                    LEFT JOIN Otpremnica otp ON otps.OtpremnicaID = otp.ID
                    LEFT JOIN RadniNalog rn ON pl.RadniNalogID = rn.ID
                    LEFT JOIN EvidencijaRada er ON pl.EvidencijaRadaID = er.ID
                    WHERE plp.PaletniListID = @ID
                    ORDER BY pl.PaletniListTip, pl.Kreirano";

                var povezani = await _databaseService.QueryAsync<PaletniListVezaModel>(
                    sqlPovezani, new { ID = izvorni.PaletniListID });

                return new PaletniListPovezanostResult
                {
                    Izvorni = izvorni,
                    Povezani = povezani.ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju povezanosti paletnih listova za {Sifra}", sifraPL);
                return new PaletniListPovezanostResult();
            }
        }

        public async Task<List<PaletniListPregledRow>> UcitajPojedinacniDokument(string tip, string sifra)
        {
            try
            {
                string? whereExtra = tip switch
                {
                    "prijemnica" => " AND pr.Sifra = @Sifra",
                    "otpremnica" => " AND otp.Sifra = @Sifra",
                    "radni_nalog" => " AND rn.Sifra = @Sifra",
                    "evidencija" => " AND er.Sifra = @Sifra",
                    _ => null
                };
                if (whereExtra == null) return new();

                var sql = $@"
                    SELECT pl.ID, pl.Sifra, pl.Kreirano AS Datum, pl.PaletniListTip,
                           pl.Tezina AS Kolicina, pl.BrojAmbalaze,
                           COALESCE(k.Naziv,'') AS Komitent,
                           COALESCE(a.Naziv,'') AS Artikal,
                           COALESCE(pak.Naziv,'') AS Ambalaza,
                           pr.Sifra AS PrijemnicaSifra,
                           otp.Sifra AS OtpremnicaSifra,
                           rn.Sifra AS RadniNalogSifra,
                           er.Sifra AS EvidencijaRadaSifra,
                           NULL AS Kvalitet
                    FROM PaletniList pl
                    LEFT JOIN Artikal a ON pl.ArtikalID = a.ID
                    LEFT JOIN Komitent k ON pl.KomitentID = k.ID
                    LEFT JOIN Pakovanje pak ON pl.PakovanjeID = pak.ID
                    LEFT JOIN PrijemnicaStavka prs ON pl.PrijemnicaStavkaID = prs.ID
                    LEFT JOIN Prijemnica pr ON prs.PrijemnicaID = pr.ID
                    LEFT JOIN OtpremnicaStavka otps ON pl.OtpremnicaStavkaID = otps.ID
                    LEFT JOIN Otpremnica otp ON otps.OtpremnicaID = otp.ID
                    LEFT JOIN RadniNalog rn ON pl.RadniNalogID = rn.ID
                    LEFT JOIN EvidencijaRada er ON pl.EvidencijaRadaID = er.ID
                    WHERE 1=1 {whereExtra}
                    ORDER BY pl.Kreirano DESC";

                var result = await _databaseService.QueryAsync<PaletniListPregledRow>(sql, new { Sifra = sifra });
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju paletnih listova za dokument {Tip}={Sifra}", tip, sifra);
                return new();
            }
        }

        public async Task<List<string>> UcitajKomitentePaletniListova(int tip)
        {
            try
            {
                var sql = @"
                    SELECT DISTINCT k.Naziv
                    FROM PaletniList pl
                    JOIN Komitent k ON pl.KomitentID = k.ID
                    WHERE pl.PaletniListTip = @Tip AND k.Naziv IS NOT NULL AND k.Naziv != ''
                    ORDER BY k.Naziv";
                var result = await _databaseService.QueryAsync<string>(sql, new { Tip = tip });
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju komitenata za PL tip {Tip}", tip);
                return new();
            }
        }

        public async Task<List<string>> UcitajArtiklePaletniListova(int tip)
        {
            try
            {
                var sql = @"
                    SELECT DISTINCT a.Naziv
                    FROM PaletniList pl
                    JOIN Artikal a ON pl.ArtikalID = a.ID
                    WHERE pl.PaletniListTip = @Tip AND a.Naziv IS NOT NULL AND a.Naziv != ''
                    ORDER BY a.Naziv";
                var result = await _databaseService.QueryAsync<string>(sql, new { Tip = tip });
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju artikala za PL tip {Tip}", tip);
                return new();
            }
        }

        public async Task<List<string>> UcitajPrijemniceZaPeriod(DateTime? odDatum, DateTime? doDatum)
        {
            try
            {
                var sql = new StringBuilder(@"
                    SELECT DISTINCT pr.Sifra
                    FROM PaletniList pl
                    JOIN PrijemnicaStavka prs ON pl.PrijemnicaStavkaID = prs.ID
                    JOIN Prijemnica pr ON prs.PrijemnicaID = pr.ID
                    WHERE pl.PaletniListTip = 1");
                var p = new DynamicParameters();
                if (odDatum.HasValue) { sql.Append(" AND pl.Kreirano >= @OdDatum"); p.Add("@OdDatum", odDatum.Value.Date); }
                if (doDatum.HasValue) { sql.Append(" AND pl.Kreirano < @DoDatum"); p.Add("@DoDatum", doDatum.Value.Date.AddDays(1)); }
                sql.Append(" ORDER BY pr.Sifra");
                var result = await _databaseService.QueryAsync<string>(sql.ToString(), p);
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju prijemnica za period");
                return new();
            }
        }

        public async Task<List<string>> UcitajOtpremnicaZaPeriod(DateTime? odDatum, DateTime? doDatum)
        {
            try
            {
                var sql = new StringBuilder(@"
                    SELECT DISTINCT otp.Sifra
                    FROM PaletniList pl
                    JOIN OtpremnicaStavka otps ON pl.OtpremnicaStavkaID = otps.ID
                    JOIN Otpremnica otp ON otps.OtpremnicaID = otp.ID
                    WHERE pl.PaletniListTip = 3");
                var p = new DynamicParameters();
                if (odDatum.HasValue) { sql.Append(" AND pl.Kreirano >= @OdDatum"); p.Add("@OdDatum", odDatum.Value.Date); }
                if (doDatum.HasValue) { sql.Append(" AND pl.Kreirano < @DoDatum"); p.Add("@DoDatum", doDatum.Value.Date.AddDays(1)); }
                sql.Append(" ORDER BY otp.Sifra");
                var result = await _databaseService.QueryAsync<string>(sql.ToString(), p);
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju otpremnica za period");
                return new();
            }
        }

        public async Task<List<string>> UcitajRadneNalogeZaPeriod(DateTime? odDatum, DateTime? doDatum)
        {
            try
            {
                var sql = new StringBuilder(@"
                    SELECT DISTINCT rn.Sifra
                    FROM PaletniList pl
                    JOIN RadniNalog rn ON pl.RadniNalogID = rn.ID
                    WHERE pl.PaletniListTip = 2");
                var p = new DynamicParameters();
                if (odDatum.HasValue) { sql.Append(" AND pl.Kreirano >= @OdDatum"); p.Add("@OdDatum", odDatum.Value.Date); }
                if (doDatum.HasValue) { sql.Append(" AND pl.Kreirano < @DoDatum"); p.Add("@DoDatum", doDatum.Value.Date.AddDays(1)); }
                sql.Append(" ORDER BY rn.Sifra");
                var result = await _databaseService.QueryAsync<string>(sql.ToString(), p);
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju radnih naloga za period");
                return new();
            }
        }

        // Helper: zajednički filteri za sve tabove
        private void ApplyPaletniListFilter(StringBuilder sql, DynamicParameters p, PaletniListPregledFilter filter)
        {
            if (filter.OdDatum.HasValue)
            {
                sql.Append(" AND pl.Kreirano >= @OdDatum");
                p.Add("@OdDatum", filter.OdDatum.Value.Date);
            }
            if (filter.DoDatum.HasValue)
            {
                sql.Append(" AND pl.Kreirano < @DoDatum");
                p.Add("@DoDatum", filter.DoDatum.Value.Date.AddDays(1));
            }
            if (!string.IsNullOrWhiteSpace(filter.SifraPL))
            {
                sql.Append(" AND pl.Sifra LIKE @SifraPL");
                p.Add("@SifraPL", $"%{filter.SifraPL.Trim()}%");
            }
            if (!string.IsNullOrWhiteSpace(filter.Komitent))
            {
                sql.Append(" AND k.Naziv LIKE @Komitent");
                p.Add("@Komitent", $"%{filter.Komitent.Trim()}%");
            }
            if (!string.IsNullOrWhiteSpace(filter.Artikal))
            {
                sql.Append(" AND a.Naziv LIKE @Artikal");
                p.Add("@Artikal", $"%{filter.Artikal.Trim()}%");
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

        // ══════════════════════════════════════════════════════════════════════
        // KVALITET TAB
        // ══════════════════════════════════════════════════════════════════════

        public async Task<List<PaletniListKvalitetRow>> UcitajKvalitetIzvestaj(PaletniListKvalitetFilter filter)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filter.ArtikalID))
                    return new();

                var sql = new StringBuilder(@"
                    SELECT
                        pr.Sifra AS PrijemnicaSifra,
                        COALESCE(k.Naziv, '') AS Komitent,
                        COALESCE(a.Naziv, '') AS Artikal,
                        SUM(CASE WHEN aStavka.Naziv LIKE '%I klasa%'
                                   OR aStavka.Naziv LIKE '%I Klasa%'
                                   OR aStavka.Naziv LIKE '%1. klasa%'
                                   OR aStavka.Naziv LIKE '%1.klasa%'
                             THEN pls.Kolicina ELSE 0 END) AS KlasaI,
                        SUM(CASE WHEN aStavka.Naziv LIKE '%II klasa%'
                                   OR aStavka.Naziv LIKE '%II Klasa%'
                                   OR aStavka.Naziv LIKE '%2. klasa%'
                                   OR aStavka.Naziv LIKE '%2.klasa%'
                             THEN pls.Kolicina ELSE 0 END) AS KlasaII,
                        SUM(CASE WHEN aStavka.Naziv LIKE '%III klasa%'
                                   OR aStavka.Naziv LIKE '%III Klasa%'
                                   OR aStavka.Naziv LIKE '%3. klasa%'
                                   OR aStavka.Naziv LIKE '%3.klasa%'
                             THEN pls.Kolicina ELSE 0 END) AS KlasaIII
                    FROM PaletniListStavka pls
                    JOIN PaletniList pl ON pls.PaletniListID = pl.ID
                    JOIN Artikal a ON pl.ArtikalID = a.ID
                    JOIN Artikal aStavka ON pls.ArtikalID = aStavka.ID
                    LEFT JOIN Komitent k ON pl.KomitentID = k.ID
                    LEFT JOIN PrijemnicaStavka prs ON pl.PrijemnicaStavkaID = prs.ID
                    LEFT JOIN Prijemnica pr ON prs.PrijemnicaID = pr.ID
                    WHERE pl.PaletniListTip = 1
                      AND a.MagacinID IN (2, 3)
                      AND pl.ArtikalID = @ArtikalID
                ");

                var p = new DynamicParameters();
                p.Add("@ArtikalID", filter.ArtikalID);

                if (filter.OdDatum.HasValue)
                {
                    sql.Append(" AND pls.Kreirano >= @OdDatum");
                    p.Add("@OdDatum", filter.OdDatum.Value.Date);
                }
                if (filter.DoDatum.HasValue)
                {
                    sql.Append(" AND pls.Kreirano < @DoDatum");
                    p.Add("@DoDatum", filter.DoDatum.Value.Date.AddDays(1));
                }
                if (!string.IsNullOrWhiteSpace(filter.Komitent))
                {
                    sql.Append(" AND k.Naziv = @Komitent");
                    p.Add("@Komitent", filter.Komitent.Trim());
                }
                if (!string.IsNullOrWhiteSpace(filter.Prijemnica))
                {
                    sql.Append(" AND pr.Sifra = @Prijemnica");
                    p.Add("@Prijemnica", filter.Prijemnica.Trim());
                }

                sql.Append(@"
                    GROUP BY pr.Sifra, k.Naziv, a.Naziv
                    ORDER BY pr.Sifra, k.Naziv
                    LIMIT 2000
                ");

                var result = await _databaseService.QueryAsync<PaletniListKvalitetRow>(sql.ToString(), p);
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju Kvalitet izveštaja");
                return new();
            }
        }

        public async Task<List<(string ID, string Naziv)>> UcitajArtikleZaKvalitet()
        {
            try
            {
                const string sql = @"
                    SELECT DISTINCT a.ID, a.Naziv
                    FROM Artikal a
                    WHERE a.MagacinID IN (2, 3)
                      AND EXISTS (
                          SELECT 1 FROM PaletniList pl WHERE pl.ArtikalID = a.ID AND pl.PaletniListTip = 1
                      )
                    ORDER BY a.Naziv
                ";
                var rows = await _databaseService.QueryAsync<(long ID, string Naziv)>(sql, null);
                return rows.Select(r => (r.ID.ToString(), r.Naziv)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju artikala za Kvalitet");
                return new();
            }
        }

        public async Task<List<string>> UcitajKomitentePaletniListovaKvalitet()
        {
            try
            {
                const string sql = @"
                    SELECT DISTINCT k.Naziv
                    FROM PaletniList pl
                    JOIN Komitent k ON pl.KomitentID = k.ID
                    JOIN Artikal a ON pl.ArtikalID = a.ID
                    WHERE pl.PaletniListTip = 1 AND a.MagacinID IN (2, 3)
                    ORDER BY k.Naziv
                ";
                var result = await _databaseService.QueryAsync<string>(sql, null);
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju komitenata za Kvalitet");
                return new();
            }
        }

        public async Task<List<string>> UcitajPrijemniceZaPeriodKvalitet(DateTime? odDatum, DateTime? doDatum)
        {
            try
            {
                var sql = new StringBuilder(@"
                    SELECT DISTINCT pr.Sifra
                    FROM PaletniList pl
                    JOIN Artikal a ON pl.ArtikalID = a.ID
                    LEFT JOIN PrijemnicaStavka prs ON pl.PrijemnicaStavkaID = prs.ID
                    LEFT JOIN Prijemnica pr ON prs.PrijemnicaID = pr.ID
                    WHERE pl.PaletniListTip = 1 AND a.MagacinID IN (2, 3) AND pr.Sifra IS NOT NULL
                ");
                var p = new DynamicParameters();
                if (odDatum.HasValue)
                {
                    sql.Append(" AND pl.Kreirano >= @OdDatum");
                    p.Add("@OdDatum", odDatum.Value.Date);
                }
                if (doDatum.HasValue)
                {
                    sql.Append(" AND pl.Kreirano < @DoDatum");
                    p.Add("@DoDatum", doDatum.Value.Date.AddDays(1));
                }
                sql.Append(" ORDER BY pr.Sifra");
                var result = await _databaseService.QueryAsync<string>(sql.ToString(), p);
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju prijemnica za Kvalitet");
                return new();
            }
        }
    }
}
