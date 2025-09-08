using FruitSysWeb.Models;
using FruitSysWeb.Services.Interfaces;
using FruitSysWeb.Services.Models.Requests;
using System.Text;

namespace FruitSysWeb.Services.Implementations.IzvestajService
{
    public class ProizvodnjaService : IProizvodnjaService
    {
        private readonly DatabaseService _databaseService;

        public ProizvodnjaService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<List<ProizvodnjaModel>> UcitajIzvestajProizvodnje(FilterRequest filterRequest)
        {
            try
            {
                var sql = new StringBuilder();
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
                    WHERE 1=1
                ");

                var parameters = new Dictionary<string, object>();

                // Datum filteri
                if (filterRequest.OdDatum.HasValue)
                {
                    sql.Append(" AND DATE(rn.DatumPocetka) >= @OdDatum");
                    parameters.Add("@OdDatum", filterRequest.OdDatum.Value.Date);
                }

                if (filterRequest.DoDatum.HasValue)
                {
                    sql.Append(" AND DATE(rn.DatumPocetka) <= @DoDatum");
                    parameters.Add("@DoDatum", filterRequest.DoDatum.Value.Date);
                }

                // Radni nalog filter
                if (!string.IsNullOrWhiteSpace(filterRequest.RadniNalog))
                {
                    sql.Append(" AND vpp.RadniNalog LIKE @RadniNalog");
                    parameters.Add("@RadniNalog", $"%{filterRequest.RadniNalog}%");
                }

                // Komitent filter
                if (filterRequest.KomitentId.HasValue && filterRequest.KomitentId > 0)
                {
                    sql.Append(" AND vpp.KomitentID = @KomitentId");
                    parameters.Add("@KomitentId", filterRequest.KomitentId.Value);
                }

                // Artikal filter
                if (filterRequest.ArtikalId.HasValue && filterRequest.ArtikalId > 0)
                {
                    sql.Append(" AND vpp.ArtikalID = @ArtikalId");
                    parameters.Add("@ArtikalId", filterRequest.ArtikalId.Value);
                }

                // Tip artikla filter
                if (filterRequest.TipArtikla.HasValue)
                {
                    sql.Append(" AND vpp.RpArtikalTip = @TipArtikla");
                    parameters.Add("@TipArtikla", filterRequest.TipArtikla.Value);
                }

                // Količina filteri
                if (filterRequest.MinKolicina.HasValue)
                {
                    sql.Append(" AND vpp.Kolicina >= @MinKolicina");
                    parameters.Add("@MinKolicina", filterRequest.MinKolicina.Value);
                }

                if (filterRequest.MaxKolicina.HasValue)
                {
                    sql.Append(" AND vpp.Kolicina <= @MaxKolicina");
                    parameters.Add("@MaxKolicina", filterRequest.MaxKolicina.Value);
                }

                // Multiple artikal IDs filter
                if (filterRequest.ArtikalIds != null && filterRequest.ArtikalIds.Any())
                {
                    var artikalIdsList = string.Join(",", filterRequest.ArtikalIds);
                    sql.Append($" AND vpp.ArtikalID IN ({artikalIdsList})");
                }

                // Multiple komitent IDs filter
                if (filterRequest.KomitentIds != null && filterRequest.KomitentIds.Any())
                {
                    var komitentIdsList = string.Join(",", filterRequest.KomitentIds);
                    sql.Append($" AND vpp.KomitentID IN ({komitentIdsList})");
                }

                sql.Append(" ORDER BY rn.DatumPocetka DESC, vpp.RadniNalog");

                var rezultat = await _databaseService.QueryAsync<ProizvodnjaModel>(sql.ToString(), parameters);
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajIzvestajProizvodnje: {ex.Message}");
                throw;
            }
        }

