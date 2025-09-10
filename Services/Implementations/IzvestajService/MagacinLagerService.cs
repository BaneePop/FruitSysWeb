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
                    x => (string)x.BaseArtikal ?? "Nepoznato",
                    x => (decimal)x.UkupnaKolicina
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju strukture kesa: {ex.Message}");
                return new Dictionary<string, decimal>();
            }
        }

        // ISPRAVKA: Automatski ne učitava ništa sa manje od 10kg ili 10kom
        public async Task<List<MagacinLagerModel>> UcitajLagerStanje()
        {
            try
            {
                var sql = @"
                    SELECT 
                        ml.ArtikalID,
                        a.MagacinID as ArtikalTip,
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

        // ISPRAVKA: Filtriranje prema dokumentu - uklonjen rok važenja i min/max količina
        public async Task<List<MagacinLagerModel>> UcitajLagerStanjeSaFilterima(FilterRequest filterRequest)
        {
            try
            {
                var sql = new StringBuilder();
                sql.Append(@"
                    SELECT 
                        ml.ArtikalID,
                        COALESCE(a.MagacinID, ml.ArtikalTip) as Tip,
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

                // POUZE POTREBNI FILTERI prema dokumentu

                // Filtriranje po MagacinID
                if (!string.IsNullOrEmpty(filterRequest.Tip))
                {
                    if (int.TryParse(filterRequest.Tip, out int magacinId))
                    {
                        sql.Append(" AND a.MagacinID = @MagacinId");
                        parameters.Add("@MagacinId", magacinId);
                    }
                }

                // Filtriranje po pakovanju - PODRŽAN PADAJUĆI MENI
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

                // UKLONJENI: Rok važenja filteri prema dokumentu
                // UKLONJENI: Min/Max količina filteri prema dokumentu

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

        // DODATO: Metoda za dobijanje liste pakovanja za padajući meni
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

        // DODATO: LAGER PROIZVODNJE iz vwRadniNalogLager - prema dokumentu 
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

        // POSTOJEĆE METODE - MODIFIKOVANE

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
                var sql = @"
                    SELECT 
                        ml.ArtikalID,
                        a.MagacinID as ArtikalTip,
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

        // Overload za int
        public async Task<List<MagacinLagerModel>> UcitajLagerStanjePoArtiklu(int artikalId)
        {
            return await UcitajLagerStanjePoArtiklu((long)artikalId);
        }

        public async Task<List<MagacinLagerModel>> UcitajLagerStanjePoTipu(int magacinId)
        {
            try
            {
                var sql = @"
                    SELECT 
                        ml.ArtikalID,
                        a.MagacinID as ArtikalTip,
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

        // DODANO: Specifične metode za MagacinID (prema dokumentu)
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

        // OSTALE METODE bez izmena...
        public async Task<List<MagacinLagerModel>> UcitajLagerStanjeSaFilterima(string filter)
        {
            try
            {
                var sql = @"
                    SELECT 
                        ml.ArtikalID,
                        a.MagacinID as ArtikalTip,
                        ml.Artikal,
                        ml.Kolicina,
                        ml.Pakovanje,
                        ml.JM
                    FROM vwMagacinLager ml
                    LEFT JOIN Artikal a ON ml.ArtikalID = a.ID
                    WHERE (ml.Kolicina >= 10 OR LOWER(ml.JM) LIKE '%kg%' AND ml.Kolicina >= 10)
                      AND ml.Kolicina IS NOT NULL
                      AND ml.Artikal IS NOT NULL
                      AND a.MagacinID != 7  -- ISKLJUČI KALO I RASTUR";

                if (!string.IsNullOrEmpty(filter))
                {
                    sql += @" AND (ml.Artikal LIKE @Filter OR ml.Pakovanje LIKE @Filter)";
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
                var sql = @"
                    SELECT 
                        ml.ArtikalID,
                        a.MagacinID as ArtikalTip,
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

        // UKLONJENE metode za rok važenja - prema dokumentu se ne koriste

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
                var sql = @"
                    SELECT 
                        ml.ArtikalID,
                        a.MagacinID as ArtikalTip,
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

        

        /* public async Task<Dictionary<string, decimal>> UcitajStrukturuKesa()
        {
            try
            {
                var sql = @"
                    SELECT 
                        ml.Artikal,
                        SUM(ml.Kolicina) as UkupnaKolicina
                    FROM vwMagacinLager ml
                    LEFT JOIN Artikal a ON ml.ArtikalID = a.ID
                    WHERE a.Tip = 2  -- AMBALAZA
                      AND a.GrupnaAmbalaza = 0  -- KESE
                      AND ml.Kolicina >= 10
                      AND ml.Kolicina IS NOT NULL
                      AND a.Aktivno = 1
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
                Console.WriteLine($"Greska pri ucitavanju strukture kesa: {ex.Message}");
                return new Dictionary<string, decimal>();
            }
        } */
    }
}