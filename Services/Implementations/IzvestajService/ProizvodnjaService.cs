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
                    rn.Sifra as RadniNalog,
                    vpp.Artikal,
                    CAST(CASE a.Tip
                        WHEN 1 THEN 'Sirovina'
                        WHEN 2 THEN 'Ambalaza'
                        WHEN 3 THEN 'Potrošni materijal'
                        WHEN 4 THEN 'Gotova roba'
                        WHEN 5 THEN 'Oprema'
                        WHEN 7 THEN 'Klase'
                        ELSE CONCAT('Tip ', a.Tip)
                    END AS CHAR(50)) as TipArtikla,
                    CASE 
                        WHEN a.Tip NOT IN (2, 4) THEN vpp.Kolicina 
                        ELSE 0 
                    END as KolicinaRoba,
                    CASE 
                        WHEN a.Tip = 2 THEN vpp.Kolicina 
                        ELSE 0 
                    END as KolicinaAmbalaza,
                    CASE 
                        WHEN a.Tip = 4 THEN vpp.Kolicina 
                        ELSE 0 
                    END as GotovProizvod,
                    vpp.Kolicina,
                    vpp.Komitent,
                    rn.ID as RadniNalogID,
                    rn.KomitentID,
                    rn.DokumentStatus,
                    vpp.ArtikalID,
                    vpp.ArtikalPrvaKlasifikacijaID,
                    a.Tip as Tip
                FROM RadniNalog rn
                LEFT JOIN vPreradaPregled vpp ON rn.ID = vpp.RadniNalogID
                LEFT JOIN Artikal a ON vpp.ArtikalID = a.ID
                WHERE rn.Aktivno = 1
                    AND a.Aktivno = 1
                    AND a.ID IS NOT NULL
                    AND a.MagacinID != 7  -- ISKLJUČI KALO I RASTUR
                ");

                var parameters = new Dictionary<string, object>();

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
                Console.WriteLine($"Greška u UcitajIzvestajProizvodnje: {ex.Message}");
                throw;
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
                    LEFT JOIN Artikal a ON vpp.ArtikalID = a.ID
                    WHERE a.MagacinID = 6  -- GOTOVI PROIZVODI
                      AND a.Aktivno = 1
                ");

                var parameters = new Dictionary<string, object>();

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
                Console.WriteLine($"Greška u UcitajGotoveProizvodePoslednjihDana: {ex.Message}");
                return 0;
            }
        }

        public async Task<Dictionary<string, decimal>> UcitajTopKupcePoKilogramima(FilterRequest filterRequest)
        {
            try
            {
                var sql = new StringBuilder();
                sql.Append(@"
                    SELECT 
                        COALESCE(vpp.Komitent, 'Nepoznato') as Komitent,
                        SUM(ABS(vpp.Kolicina)) as UkupnaKolicina
                    FROM vPreradaPregled vpp
                    LEFT JOIN RadniNalog rn ON vpp.RadniNalogID = rn.ID
                    LEFT JOIN Artikal a ON vpp.ArtikalID = a.ID
                    WHERE a.MagacinID = 6  -- GOTOVI PROIZVODI (IZLAZ - KUPCI)
                      AND vpp.Kolicina < 0  -- NEGATIVNA KOLICINA = IZLAZ
                      AND EXISTS (SELECT 1 FROM Artikal ar WHERE ar.ID = vpp.ArtikalID AND ar.Aktivno = 1)
                      AND vpp.Komitent IS NOT NULL
                      AND vpp.Komitent != ''
                ");

                var parameters = new Dictionary<string, object>();

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
                Console.WriteLine($"Greška u UcitajTopKupcePoKilogramima: {ex.Message}");
                return new Dictionary<string, decimal>();
            }
        }

        public async Task<Dictionary<string, decimal>> UcitajTopDobavljacePoKilogramima(FilterRequest filterRequest)
        {
            try
            {
                var sql = new StringBuilder();
                sql.Append(@"
                    SELECT 
                        COALESCE(vpp.Komitent, 'Nepoznato') as Komitent,
                        SUM(ABS(vpp.Kolicina)) as UkupnaKolicina
                    FROM vPreradaPregled vpp
                    LEFT JOIN RadniNalog rn ON vpp.RadniNalogID = rn.ID
                    LEFT JOIN Artikal a ON vpp.ArtikalID = a.ID
                    WHERE a.MagacinID IN (2, 3)  -- SVEZA ROBA I SIROVINE (ULAZ - DOBAVLJACI)
                      AND vpp.Kolicina > 0  -- POZITIVNA KOLICINA = ULAZ
                      AND EXISTS (SELECT 1 FROM Artikal ar WHERE ar.ID = vpp.ArtikalID AND ar.Aktivno = 1)
                      AND vpp.Komitent IS NOT NULL
                      AND vpp.Komitent != ''
                ");

                var parameters = new Dictionary<string, object>();

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
                Console.WriteLine($"Greška u UcitajTopDobavljacePoKilogramima: {ex.Message}");
                return new Dictionary<string, decimal>();
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
                    LEFT JOIN Artikal a ON vpp.ArtikalID = a.ID
                    WHERE a.MagacinID = 6  -- GOTOVI PROIZVODI
                      AND a.Aktivno = 1
                ");

                var parameters = new Dictionary<string, object>();

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
                    GROUP BY vpp.Artikal
                    HAVING SUM(vpp.Kolicina) > 0
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

        public async Task<List<string>> UcitajListuRadnihNaloga(FilterRequest filterRequest)
        {
            try
            {
                var sql = new StringBuilder();
                sql.Append(@"
                    SELECT DISTINCT rn.Sifra
                    FROM RadniNalog rn
                    WHERE rn.Aktivno = 1 AND rn.Sifra IS NOT NULL AND rn.Sifra != ''
                ");

                var parameters = new Dictionary<string, object>();

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

                sql.Append(" ORDER BY rn.Sifra DESC");

                var rezultat = await _databaseService.QueryAsync<string>(sql.ToString(), parameters);
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajListuRadnihNaloga: {ex.Message}");
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
                Console.WriteLine($"Greška u UcitajBrojAktivnihNaloga: {ex.Message}");
                return 0;
            }
        }

        public async Task<Dictionary<string, decimal>> UcitajProizvodnjuPoKomitentima(FilterRequest filterRequest)
        {
            try
            {
                var sql = new StringBuilder();
                sql.Append(@"
                    SELECT 
                        COALESCE(vpp.Komitent, 'Nepoznato') as Komitent,
                        SUM(vpp.Kolicina) as UkupnaKolicina
                    FROM vPreradaPregled vpp
                    LEFT JOIN RadniNalog rn ON vpp.RadniNalogID = rn.ID
                    WHERE vpp.RpArtikalTip = 4  -- SAMO GOTOVI PROIZVODI
                    AND EXISTS (SELECT 1 FROM Artikal a WHERE a.ID = vpp.ArtikalID AND a.Aktivno = 1)
                ");

                var parameters = new Dictionary<string, object>();

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

        public async Task<Dictionary<int, decimal>> UcitajProizvodnjuPoTipovima(FilterRequest filterRequest)
        {
            try
            {
                var sql = new StringBuilder();
                sql.Append(@"
                    SELECT 
                        vpp.RpArtikalTip as Tip,
                        SUM(vpp.Kolicina) as UkupnaKolicina
                    FROM vPreradaPregled vpp
                    LEFT JOIN RadniNalog rn ON vpp.RadniNalogID = rn.ID
                    WHERE EXISTS (SELECT 1 FROM Artikal a WHERE a.ID = vpp.ArtikalID AND a.Aktivno = 1)
                ");

                var parameters = new Dictionary<string, object>();

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
                    x => (int)x.Tip,
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
                    WHERE vpp.RpArtikalTip = 4  -- SAMO GOTOVI PROIZVODI
                    AND EXISTS (SELECT 1 FROM Artikal a WHERE a.ID = vpp.ArtikalID AND a.Aktivno = 1)
                ");

                var parameters = new Dictionary<string, object>();

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

                sql.Append($@"
                    ORDER BY vpp.Kolicina DESC
                    LIMIT {brojNaloga}
                ");

                var rezultat = await _databaseService.QueryAsync<ProizvodnjaModel>(sql.ToString(), parameters);
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajNajproduktivnijeNaloge: {ex.Message}");
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
