using FruitSysWeb.Models;
using FruitSysWeb.Services.Models.Requests;
using System.Text;
using FruitSysWeb.Services.Interfaces;

namespace FruitSysWeb.Services.Implementations.IzvestajService
{
    public class MagacinLagerService : IMagacinLagerService
    {
        private readonly DatabaseService _databaseService;

        public MagacinLagerService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        // ============================================
        // 🔧 KLJUČNA ISPRAVKA: TipArtikla Mapping
        // ============================================
        // Koristi isti pattern kao ProizvodnjaService - a.MagacinID sa CASE statement

        private const string TipArtiklaCaseStatement = @"
            CAST(CASE a.MagacinID
                WHEN 2 THEN 'Sveza Roba'
                WHEN 3 THEN 'Sirovine'
                WHEN 4 THEN 'Ambalaza'
                WHEN 5 THEN 'Polu Proizvod'
                WHEN 6 THEN 'Gotov Proizvod'
                WHEN 8 THEN 'Usl.Mleko'
                WHEN 9 THEN 'Repromaterijal'
                WHEN 10 THEN 'Đubriva'
                WHEN 11 THEN 'Usl. Voće'
                WHEN 12 THEN 'Usl. Meso'
                ELSE CONCAT('MagacinID ', a.MagacinID)
            END AS CHAR(50))";