        public async Task<Dictionary<string, decimal>> UcitajProizvodnjuPoArtiklima(FilterRequest filterRequest)
        {
            try
            {
                var sql = new StringBuilder();
                sql.Append(@"
                    SELECT 
                        vpp.Artikal,
                        SUM(vpp.Kolicina) as UkupnaKolicina
                    FROM vPreradaPregled vpp
                    LEFT JOIN RadniNalog rn ON vpp.RadniNalogID = rn.ID
                    WHERE 1=1
                ");

                var parameters = new Dictionary<string, object>();

                // Datum filteri
                if (filterRequest.OdDatum.HasValue)
                {
                    sql.Append(" AND DATE(rn.DatumPocetka) >= @OdDatum");
                    parameters.Add("@OdDatum", filterRequest.OdDatum.Value.Date);
                }

                if (filterRequest.DoDatum.HasValue)
                {
                    sql.Append(" AND DATE(rn.DatumPocetka) <= @DoDatum");
                    parameters.Add("@DoDatum", filterRequest.DoDatum.Value.Date);
                }

                sql.Append(@"
                    GROUP BY vpp.Artikal, vpp.ArtikalID
                    ORDER BY UkupnaKolicina DESC
                ");

                var rezultat = await _databaseService.QueryAsync<dynamic>(sql.ToString(), parameters);
                
                return rezultat.ToDictionary(
                    x => (string)x.Artikal ?? "Nepoznato",
                    x => (decimal)x.UkupnaKolicina
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajProizvodnjuPoArtiklima: {ex.Message}");
                return new Dictionary<string, decimal>();
            }
        }

        public async Task<List<ProizvodnjaModel>> UcitajSveRadneNaloge()
        {
            try
            {
                var filterRequest = new FilterRequest
                {
                    OdDatum = DateTime.Now.AddYears(-1),
                    DoDatum = DateTime.Now
                };

                return await UcitajIzvestajProizvodnje(filterRequest);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajSveRadneNaloge: {ex.Message}");
                return new List<ProizvodnjaModel>();
            }
        }

        public async Task<decimal> UcitajUkupnuProizvodnju(FilterRequest filterRequest)
        {
            try
            {
                var sql = new StringBuilder();
                sql.Append(@"
                    SELECT COALESCE(SUM(vpp.Kolicina), 0) as UkupnaKolicina
                    FROM vPreradaPregled vpp
                    LEFT JOIN RadniNalog rn ON vpp.RadniNalogID = rn.ID
                    WHERE 1=1
                ");

                var parameters = new Dictionary<string, object>();

                // Datum filteri
                if (filterRequest.OdDatum.HasValue)
                {
                    sql.Append(" AND DATE(rn.DatumPocetka) >= @OdDatum");
                    parameters.Add("@OdDatum", filterRequest.OdDatum.Value.Date);
                }

                if (filterRequest.DoDatum.HasValue)
                {
                    sql.Append(" AND DATE(rn.DatumPocetka) <= @DoDatum");
                    parameters.Add("@DoDatum", filterRequest.DoDatum.Value.Date);
                }

                var rezultat = await _databaseService.ExecuteScalarAsync<decimal>(sql.ToString(), parameters);
                return rezultat;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajUkupnuProizvodnju: {ex.Message}");
                return 0;
            }
        }

        public async Task<int> UcitajBrojAktivnihNaloga(FilterRequest filterRequest)
        {
            try
            {
                var sql = @"
                    SELECT COUNT(DISTINCT rn.ID) as BrojNaloga
                    FROM RadniNalog rn
                    WHERE rn.Aktivno = 1 AND rn.DokumentStatus IN (1, 2)
                ";

                var rezultat = await _databaseService.ExecuteScalarAsync<int>(sql);
                return rezultat;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajBrojAktivnihNaloga: {ex.Message}");
                return 0;
            }
        }

        public async Task<List<ProizvodnjaModel>> UcitajRadneNalogePoDatumu(DateTime odDatum, DateTime doDatum)
        {
            var filterRequest = new FilterRequest
            {
                OdDatum = odDatum,
                DoDatum = doDatum
            };

            return await UcitajIzvestajProizvodnje(filterRequest);
        }

        public async Task<List<ProizvodnjaModel>> UcitajRadneNalogePoKomitentu(long komitentId)
        {
            var filterRequest = new FilterRequest
            {
                KomitentId = komitentId
            };

            return await UcitajIzvestajProizvodnje(filterRequest);
        }

        public async Task<List<ProizvodnjaModel>> UcitajRadneNalogePoArtiklu(long artikalId)
        {
            var filterRequest = new FilterRequest
            {
                ArtikalId = artikalId
            };

            return await UcitajIzvestajProizvodnje(filterRequest);
        }

        public async Task<List<ProizvodnjaModel>> UcitajRadneNalogePoTipu(int tipArtikla)
        {
            var filterRequest = new FilterRequest
            {
                TipArtikla = tipArtikla
            };

            return await UcitajIzvestajProizvodnje(filterRequest);
        }

        public async Task<Dictionary<string, decimal>> UcitajProizvodnjuPoKomitentima(FilterRequest filterRequest)
        {
            try
            {
                var sql = new StringBuilder();
                sql.Append(@"
                    SELECT 
                        vpp.Komitent,
                        SUM(vpp.Kolicina) as UkupnaKolicina
                    FROM vPreradaPregled vpp
                    LEFT JOIN RadniNalog rn ON vpp.RadniNalogID = rn.ID
                    WHERE 1=1
                ");

                var parameters = new Dictionary<string, object>();

                // Datum filteri
                if (filterRequest.OdDatum.HasValue)
                {
                    sql.Append(" AND DATE(rn.DatumPocetka) >= @OdDatum");
                    parameters.Add("@OdDatum", filterRequest.OdDatum.Value.Date);
                }

                if (filterRequest.DoDatum.HasValue)
                {
                    sql.Append(" AND DATE(rn.DatumPocetka) <= @DoDatum");
                    parameters.Add("@DoDatum", filterRequest.DoDatum.Value.Date);
                }

                sql.Append(@"
                    GROUP BY vpp.Komitent, vpp.KomitentID
                    ORDER BY UkupnaKolicina DESC
                ");

                var rezultat = await _databaseService.QueryAsync<dynamic>(sql.ToString(), parameters);
                
                return rezultat.ToDictionary(
                    x => (string)x.Komitent ?? "Nepoznato",
                    x => (decimal)x.UkupnaKolicina
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajProizvodnjuPoKomitentima: {ex.Message}");
                return new Dictionary<string, decimal>();
            }
        }

        public async Task<Dictionary<string, decimal>> UcitajProizvodnjuPoMesecima(FilterRequest filterRequest)
        {
            try
            {
                var sql = @"
                    SELECT 
                        DATE_FORMAT(rn.DatumPocetka, '%Y-%m') as Mesec,
                        SUM(vpp.Kolicina) as UkupnaKolicina
                    FROM vPreradaPregled vpp
                    LEFT JOIN RadniNalog rn ON vpp.RadniNalogID = rn.ID
                    WHERE rn.DatumPocetka >= CURDATE() - INTERVAL 12 MONTH
                    GROUP BY DATE_FORMAT(rn.DatumPocetka, '%Y-%m')
                    ORDER BY Mesec
                ";

                var rezultat = await _databaseService.QueryAsync<dynamic>(sql);
                
                return rezultat.ToDictionary(
                    x => (string)x.Mesec ?? "Nepoznato",
                    x => (decimal)x.UkupnaKolicina
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajProizvodnjuPoMesecima: {ex.Message}");
                return new Dictionary<string, decimal>();
            }
        }

        public async Task<Dictionary<int, decimal>> UcitajProizvodnjuPoTipovima(FilterRequest filterRequest)
        {
            try
            {
                var sql = new StringBuilder();
                sql.Append(@"
                    SELECT 
                        vpp.RpArtikalTip as TipArtikla,
                        SUM(vpp.Kolicina) as UkupnaKolicina
                    FROM vPreradaPregled vpp
                    LEFT JOIN RadniNalog rn ON vpp.RadniNalogID = rn.ID
                    WHERE 1=1
                ");

                var parameters = new Dictionary<string, object>();

                // Datum filteri
                if (filterRequest.OdDatum.HasValue)
                {
                    sql.Append(" AND DATE(rn.DatumPocetka) >= @OdDatum");
                    parameters.Add("@OdDatum", filterRequest.OdDatum.Value.Date);
                }

                if (filterRequest.DoDatum.HasValue)
                {
                    sql.Append(" AND DATE(rn.DatumPocetka) <= @DoDatum");
                    parameters.Add("@DoDatum", filterRequest.DoDatum.Value.Date);
                }

                sql.Append(@"
                    GROUP BY vpp.RpArtikalTip
                    ORDER BY UkupnaKolicina DESC
                ");

                var rezultat = await _databaseService.QueryAsync<dynamic>(sql.ToString(), parameters);
                
                return rezultat.ToDictionary(
                    x => (int)x.TipArtikla,
                    x => (decimal)x.UkupnaKolicina
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajProizvodnjuPoTipovima: {ex.Message}");
                return new Dictionary<int, decimal>();
            }
        }

        public async Task<List<ProizvodnjaModel>> UcitajNajproduktivnijeNaloge(FilterRequest filterRequest, int brojNaloga = 10)
        {
            try
            {
                var podaci = await UcitajIzvestajProizvodnje(filterRequest);
                
                return podaci
                    .OrderByDescending(p => p.Kolicina)
                    .Take(brojNaloga)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajNajproduktivnijeNaloge: {ex.Message}");
                return new List<ProizvodnjaModel>();
            }
        }

        public async Task<List<PreradaPregledModel>> UcitajPreraduPregled(FilterRequest filterRequest)
        {
            try
            {
                var podaci = await UcitajIzvestajProizvodnje(filterRequest);
                
                // Konvertuj u prerada pregled format
                return podaci.Select(pm => new PreradaPregledModel
                {
                    Datum = pm.Datum,
                    RadniNalog = pm.RadniNalog,
                    Artikal = pm.Artikal,
                    KolicinaGotovProizvod = pm.TipArtikla == 2 ? pm.Kolicina : 0,
                    KolicinaSirovina = pm.TipArtikla != 2 ? pm.Kolicina : 0,
                    Komitent = pm.Komitent
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajPreraduPregled: {ex.Message}");
                return new List<PreradaPregledModel>();
            }
        }

        public async Task<Dictionary<string, decimal>> UcitajAnalizuPreradePoArtiklima(FilterRequest filterRequest)
        {
            try
            {
                var preradaPodaci = await UcitajPreraduPregled(filterRequest);
                
                return preradaPodaci
                    .GroupBy(p => p.Artikal)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Sum(p => (p.KolicinaGotovProizvod ?? 0) + (p.KolicinaSirovina ?? 0))
                    );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajAnalizuPreradePoArtiklima: {ex.Message}");
                return new Dictionary<string, decimal>();
            }
        }
    }
}