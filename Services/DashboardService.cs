using FruitSysWeb.Services.Models.Requests;
using FruitSysWeb.Models;
using FruitSysWeb.Services.Interfaces;
using FruitSysWeb.Services;
using FruitSysWeb.Components.Shared.Filters;


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

        #region CHART METODE - REALNI SQL UPITI

        /// <summary>
        /// Učitava mesečnu prodaju iz Faktura tabele za poslednje godine
        /// </summary>
        public async Task<Dictionary<DateTime, double>> GetMonthlySalesAsync()
        {
            try
            {
                var sql = @"
                    SELECT 
                        DATE_FORMAT(f.Datum, '%Y-%m-01') as Mesec,
                        SUM(f.Bruto) as Prodaja
                    FROM Faktura f
                    WHERE f.Datum >= DATE_SUB(NOW(), INTERVAL 12 MONTH)
                      AND f.DokumentStatus = 3
                      AND f.Aktivno = 1
                    GROUP BY DATE_FORMAT(f.Datum, '%Y-%m')
                    ORDER BY Mesec";

                var result = await _database.QueryAsync<dynamic>(sql);

                return result.ToDictionary(
                    x => DateTime.Parse((string)x.Mesec),
                    x => (double)(decimal)x.Prodaja
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju mesečne prodaje");
                return new Dictionary<DateTime, double>();
            }
        }

        /// <summary>
        /// Učitava trend prihoda (Neto) iz Faktura tabele
        /// </summary>
        public async Task<Dictionary<DateTime, double>> GetRevenueTrendAsync()
        {
            try
            {
                var sql = @"
                    SELECT 
                        DATE_FORMAT(f.Datum, '%Y-%m-01') as Mesec,
                        SUM(f.Neto) as Prihod
                    FROM Faktura f
                    WHERE f.Datum >= DATE_SUB(NOW(), INTERVAL 12 MONTH)
                      AND f.DokumentStatus = 3
                      AND f.Aktivno = 1
                    GROUP BY DATE_FORMAT(f.Datum, '%Y-%m')
                    ORDER BY Mesec";

                var result = await _database.QueryAsync<dynamic>(sql);

                return result.ToDictionary(
                    x => DateTime.Parse((string)x.Mesec),
                    x => (double)(decimal)x.Prihod
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju trenda prihoda");
                return new Dictionary<DateTime, double>();
            }
        }

        /// <summary>
        /// Učitava kumulativni rast broja kupaca iz Komitent tabele
        /// </summary>
        public async Task<Dictionary<DateTime, double>> GetCustomerGrowthAsync()
        {
            try
            {
                var sql = @"
                    SELECT 
                        DATE_FORMAT(k.Kreirano, '%Y-%m-01') as Mesec,
                        COUNT(*) as BrojKupaca
                    FROM Komitent k
                    WHERE k.Kreirano >= DATE_SUB(NOW(), INTERVAL 12 MONTH)
                      AND k.JeKupac = 1
                      AND k.Aktivno = 1
                    GROUP BY DATE_FORMAT(k.Kreirano, '%Y-%m')
                    ORDER BY Mesec";

                var result = await _database.QueryAsync<dynamic>(sql);

                // Kumulativni zbir
                var cumulative = 0.0;
                return result.ToDictionary(
                    x => DateTime.Parse((string)x.Mesec),
                    x =>
                    {
                        cumulative += (int)x.BrojKupaca;
                        return cumulative;
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju rasta kupaca");
                return new Dictionary<DateTime, double>();
            }
        }

        #endregion

        #region LAGER METODE - OSNOVNE

        /// <summary>
        /// Učitava kompletno stanje lagera iz vwMagacinLager view-a
        /// </summary>
        public async Task<List<MagacinLagerModel>> UcitajLagerStanje()
        {
            try
            {
                var sql = @"
                    SELECT * 
                    FROM vwMagacinLager 
                    WHERE Kolicina > 0 
                    ORDER BY Artikal";

                var result = await _database.QueryAsync<MagacinLagerModel>(sql);
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju stanja lagera");
                return new List<MagacinLagerModel>();
            }
        }

        /// <summary>
        /// Učitava stanje lagera sa filterima (FilterRequest verzija)
        /// </summary>
        public async Task<List<MagacinLagerModel>> UcitajLagerStanjeSaFilterima(FilterRequest filterRequest)
        {
            try
            {
                var sql = @"
                    SELECT * 
                    FROM vwMagacinLager 
                    WHERE Kolicina > 0";

                var parameters = new Dictionary<string, object>();

                if (filterRequest.ArtikalId.HasValue && filterRequest.ArtikalId > 0)
                {
                    sql += " AND ArtikalID = @ArtikalId";
                    parameters.Add("@ArtikalId", filterRequest.ArtikalId.Value);
                }

                if (!string.IsNullOrWhiteSpace(filterRequest.Tip))
                {
                    sql += " AND Tip = @Tip";
                    parameters.Add("@Tip", filterRequest.Tip);
                }

                if (!string.IsNullOrWhiteSpace(filterRequest.Pakovanje))
                {
                    sql += " AND Pakovanje = @Pakovanje";
                    parameters.Add("@Pakovanje", filterRequest.Pakovanje);
                }

                sql += " ORDER BY Artikal";

                var result = await _database.QueryAsync<MagacinLagerModel>(sql, parameters);
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju lagera sa filterima");
                return new List<MagacinLagerModel>();
            }
        }

        /// <summary>
        /// Učitava stanje lagera sa filterima (string verzija - legacy)
        /// </summary>
        public async Task<List<MagacinLagerModel>> UcitajLagerStanjeSaFilterima(string filter)
        {
            try
            {
                var sql = @"
                    SELECT * 
                    FROM vwMagacinLager 
                    WHERE Kolicina > 0 
                      AND (Artikal LIKE @Filter OR Pakovanje LIKE @Filter)
                    ORDER BY Artikal";

                var result = await _database.QueryAsync<MagacinLagerModel>(
                    sql,
                    new { Filter = $"%{filter}%" }
                );
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri pretrazi lagera");
                return new List<MagacinLagerModel>();
            }
        }

        /// <summary>
        /// Učitava lager proizvodnje - radne naloge u toku
        /// </summary>
        public async Task<List<RadniNalogLagerModel>> UcitajRadneNalogeLager()
        {
            try
            {
                var sql = @"
                    SELECT * 
                    FROM vwRadniNalogLager 
                    ORDER BY DatumPocetka DESC";

                var result = await _database.QueryAsync<RadniNalogLagerModel>(sql);
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju lagera proizvodnje");
                return new List<RadniNalogLagerModel>();
            }
        }

        #endregion

        #region LAGER METODE - PO ARTIKLU I TIPU

        /// <summary>
        /// Učitava stanje lagera za specifičan artikal (long verzija)
        /// </summary>
        public Task<List<MagacinLagerModel>> UcitajLagerStanjePoArtiklu(long artikalId)
        {
            return UcitajLagerStanjePoArtiklu((int)artikalId);
        }

        /// <summary>
        /// Učitava stanje lagera za specifičan artikal (int verzija)
        /// </summary>
        public async Task<List<MagacinLagerModel>> UcitajLagerStanjePoArtiklu(int artikalId)
        {
            try
            {
                var sql = @"
                    SELECT * 
                    FROM vwMagacinLager 
                    WHERE ArtikalID = @ArtikalId";

                var result = await _database.QueryAsync<MagacinLagerModel>(
                    sql,
                    new { ArtikalId = artikalId }
                );
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju lagera po artiklu");
                return new List<MagacinLagerModel>();
            }
        }

        /// <summary>
        /// Učitava stanje lagera po tipu artikla (MagacinID)
        /// </summary>
        public async Task<List<MagacinLagerModel>> UcitajLagerStanjePoTipu(int artikalTip)
        {
            try
            {
                var sql = @"
                    SELECT * 
                    FROM vwMagacinLager 
                    WHERE Tip = @Tip 
                      AND Kolicina > 0
                    ORDER BY Artikal";

                var result = await _database.QueryAsync<MagacinLagerModel>(
                    sql,
                    new { Tip = artikalTip }
                );
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju lagera po tipu");
                return new List<MagacinLagerModel>();
            }
        }

        /// <summary>
        /// Učitava stanje lagera po lotu
        /// </summary>
        public async Task<List<MagacinLagerModel>> UcitajLagerStanjePoLotu(string lot)
        {
            try
            {
                var sql = @"
                    SELECT * 
                    FROM vwMagacinLager 
                    WHERE Lot = @Lot";

                var result = await _database.QueryAsync<MagacinLagerModel>(
                    sql,
                    new { Lot = lot }
                );
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju lagera po lotu");
                return new List<MagacinLagerModel>();
            }
        }

        #endregion

        #region LAGER METODE - SPECIFIČNI TIPOVI

        /// <summary>
        /// Učitava samo gotove proizvode (MagacinID = 6)
        /// </summary>
        public async Task<List<MagacinLagerModel>> UcitajGotoveRobe()
        {
            return await UcitajLagerStanjePoTipu(6); // MagacinID 6 = Gotov Proizvod
        }

        /// <summary>
        /// Učitava samo sirovine (MagacinID = 3)
        /// </summary>
        public async Task<List<MagacinLagerModel>> UcitajSirovine()
        {
            return await UcitajLagerStanjePoTipu(3); // MagacinID 3 = Sirovine
        }

        /// <summary>
        /// Učitava samo ambalažu (MagacinID = 4)
        /// </summary>
        public async Task<List<MagacinLagerModel>> UcitajAmbalaze()
        {
            return await UcitajLagerStanjePoTipu(4); // MagacinID 4 = Ambalaza
        }

        #endregion

        #region LAGER METODE - UPOZORENJA I MINIMUMI

        /// <summary>
        /// Učitava artikle ispod minimum količine (sa parametrom)
        /// </summary>
        public async Task<List<MagacinLagerModel>> UcitajArtikleIspodMinimuma(decimal minKolicina = 10)
        {
            try
            {
                var sql = @"
                    SELECT * 
                    FROM vwMagacinLager 
                    WHERE Kolicina < @MinKolicina 
                      AND Kolicina >= 0
                    ORDER BY Kolicina ASC";

                var result = await _database.QueryAsync<MagacinLagerModel>(
                    sql,
                    new { MinKolicina = minKolicina }
                );
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju artikala ispod minimuma");
                return new List<MagacinLagerModel>();
            }
        }

        /// <summary>
        /// Učitava artikle ispod minimum količine (bez parametra - koristi 10)
        /// </summary>
        public async Task<List<MagacinLagerModel>> UcitajArtikleIspodMinimuma()
        {
            return await UcitajArtikleIspodMinimuma(10);
        }

        #endregion

        #region RADNI NALOZI LAGER

        /// <summary>
        /// Učitava lager proizvodnje - alias za UcitajRadneNalogeLager
        /// </summary>
        public async Task<List<RadniNalogLagerModel>> UcitajLagerProizvodnje()
        {
            return await UcitajRadneNalogeLager();
        }

        /// <summary>
        /// Učitava samo otvorene radne naloge
        /// </summary>
        public async Task<List<RadniNalogLagerModel>> UcitajOtvoreneRadneNaloge()
        {
            try
            {
                var sql = @"
                    SELECT * 
                    FROM vwRadniNalogLager 
                    WHERE Status = 'Otvoren' 
                    ORDER BY DatumPocetka DESC";

                var result = await _database.QueryAsync<RadniNalogLagerModel>(sql);
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju otvorenih radnih naloga");
                return new List<RadniNalogLagerModel>();
            }
        }

        /// <summary>
        /// Učitava radne naloge po statusu
        /// </summary>
        public async Task<List<RadniNalogLagerModel>> UcitajRadneNalogePoStatusu(int status)
        {
            try
            {
                var sql = @"
                    SELECT * 
                    FROM vwRadniNalogLager 
                    WHERE StatusID = @Status 
                    ORDER BY DatumPocetka DESC";

                var result = await _database.QueryAsync<RadniNalogLagerModel>(
                    sql,
                    new { Status = status }
                );
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju radnih naloga po statusu");
                return new List<RadniNalogLagerModel>();
            }
        }

        #endregion

        #region STATISTIKE I IZVEŠTAJI

        /// <summary>
        /// Izračunava ukupnu vrednost lagera
        /// </summary>
        public async Task<decimal> UcitajUkupnuVrednostLager()
        {
            try
            {
                var sql = @"
                    SELECT COALESCE(SUM(Kolicina * IFNULL(Cena, 0)), 0) 
                    FROM vwMagacinLager
                    WHERE Kolicina > 0";

                return await _database.ExecuteScalarAsync<decimal>(sql);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri izračunavanju ukupne vrednosti lagera");
                return 0m;
            }
        }

        /// <summary>
        /// Učitava listu svih pakovanja
        /// </summary>
        public async Task<List<string>> UcitajListuPakovanja()
        {
            try
            {
                var sql = @"
                    SELECT DISTINCT Pakovanje 
                    FROM vwMagacinLager 
                    WHERE Pakovanje IS NOT NULL 
                      AND Pakovanje != ''
                    ORDER BY Pakovanje";

                var result = await _database.QueryAsync<string>(sql);
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju liste pakovanja");
                return new List<string>();
            }
        }

        /// <summary>
        /// Učitava kompletne statistike lagera
        /// </summary>
        public async Task<Dictionary<string, decimal>> UcitajStatistikeLagera()
        {
            try
            {
                var stats = new Dictionary<string, decimal>
                {
                    ["Ukupno artikala"] = await _database.ExecuteScalarAsync<decimal>(
                        "SELECT COUNT(*) FROM vwMagacinLager WHERE Kolicina > 0"
                    ),
                    ["Ukupna količina"] = await _database.ExecuteScalarAsync<decimal>(
                        "SELECT COALESCE(SUM(Kolicina), 0) FROM vwMagacinLager WHERE Kolicina > 0"
                    ),
                    ["Ukupna vrednost"] = await UcitajUkupnuVrednostLager(),
                    ["Sirovine"] = await _database.ExecuteScalarAsync<decimal>(
                        "SELECT COUNT(*) FROM vwMagacinLager WHERE Tip = 3 AND Kolicina > 0"
                    ),
                    ["Ambalaza"] = await _database.ExecuteScalarAsync<decimal>(
                        "SELECT COUNT(*) FROM vwMagacinLager WHERE Tip = 4 AND Kolicina > 0"
                    ),
                    ["Gotovi proizvodi"] = await _database.ExecuteScalarAsync<decimal>(
                        "SELECT COUNT(*) FROM vwMagacinLager WHERE Tip = 6 AND Kolicina > 0"
                    )
                };
                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju statistika lagera");
                return new Dictionary<string, decimal>();
            }
        }

        #endregion

        #region DASHBOARD - STRUKTURA LAGERA (TOP 5)

        /// <summary>
        /// Učitava strukturu sirovina - TOP 5 za čitljivost
        /// </summary>
        public async Task<Dictionary<string, decimal>> UcitajStrukturuSirovina()
        {
            try
            {
                var sql = @"
                    SELECT 
                        ml.Artikal,
                        SUM(ml.Kolicina) as UkupnaKolicina 
                    FROM vwMagacinLager ml
                    LEFT JOIN Artikal a ON ml.ArtikalID = a.ID
                    WHERE a.MagacinID = 3
                      AND ml.Kolicina >= 10
                    GROUP BY ml.Artikal
                    ORDER BY UkupnaKolicina DESC 
                    LIMIT 5";

                var result = await _database.QueryAsync<(string Artikal, decimal UkupnaKolicina)>(sql);
                return result.ToDictionary(x => x.Artikal, x => x.UkupnaKolicina);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju strukture sirovina");
                return new Dictionary<string, decimal>();
            }
        }

        /// <summary>
        /// Učitava strukturu gotovih proizvoda - TOP 5 za čitljivost
        /// </summary>
        public async Task<Dictionary<string, decimal>> UcitajStrukturuGotovihProizvoda()
        {
            try
            {
                var sql = @"
                    SELECT 
                        ml.Artikal,
                        SUM(ml.Kolicina) as UkupnaKolicina 
                    FROM vwMagacinLager ml
                    LEFT JOIN Artikal a ON ml.ArtikalID = a.ID
                    WHERE a.MagacinID = 6
                      AND ml.Kolicina >= 1
                    GROUP BY ml.Artikal
                    ORDER BY UkupnaKolicina DESC 
                    LIMIT 5";

                var result = await _database.QueryAsync<(string Artikal, decimal UkupnaKolicina)>(sql);
                return result.ToDictionary(x => x.Artikal, x => x.UkupnaKolicina);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju strukture gotovih proizvoda");
                return new Dictionary<string, decimal>();
            }
        }

        /// <summary>
        /// Učitava strukturu ambalaze (sve tipove) - TOP 5 za čitljivost
        /// </summary>
        public async Task<Dictionary<string, decimal>> UcitajStrukturuAmbalaze()
        {
            try
            {
                var sql = @"
                    SELECT 
                        ml.Artikal,
                        SUM(ml.Kolicina) as UkupnaKolicina 
                    FROM vwMagacinLager ml
                    LEFT JOIN Artikal a ON ml.ArtikalID = a.ID
                    WHERE a.MagacinID = 4
                      AND ml.Kolicina >= 10
                    GROUP BY ml.Artikal
                    ORDER BY UkupnaKolicina DESC 
                    LIMIT 5";

                var result = await _database.QueryAsync<(string Artikal, decimal UkupnaKolicina)>(sql);
                return result.ToDictionary(x => x.Artikal, x => x.UkupnaKolicina);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju strukture ambalaze");
                return new Dictionary<string, decimal>();
            }
        }

        #endregion
    }
}