        // NOVE METODE za Kutije i Kese
        public async Task<Dictionary<string, decimal>> UcitajStrukturuKutija()
        {
            try
            {
                var sql = @"
                    SELECT 
                        ml.Artikal,
                        SUM(ml.Kolicina) as UkupnaKolicina
                    FROM vwMagacinLager ml
                    LEFT JOIN Artikal a ON ml.ArtikalID = a.ID
                    WHERE a.MagacinID = 4  -- AMBALAZA
                      AND a.GrupnaAmbalaza = 1  -- KUTIJE/DZAKOVI
                      AND ml.Kolicina >= 10
                      AND ml.Kolicina IS NOT NULL
                      AND a.Aktivno = 1
                      AND ml.Artikal NOT LIKE '%POLOVNE%'
                      AND ml.Artikal NOT LIKE '%POL.%'
                      AND ml.Artikal NOT LIKE '%Prijem%'
                      AND ml.Artikal NOT LIKE '%PRIJEM%'
                      AND ml.Artikal NOT LIKE '%Kutija Prijem%'
                    GROUP BY ml.Artikal, ml.ArtikalID
                    ORDER BY UkupnaKolicina DESC
                    LIMIT 10
                ";

                var rezultat = await _databaseService.QueryAsync<dynamic>(sql);

                return rezultat.ToDictionary(
                    x => (string)x.Artikal ?? "Nepoznato",
                    x => (decimal)x.UkupnaKolicina
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju strukture kutija: {ex.Message}");
                return new Dictionary<string, decimal>();
            }
        }

        public async Task<Dictionary<string, decimal>> UcitajStrukturuKesa()
        {
            try
            {
                var sql = @"
                    SELECT 
                        ml.Artikal,
                        SUM(ml.Kolicina) as UkupnaKolicina
                    FROM vwMagacinLager ml
                    LEFT JOIN Artikal a ON ml.ArtikalID = a.ID
                    WHERE a.MagacinID = 4  -- AMBALAZA
                      AND a.GrupnaAmbalaza = 0  -- KESE
                      AND ml.Kolicina >= 10
                      AND ml.Kolicina IS NOT NULL
                      AND a.Aktivno = 1
                      AND ml.Artikal NOT LIKE '%POLOVNE%'
                      AND ml.Artikal NOT LIKE '%POL.%'
                      AND ml.Artikal NOT LIKE '%Prijem%'
                      AND ml.Artikal NOT LIKE '%PRIJEM%'
                    GROUP BY ml.Artikal, ml.ArtikalID
                    ORDER BY UkupnaKolicina DESC
                    LIMIT 10
                ";

                var rezultat = await _databaseService.QueryAsync<dynamic>(sql);

                return rezultat.ToDictionary(
                    x => (string)x.Artikal ?? "Nepoznato",
                    x => (decimal)x.UkupnaKolicina
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju strukture kesa: {ex.Message}");
                return new Dictionary<string, decimal>();
            }
        }

        // ============================================
        // 🔧 ISPRAVKA: UcitajLagerStanje - sa pravim TipArtikla
        // ============================================
        public async Task<List<MagacinLagerModel>> UcitajLagerStanje()
        {
            try
            {
                var sql = $@"
                    SELECT 
                        ml.ArtikalID,
                        a.MagacinID as Tip,
                        {TipArtiklaCaseStatement} as TipArtikla,
                        ml.Artikal,
                        ml.Kolicina,
                        ml.Pakovanje,
                        ml.JM
                    FROM vwMagacinLager ml
                    LEFT JOIN Artikal a ON ml.ArtikalID = a.ID
                    WHERE (ml.Kolicina >= 10 OR LOWER(ml.JM) LIKE '%kg%' AND ml.Kolicina >= 10)
                      AND ml.Kolicina IS NOT NULL
                      AND ml.Artikal IS NOT NULL
                      AND a.Aktivno = 1
                      AND a.MagacinID != 7  -- ISKLJUČI KALO I RASTUR
                    ORDER BY ml.Artikal
                ";

                var rezultat = await _databaseService.QueryAsync<MagacinLagerModel>(sql);
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju lager stanja: {ex.Message}");
                return new List<MagacinLagerModel>();
            }
        }

        // ============================================
        // 🔧 ISPRAVKA: UcitajLagerStanjeSaFilterima - sa pravim TipArtikla
        // ============================================
        public async Task<List<MagacinLagerModel>> UcitajLagerStanjeSaFilterima(FilterRequest filterRequest)
        {
            try
            {
                var sql = new StringBuilder();
                sql.Append($@"
                    SELECT 
                        ml.ArtikalID,
                        a.Tip,
                        {TipArtiklaCaseStatement} as TipArtikla,
                        ml.Artikal,
                        ml.Kolicina,
                        ml.Pakovanje,
                        ml.JM
                    FROM vwMagacinLager ml
                    LEFT JOIN Artikal a ON ml.ArtikalID = a.ID
                    WHERE ml.Kolicina IS NOT NULL
                      AND ml.Artikal IS NOT NULL
                      AND (ml.Kolicina >= 10 OR LOWER(ml.JM) LIKE '%kg%' AND ml.Kolicina >= 10)
                      AND a.Aktivno = 1
                      AND a.MagacinID != 7  -- ISKLJUČI KALO I RASTUR
                ");

                var parameters = new Dictionary<string, object>();

                // Filtriranje po MagacinID
                if (!string.IsNullOrEmpty(filterRequest.Tip))
                {
                    if (int.TryParse(filterRequest.Tip, out int magacinId))
                    {
                        sql.Append(" AND a.MagacinID = @MagacinId");
                        parameters.Add("@MagacinId", magacinId);
                    }
                }

                // Filtriranje po pakovanju
                if (!string.IsNullOrEmpty(filterRequest.Pakovanje))
                {
                    sql.Append(" AND ml.Pakovanje = @Pakovanje");
                    parameters.Add("@Pakovanje", filterRequest.Pakovanje);
                }

                // Filtriranje po konkretnom artiklu
                if (filterRequest.ArtikalId.HasValue && filterRequest.ArtikalId > 0)
                {
                    sql.Append(" AND ml.ArtikalID = @ArtikalId");
                    parameters.Add("@ArtikalId", filterRequest.ArtikalId.Value);
                }

                // Filtriranje samo gotovih roba (MagacinID 6)
                if (filterRequest.SamoGotoveRobe == true)
                {
                    sql.Append(" AND a.MagacinID = 6");
                }

                // Filtriranje samo sirovina (MagacinID 3)
                if (filterRequest.SamoSirovine == true)
                {
                    sql.Append(" AND a.MagacinID = 3");
                }

                // Filtriranje samo ambalaze (MagacinID 4)
                if (filterRequest.SamoAmbalaže == true)
                {
                    sql.Append(" AND a.MagacinID = 4");
                }

                sql.Append(" ORDER BY ml.Artikal");

                var rezultat = await _databaseService.QueryAsync<MagacinLagerModel>(sql.ToString(), parameters);
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju lager stanja sa filterima: {ex.Message}");
                return new List<MagacinLagerModel>();
            }
        }

        // ============================================
        // Ostale metode sa pravim TipArtikla mapping
        // ============================================

        public async Task<List<string>> UcitajListuPakovanja()
        {
            try
            {
                var sql = @"
                    SELECT DISTINCT ml.Pakovanje
                    FROM vwMagacinLager ml
                    WHERE ml.Pakovanje IS NOT NULL 
                      AND ml.Pakovanje != ''
                      AND ml.Kolicina >= 10
                    ORDER BY ml.Pakovanje
                ";

                var rezultat = await _databaseService.QueryAsync<string>(sql);
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju liste pakovanja: {ex.Message}");
                return new List<string>();
            }
        }

        public async Task<List<RadniNalogLagerModel>> UcitajLagerProizvodnje()
        {
            try
            {
                var sql = @"
                    SELECT 
                        rnl.RadniNalogLager as BrojNaloga,
                        GROUP_CONCAT(DISTINCT rnl.Artikal SEPARATOR ', ') as Artikal,
                        SUM(rnl.Kolicina) as Kolicina,
                        SUM(rn.Kolicina) as PotrebnaKolicina,
                        GROUP_CONCAT(DISTINCT rnl.Pakovanje SEPARATOR ', ') as Pakovanje,
                        MAX(rn.DokumentStatus) as DokumentStatus
                    FROM vwRadniNalogLager rnl
                    LEFT JOIN RadniNalog rn ON rnl.RadniNalogLager = rn.Sifra
                    WHERE rnl.Kolicina IS NOT NULL
                      AND rnl.Kolicina > 0
                    GROUP BY rnl.RadniNalogLager
                    ORDER BY rnl.RadniNalogLager
                ";

                var rezultat = await _databaseService.QueryAsync<RadniNalogLagerModel>(sql);
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju lager proizvodnje: {ex.Message}");
                return new List<RadniNalogLagerModel>();
            }
        }

        public async Task<List<RadniNalogLagerModel>> UcitajRadneNalogeLager()
        {
            try
            {
                var sql = @"
                    SELECT 
                        rnl.RadniNalogLager as BrojNaloga,
                        'STALNI RN' as Komitent,
                        rnl.ArtikalID,
                        rnl.Artikal,
                        rnl.Kolicina,
                        rnl.Pakovanje,
                        rnl.BrojPakovanja,
                        rnl.Kolicina as PotrebnaKolicina,
                        2 as DokumentStatus
                    FROM vwRadniNalogLager rnl
                    WHERE rnl.Kolicina IS NOT NULL
                      AND rnl.Kolicina > 0
                    ORDER BY rnl.RadniNalogLager
                ";

                var rezultat = await _databaseService.QueryAsync<RadniNalogLagerModel>(sql);
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju radnih naloga lager: {ex.Message}");
                return new List<RadniNalogLagerModel>();
            }
        }

        public async Task<List<MagacinLagerModel>> UcitajLagerStanjePoArtiklu(long artikalId)
        {
            try
            {
                var sql = $@"
                    SELECT 
                        ml.ArtikalID,
                        a.MagacinID as Tip,
                        {TipArtiklaCaseStatement} as TipArtikla,
                        ml.Artikal,
                        ml.Kolicina,
                        ml.Pakovanje,
                        ml.JM
                    FROM vwMagacinLager ml
                    LEFT JOIN Artikal a ON ml.ArtikalID = a.ID
                    WHERE ml.ArtikalID = @ArtikalId
                      AND (ml.Kolicina >= 10 OR LOWER(ml.JM) LIKE '%kg%' AND ml.Kolicina >= 10)
                      AND ml.Kolicina IS NOT NULL
                      AND a.Aktivno = 1
                      AND a.MagacinID != 7  -- ISKLJUČI KALO I RASTUR
                    ORDER BY ml.Artikal
                ";

                var rezultat = await _databaseService.QueryAsync<MagacinLagerModel>(sql, new { ArtikalId = artikalId });
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju lager stanja po artiklu: {ex.Message}");
                return new List<MagacinLagerModel>();
            }
        }

        public async Task<List<MagacinLagerModel>> UcitajLagerStanjePoArtiklu(int artikalId)
        {
            return await UcitajLagerStanjePoArtiklu((long)artikalId);
        }

        public async Task<List<MagacinLagerModel>> UcitajLagerStanjePoTipu(int magacinId)
        {
            try
            {
                var sql = $@"
                    SELECT 
                        ml.ArtikalID,
                        a.MagacinID as Tip,
                        {TipArtiklaCaseStatement} as TipArtikla,
                        ml.Artikal,
                        ml.Kolicina,
                        ml.Pakovanje,
                        ml.JM
                    FROM vwMagacinLager ml
                    LEFT JOIN Artikal a ON ml.ArtikalID = a.ID
                    WHERE a.MagacinID = @MagacinId
                      AND (ml.Kolicina >= 10 OR LOWER(ml.JM) LIKE '%kg%' AND ml.Kolicina >= 10)
                      AND ml.Kolicina IS NOT NULL
                      AND a.Aktivno = 1
                      AND a.MagacinID != 7  -- ISKLJUČI KALO I RASTUR
                    ORDER BY ml.Artikal
                ";

                var rezultat = await _databaseService.QueryAsync<MagacinLagerModel>(sql, new { MagacinId = magacinId });
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju lager stanja po MagacinID: {ex.Message}");
                return new List<MagacinLagerModel>();
            }
        }

        public async Task<List<MagacinLagerModel>> UcitajGotoveRobe()
        {
            return await UcitajLagerStanjePoTipu(6); // 6 = Gotovi Proizvodi
        }

        public async Task<List<MagacinLagerModel>> UcitajSirovine()
        {
            return await UcitajLagerStanjePoTipu(3); // 3 = Sirovine
        }

        public async Task<List<MagacinLagerModel>> UcitajAmbalaze()
        {
            return await UcitajLagerStanjePoTipu(4); // 4 = Ambalaza
        }

        public async Task<decimal> UcitajUkupnuVrednostLager()
        {
            try
            {
                var sql = @"
                    SELECT COALESCE(SUM(ml.Kolicina), 0) as UkupnaKolicina
                    FROM vwMagacinLager ml
                    WHERE ml.Kolicina >= 10 
                      AND ml.Kolicina IS NOT NULL
                ";

                var rezultat = await _databaseService.ExecuteScalarAsync<decimal>(sql);
                return rezultat;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju ukupne vrednosti lager: {ex.Message}");
                return 0;
            }
        }

        public async Task<List<MagacinLagerModel>> UcitajLagerStanjeSaFilterima(string filter)
        {
            try
            {
                var sql = $@"
                    SELECT 
                        ml.ArtikalID,
                        a.MagacinID as Tip,
                        {TipArtiklaCaseStatement} as TipArtikla,
                        ml.Artikal,
                        ml.Kolicina,
                        ml.Pakovanje,
                        ml.JM
                    FROM vwMagacinLager ml
                    LEFT JOIN Artikal a ON ml.ArtikalID = a.ID
                    WHERE (ml.Kolicina >= 10 OR LOWER(ml.JM) LIKE '%kg%' AND ml.Kolicina >= 10)
                      AND ml.Kolicina IS NOT NULL
                      AND ml.Artikal IS NOT NULL
                      AND a.MagacinID != 7  -- ISKLJUČI KALO I RASTUR
                ";

                if (!string.IsNullOrEmpty(filter))
                {
                    sql += " AND (ml.Artikal LIKE @Filter OR ml.Pakovanje LIKE @Filter)";
                }

                sql += " ORDER BY ml.Artikal";

                var rezultat = await _databaseService.QueryAsync<MagacinLagerModel>(sql, new { Filter = $"%{filter}%" });
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju lager stanja sa filterom: {ex.Message}");
                return new List<MagacinLagerModel>();
            }
        }

        public async Task<List<MagacinLagerModel>> UcitajLagerStanjePoLotu(string lot)
        {
            try
            {
                var sql = $@"
                    SELECT 
                        ml.ArtikalID,
                        a.MagacinID as Tip,
                        {TipArtiklaCaseStatement} as TipArtikla,
                        ml.Artikal,
                        ml.Kolicina,
                        ml.Pakovanje,
                        ml.JM
                    FROM vwMagacinLager ml
                    LEFT JOIN Artikal a ON ml.ArtikalID = a.ID
                    WHERE ml.Lot LIKE @Lot
                      AND (ml.Kolicina >= 10 OR LOWER(ml.JM) LIKE '%kg%' AND ml.Kolicina >= 10)
                      AND ml.Kolicina IS NOT NULL
                      AND a.Aktivno = 1
                      AND a.MagacinID != 7  -- ISKLJUČI KALO I RASTUR
                    ORDER BY ml.Artikal
                ";

                var rezultat = await _databaseService.QueryAsync<MagacinLagerModel>(sql, new { Lot = $"%{lot}%" });
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju lager stanja po lotu: {ex.Message}");
                return new List<MagacinLagerModel>();
            }
        }

        public async Task<List<RadniNalogLagerModel>> UcitajOtvoreneRadneNaloge()
        {
            try
            {
                var sql = @"
                    SELECT 
                        rnl.RadniNalogLager as BrojNaloga,
                        'STALNI RN' as Komitent,
                        rnl.ArtikalID,
                        rnl.Artikal,
                        rnl.Kolicina,
                        rnl.Pakovanje,
                        rnl.BrojPakovanja,
                        rnl.Kolicina as PotrebnaKolicina,
                        2 as DokumentStatus
                    FROM vwRadniNalogLager rnl
                    WHERE rnl.Kolicina > 0
                      AND rnl.Kolicina IS NOT NULL
                    ORDER BY rnl.RadniNalogLager
                ";

                var rezultat = await _databaseService.QueryAsync<RadniNalogLagerModel>(sql);
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju otvorenih radnih naloga: {ex.Message}");
                return new List<RadniNalogLagerModel>();
            }
        }

        public async Task<List<RadniNalogLagerModel>> UcitajRadneNalogePoStatusu(int status)
        {
            try
            {
                var sql = @"
                    SELECT 
                        rnl.RadniNalogLager as BrojNaloga,
                        'STALNI RN' as Komitent,
                        rnl.ArtikalID,
                        rnl.Artikal,
                        rnl.Kolicina,
                        rnl.Pakovanje,
                        rnl.BrojPakovanja,
                        rnl.Kolicina as PotrebnaKolicina,
                        @Status as DokumentStatus
                    FROM vwRadniNalogLager rnl
                    WHERE rnl.Kolicina IS NOT NULL
                    ORDER BY rnl.RadniNalogLager
                ";

                var rezultat = await _databaseService.QueryAsync<RadniNalogLagerModel>(sql, new { Status = status });
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju radnih naloga po statusu: {ex.Message}");
                return new List<RadniNalogLagerModel>();
            }
        }

        public async Task<List<MagacinLagerModel>> UcitajArtikleIspodMinimuma(decimal minKolicina = 10)
        {
            try
            {
                var sql = $@"
                    SELECT 
                        ml.ArtikalID,
                        a.MagacinID as Tip,
                        {TipArtiklaCaseStatement} as TipArtikla,
                        ml.Artikal,
                        ml.Kolicina,
                        ml.Pakovanje,
                        ml.JM
                    FROM vwMagacinLager ml
                    LEFT JOIN Artikal a ON ml.ArtikalID = a.ID
                    WHERE ml.Kolicina < @MinKolicina
                      AND ml.Kolicina IS NOT NULL
                      AND a.Aktivno = 1
                      AND a.MagacinID != 7  -- ISKLJUČI KALO I RASTUR
                    ORDER BY ml.Kolicina ASC, ml.Artikal
                ";

                var rezultat = await _databaseService.QueryAsync<MagacinLagerModel>(sql, new { MinKolicina = minKolicina });
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju artikala ispod minimuma: {ex.Message}");
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
                var sql = @"
                    SELECT 
                        a.MagacinID as ArtikalTip,
                        COUNT(*) as BrojArtikala,
                        SUM(ml.Kolicina) as UkupnaKolicina,
                        AVG(ml.Kolicina) as ProsecnaKolicina
                    FROM vwMagacinLager ml
                    LEFT JOIN Artikal a ON ml.ArtikalID = a.ID
                    WHERE ml.Kolicina IS NOT NULL
                      AND ml.Kolicina >= 10
                      AND a.Aktivno = 1
                      AND a.MagacinID != 7  -- ISKLJUČI KALO I RASTUR
                    GROUP BY a.MagacinID
                    ORDER BY a.MagacinID
                ";

                var rezultat = await _databaseService.QueryAsync<dynamic>(sql);

                return rezultat.ToDictionary(
                    x => $"MagacinID {x.ArtikalTip}",
                    x => (decimal)x.UkupnaKolicina
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju statistika lagera: {ex.Message}");
                return new Dictionary<string, decimal>();
            }
        }

        // DASHBOARD - nove metode za strukturu lagera
        public async Task<Dictionary<string, decimal>> UcitajStrukturuSirovina()
        {
            try
            {
                var sql = @"
                    SELECT 
                        CASE 
                            WHEN ml.Artikal LIKE '%+' THEN LEFT(ml.Artikal, LENGTH(ml.Artikal) - 1)
                            WHEN ml.Artikal LIKE '%-' THEN LEFT(ml.Artikal, LENGTH(ml.Artikal) - 1)
                            ELSE ml.Artikal
                        END as BaseArtikal,
                        SUM(ml.Kolicina) as UkupnaKolicina
                    FROM vwMagacinLager ml
                    LEFT JOIN Artikal a ON ml.ArtikalID = a.ID
                    WHERE a.MagacinID IN (2, 3)  -- SVEZA ROBA I SIROVINE
                      AND ml.Kolicina >= 10
                      AND ml.Kolicina IS NOT NULL
                      AND a.Aktivno = 1
                      AND a.MagacinID != 7  -- ISKLJUČI KALO I RASTUR
                      AND ml.Artikal NOT LIKE '%D/Z Sljiva stenlej%'
                      AND ml.Artikal NOT LIKE '%klasa%'
                      AND ml.Artikal NOT LIKE '%KLASA%'
                      AND ml.Artikal NOT LIKE '%Klasa%'
                    GROUP BY BaseArtikal
                    ORDER BY UkupnaKolicina DESC
                    LIMIT 10
                ";

                var rezultat = await _databaseService.QueryAsync<dynamic>(sql);

                return rezultat.ToDictionary(
                    x => (string)x.BaseArtikal ?? "Nepoznato",
                    x => (decimal)x.UkupnaKolicina
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju strukture sirovina: {ex.Message}");
                return new Dictionary<string, decimal>();
            }
        }

        public async Task<Dictionary<string, decimal>> UcitajStrukturuGotovihProizvoda()
        {
            try
            {
                var sql = @"
                    SELECT 
                        CASE 
                            WHEN ml.Artikal LIKE '%+' THEN LEFT(ml.Artikal, LENGTH(ml.Artikal) - 1)
                            WHEN ml.Artikal LIKE '%-' THEN LEFT(ml.Artikal, LENGTH(ml.Artikal) - 1)
                            ELSE ml.Artikal
                        END as BaseArtikal,
                        SUM(ml.Kolicina) as UkupnaKolicina
                    FROM vwMagacinLager ml
                    LEFT JOIN Artikal a ON ml.ArtikalID = a.ID
                    WHERE a.MagacinID = 6  -- GOTOVI PROIZVODI
                      AND ml.Kolicina >= 10
                      AND ml.Kolicina IS NOT NULL
                      AND a.Aktivno = 1
                      AND a.MagacinID != 7  -- ISKLJUČI KALO I RASTUR
                      AND ml.Artikal NOT LIKE '%D/Z Šljiva%'
                      AND ml.Artikal NOT LIKE '%D/Z Sljiva%'
                    GROUP BY BaseArtikal
                    ORDER BY UkupnaKolicina DESC
                    LIMIT 10
                ";

                var rezultat = await _databaseService.QueryAsync<dynamic>(sql);

                return rezultat.ToDictionary(
                    x => (string)x.BaseArtikal ?? "Nepoznato",
                    x => (decimal)x.UkupnaKolicina
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju strukture gotovih proizvoda: {ex.Message}");
                return new Dictionary<string, decimal>();
            }
        }

        // ============================================
        // 🔧 METODE ZA ROBA STRANICU (SIROVINE I POLUPROIZVODI)
        // ============================================
        public async Task<List<MagacinLagerModel>> UcitajSirovine(FilterRequest filterRequest)
        {
            try
            {
                var sql = $@"
                    SELECT 
                        ml.ArtikalID,
                        a.MagacinID as Tip,
                        {TipArtiklaCaseStatement} as TipArtikla,
                        ml.Artikal,
                        COALESCE(SUM(ml.Kolicina), 0) as Kolicina,
                        COALESCE(SUM(rn_kolicina.Kolicina), 0) as KolicinaRadniNalog,
                        COALESCE(SUM(otvoreni_rn.Kolicina), 0) as ZaNajavljeneUtovare,
                        (COALESCE(SUM(ml.Kolicina), 0) - COALESCE(SUM(otvoreni_rn.Kolicina), 0)) as Dostupno
                    FROM vwMagacinLager ml
                    LEFT JOIN Artikal a ON ml.ArtikalID = a.ID
                    LEFT JOIN (
                        SELECT 
                            a2.ID as ArtikalID,
                            SUM(rnl.Kolicina) as Kolicina
                        FROM vwRadniNalogLager rnl
                        INNER JOIN Artikal a2 ON rnl.ArtikalID = a2.ID
                        WHERE a2.MagacinID IN (2, 3)  -- SVEZA ROBA I SIROVINE
                        GROUP BY a2.ID
                    ) rn_kolicina ON ml.ArtikalID = rn_kolicina.ArtikalID
                    LEFT JOIN (
                        SELECT 
                            ai.ArtikalID,
                            SUM(rn.Kolicina) as Kolicina
                        FROM RadniNalog rn
                        INNER JOIN ArtikalInstanca ai ON rn.ArtikalInstancaID = ai.ID
                        INNER JOIN Artikal a3 ON ai.ArtikalID = a3.ID
                        WHERE rn.DokumentStatus = 2  -- OTVORENI
                          AND a3.MagacinID IN (2, 3)  -- SVEZA ROBA I SIROVINE
                        GROUP BY ai.ArtikalID
                    ) otvoreni_rn ON ml.ArtikalID = otvoreni_rn.ArtikalID
                    WHERE a.MagacinID IN (2, 3)  -- SVEZA ROBA I SIROVINE
                      AND (ml.Kolicina >= 10 OR rn_kolicina.Kolicina > 0 OR otvoreni_rn.Kolicina > 0)
                      AND a.Aktivno = 1
                    GROUP BY ml.ArtikalID, a.MagacinID, ml.Artikal
                    ORDER BY ml.Artikal
                ";

                var rezultat = await _databaseService.QueryAsync<dynamic>(sql);

                return rezultat.Select(x => new MagacinLagerModel
                {
                    ArtikalID = (long)x.ArtikalID,
                    Tip = x.Tip != null ? (int?)Convert.ToInt32(x.Tip) : null,
                    TipArtikla = x.TipArtikla,
                    Artikal = (string)x.Artikal,
                    Kolicina = (decimal)x.Kolicina,
                    Pakovanje = ((decimal)x.KolicinaRadniNalog).ToString("N0"),
                    ZaNajavljeneUtovare = ((decimal)x.ZaNajavljeneUtovare).ToString("N0"),
                    Lot = ((decimal)x.Dostupno).ToString("N0")
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju sirovina: {ex.Message}");
                return new List<MagacinLagerModel>();
            }
        }

        public async Task<List<MagacinLagerModel>> UcitajPoluproizvode(FilterRequest filterRequest)
        {
            try
            {
                var sql = $@"
                    SELECT 
                        ml.ArtikalID,
                        a.MagacinID as Tip,
                        {TipArtiklaCaseStatement} as TipArtikla,
                        ml.Artikal,
                        COALESCE(SUM(ml.Kolicina), 0) as Kolicina,
                        COALESCE(SUM(rn_kolicina.Kolicina), 0) as KolicinaRadniNalog,
                        COALESCE(SUM(otvoreni_rn.Kolicina), 0) as ZaNajavljeneUtovare,
                        (COALESCE(SUM(ml.Kolicina), 0) - COALESCE(SUM(otvoreni_rn.Kolicina), 0)) as Dostupno
                    FROM vwMagacinLager ml
                    LEFT JOIN Artikal a ON ml.ArtikalID = a.ID
                    LEFT JOIN (
                        SELECT 
                            a2.ID as ArtikalID,
                            SUM(rnl.Kolicina) as Kolicina
                        FROM vwRadniNalogLager rnl
                        INNER JOIN Artikal a2 ON rnl.ArtikalID = a2.ID
                        WHERE a2.MagacinID = 5  -- POLUPROIZVODI
                        GROUP BY a2.ID
                    ) rn_kolicina ON ml.ArtikalID = rn_kolicina.ArtikalID
                    LEFT JOIN (
                        SELECT 
                            ai.ArtikalID,
                            SUM(rn.Kolicina) as Kolicina
                        FROM RadniNalog rn
                        INNER JOIN ArtikalInstanca ai ON rn.ArtikalInstancaID = ai.ID
                        INNER JOIN Artikal a3 ON ai.ArtikalID = a3.ID
                        WHERE rn.DokumentStatus = 2  -- OTVORENI
                          AND a3.MagacinID = 5  -- POLUPROIZVODI
                        GROUP BY ai.ArtikalID
                    ) otvoreni_rn ON ml.ArtikalID = otvoreni_rn.ArtikalID
                    WHERE a.MagacinID = 5  -- POLUPROIZVODI
                      AND (ml.Kolicina >= 10 OR rn_kolicina.Kolicina > 0 OR otvoreni_rn.Kolicina > 0)
                      AND a.Aktivno = 1
                    GROUP BY ml.ArtikalID, a.MagacinID, ml.Artikal
                    ORDER BY ml.Artikal
                ";

                var rezultat = await _databaseService.QueryAsync<dynamic>(sql);

                return rezultat.Select(x => new MagacinLagerModel
                {
                    ArtikalID = (long)x.ArtikalID,
                    Tip = x.Tip != null ? (int?)Convert.ToInt32(x.Tip) : null,
                    TipArtikla = x.TipArtikla,
                    Artikal = (string)x.Artikal,
                    Kolicina = (decimal)x.Kolicina,
                    Pakovanje = ((decimal)x.KolicinaRadniNalog).ToString("N0"),
                    ZaNajavljeneUtovare = ((decimal)x.ZaNajavljeneUtovare).ToString("N0"),
                    Lot = ((decimal)x.Dostupno).ToString("N0")
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju poluproizvoda: {ex.Message}");
                return new List<MagacinLagerModel>();
            }
        }

        // ============================================
        // 🔧 AMBALAŽNE METODE ZA STRANICU
        // ============================================
        public async Task<List<MagacinLagerModel>> UcitajKutije(FilterRequest filterRequest)
        {
            try
            {
                var sql = $@"
                    SELECT 
                        ml.ArtikalID,
                        a.MagacinID as Tip,
                        {TipArtiklaCaseStatement} as TipArtikla,
                        ml.Artikal,
                        COALESCE(SUM(ml.Kolicina), 0) as Kolicina,
                        COALESCE(SUM(rn_kolicina.BrojPakovanja), 0) as KolicinaRadniNalog,
                        COALESCE(SUM(otvoreni_rn.BrojPakovanja), 0) as ZaNajavljeneUtovare,
                        (COALESCE(SUM(ml.Kolicina), 0) - COALESCE(SUM(otvoreni_rn.BrojPakovanja), 0)) as Dostupno
                    FROM vwMagacinLager ml
                    LEFT JOIN Artikal a ON ml.ArtikalID = a.ID
                    LEFT JOIN (
                        SELECT 
                            a2.Naziv as Artikal,
                            SUM(rnl.BrojPakovanja) as BrojPakovanja
                        FROM vwRadniNalogLager rnl
                        INNER JOIN Pakovanje p ON rnl.PakovanjeID = p.ID
                        INNER JOIN Artikal a2 ON p.GpAmbalazaID = a2.ID
                        WHERE a2.GrupnaAmbalaza = 1  -- KUTIJE
                          AND rnl.PakovanjeTip IN (2, 3)
                        GROUP BY a2.Naziv
                    ) rn_kolicina ON ml.Artikal = rn_kolicina.Artikal
                    LEFT JOIN (
                        SELECT 
                            a3.Naziv as Artikal,
                            SUM(rn.BrojPakovanja) as BrojPakovanja
                        FROM RadniNalog rn
                        INNER JOIN ArtikalInstanca ai ON rn.ArtikalInstancaID = ai.ID
                        INNER JOIN Pakovanje p2 ON ai.PakovanjeID = p2.ID
                        INNER JOIN Artikal a3 ON p2.GpAmbalazaID = a3.ID
                        WHERE rn.DokumentStatus = 2  -- OTVORENI
                          AND a3.GrupnaAmbalaza = 1  -- KUTIJE
                        GROUP BY a3.Naziv
                    ) otvoreni_rn ON ml.Artikal = otvoreni_rn.Artikal
                    WHERE a.MagacinID = 4  -- AMBALAZA
                      AND a.GrupnaAmbalaza = 1  -- KUTIJE/DŽAKOVI
                      AND (ml.Kolicina >= 10 OR rn_kolicina.BrojPakovanja > 0 OR otvoreni_rn.BrojPakovanja > 0)
                      AND a.Aktivno = 1
                      AND ml.Artikal NOT LIKE '%POLOVNE%'
                      AND ml.Artikal NOT LIKE '%POL.%'
                      AND ml.Artikal NOT LIKE '%Prijem%'
                      AND ml.Artikal NOT LIKE '%PRIJEM%'
                      AND ml.Artikal NOT LIKE '%Kutija Prijem%'
                      AND ml.AmbalazaTip = 3
                    GROUP BY ml.ArtikalID, a.MagacinID, ml.Artikal
                    ORDER BY ml.Artikal
                ";

                var rezultat = await _databaseService.QueryAsync<dynamic>(sql);

                return rezultat.Select(x => new MagacinLagerModel
                {
                    ArtikalID = (long)x.ArtikalID,
                    Tip = x.Tip != null ? (int?)Convert.ToInt32(x.Tip) : null,
                    TipArtikla = x.TipArtikla,
                    Artikal = (string)x.Artikal,
                    Kolicina = (decimal)x.Kolicina,
                    Pakovanje = ((decimal)x.KolicinaRadniNalog).ToString("N0"),
                    ZaNajavljeneUtovare = ((decimal)x.ZaNajavljeneUtovare).ToString("N0"),
                    Lot = ((decimal)x.Dostupno).ToString("N0")
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju kutija: {ex.Message}");
                return new List<MagacinLagerModel>();
            }
        }

        public async Task<List<MagacinLagerModel>> UcitajKese(FilterRequest filterRequest)
        {
            try
            {
                var sql = $@"
                    SELECT 
                        ml.ArtikalID,
                        a.MagacinID as Tip,
                        {TipArtiklaCaseStatement} as TipArtikla,
                        ml.Artikal,
                        COALESCE(SUM(ml.Kolicina), 0) as Kolicina,
                        COALESCE(SUM(rn_kolicina.BrojPakovanja), 0) as KolicinaRadniNalog,
                        COALESCE(SUM(otvoreni_rn.BrojPakovanja), 0) as ZaNajavljeneUtovare,
                        (COALESCE(SUM(ml.Kolicina), 0) - COALESCE(SUM(otvoreni_rn.BrojPakovanja), 0)) as Dostupno
                    FROM vwMagacinLager ml
                    LEFT JOIN Artikal a ON ml.ArtikalID = a.ID
                    LEFT JOIN (
                        SELECT 
                            a2.Naziv as Artikal,
                            SUM(
                                CASE 
                                    WHEN rnl.PakovanjeTip = 2 THEN rnl.BrojPakovanja * p.BrojJPuGP
                                    ELSE rnl.BrojPakovanja
                                END
                            ) as BrojPakovanja
                        FROM vwRadniNalogLager rnl
                        INNER JOIN Pakovanje p ON rnl.PakovanjeID = p.ID
                        INNER JOIN Artikal a2 ON p.JpAmbalazaID = a2.ID
                        WHERE a2.GrupnaAmbalaza = 0  -- KESE
                          AND rnl.PakovanjeTip IN (1, 2, 3)
                        GROUP BY a2.Naziv
                    ) rn_kolicina ON ml.Artikal = rn_kolicina.Artikal
                    LEFT JOIN (
                        SELECT 
                            a3.Naziv as Artikal,
                            SUM(
                                CASE 
                                    WHEN p2.BrojJPuGP > 0 THEN rn.BrojPakovanja * p2.BrojJPuGP
                                    ELSE rn.BrojPakovanja
                                END
                            ) as BrojPakovanja
                        FROM RadniNalog rn
                        INNER JOIN ArtikalInstanca ai ON rn.ArtikalInstancaID = ai.ID
                        INNER JOIN Pakovanje p2 ON ai.PakovanjeID = p2.ID
                        INNER JOIN Artikal a3 ON p2.JpAmbalazaID = a3.ID
                        WHERE rn.DokumentStatus = 2  -- OTVORENI
                          AND a3.GrupnaAmbalaza = 0  -- KESE
                        GROUP BY a3.Naziv
                    ) otvoreni_rn ON ml.Artikal = otvoreni_rn.Artikal
                    WHERE a.MagacinID = 4  -- AMBALAZA
                      AND a.GrupnaAmbalaza = 0  -- KESE
                      AND (ml.Kolicina >= 10 OR rn_kolicina.BrojPakovanja > 0 OR otvoreni_rn.BrojPakovanja > 0)
                      AND a.Aktivno = 1
                      AND ml.Artikal NOT LIKE '%POLOVNE%'
                      AND ml.Artikal NOT LIKE '%POL.%'
                      AND ml.Artikal NOT LIKE '%Prijem%'
                      AND ml.Artikal NOT LIKE '%PRIJEM%'
                      AND ml.Artikal NOT LIKE '%DŽAK%'
                      AND ml.AmbalazaTip = 2
                    GROUP BY ml.ArtikalID, a.MagacinID, ml.Artikal
                    ORDER BY ml.Artikal
                ";

                var rezultat = await _databaseService.QueryAsync<dynamic>(sql);

                return rezultat.Select(x => new MagacinLagerModel
                {
                    ArtikalID = (long)x.ArtikalID,
                    Tip = x.Tip != null ? (int?)Convert.ToInt32(x.Tip) : null,
                    TipArtikla = x.TipArtikla,
                    Artikal = (string)x.Artikal,
                    Kolicina = (decimal)x.Kolicina,
                    Pakovanje = ((decimal)x.KolicinaRadniNalog).ToString("N0"),
                    ZaNajavljeneUtovare = ((decimal)x.ZaNajavljeneUtovare).ToString("N0"),
                    Lot = ((decimal)x.Dostupno).ToString("N0")
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju kesa: {ex.Message}");
                return new List<MagacinLagerModel>();
            }
        }
    }
}
