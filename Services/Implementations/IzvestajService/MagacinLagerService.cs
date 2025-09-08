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

        // POPRAVLJENO: Dodato filtriranje minimalne količine i null vrednosti
        public async Task<List<MagacinLagerModel>> UcitajLagerStanje()
        {
            try
            {
                var sql = @"
                    SELECT 
                        ArtikalID,
                        ArtikalTip,
                        Artikal,
                        Kolicina,
                        Pakovanje,
                        JM
                    FROM vwMagacinLager
                    WHERE Kolicina >= 10 
                      AND Kolicina IS NOT NULL
                      AND Artikal IS NOT NULL
                    ORDER BY Artikal
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

        // DODANO: Novo filtriranje sa FilterRequest parametrima
        public async Task<List<MagacinLagerModel>> UcitajLagerStanjeSaFilterima(FilterRequest filterRequest)
        {
            try
            {
                var sql = new StringBuilder();
                sql.Append(@"
                    SELECT 
                        ml.ArtikalID,
                        ml.ArtikalTip,
                        ml.Artikal,
                        ml.Kolicina,
                        ml.Pakovanje,
                        ml.JM
                    FROM vwMagacinLager ml
                    WHERE ml.Kolicina IS NOT NULL
                      AND ml.Artikal IS NOT NULL
                ");

                var parameters = new Dictionary<string, object>();

                // Osnovno filtriranje minimalne količine
                var minKolicina = filterRequest.MinimalnaKolicinaLager ?? 10;
                sql.Append(" AND ml.Kolicina >= @MinKolicina");
                parameters.Add("@MinKolicina", minKolicina);

                // Filtriranje po tipu artikla
                if (!string.IsNullOrEmpty(filterRequest.ArtikalTip))
                {
                    if (int.TryParse(filterRequest.ArtikalTip, out int tipInt))
                    {
                        sql.Append(" AND ml.ArtikalTip = @ArtikalTip");
                        parameters.Add("@ArtikalTip", tipInt.ToString());
                    }
                }

                // Filtriranje po specifičnom artiklu
                if (filterRequest.ArtikalId.HasValue && filterRequest.ArtikalId > 0)
                {
                    sql.Append(" AND ml.ArtikalID = @ArtikalId");
                    parameters.Add("@ArtikalId", filterRequest.ArtikalId.Value);
                }

                // Filtriranje samo gotovih roba (tip 4 prema dokumentu)
                if (filterRequest.SamoGotoveRobe == true)
                {
                    sql.Append(" AND ml.ArtikalTip = '4'");
                }

                // Filtriranje samo sirovina (tip 1)
                if (filterRequest.SamoSirovine == true)
                {
                    sql.Append(" AND ml.ArtikalTip = '1'");
                }

                // Filtriranje samo ambalaze (tip 2)
                if (filterRequest.SamoAmbalaže == true)
                {
                    sql.Append(" AND ml.ArtikalTip = '2'");
                }

                // Filtriranje po pakovanju
                if (!string.IsNullOrEmpty(filterRequest.Pakovanje))
                {
                    sql.Append(" AND ml.Pakovanje LIKE @Pakovanje");
                    parameters.Add("@Pakovanje", $"%{filterRequest.Pakovanje}%");
                }

                // Dodatno filtriranje količine
                if (filterRequest.MinKolicina.HasValue)
                {
                    sql.Append(" AND ml.Kolicina >= @MinKolicinaCustom");
                    parameters.Add("@MinKolicinaCustom", filterRequest.MinKolicina.Value);
                }

                if (filterRequest.MaxKolicina.HasValue)
                {
                    sql.Append(" AND ml.Kolicina <= @MaxKolicina");
                    parameters.Add("@MaxKolicina", filterRequest.MaxKolicina.Value);
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

        // Overload za string filter
        public async Task<List<MagacinLagerModel>> UcitajLagerStanjeSaFilterima(string filter)
        {
            try
            {
                var sql = @"
                    SELECT 
                        ArtikalID,
                        ArtikalTip,
                        Artikal,
                        Kolicina,
                        Pakovanje,
                        JM
                    FROM vwMagacinLager
                    WHERE Kolicina >= 10 
                      AND Kolicina IS NOT NULL
                      AND Artikal IS NOT NULL";

                if (!string.IsNullOrEmpty(filter))
                {
                    sql += @" AND (Artikal LIKE @Filter OR Pakovanje LIKE @Filter)";
                }

                sql += " ORDER BY Artikal";
                
                var rezultat = await _databaseService.QueryAsync<MagacinLagerModel>(sql, new { Filter = $"%{filter}%" });
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju lager stanja sa filterom: {ex.Message}");
                return new List<MagacinLagerModel>();
            }
        }

        public async Task<List<RadniNalogLagerModel>> UcitajRadneNalogeLager()
        {
            try
            {
                var sql = @"
                    SELECT 
                        rwl.RadniNalogLager as BrojNaloga,
                        'STALNI RN' as Komitent,
                        rwl.ArtikalID,
                        rwl.Artikal,
                        rwl.Kolicina,
                        rwl.Pakovanje,
                        rwl.BrojPakovanja,
                        rwl.Kolicina as PotrebnaKolicina,
                        2 as DokumentStatus
                    FROM vwRadniNalogLager rwl
                    WHERE rwl.Kolicina IS NOT NULL
                    ORDER BY rwl.RadniNalogLager
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
                        ArtikalID,
                        ArtikalTip,
                        Artikal,
                        Kolicina,
                        Pakovanje,
                        JM
                    FROM vwMagacinLager
                    WHERE ArtikalID = @ArtikalId
                      AND Kolicina >= 10
                      AND Kolicina IS NOT NULL
                    ORDER BY Artikal
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

        // POPRAVLJENO: Dodato filtriranje za gotove robe
        public async Task<List<MagacinLagerModel>> UcitajLagerStanjePoTipu(int artikalTip)
        {
            try
            {
                var sql = @"
                    SELECT 
                        ArtikalID,
                        ArtikalTip,
                        Artikal,
                        Kolicina,
                        Pakovanje,
                        JM
                    FROM vwMagacinLager
                    WHERE ArtikalTip = @ArtikalTip
                      AND Kolicina >= 10
                      AND Kolicina IS NOT NULL
                    ORDER BY Artikal
                ";
                
                var rezultat = await _databaseService.QueryAsync<MagacinLagerModel>(sql, new { ArtikalTip = artikalTip.ToString() });
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju lager stanja po tipu: {ex.Message}");
                return new List<MagacinLagerModel>();
            }
        }

        // DODANO: Specifična metoda za gotove robe (tip 4)
        public async Task<List<MagacinLagerModel>> UcitajGotoveRobe()
        {
            return await UcitajLagerStanjePoTipu(4); // 4 = Gotova roba prema dokumentu
        }

        // DODANO: Metoda za sirovine (tip 1)
        public async Task<List<MagacinLagerModel>> UcitajSirovine()
        {
            return await UcitajLagerStanjePoTipu(1); // 1 = Sirovina
        }

        // DODANO: Metoda za ambalaze (tip 2)
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
                    WHERE ml.Kolicina > 0 
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

        public async Task<List<MagacinLagerModel>> UcitajLagerStanjePoLotu(string lot)
        {
            try
            {
                var sql = @"
                    SELECT 
                        ArtikalID,
                        ArtikalTip,
                        Artikal,
                        Kolicina,
                        Pakovanje,
                        JM
                    FROM vwMagacinLager
                    WHERE Lot LIKE @Lot
                      AND Kolicina >= 10
                      AND Kolicina IS NOT NULL
                    ORDER BY Artikal
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

        public async Task<List<MagacinLagerModel>> UcitajLagerStanjePoRokuVazenja(DateTime rokVazenja)
        {
            try
            {
                var sql = @"
                    SELECT 
                        ArtikalID,
                        ArtikalTip,
                        Artikal,
                        Kolicina,
                        Pakovanje,
                        JM
                    FROM vwMagacinLager
                    WHERE DATE(RokVazenja) <= @RokVazenja
                      AND RokVazenja != '2099-12-31 00:00:00'
                      AND Kolicina >= 10
                      AND Kolicina IS NOT NULL
                    ORDER BY RokVazenja ASC, Artikal
                ";
                
                var rezultat = await _databaseService.QueryAsync<MagacinLagerModel>(sql, new { RokVazenja = rokVazenja.Date });
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju lager stanja po roku važenja: {ex.Message}");
                return new List<MagacinLagerModel>();
            }
        }

        // Overload bez parametara - vraća artikle koji ističu u narednih 30 dana
        public async Task<List<MagacinLagerModel>> UcitajLagerStanjePoRokuVazenja()
        {
            return await UcitajLagerStanjePoRokuVazenja(DateTime.Now.AddDays(30));
        }

        public async Task<List<RadniNalogLagerModel>> UcitajOtvoreneRadneNaloge()
        {
            try
            {
                var sql = @"
                    SELECT 
                        rwl.RadniNalogLager as BrojNaloga,
                        'STALNI RN' as Komitent,
                        rwl.ArtikalID,
                        rwl.Artikal,
                        rwl.Kolicina,
                        rwl.Pakovanje,
                        rwl.BrojPakovanja,
                        rwl.Kolicina as PotrebnaKolicina,
                        2 as DokumentStatus
                    FROM vwRadniNalogLager rwl
                    WHERE rwl.Kolicina > 0
                      AND rwl.Kolicina IS NOT NULL
                    ORDER BY rwl.RadniNalogLager
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
                        rwl.RadniNalogLager as BrojNaloga,
                        'STALNI RN' as Komitent,
                        rwl.ArtikalID,
                        rwl.Artikal,
                        rwl.Kolicina,
                        rwl.Pakovanje,
                        rwl.BrojPakovanja,
                        rwl.Kolicina as PotrebnaKolicina,
                        @Status as DokumentStatus
                    FROM vwRadniNalogLager rwl
                    WHERE rwl.Kolicina IS NOT NULL
                    ORDER BY rwl.RadniNalogLager
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

        // DODANO: Metoda za analizu lagera - artikli ispod minimuma
        public async Task<List<MagacinLagerModel>> UcitajArtikleIspodMinimuma(decimal minKolicina = 10)
        {
            try
            {
                var sql = @"
                    SELECT 
                        ArtikalID,
                        ArtikalTip,
                        Artikal,
                        Kolicina,
                        Pakovanje,
                        JM
                    FROM vwMagacinLager
                    WHERE Kolicina < @MinKolicina
                      AND Kolicina IS NOT NULL
                    ORDER BY Kolicina ASC, Artikal
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

        // Overload bez parametara - koristi default minimum 10
        public async Task<List<MagacinLagerModel>> UcitajArtikleIspodMinimuma()
        {
            return await UcitajArtikleIspodMinimuma(10);
        }

        // DODANO: Statistike lagera
        public async Task<Dictionary<string, decimal>> UcitajStatistikeLagera()
        {
            try
            {
                var sql = @"
                    SELECT 
                        ArtikalTip,
                        COUNT(*) as BrojArtikala,
                        SUM(Kolicina) as UkupnaKolicina,
                        AVG(Kolicina) as ProsecnaKolicina
                    FROM vwMagacinLager
                    WHERE Kolicina IS NOT NULL
                      AND Kolicina >= 10
                    GROUP BY ArtikalTip
                    ORDER BY ArtikalTip
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
