using FruitSysWeb.Services.Models.Requests;
using FruitSysWeb.Models;

namespace FruitSysWeb.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly DatabaseService _database;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(DatabaseService database, ILogger<DashboardService> logger)
        {
            _database = database;
            _logger = logger;
        }

        // Chart metode - mock data za početak
        public async Task<Dictionary<DateTime, double>> GetMonthlySalesAsync()
        {
            try
            {
                // Mock podatci - možete ih zameniti pravim upitima
                var data = new Dictionary<DateTime, double>
                {
                    { new DateTime(2025, 1, 1), 125000 },
                    { new DateTime(2025, 2, 1), 135000 },
                    { new DateTime(2025, 3, 1), 145000 },
                    { new DateTime(2025, 4, 1), 155000 },
                    { new DateTime(2025, 5, 1), 165000 },
                    { new DateTime(2025, 6, 1), 175000 },
                    { new DateTime(2025, 7, 1), 185000 },
                    { new DateTime(2025, 8, 1), 195000 },
                    { new DateTime(2025, 9, 1), 205000 }
                };
                return await Task.FromResult(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching monthly sales data");
                return new Dictionary<DateTime, double>();
            }
        }

        public async Task<Dictionary<DateTime, double>> GetRevenueTrendAsync()
        {
            try
            {
                // Mock podatci
                var data = new Dictionary<DateTime, double>
                {
                    { new DateTime(2025, 1, 1), 85000 },
                    { new DateTime(2025, 2, 1), 92000 },
                    { new DateTime(2025, 3, 1), 98000 },
                    { new DateTime(2025, 4, 1), 105000 },
                    { new DateTime(2025, 5, 1), 112000 },
                    { new DateTime(2025, 6, 1), 118000 },
                    { new DateTime(2025, 7, 1), 125000 },
                    { new DateTime(2025, 8, 1), 132000 },
                    { new DateTime(2025, 9, 1), 138000 }
                };
                return await Task.FromResult(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching revenue trend data");
                return new Dictionary<DateTime, double>();
            }
        }

        public async Task<Dictionary<DateTime, double>> GetCustomerGrowthAsync()
        {
            try
            {
                // Mock podatci
                var data = new Dictionary<DateTime, double>
                {
                    { new DateTime(2025, 1, 1), 245 },
                    { new DateTime(2025, 2, 1), 258 },
                    { new DateTime(2025, 3, 1), 272 },
                    { new DateTime(2025, 4, 1), 286 },
                    { new DateTime(2025, 5, 1), 301 },
                    { new DateTime(2025, 6, 1), 315 },
                    { new DateTime(2025, 7, 1), 330 },
                    { new DateTime(2025, 8, 1), 344 },
                    { new DateTime(2025, 9, 1), 359 }
                };
                return await Task.FromResult(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching customer growth data");
                return new Dictionary<DateTime, double>();
            }
        }

        // Lager metode
        public async Task<List<MagacinLagerModel>> UcitajLagerStanje()
        {
            try
            {
                var sql = @"SELECT * FROM vwMagacinLager WHERE Kolicina > 0 ORDER BY Artikal";
                var result = await _database.QueryAsync<MagacinLagerModel>(sql);
                return result.ToList(); // ISPRAVKA: Dodao .ToList()
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading lager stanje");
                return new List<MagacinLagerModel>();
            }
        }

        public async Task<List<MagacinLagerModel>> UcitajLagerStanjeSaFilterima(FilterRequest filterRequest)
        {
            return await UcitajLagerStanje();
        }

        public async Task<List<MagacinLagerModel>> UcitajLagerStanjeSaFilterima(string filter)
        {
            return await UcitajLagerStanje();
        }

        public async Task<List<RadniNalogLagerModel>> UcitajRadneNalogeLager()
        {
            try
            {
                var sql = @"SELECT * FROM vwRadniNalogLager ORDER BY DatumPocetka DESC";
                var result = await _database.QueryAsync<RadniNalogLagerModel>(sql);
                return result.ToList(); // ISPRAVKA: Dodao .ToList()
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading radni nalozi lager");
                return new List<RadniNalogLagerModel>();
            }
        }

        public Task<List<MagacinLagerModel>> UcitajLagerStanjePoArtiklu(long artikalId)
        {
            return UcitajLagerStanjePoArtiklu((int)artikalId);
        }

        public async Task<List<MagacinLagerModel>> UcitajLagerStanjePoArtiklu(int artikalId)
        {
            try
            {
                var sql = @"SELECT * FROM vwMagacinLager WHERE ArtikalID = @ArtikalId";
                var result = await _database.QueryAsync<MagacinLagerModel>(sql, new { ArtikalId = artikalId });
                return result.ToList(); // ISPRAVKA: Dodao .ToList()
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading lager stanje po artiklu");
                return new List<MagacinLagerModel>();
            }
        }

        public async Task<List<MagacinLagerModel>> UcitajLagerStanjePoTipu(int artikalTip)
        {
            try
            {
                var sql = @"SELECT * FROM vwMagacinLager WHERE Tip = @Tip";
                var result = await _database.QueryAsync<MagacinLagerModel>(sql, new { Tip = artikalTip });
                return result.ToList(); // ISPRAVKA: Dodao .ToList()
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading lager stanje po tipu");
                return new List<MagacinLagerModel>();
            }
        }

        public async Task<decimal> UcitajUkupnuVrednostLager()
        {
            try
            {
                var sql = @"SELECT SUM(Kolicina * IFNULL(Cena, 0)) FROM vwMagacinLager";
                return await _database.ExecuteScalarAsync<decimal>(sql); // ISPRAVKA: Promenio na ExecuteScalarAsync
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating ukupna vrednost lager");
                return 0m;
            }
        }

        public async Task<List<string>> UcitajListuPakovanja()
        {
            try
            {
                var sql = @"SELECT DISTINCT Pakovanje FROM vwMagacinLager WHERE Pakovanje IS NOT NULL ORDER BY Pakovanje";
                var result = await _database.QueryAsync<string>(sql);
                return result.ToList(); // ISPRAVKA: Dodao .ToList()
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading lista pakovanja");
                return new List<string>();
            }
        }

        public async Task<List<RadniNalogLagerModel>> UcitajLagerProizvodnje()
        {
            return await UcitajRadneNalogeLager();
        }

        public async Task<List<MagacinLagerModel>> UcitajGotoveRobe()
        {
            return await UcitajLagerStanjePoTipu(4); // Tip 4 = Gotova roba
        }

        public async Task<List<MagacinLagerModel>> UcitajSirovine()
        {
            return await UcitajLagerStanjePoTipu(1); // Tip 1 = Sirovina
        }

        public async Task<List<MagacinLagerModel>> UcitajAmbalaze()
        {
            return await UcitajLagerStanjePoTipu(2); // Tip 2 = Ambalaza
        }

        public async Task<List<MagacinLagerModel>> UcitajArtikleIspodMinimuma(decimal minKolicina = 10)
        {
            try
            {
                var sql = @"SELECT * FROM vwMagacinLager WHERE Kolicina < @MinKolicina ORDER BY Kolicina";
                var result = await _database.QueryAsync<MagacinLagerModel>(sql, new { MinKolicina = minKolicina });
                return result.ToList(); // ISPRAVKA: Dodao .ToList()
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading artikli ispod minimuma");
                return new List<MagacinLagerModel>();
            }
        }

        public async Task<List<MagacinLagerModel>> UcitajArtikleIspodMinimuma()
        {
            return await UcitajArtikleIspodMinimuma(10);
        }

        public async Task<Dictionary<string, decimal>> UcitajStatistikeLagera()
        {
            try
            {
                var stats = new Dictionary<string, decimal>
                {
                    ["Ukupno artikala"] = await _database.ExecuteScalarAsync<decimal>("SELECT COUNT(*) FROM vwMagacinLager"), // ISPRAVKA
                    ["Ukupna količina"] = await _database.ExecuteScalarAsync<decimal>("SELECT SUM(Kolicina) FROM vwMagacinLager"), // ISPRAVKA
                    ["Ukupna vrednost"] = await UcitajUkupnuVrednostLager(),
                    ["Sirovine"] = await _database.ExecuteScalarAsync<decimal>("SELECT COUNT(*) FROM vwMagacinLager WHERE Tip = 1"), // ISPRAVKA
                    ["Ambalaza"] = await _database.ExecuteScalarAsync<decimal>("SELECT COUNT(*) FROM vwMagacinLager WHERE Tip = 2"), // ISPRAVKA
                    ["Gotovi proizvodi"] = await _database.ExecuteScalarAsync<decimal>("SELECT COUNT(*) FROM vwMagacinLager WHERE Tip = 4") // ISPRAVKA
                };
                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading lager statistike");
                return new Dictionary<string, decimal>();
            }
        }

        public async Task<Dictionary<string, decimal>> UcitajStrukturuSirovina()
        {
            try
            {
                var sql = @"
                    SELECT Artikal, SUM(Kolicina) as Kolicina 
                    FROM vwMagacinLager 
                    WHERE Tip = 1 AND Kolicina > 0
                    GROUP BY Artikal 
                    ORDER BY Kolicina DESC 
                    LIMIT 10";
                    
                var result = await _database.QueryAsync<(string Artikal, decimal Kolicina)>(sql);
                return result.ToDictionary(x => x.Artikal, x => x.Kolicina);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading struktura sirovina");
                // Mock podatci kao fallback
                return new Dictionary<string, decimal>
                {
                    {"Šećer", 1500.5m},
                    {"Brašno", 2300.0m},
                    {"Ulje", 800.75m},
                    {"Jaja", 650.25m},
                    {"Mleko", 1200.0m}
                };
            }
        }

        public async Task<Dictionary<string, decimal>> UcitajStrukturuGotovihProizvoda()
        {
            try
            {
                var sql = @"
                    SELECT Artikal, SUM(Kolicina) as Kolicina 
                    FROM vwMagacinLager 
                    WHERE Tip = 4 AND Kolicina > 0
                    GROUP BY Artikal 
                    ORDER BY Kolicina DESC 
                    LIMIT 10";
                    
                var result = await _database.QueryAsync<(string Artikal, decimal Kolicina)>(sql);
                return result.ToDictionary(x => x.Artikal, x => x.Kolicina);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading struktura gotovih proizvoda");
                // Mock podatci kao fallback
                return new Dictionary<string, decimal>
                {
                    {"Hleb", 450.5m},
                    {"Kifle", 320.0m},
                    {"Kolači", 180.75m},
                    {"Torte", 95.25m},
                    {"Pecivo", 275.0m}
                };
            }
        }

        public async Task<Dictionary<string, decimal>> UcitajStrukturuAmbalaze()
        {
            try
            {
                var sql = @"
                    SELECT Artikal, SUM(Kolicina) as Kolicina 
                    FROM vwMagacinLager 
                    WHERE Tip = 2 AND Kolicina > 0
                    GROUP BY Artikal 
                    ORDER BY Kolicina DESC 
                    LIMIT 10";
                    
                var result = await _database.QueryAsync<(string Artikal, decimal Kolicina)>(sql);
                return result.ToDictionary(x => x.Artikal, x => x.Kolicina);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading struktura ambalaze");
                // Mock podatci kao fallback
                return new Dictionary<string, decimal>
                {
                    {"Kese", 2500.0m},
                    {"Kutije", 1200.5m},
                    {"Folije", 800.75m},
                    {"Etikete", 5000.0m}
                };
            }
        }

        public async Task<List<MagacinLagerModel>> UcitajLagerStanjePoLotu(string lot)
        {
            try
            {
                var sql = @"SELECT * FROM vwMagacinLager WHERE Lot = @Lot";
                var result = await _database.QueryAsync<MagacinLagerModel>(sql, new { Lot = lot });
                return result.ToList(); // ISPRAVKA: Dodao .ToList()
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading lager stanje po lotu");
                return new List<MagacinLagerModel>();
            }
        }

        public async Task<List<RadniNalogLagerModel>> UcitajOtvoreneRadneNaloge()
        {
            try
            {
                var sql = @"SELECT * FROM vwRadniNalogLager WHERE Status = 'Otvoren' ORDER BY DatumPocetka DESC";
                var result = await _database.QueryAsync<RadniNalogLagerModel>(sql);
                return result.ToList(); // ISPRAVKA: Dodao .ToList()
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading otvorene radne naloge");
                return new List<RadniNalogLagerModel>();
            }
        }

        public async Task<List<RadniNalogLagerModel>> UcitajRadneNalogePoStatusu(int status)
        {
            try
            {
                var sql = @"SELECT * FROM vwRadniNalogLager WHERE StatusID = @Status ORDER BY DatumPocetka DESC";
                var result = await _database.QueryAsync<RadniNalogLagerModel>(sql);
                return result.ToList(); // ISPRAVKA: Dodao .ToList()
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading radne naloge po statusu");
                return new List<RadniNalogLagerModel>();
            }
        }
    }
}
