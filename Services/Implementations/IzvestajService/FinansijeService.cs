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
                        fm.Potrazuje,
                        fm.Duguje,
                        fm.Saldo,
                        fm.Roba,
                        fm.Provizija,
                        fm.Prevoz,
                        fm.Marza,
                        fm.PCenaPrijem,
                        fm.PCenaUkupno,
                        fm.Uplata,
                        fm.PorezIznos,
                        fm.PorezIznos2,
                        fm.UplataPdv,
                        fm.NetoIznosOtkup,
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
                if (!string.IsNullOrEmpty(filterRequest.ArtikalTip))
                {
                    if (int.TryParse(filterRequest.ArtikalTip, out int artikalTipInt))
                    {
                        // Ovde treba join sa Artikli tabelom da dobijemo tip
                        sql.Append(" AND EXISTS (SELECT 1 FROM Artikli a WHERE a.ID = fm.ArtikalID AND a.Tip = @ArtikalTip)");
                        parameters.Add("@ArtikalTip", artikalTipInt);
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
                        fm.Komitent,
                        SUM(fm.Saldo) as UkupnoSaldo
                    FROM vPrometFinansijev9 fm
                    WHERE fm.Kupac = 1
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

                sql.Append(@"
                    GROUP BY fm.Komitent, fm.KomitentID
                    ORDER BY UkupnoSaldo DESC
                    LIMIT 5
                ");

                var rezultat = await _databaseService.QueryAsync<dynamic>(sql.ToString(), parameters);
                
                return rezultat.ToDictionary(
                    x => (string)x.Komitent ?? "Nepoznato",
                    x => (decimal)x.UkupnoSaldo
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
                        fm.Komitent,
                        SUM(fm.Saldo) as UkupnoSaldo
                    FROM vPrometFinansijev9 fm
                    WHERE fm.Dobavljac = 1
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

                sql.Append(@"
                    GROUP BY fm.Komitent, fm.KomitentID
                    ORDER BY UkupnoSaldo DESC
                    LIMIT 5
                ");

                var rezultat = await _databaseService.QueryAsync<dynamic>(sql.ToString(), parameters);
                
                return rezultat.ToDictionary(
                    x => (string)x.Komitent ?? "Nepoznato",
                    x => (decimal)x.UkupnoSaldo
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