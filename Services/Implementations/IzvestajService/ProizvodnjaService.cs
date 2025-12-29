using FruitSysWeb.Models;
using FruitSysWeb.Services.Interfaces;
using FruitSysWeb.Services.Models.Requests;
using FruitSysWeb.Services.Core;
using System.Text;
using Microsoft.Extensions.Logging;

namespace FruitSysWeb.Services.Implementations.IzvestajService
{
    public class ProizvodnjaService : BaseService, IProizvodnjaService
    {
        private readonly DatabaseService _databaseService;
        private readonly ILogger<ProizvodnjaService> _logger;
        private readonly CacheService _cacheService;

        public ProizvodnjaService(DatabaseService databaseService,
            ILogger<ProizvodnjaService> logger,
            CacheService cacheService)
        {
            _databaseService = databaseService;
            _logger = logger;
            _cacheService = cacheService;
        }

        public async Task<List<ProizvodnjaModel>> UcitajIzvestajProizvodnje(FilterRequest filterRequest)
        {
            try
            {
                var sql = CreateSqlBuilder();
                sql.Append(@"
                SELECT 
                    rn.DatumPocetka as Datum,
                    rn.Sifra as RadniNalog,
                    vpp.Artikal,
                    CAST(CASE a.MagacinID
                        WHEN 2 THEN 'Sveza Roba'
                        WHEN 3 THEN 'Sirovine'
                        WHEN 4 THEN 'Ambalaza'
                        WHEN 5 THEN 'PoluProizvodi'
                        WHEN 6 THEN 'Gotov Proizvod'
                        WHEN 8 THEN 'Usl. Mlečni'
                        WHEN 9 THEN 'Repromaterijal'
                        WHEN 10 THEN 'Đubriva'
                        WHEN 11 THEN 'Usl. Voće'
                        WHEN 12 THEN 'Usl. Meso'
                        ELSE CONCAT('MagacinID ', a.MagacinID)
                    END AS CHAR(50)) as TipArtikla,
                    CASE 
                        WHEN a.MagacinID NOT IN (4, 6) THEN vpp.Kolicina 
                        ELSE 0 
                    END as KolicinaRoba,
                    CASE 
                        WHEN a.MagacinID = 4 THEN vpp.Kolicina 
                        ELSE 0 
                    END as KolicinaAmbalaza,
                    CASE 
                        WHEN a.MagacinID = 6 THEN vpp.Kolicina 
                        ELSE 0 
                    END as GotovProizvod,
                    vpp.Kolicina,
                    vpp.Komitent,
                    rn.ID as RadniNalogID,
                    rn.KomitentID,
                    rn.DokumentStatus,
                    vpp.ArtikalID,
                    vpp.ArtikalPrvaKlasifikacijaID,
                    a.MagacinID as Tip
                FROM RadniNalog rn
                LEFT JOIN vPreradaPregled vpp ON rn.ID = vpp.RadniNalogID
                LEFT JOIN Artikal a ON vpp.ArtikalID = a.ID
                WHERE rn.Aktivno = 1
                    AND a.Aktivno = 1
                    AND a.ID IS NOT NULL
                    AND a.MagacinID != 7  -- ISKLJUČI KALO I RASTUR
                ");

                var parameters = CreateParameters();

                // Apply date filter using BaseService
                ApplyDateFilter(sql, parameters, filterRequest, "rn.DatumPocetka");

                if (!string.IsNullOrWhiteSpace(filterRequest.RadniNalog))
                {
                    sql.Append(" AND rn.Sifra = @RadniNalog");
                    parameters.Add("@RadniNalog", filterRequest.RadniNalog);
                }

                if (filterRequest.KomitentId.HasValue && filterRequest.KomitentId > 0)
                {
                    sql.Append(" AND rn.KomitentID = @KomitentId");
                    parameters.Add("@KomitentId", filterRequest.KomitentId.Value);
                }

                if (filterRequest.ArtikalKlasifikacijaId.HasValue && filterRequest.ArtikalKlasifikacijaId > 0)
                {
                    sql.Append(" AND vpp.ArtikalPrvaKlasifikacijaID = @ArtikalKlasifikacijaId");
                    parameters.Add("@ArtikalKlasifikacijaId", filterRequest.ArtikalKlasifikacijaId.Value);
                }

                sql.Append(" ORDER BY rn.DatumPocetka DESC, rn.Sifra, vpp.Artikal");

                var rezultat = await _databaseService.QueryAsync<ProizvodnjaModel>(sql.ToString(), parameters);
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajIzvestajProizvodnje");
                throw;
            }
        }

        public async Task<decimal> UcitajUkupnuProizvodnju(FilterRequest filterRequest)
        {
            try
            {
                var sql = CreateSqlBuilder();
                sql.Append(@"
                    SELECT COALESCE(SUM(vpp.Kolicina), 0) as UkupnaKolicina
                    FROM vPreradaPregled vpp
                    LEFT JOIN RadniNalog rn ON vpp.RadniNalogID = rn.ID
                    LEFT JOIN Artikal a ON vpp.ArtikalID = a.ID
                    WHERE a.MagacinID = 6  -- GOTOVI PROIZVODI
                      AND a.Aktivno = 1
                ");

                var parameters = CreateParameters();

                // Apply date filter using BaseService
                ApplyDateFilter(sql, parameters, filterRequest, "rn.DatumPocetka");

                var rezultat = await _databaseService.ExecuteScalarAsync<decimal>(sql.ToString(), parameters);
                return rezultat;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajUkupnuProizvodnju");
                return 0;
            }
        }

        public async Task<decimal> UcitajGotoveProizvodePoslednjihDana(int dana)
        {
            try
            {
                var sql = @"
                    SELECT COALESCE(SUM(vpp.Kolicina), 0) as UkupnaKolicina
                    FROM vPreradaPregled vpp
                    LEFT JOIN RadniNalog rn ON vpp.RadniNalogID = rn.ID
                    LEFT JOIN Artikal a ON vpp.ArtikalID = a.ID
                    WHERE a.MagacinID = 6  -- GOTOVI PROIZVODI
                      AND DATE(rn.DatumPocetka) >= DATE_SUB(CURDATE(), INTERVAL @Dana DAY)
                      AND a.Aktivno = 1
                ";

                var rezultat = await _databaseService.ExecuteScalarAsync<decimal>(sql, new { Dana = dana });
                return rezultat;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajGotoveProizvodePoslednjihDana");
                return 0;
            }
        }

        public async Task<Dictionary<string, decimal>> UcitajTopKupcePoKilogramima(FilterRequest filterRequest)
        {
            try
            {
                // ✅ OPTIMIZACIJA: Keširanje Top 5 Kupaca za 5 minuta
                var cacheKey = CacheService.BuildKey(
                    CacheKeys.TopKupci,
                    filterRequest.OdDatum?.ToString("yyyy-MM-dd") ?? "null",
                    filterRequest.DoDatum?.ToString("yyyy-MM-dd") ?? "null"
                );

                return await _cacheService.GetOrCreateAsync(
                    cacheKey,
                    async () =>
                    {
                        var sql = CreateSqlBuilder();
                        sql.Append(@"
        SELECT
        COALESCE(k.Naziv, vpp.Komitent, 'Nepoznato') as Komitent,
        SUM(ABS(vpp.Kolicina)) as UkupnaKolicina
        FROM vPreradaPregled vpp
        LEFT JOIN RadniNalog rn ON vpp.RadniNalogID = rn.ID
        LEFT JOIN Artikal a ON vpp.ArtikalID = a.ID
        LEFT JOIN Komitent k ON vpp.KomitentID = k.ID
        WHERE a.MagacinID = 6  -- GOTOVI PROIZVODI
        AND vpp.Kolicina > 0 -- POZITIVNA KOLICINA = PRODAJA/IZLAZ
        AND a.Aktivno = 1
        AND vpp.Komitent IS NOT NULL
          AND vpp.Komitent != ''
                        ");

                        var parameters = CreateParameters();

                        // Apply date filter using BaseService
                        ApplyDateFilter(sql, parameters, filterRequest, "rn.DatumPocetka");

                        sql.Append(@"
                GROUP BY COALESCE(k.ID, vpp.KomitentID), COALESCE(k.Naziv, vpp.Komitent)
                HAVING SUM(ABS(vpp.Kolicina)) > 0
                ORDER BY UkupnaKolicina DESC
                    LIMIT 5
            ");

                        var rezultat = await _databaseService.QueryAsync<dynamic>(sql.ToString(), parameters);

                        return rezultat.ToDictionary(
                            x => (string)x.Komitent ?? "Nepoznato",
                            x => (decimal)x.UkupnaKolicina
                        );
                    },
                    CacheService.DefaultExpiration  // 5 minuta
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajTopKupcePoKilogramima");
                return new Dictionary<string, decimal>();
            }
        }

        public async Task<Dictionary<string, decimal>> UcitajTopDobavljacePoKilogramima(FilterRequest filterRequest)
        {
            try
            {
                // ✅ OPTIMIZACIJA: Keširanje Top 5 Dobavljača za 5 minuta
                var cacheKey = CacheService.BuildKey(
                    CacheKeys.TopDobavljaci,
                    filterRequest.OdDatum?.ToString("yyyy-MM-dd") ?? "null",
                    filterRequest.DoDatum?.ToString("yyyy-MM-dd") ?? "null"
                );

                return await _cacheService.GetOrCreateAsync(
                    cacheKey,
                    async () =>
                    {
                        var sql = CreateSqlBuilder();
                        sql.Append(@"
        SELECT
        COALESCE(k.Naziv, vrp.Komitent, 'Nepoznato') as Komitent,
        SUM(ABS(vrp.Ulaz)) as UkupnaKolicina
        FROM vPrometRoba vrp
        LEFT JOIN Artikal a ON vrp.ArtikalID = a.ID
        LEFT JOIN Komitent k ON vrp.KomitentID = k.ID
        WHERE a.MagacinID IN (2, 3, 5)  -- SVEZA ROBA I SIROVINE
        AND vrp.DOKUMENT LIKE 'PR-%'
        AND vrp.Ulaz > 0  -- POZITIVNA KOLICINA = NABAVKA/ULAZ
        AND vrp.DokumentStatus = (3)

          AND vrp.Komitent != ''
                        ");

                        var parameters = CreateParameters();

                        // Apply date filter using BaseService
                        ApplyDateFilter(sql, parameters, filterRequest, "vrp.Datum");

                        sql.Append(@"
                GROUP BY COALESCE(k.ID, vrp.KomitentID), COALESCE(k.Naziv, vrp.Komitent)
                HAVING SUM(ABS(vrp.Ulaz)) > 0
                ORDER BY UkupnaKolicina DESC
                    LIMIT 5
            ");

                        var rezultat = await _databaseService.QueryAsync<dynamic>(sql.ToString(), parameters);

                        return rezultat.ToDictionary(
                            x => (string)x.Komitent ?? "Nepoznato",
                            x => (decimal)x.UkupnaKolicina
                        );
                    },
                    CacheService.DefaultExpiration  // 5 minuta
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajTopDobavljacePoKilogramima");
                return new Dictionary<string, decimal>();
            }
        }

        public async Task<Dictionary<string, decimal>> UcitajProizvodnjuPoArtiklima(FilterRequest filterRequest)
        {
            try
            {
                var sql = CreateSqlBuilder();
                sql.Append(@"
                    SELECT 
                        CASE 
                            WHEN vpp.Artikal LIKE '%+' THEN LEFT(vpp.Artikal, LENGTH(vpp.Artikal) - 1)
                            WHEN vpp.Artikal LIKE '%-' THEN LEFT(vpp.Artikal, LENGTH(vpp.Artikal) - 1)
                            ELSE vpp.Artikal
                        END as BaseArtikal,
                        SUM(vpp.Kolicina) as UkupnaKolicina
                    FROM vPreradaPregled vpp
                    LEFT JOIN RadniNalog rn ON vpp.RadniNalogID = rn.ID
                    LEFT JOIN Artikal a ON vpp.ArtikalID = a.ID
                    WHERE a.MagacinID = 6  -- GOTOVI PROIZVODI
                    AND a.Aktivno = 1
                    
                ");

                var parameters = CreateParameters();

                // Apply date filter using BaseService
                ApplyDateFilter(sql, parameters, filterRequest, "rn.DatumPocetka");

                sql.Append(@"
                    GROUP BY BaseArtikal
                    ORDER BY UkupnaKolicina DESC
                ");

                var rezultat = await _databaseService.QueryAsync<dynamic>(sql.ToString(), parameters);

                return rezultat.ToDictionary(
                    x => (string)x.BaseArtikal ?? "Nepoznato",
                    x => (decimal)x.UkupnaKolicina
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajProizvodnjuPoArtiklima");
                return new Dictionary<string, decimal>();
            }
        }

        public async Task<List<string>> UcitajListuRadnihNaloga(FilterRequest filterRequest)
        {
            try
            {
                var sql = CreateSqlBuilder();
                sql.Append(@"
                    SELECT DISTINCT rn.Sifra
                    FROM RadniNalog rn
                    WHERE rn.Aktivno = 1 AND rn.Sifra IS NOT NULL AND rn.Sifra != ''
                ");

                var parameters = CreateParameters();

                // Apply date filter using BaseService
                ApplyDateFilter(sql, parameters, filterRequest, "rn.DatumPocetka");

                sql.Append(" ORDER BY rn.Sifra DESC");

                var rezultat = await _databaseService.QueryAsync<string>(sql.ToString(), parameters);
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajListuRadnihNaloga");
                return new List<string>();
            }
        }

        public async Task<int> UcitajBrojAktivnihNaloga(FilterRequest filterRequest)
        {
            try
            {
                var sql = @"
                    SELECT COUNT(DISTINCT rn.ID) as BrojNaloga
                    FROM RadniNalog rn
                    WHERE rn.Aktivno = 1 AND rn.DokumentStatus = 2  -- 2 = Otvoren prema dokumentu
                ";

                var rezultat = await _databaseService.ExecuteScalarAsync<int>(sql);
                return rezultat;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajBrojAktivnihNaloga");
                return 0;
            }
        }

        public async Task<Dictionary<string, decimal>> UcitajProizvodnjuPoKomitentima(FilterRequest filterRequest)
        {
            try
            {
                var sql = CreateSqlBuilder();
                sql.Append(@"
                    SELECT 
                        COALESCE(vpp.Komitent, 'Nepoznato') as Komitent,
                        SUM(vpp.Kolicina) as UkupnaKolicina
                    FROM vPreradaPregled vpp
                    LEFT JOIN RadniNalog rn ON vpp.RadniNalogID = rn.ID
                    LEFT JOIN Artikal a ON vpp.ArtikalID = a.ID
                    WHERE a.MagacinID = 6  -- SAMO GOTOVI PROIZVODI
                    AND a.Aktivno = 1
                    AND vpp.Komitent IS NOT NULL
                    AND vpp.Komitent != ''
                ");

                var parameters = CreateParameters();

                // Apply date filter using BaseService
                ApplyDateFilter(sql, parameters, filterRequest, "rn.DatumPocetka");

                sql.Append(@"
                    GROUP BY vpp.Komitent, vpp.KomitentID
                    HAVING SUM(vpp.Kolicina) > 0
                    ORDER BY UkupnaKolicina DESC
                    LIMIT 10
                ");

                var rezultat = await _databaseService.QueryAsync<dynamic>(sql.ToString(), parameters);

                return rezultat.ToDictionary(
                    x => (string)x.Komitent ?? "Nepoznato",
                    x => (decimal)x.UkupnaKolicina
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajProizvodnjuPoKomitentima");
                return new Dictionary<string, decimal>();
            }
        }

        public async Task<Dictionary<int, decimal>> UcitajProizvodnjuPoTipovima(FilterRequest filterRequest)
        {
            try
            {
                var sql = CreateSqlBuilder();
                sql.Append(@"
                    SELECT 
                        a.MagacinID as Tip,
                        SUM(vpp.Kolicina) as UkupnaKolicina
                    FROM vPreradaPregled vpp
                    LEFT JOIN RadniNalog rn ON vpp.RadniNalogID = rn.ID
                    LEFT JOIN Artikal a ON vpp.ArtikalID = a.ID
                    WHERE a.Aktivno = 1
                      AND a.MagacinID != 7  -- ISKLJUČI KALO I RASTUR
                ");

                var parameters = CreateParameters();

                // Apply date filter using BaseService
                ApplyDateFilter(sql, parameters, filterRequest, "rn.DatumPocetka");

                sql.Append(@"
                    GROUP BY a.MagacinID
                    HAVING SUM(vpp.Kolicina) > 0
                    ORDER BY UkupnaKolicina DESC
                ");

                var rezultat = await _databaseService.QueryAsync<dynamic>(sql.ToString(), parameters);

                return rezultat.ToDictionary(
                    x => (int)x.Tip,
                    x => (decimal)x.UkupnaKolicina
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajProizvodnjuPoTipovima");
                return new Dictionary<int, decimal>();
            }
        }

        public async Task<List<ProizvodnjaModel>> UcitajNajproduktivnijeNaloge(FilterRequest filterRequest, int brojNaloga = 10)
        {
            try
            {
                var sql = CreateSqlBuilder();
                sql.Append(@"
                    SELECT 
                        rn.DatumPocetka as Datum,
                        vpp.RadniNalog,
                        vpp.Artikal,
                        vpp.Kolicina,
                        vpp.Komitent,
                        vpp.ArtikalPrvaKlasifikacija as Klasifikacija,
                        vpp.RpArtikalTip as TipArtikla,
                        vpp.RadniNalogID,
                        vpp.ArtikalID,
                        vpp.KomitentID
                    FROM vPreradaPregled vpp
                    LEFT JOIN RadniNalog rn ON vpp.RadniNalogID = rn.ID
                    WHERE vpp.RpArtikalTip = 4  -- SAMO GOTOVI PROIZVODI
                    AND EXISTS (SELECT 1 FROM Artikal a WHERE a.ID = vpp.ArtikalID AND a.Aktivno = 1)
                ");

                var parameters = CreateParameters();

                // Apply date filter using BaseService
                ApplyDateFilter(sql, parameters, filterRequest, "rn.DatumPocetka");

                sql.Append(@"
                    ORDER BY vpp.Kolicina DESC
                    LIMIT @BrojNaloga
                ");

                // Add LIMIT parameter safely
                parameters.Add("@BrojNaloga", brojNaloga);

                var rezultat = await _databaseService.QueryAsync<ProizvodnjaModel>(sql.ToString(), parameters);
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajNajproduktivnijeNaloge");
                return new List<ProizvodnjaModel>();
            }
        }

        // Placeholder metode koje nisu implementirane (dodaj implementaciju po potrebi)
        public Task<List<ProizvodnjaModel>> UcitajSveRadneNaloge() => UcitajIzvestajProizvodnje(new FilterRequest());
        public Task<List<ProizvodnjaModel>> UcitajRadneNalogePoDatumu(DateTime odDatum, DateTime doDatum) =>
            UcitajIzvestajProizvodnje(new FilterRequest { OdDatum = odDatum, DoDatum = doDatum });
        public Task<List<ProizvodnjaModel>> UcitajRadneNalogePoKomitentu(long komitentId) =>
            UcitajIzvestajProizvodnje(new FilterRequest { KomitentId = komitentId });
        public Task<List<ProizvodnjaModel>> UcitajRadneNalogePoArtiklu(long artikalId) =>
            UcitajIzvestajProizvodnje(new FilterRequest { ArtikalId = artikalId });
        public Task<List<ProizvodnjaModel>> UcitajRadneNalogePoTipu(int tipArtikla) =>
            UcitajIzvestajProizvodnje(new FilterRequest { TipArtikla = tipArtikla });

        public Task<Dictionary<string, decimal>> UcitajProizvodnjuPoMesecima(FilterRequest filterRequest) =>
            Task.FromResult(new Dictionary<string, decimal>());
        public Task<List<PreradaPregledModel>> UcitajPreraduPregled(FilterRequest filterRequest) =>
            Task.FromResult(new List<PreradaPregledModel>());
        public Task<Dictionary<string, decimal>> UcitajAnalizuPreradePoArtiklima(FilterRequest filterRequest) =>
            Task.FromResult(new Dictionary<string, decimal>());

        // Metode koje možda nedostaju u Interface-u (dodaj u Interface ako treba)
        public Task<Dictionary<string, decimal>> UcitajTopDobavljaceKutija(FilterRequest filterRequest) =>
            Task.FromResult(new Dictionary<string, decimal>());
        public Task<Dictionary<string, decimal>> UcitajTopDobavljaceKesa(FilterRequest filterRequest) =>
            Task.FromResult(new Dictionary<string, decimal>());
    }
}
