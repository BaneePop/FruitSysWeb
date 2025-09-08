using FruitSysWeb.Models;
using FruitSysWeb.Services.Models.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        // ISPRAVKA: Automatski ne učitava ništa sa manje od 10kg ili 10kom
        public async Task<List<MagacinLagerModel>> UcitajLagerStanje()
        {
            try
            {
                var sql = @"
                    SELECT 
                        ml.ArtikalID,
                        a.Tip as ArtikalTip,
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
                        COALESCE(a.Tip, ml.ArtikalTip) as Tip,
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
                ");

                var parameters = new Dictionary<string, object>();

                // POUZE POTREBNI FILTERI prema dokumentu

                // Filtriranje po tipu artikla
                if (!string.IsNullOrEmpty(filterRequest.Tip))
                {
                    if (int.TryParse(filterRequest.Tip, out int tipInt))
                    {
                        sql.Append(" AND a.Tip = @Tip");
                        parameters.Add("@Tip", tipInt);
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

                // Filtriranje samo gotovih roba (tip 4)
                if (filterRequest.SamoGotoveRobe == true)
                {
                    sql.Append(" AND a.Tip = 4");
                }

                // Filtriranje samo sirovina (tip 1)
                if (filterRequest.SamoSirovine == true)
                {
                    sql.Append(" AND a.Tip = 1");
                }

                // Filtriranje samo ambalaze (tip 2)
                if (filterRequest.SamoAmbalaže == true)
                {
                    sql.Append(" AND a.Tip = 2");
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
                        a.Tip as ArtikalTip,
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

        public async Task<List<MagacinLagerModel>> UcitajLagerStanjePoTipu(int artikalTip)
        {
            try
            {
                var sql = @"
                    SELECT 
                        ml.ArtikalID,
                        a.Tip as ArtikalTip,
                        ml.Artikal,
                        ml.Kolicina,
                        ml.Pakovanje,
                        ml.JM
                    FROM vwMagacinLager ml
                    LEFT JOIN Artikal a ON ml.ArtikalID = a.ID
                    WHERE a.Tip = @ArtikalTip
                      AND (ml.Kolicina >= 10 OR LOWER(ml.JM) LIKE '%kg%' AND ml.Kolicina >= 10)
                      AND ml.Kolicina IS NOT NULL
                      AND a.Aktivno = 1
                    ORDER BY ml.Artikal
                ";
                
                var rezultat = await _databaseService.QueryAsync<MagacinLagerModel>(sql, new { ArtikalTip = artikalTip });
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju lager stanja po tipu: {ex.Message}");
                return new List<MagacinLagerModel>();
            }
        }

        // DODANO: Specifične metode za tipove (prema dokumentu)
        public async Task<List<MagacinLagerModel>> UcitajGotoveRobe()
        {
            return await UcitajLagerStanjePoTipu(4); // 4 = Gotova roba
        }

        public async Task<List<MagacinLagerModel>> UcitajSirovine()
        {
            return await UcitajLagerStanjePoTipu(1); // 1 = Sirovina
        }

        public async Task<List<MagacinLagerModel>> UcitajAmbalaze()
        {
            return await UcitajLagerStanjePoTipu(2); // 2 = Ambalaza
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
                        a.Tip as ArtikalTip,
                        ml.Artikal,
                        ml.Kolicina,
                        ml.Pakovanje,
                        ml.JM
                    FROM vwMagacinLager ml
                    LEFT JOIN Artikal a ON ml.ArtikalID = a.ID
                    WHERE (ml.Kolicina >= 10 OR LOWER(ml.JM) LIKE '%kg%' AND ml.Kolicina >= 10)
                      AND ml.Kolicina IS NOT NULL
                      AND ml.Artikal IS NOT NULL";

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
                        a.Tip as ArtikalTip,
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
                        a.Tip as ArtikalTip,
                        ml.Artikal,
                        ml.Kolicina,
                        ml.Pakovanje,
                        ml.JM
                    FROM vwMagacinLager ml
                    LEFT JOIN Artikal a ON ml.ArtikalID = a.ID
                    WHERE ml.Kolicina < @MinKolicina
                      AND ml.Kolicina IS NOT NULL
                      AND a.Aktivno = 1
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
                        a.Tip as ArtikalTip,
                        COUNT(*) as BrojArtikala,
                        SUM(ml.Kolicina) as UkupnaKolicina,
                        AVG(ml.Kolicina) as ProsecnaKolicina
                    FROM vwMagacinLager ml
                    LEFT JOIN Artikal a ON ml.ArtikalID = a.ID
                    WHERE ml.Kolicina IS NOT NULL
                      AND ml.Kolicina >= 10
                      AND a.Aktivno = 1
                    GROUP BY a.Tip
                    ORDER BY a.Tip
                ";
                
                var rezultat = await _databaseService.QueryAsync<dynamic>(sql);
                
                return rezultat.ToDictionary(
                    x => $"Tip {x.ArtikalTip}",
                    x => (decimal)x.UkupnaKolicina
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju statistika lagera: {ex.Message}");
                return new Dictionary<string, decimal>();
            }
        }
    }
}