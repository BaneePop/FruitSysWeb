using FruitSysWeb.Models;
using FruitSysWeb.Services.Interfaces;
using FruitSysWeb.Services.Models.Requests;
using System.Text;

namespace FruitSysWeb.Services.Implementations.IzvestajService
{
    public class FinansijeService : IFinansijeService
    {
        private readonly DatabaseService _databaseService;

        public FinansijeService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<List<FinansijeModel>> UcitajFinansijskiIzvestaj(FilterRequest filterRequest)
        {
            try
            {
                var sql = new StringBuilder();
                sql.Append(@"
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
                    WHERE 1=1
                ");

                var parameters = new Dictionary<string, object>();

                // Datum filteri
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
                        sql.Append(" AND EXISTS (SELECT 1 FROM Artikal a WHERE a.ID = fm.ArtikalID AND a.Tip = @ArtikalTip)");
                        parameters.Add("@ArtikalTip", TipInt);
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
                Console.WriteLine($"Greška u UcitajFinansijskiIzvestaj: {ex.Message}");
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
                Console.WriteLine($"Greška u UcitajUkupnoSaldo: {ex.Message}");
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
                    LEFT JOIN Artikal a ON o.ArtikalID = a.ID
                    WHERE o.Aktivno = 1
                      AND k.Aktivno = 1
                      AND a.Aktivno = 1
                      AND a.MagacinID != 7  -- ISKLJUČI KALO I RASTUR
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
                Console.WriteLine($"Greška u UcitajTopKupce: {ex.Message}");
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
                    LEFT JOIN Artikal a ON p.ArtikalID = a.ID
                    WHERE p.Aktivno = 1
                      AND k.Aktivno = 1
                      AND a.Aktivno = 1
                      AND a.MagacinID != 7  -- ISKLJUČI KALO I RASTUR
                      AND p.DokumentStatus IN (2, 3)  -- OTVORENO ILI ZATVORENO
                ");

                var parameters = new Dictionary<string, object>();

                // Datum filteri
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
                Console.WriteLine($"Greška u UcitajTopDobavljace: {ex.Message}");
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
                Console.WriteLine($"Greška u UcitajUkupanPromet: {ex.Message}");
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
                Console.WriteLine($"Greška u UcitajUkupnuZaradu: {ex.Message}");
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
                Console.WriteLine($"Greška u UcitajUkupnuZaduzenju: {ex.Message}");
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
                Console.WriteLine($"Greška u UcitajUkupnoPotrazenost: {ex.Message}");
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
                Console.WriteLine($"Greška u UcitajSaldoPoMesecima: {ex.Message}");
                return new Dictionary<string, decimal>();
            }
        }
    }
}