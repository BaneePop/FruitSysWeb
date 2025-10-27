using FruitSysWeb.Models;
using FruitSysWeb.Services.Interfaces;
using FruitSysWeb.Services.Models.Requests;
using FruitSysWeb.Services.Core;
using System.Text;
using Microsoft.Extensions.Logging;

namespace FruitSysWeb.Services.Implementations.IzvestajService
{
    public class FinansijeService : BaseService, IFinansijeService
    {
        private readonly DatabaseService _databaseService;
        private readonly ILogger<FinansijeService> _logger;

        public FinansijeService(DatabaseService databaseService,
            ILogger<FinansijeService> logger)
        {
            _databaseService = databaseService;
            _logger = logger;
        }

        public async Task<List<FinansijeModel>> UcitajFinansijskiIzvestaj(FilterRequest filterRequest)
        {
            try
            {
                var sql = CreateSqlBuilder(@"
                    SELECT
                        fm.ID,
                        fm.KomitentID,
                        fm.Komitent,
                        fm.Otkupljivac,
                        fm.Proizvodjac,
                        fm.Dobavljac,
                        fm.Kupac,
                        fm.DokumentID,
                        fm.Datum,
                        fm.Dokument,
                        fm.DokumentTip,
                        fm.DokumentStatus,
                        fm.ArtikalID,
                        fm.Artikal,
                        fm.ArtikalPrvaKlasifikacijaID,
                        fm.Kolicina,
                        a.MagacinID as ArtikalMagacinId,
                        COALESCE(fm.Potrazuje, 0) as Potrazuje,
                        COALESCE(fm.Duguje, 0) as Duguje,
                        COALESCE(fm.Saldo, 0) as Saldo,
                        COALESCE(fm.Roba, 0) as Roba,
                        COALESCE(fm.Provizija, 0) as Provizija,
                        COALESCE(fm.Prevoz, 0) as Prevoz,
                        COALESCE(fm.Marza, 0) as Marza,
                        COALESCE(fm.PCenaPrijem, 0) as PCenaPrijem,
                        COALESCE(fm.PCenaUkupno, 0) as PCenaUkupno,
                        COALESCE(fm.Uplata, 0) as Uplata,
                        COALESCE(fm.PorezIznos, 0) as PorezIznos,
                        COALESCE(fm.PorezIznos2, 0) as PorezIznos2,
                        COALESCE(fm.UplataPdv, 0) as UplataPdv,
                        COALESCE(fm.NetoIznosOtkup, 0) as NetoIznosOtkup,
                        -- Dodatni podaci
                        0 as FizickoLice,
                        0 as ArtikalTip,
                        0 as JedinicaMereID,
                        0 as OtkupniArtikal
                    FROM vPrometFinansijev9 fm
                    LEFT JOIN Artikal a ON fm.ArtikalID = a.ID
                    WHERE fm.DokumentStatus != 4
                    AND fm.DokumentStatus != 2

                ");

                var parameters = CreateParameters();

                // Apply common filters using BaseService helper method
                ApplyCommonFilters(sql, parameters, filterRequest,
                    dateColumnName: "fm.Datum",
                    komitentColumnName: "fm.KomitentID",
                    artikalColumnName: "fm.ArtikalID");

                // POPRAVLJENO: Komitent tip filter - koristimo boolean kolone
                if (!string.IsNullOrEmpty(filterRequest.KomitentTip))
                {
                    switch (filterRequest.KomitentTip.ToLower())
                    {
                        case "kupac":
                            sql.Append(" AND fm.Kupac = 1");
                            break;
                        case "dobavljac":
                            sql.Append(" AND fm.Dobavljac = 1");
                            break;
                        case "proizvodjac":
                            sql.Append(" AND fm.Proizvodjac = 1");
                            break;
                        case "otkupljivac":
                            sql.Append(" AND fm.Otkupljivac = 1");
                            break;
                    }
                }

                // POPRAVLJENO: Artikal tip filter - dodano
                if (!string.IsNullOrEmpty(filterRequest.Tip))
                {
                    if (int.TryParse(filterRequest.Tip, out int TipInt))
                    {
                        // Ovde treba join sa Artikal tabelom da dobijemo tip
                        sql.Append(" AND a.MagacinID = @TipArtikla");
                        parameters.Add("@TipArtikla", TipInt);
                    }
                }

                // Dokument tip filter
                if (!string.IsNullOrEmpty(filterRequest.DokumentTip))
                {
                    sql.Append(" AND fm.DokumentTip = @DokumentTip");
                    parameters.Add("@DokumentTip", filterRequest.DokumentTip);
                }

                // Saldo filteri
                if (filterRequest.MinSaldo.HasValue)
                {
                    sql.Append(" AND fm.Saldo >= @MinSaldo");
                    parameters.Add("@MinSaldo", filterRequest.MinSaldo.Value);
                }

                if (filterRequest.MaxSaldo.HasValue)
                {
                    sql.Append(" AND fm.Saldo <= @MaxSaldo");
                    parameters.Add("@MaxSaldo", filterRequest.MaxSaldo.Value);
                }

                sql.Append(" ORDER BY fm.Datum DESC, fm.Dokument");

                var rezultat = await _databaseService.QueryAsync<FinansijeModel>(sql.ToString(), parameters);
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajFinansijskiIzvestaj");
                throw;
            }
        }

        public async Task<decimal> UcitajUkupnoSaldo(FilterRequest filterRequest)
        {
            try
            {
                var sql = new StringBuilder();
                sql.Append(@"
                    SELECT COALESCE(SUM(fm.Saldo), 0) as UkupnoSaldo
                    FROM vPrometFinansijev9 fm
                    WHERE 1=1
                ");

                var parameters = new Dictionary<string, object>();

                // Komitent filter
                if (filterRequest.KomitentId.HasValue && filterRequest.KomitentId > 0)
                {
                    sql.Append(" AND fm.KomitentID = @KomitentId");
                    parameters.Add("@KomitentId", filterRequest.KomitentId.Value);
                }

                // Artikal filter
                if (filterRequest.ArtikalId.HasValue && filterRequest.ArtikalId > 0)
                {
                    sql.Append(" AND fm.ArtikalID = @ArtikalId");
                    parameters.Add("@ArtikalId", filterRequest.ArtikalId.Value);
                }

                // Artikal tip filter
                if (!string.IsNullOrEmpty(filterRequest.Tip))
                {
                    if (int.TryParse(filterRequest.Tip, out int tipInt))
                    {
                        sql.Append(" AND EXISTS (SELECT 1 FROM Artikal a WHERE a.ID = fm.ArtikalID AND a.Tip = @ArtikalTip)");
                        parameters.Add("@ArtikalTip", tipInt);
                    }
                }

                // Komitent tip filter
                if (!string.IsNullOrEmpty(filterRequest.KomitentTip))
                {
                    switch (filterRequest.KomitentTip.ToLower())
                    {
                        case "kupac":
                            sql.Append(" AND fm.Kupac = 1");
                            break;
                        case "dobavljac":
                            sql.Append(" AND fm.Dobavljac = 1");
                            break;
                        case "proizvodjac":
                            sql.Append(" AND fm.Proizvodjac = 1");
                            break;
                        case "otkupljivac":
                            sql.Append(" AND fm.Otkupljivac = 1");
                            break;
                    }
                }

                var rezultat = await _databaseService.ExecuteScalarAsync<decimal>(sql.ToString(), parameters);
                return rezultat;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajUkupnoSaldo");
                return 0;
            }
        }

        public async Task<Dictionary<string, decimal>> UcitajTopKupce(FilterRequest filterRequest)
        {
            try
            {
                var sql = new StringBuilder();
                sql.Append(@"
                    SELECT 
                        k.Naziv as Komitent,
                        SUM(ABS(COALESCE(o.Kolicina, 0))) as UkupnaKolicina
                    FROM Otpremnica o
                    LEFT JOIN Komitent k ON o.KomitentID = k.ID
                    WHERE o.Aktivno = 1
                      AND k.Aktivno = 1
                      AND o.MagacinID != 7  -- ISKLJUČI KALO I RASTUR
                      AND o.DokumentStatus IN (2, 3)  -- OTVORENO ILI ZATVORENO
                ");

                var parameters = new Dictionary<string, object>();

                // Datum filteri za Otpremnicu
                if (filterRequest.OdDatum.HasValue)
                {
                    sql.Append(" AND DATE(o.Datum) >= @OdDatum");
                    parameters.Add("@OdDatum", filterRequest.OdDatum.Value.Date);
                }

                if (filterRequest.DoDatum.HasValue)
                {
                    sql.Append(" AND DATE(o.Datum) <= @DoDatum");
                    parameters.Add("@DoDatum", filterRequest.DoDatum.Value.Date);
                }

                sql.Append(@"
                    GROUP BY k.ID, k.Naziv
                    HAVING UkupnaKolicina > 0
                    ORDER BY UkupnaKolicina DESC
                    LIMIT 5
                ");

                var rezultat = await _databaseService.QueryAsync<dynamic>(sql.ToString(), parameters);

                return rezultat.ToDictionary(
                    x => (string)x.Komitent ?? "Nepoznato",
                    x => (decimal)x.UkupnaKolicina
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajTopKupce");
                return new Dictionary<string, decimal>();
            }
        }

        public async Task<Dictionary<string, decimal>> UcitajTopDobavljace(FilterRequest filterRequest)
        {
            try
            {
                var sql = new StringBuilder();
                sql.Append(@"
                    SELECT 
                        k.Naziv as Komitent,
                        SUM(ABS(COALESCE(p.Kolicina, 0))) as UkupnaKolicina
                    FROM Prijemnica p
                    LEFT JOIN Komitent k ON p.KomitentID = k.ID
                    WHERE p.Aktivno = 1
                      AND k.Aktivno = 1
                      AND p.MagacinID != 7  -- ISKLJUČI KALO I RASTUR
                      AND p.DokumentStatus IN (2, 3)  -- OTVORENO ILI ZATVORENO
                ");

                var parameters = new Dictionary<string, object>();

                // Datum filteri za Prijemnicu
                if (filterRequest.OdDatum.HasValue)
                {
                    sql.Append(" AND DATE(p.Datum) >= @OdDatum");
                    parameters.Add("@OdDatum", filterRequest.OdDatum.Value.Date);
                }

                if (filterRequest.DoDatum.HasValue)
                {
                    sql.Append(" AND DATE(p.Datum) <= @DoDatum");
                    parameters.Add("@DoDatum", filterRequest.DoDatum.Value.Date);
                }

                sql.Append(@"
                    GROUP BY k.ID, k.Naziv
                    HAVING UkupnaKolicina > 0
                    ORDER BY UkupnaKolicina DESC
                    LIMIT 5
                ");

                var rezultat = await _databaseService.QueryAsync<dynamic>(sql.ToString(), parameters);

                return rezultat.ToDictionary(
                    x => (string)x.Komitent ?? "Nepoznato",
                    x => (decimal)x.UkupnaKolicina
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajTopDobavljace");
                return new Dictionary<string, decimal>();
            }
        }

        // DODATO: Implementacija metoda iz interfejsa koje su nedostajale
        public async Task<List<FinansijeModel>> UcitajSaldoPoKomitentima(FilterRequest filterRequest)
        {
            return await UcitajFinansijskiIzvestaj(filterRequest);
        }

        public async Task<List<FinansijeModel>> UcitajPrometePoArtiklima(FilterRequest filterRequest)
        {
            return await UcitajFinansijskiIzvestaj(filterRequest);
        }

        public async Task<decimal> UcitajUkupanPromet(FilterRequest filterRequest)
        {
            try
            {
                var sql = new StringBuilder();
                sql.Append(@"
                    SELECT COALESCE(SUM(ABS(fm.Potrazuje) + ABS(fm.Duguje)), 0) as UkupanPromet
                    FROM vPrometFinansijev9 fm
                    WHERE 1=1
                ");

                var parameters = new Dictionary<string, object>();

                if (filterRequest.OdDatum.HasValue)
                {
                    sql.Append(" AND DATE(fm.Datum) >= @OdDatum");
                    parameters.Add("@OdDatum", filterRequest.OdDatum.Value.Date);
                }

                if (filterRequest.DoDatum.HasValue)
                {
                    sql.Append(" AND DATE(fm.Datum) <= @DoDatum");
                    parameters.Add("@DoDatum", filterRequest.DoDatum.Value.Date);
                }

                var rezultat = await _databaseService.ExecuteScalarAsync<decimal>(sql.ToString(), parameters);
                return rezultat;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajUkupanPromet");
                return 0;
            }
        }

        public async Task<decimal> UcitajUkupnuZaradu(FilterRequest filterRequest)
        {
            try
            {
                var sql = new StringBuilder();
                sql.Append(@"
                    SELECT COALESCE(SUM(fm.Marza), 0) as UkupnaZarada
                    FROM vPrometFinansijev9 fm
                    WHERE 1=1
                ");

                var parameters = new Dictionary<string, object>();

                if (filterRequest.OdDatum.HasValue)
                {
                    sql.Append(" AND DATE(fm.Datum) >= @OdDatum");
                    parameters.Add("@OdDatum", filterRequest.OdDatum.Value.Date);
                }

                if (filterRequest.DoDatum.HasValue)
                {
                    sql.Append(" AND DATE(fm.Datum) <= @DoDatum");
                    parameters.Add("@DoDatum", filterRequest.DoDatum.Value.Date);
                }

                var rezultat = await _databaseService.ExecuteScalarAsync<decimal>(sql.ToString(), parameters);
                return rezultat;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajUkupnuZaradu");
                return 0;
            }
        }

        // POSTOJEĆE METODE koje su izostavlje iz interfejsa ali su implementirane
        public async Task<List<FinansijeModel>> UcitajFinansijePoKomitentu(long komitentId, FilterRequest filterRequest)
        {
            filterRequest.KomitentId = komitentId;
            return await UcitajFinansijskiIzvestaj(filterRequest);
        }

        public async Task<List<FinansijeModel>> UcitajFinansijePoArtiklu(long artikalId, FilterRequest filterRequest)
        {
            filterRequest.ArtikalId = artikalId;
            return await UcitajFinansijskiIzvestaj(filterRequest);
        }

        public async Task<decimal> UcitajUkupnuZaduzenju(FilterRequest filterRequest)
        {
            try
            {
                var sql = new StringBuilder();
                sql.Append(@"
                    SELECT COALESCE(SUM(fm.Duguje), 0) as UkupnoDuguje
                    FROM vPrometFinansijev9 fm
                    WHERE 1=1
                ");

                var parameters = new Dictionary<string, object>();

                if (filterRequest.OdDatum.HasValue)
                {
                    sql.Append(" AND DATE(fm.Datum) >= @OdDatum");
                    parameters.Add("@OdDatum", filterRequest.OdDatum.Value.Date);
                }

                if (filterRequest.DoDatum.HasValue)
                {
                    sql.Append(" AND DATE(fm.Datum) <= @DoDatum");
                    parameters.Add("@DoDatum", filterRequest.DoDatum.Value.Date);
                }

                var rezultat = await _databaseService.ExecuteScalarAsync<decimal>(sql.ToString(), parameters);
                return rezultat;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajUkupnuZaduzenju");
                return 0;
            }
        }

        public async Task<decimal> UcitajUkupnoPotrazenost(FilterRequest filterRequest)
        {
            try
            {
                var sql = new StringBuilder();
                sql.Append(@"
                    SELECT COALESCE(SUM(fm.Potrazuje), 0) as UkupnoPotrazuje
                    FROM vPrometFinansijev9 fm
                    WHERE 1=1
                ");

                var parameters = new Dictionary<string, object>();

                if (filterRequest.OdDatum.HasValue)
                {
                    sql.Append(" AND DATE(fm.Datum) >= @OdDatum");
                    parameters.Add("@OdDatum", filterRequest.OdDatum.Value.Date);
                }

                if (filterRequest.DoDatum.HasValue)
                {
                    sql.Append(" AND DATE(fm.Datum) <= @DoDatum");
                    parameters.Add("@DoDatum", filterRequest.DoDatum.Value.Date);
                }

                var rezultat = await _databaseService.ExecuteScalarAsync<decimal>(sql.ToString(), parameters);
                return rezultat;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajUkupnoPotrazenost");
                return 0;
            }
        }

        public async Task<Dictionary<string, decimal>> UcitajSaldoPoMesecima(FilterRequest filterRequest)
        {
            try
            {
                var sql = @"
                    SELECT 
                        DATE_FORMAT(fm.Datum, '%Y-%m') as Mesec,
                        SUM(fm.Saldo) as UkupnoSaldo
                    FROM vPrometFinansijev9 fm
                    WHERE fm.Datum >= CURDATE() - INTERVAL 12 MONTH
                    GROUP BY DATE_FORMAT(fm.Datum, '%Y-%m')
                    ORDER BY Mesec
                ";

                var rezultat = await _databaseService.QueryAsync<dynamic>(sql);

                return rezultat.ToDictionary(
                    x => (string)x.Mesec ?? "Nepoznato",
                    x => (decimal)x.UkupnoSaldo
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajSaldoPoMesecima");
                return new Dictionary<string, decimal>();
            }
        }

        // ========================================
        // NOVO: BRZI PREGLED - ROBA NA ZALIHAMA
        // ========================================
        public async Task<List<RobaNaZalihamaModel>> UcitajRobuNaZalihama(
            List<long> artikalIds,
            DateTime? odDatum = null,
            DateTime? doDatum = null)
        {
            try
            {
                if (artikalIds == null || !artikalIds.Any())
                {
                    _logger.LogInformation("Lista artikala je prazna!");
                    return new List<RobaNaZalihamaModel>();
                }

                var rezultat = new List<RobaNaZalihamaModel>();

                // Build parameterized IN clause (@ArtikalID0, @ArtikalID1, @ArtikalID2, ...)
                var artikalIdParams = new List<string>();
                var sharedParameters = new Dictionary<string, object>();
                for (int i = 0; i < artikalIds.Count; i++)
                {
                    var paramName = $"@ArtikalID{i}";
                    artikalIdParams.Add(paramName);
                    sharedParameters.Add(paramName, artikalIds[i]);
                }
                var artikalIdsInClause = string.Join(",", artikalIdParams);

                // === STEP 1: NABAVKA (KL- dokumenti) ===
                var sqlNabavka = $@"
                    SELECT
                        vp.ArtikalID,
                        vp.Artikal,
                        SUM(ABS(vp.Kolicina)) as NabavkaKg,
                        SUM(ABS(vp.Potrazuje)) as NabavkaVrednost
                    FROM vPrometFinansijev9 vp
                    WHERE vp.ArtikalID IN ({artikalIdsInClause})
                      AND vp.Dokument LIKE 'KL-%'
                      AND vp.DokumentStatus != 4
                      AND vp.Cena > 0
                ";

                var parametersNabavka = new Dictionary<string, object>(sharedParameters);
                if (odDatum.HasValue)
                {
                    sqlNabavka += " AND DATE(vp.Datum) >= @OdDatum";
                    parametersNabavka.Add("@OdDatum", odDatum.Value.Date);
                }
                if (doDatum.HasValue)
                {
                    sqlNabavka += " AND DATE(vp.Datum) <= @DoDatum";
                    parametersNabavka.Add("@DoDatum", doDatum.Value.Date);
                }
                sqlNabavka += " GROUP BY vp.ArtikalID, vp.Artikal";

                var nabavkaData = await _databaseService.QueryAsync<dynamic>(sqlNabavka, parametersNabavka);

                // === STEP 2: PRODAJA (FK- dokumenti) ===
                var sqlProdaja = $@"
                    SELECT
                        vp.ArtikalID,
                        SUM(ABS(vp.Kolicina)) as ProdajaKg,
                        SUM(ABS(vp.Duguje)) as ProdajaVrednost
                    FROM vPrometFinansijev9 vp
                    WHERE vp.ArtikalID IN ({artikalIdsInClause})
                      AND vp.Dokument LIKE 'FK-%'
                      AND vp.DokumentStatus != 4
                      AND vp.Cena > 0
                ";

                var parametersProdaja = new Dictionary<string, object>(sharedParameters);
                if (odDatum.HasValue)
                {
                    sqlProdaja += " AND DATE(vp.Datum) >= @OdDatum";
                    parametersProdaja.Add("@OdDatum", odDatum.Value.Date);
                }
                if (doDatum.HasValue)
                {
                    sqlProdaja += " AND DATE(vp.Datum) <= @DoDatum";
                    parametersProdaja.Add("@DoDatum", doDatum.Value.Date);
                }
                sqlProdaja += " GROUP BY vp.ArtikalID";

                var prodajaData = await _databaseService.QueryAsync<dynamic>(sqlProdaja, parametersProdaja);

                // === STEP 3: LAGER + CENE ===
                var sqlLager = $@"
                    SELECT
                        ml.ArtikalID,
                        SUM(ml.Kolicina) as LagerKg,
                        COALESCE(kac.BrutoCena, 0) as BrutoCena
                    FROM vwMagacinLager ml
                    LEFT JOIN KalkulacijaArtikalCena kac ON ml.ArtikalID = kac.ArtikalID
                    WHERE ml.ArtikalID IN ({artikalIdsInClause})
                    GROUP BY ml.ArtikalID, kac.BrutoCena
                ";

                var lagerData = await _databaseService.QueryAsync<dynamic>(sqlLager, sharedParameters);

                // === STEP 4: KOMBINOVANJE PODATAKA ===
                foreach (var nabavka in nabavkaData)
                {
                    var model = new RobaNaZalihamaModel
                    {
                        ArtikalID = Convert.ToInt64(nabavka.ArtikalID),
                        Artikal = (string)nabavka.Artikal ?? "Nepoznato",
                        NabavkaKg = (decimal)nabavka.NabavkaKg,
                        NabavkaVrednost = (decimal)nabavka.NabavkaVrednost
                    };

                    // Dodaj prodaju ako postoji
                    var prodaja = prodajaData.FirstOrDefault(p => Convert.ToInt64(p.ArtikalID) == model.ArtikalID);
                    if (prodaja != null)
                    {
                        model.ProdajaKg = (decimal)prodaja.ProdajaKg;
                        model.ProdajaVrednost = (decimal)prodaja.ProdajaVrednost;
                    }

                    // Dodaj lager ako postoji
                    var lager = lagerData.FirstOrDefault(l => Convert.ToInt64(l.ArtikalID) == model.ArtikalID);
                    if (lager != null)
                    {
                        model.LagerKg = (decimal)lager.LagerKg;
                        model.BrutoCena = (decimal)lager.BrutoCena;
                        model.LagerVrednost = model.LagerKg * model.BrutoCena;
                    }

                    rezultat.Add(model);
                }

                _logger.LogInformation($"Učitano {rezultat.Count} artikala za brzi pregled");
                return rezultat;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajRobuNaZalihama");
                _logger.LogInformation($"Stack trace: {ex.StackTrace}");
                return new List<RobaNaZalihamaModel>();
            }
        }
        public async Task<List<ZbirniFinansijeModel>> UcitajZbirnePodatkePoRadnomDanu(FilterRequest filterRequest)
        {
        try
        {
        var sql = new StringBuilder();
        sql.Append(@"
            SELECT
                DATE(DATE_SUB(vp.Datum, INTERVAL 4 HOUR)) as Datum,
                k.ID as KomitentID,
                k.Naziv as Komitent,
                a.MagacinID as Artikal,
                COUNT(DISTINCT vp.DokumentID) as BrojDokumenata,
                SUM(vp.Kolicina) as UkupnaKolicina,
                SUM(vp.Potrazuje) as UkupnoPotrazuje,
                SUM(vp.Duguje) as UkupnoDuguje
            FROM vPrometFinansijev9 vp
            LEFT JOIN Komitent k ON vp.KomitentID = k.ID
            LEFT JOIN Artikal a ON vp.ArtikalID = a.ID
            WHERE vp.DokumentStatus = 3
        ");

        var parameters = new Dictionary<string, object>();

        // Prilagođeni datumi za radni dan 04:00-03:59
        if (filterRequest.OdDatum.HasValue)
        {
            var adjustedOdDatum = filterRequest.OdDatum.Value.AddHours(4);
            sql.Append(" AND vp.Datum >= @OdDatum");
            parameters.Add("@OdDatum", adjustedOdDatum);
        }

        if (filterRequest.DoDatum.HasValue)
        {
            var adjustedDoDatum = filterRequest.DoDatum.Value.AddDays(1).AddHours(4);
            sql.Append(" AND vp.Datum < @DoDatum");
            parameters.Add("@DoDatum", adjustedDoDatum);
        }

        if (filterRequest.KomitentId.HasValue && filterRequest.KomitentId > 0)
        {
            sql.Append(" AND vp.KomitentID = @KomitentId");
            parameters.Add("@KomitentId", filterRequest.KomitentId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filterRequest.KomitentTip))
        {
            sql.Append(" AND k.Tip = @KomitentTip");
            parameters.Add("@KomitentTip", filterRequest.KomitentTip);
        }

        if (filterRequest.ArtikalId.HasValue && filterRequest.ArtikalId > 0)
        {
            sql.Append(" AND vp.ArtikalID = @ArtikalId");
            parameters.Add("@ArtikalId", filterRequest.ArtikalId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filterRequest.Tip))
        {
            sql.Append(" AND a.MagacinID = @Tip");
            parameters.Add("@Tip", filterRequest.Tip);
        }

        // Isključi FK- i UP- dokumente
        sql.Append(" AND vp.DokumentID NOT LIKE 'FK-%'");
        sql.Append(" AND vp.DokumentID NOT LIKE 'UP-%'");

        sql.Append(@"
            GROUP BY DATE(DATE_SUB(vp.Datum, INTERVAL 4 HOUR)), k.ID, k.Naziv, a.MagacinID
            ORDER BY Datum DESC, k.Naziv, a.MagacinID
        ");

        var rezultat = await _databaseService.QueryAsync<ZbirniFinansijeModel>(sql.ToString(), parameters);
        return rezultat.ToList();
        }
        catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajZbirnePodatkePoRadnomDanu");
                return new List<ZbirniFinansijeModel>();
            }
        }
    }
}