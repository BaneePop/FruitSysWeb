using FruitSysWeb.Models;
using FruitSysWeb.Services.Models.Requests;
using FruitSysWeb.Services.Interfaces;
using System.Text;

namespace FruitSysWeb.Services.Implementations.IzvestajService
{
    public class UgovorService : IUgovorService
    {
        private readonly DatabaseService _databaseService;

        public UgovorService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<List<UgovorModel>> UcitajAktivneUgovore()
        {
            try
            {
                var sql = @"
                    SELECT 
                        up.ID as UgovorID,
                        up.Sifra as BrojUgovora,
                        k.Naziv as Komitent,
                        up.KomitentID,
                        a.Naziv as Artikal,
                        ups.ArtikalID,
                        ups.Kolicina as UgovorenaKolicina,
                        COALESCE(ups.JedinicnaCenaEur, 0) as JedinicnaCenaEur,
                        COALESCE((
                            SELECT SUM(rn.Kolicina) 
                            FROM RadniNalog rn 
                            WHERE rn.UgovorProdajaID = up.ID 
                            AND rn.DokumentStatus = 3  -- SAMO ZAVRŠENI RADNI NALOZI
                            AND rn.Aktivno = 1
                        ), 0) as Isporuceno,
                        (ups.Kolicina - COALESCE((
                            SELECT SUM(rn.Kolicina) 
                            FROM RadniNalog rn 
                            WHERE rn.UgovorProdajaID = up.ID 
                            AND rn.DokumentStatus = 3  -- SAMO ZAVRŠENI RADNI NALOZI
                            AND rn.Aktivno = 1
                        ), 0)) as PreostalaKolicina,
                        up.DokumentStatus,
                        up.Datum as DatumUgovora
                    FROM UgovorProdaja up
                    INNER JOIN UgovorProdajaStavka ups ON up.ID = ups.UgovorProdajaID
                    INNER JOIN Komitent k ON up.KomitentID = k.ID
                    INNER JOIN Artikal a ON ups.ArtikalID = a.ID
                    WHERE up.DokumentStatus = 2  -- SAMO AKTIVNI UGOVORI
                      AND up.Aktivno = 1
                      AND (ups.Kolicina - COALESCE((
                            SELECT SUM(rn.Kolicina) 
                            FROM RadniNalog rn 
                            WHERE rn.UgovorProdajaID = up.ID 
                            AND rn.DokumentStatus = 3
                            AND rn.Aktivno = 1
                        ), 0)) > 0  -- SAMO NEPOTPUNO ISPORUČENI
                    ORDER BY up.Datum DESC
                ";

                var rezultat = await _databaseService.QueryAsync<UgovorModel>(sql);
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju aktivnih ugovora: {ex.Message}");
                return new List<UgovorModel>();
            }
        }

        public async Task<List<UgovorModel>> UcitajUgovoreSaFilterima(FilterRequest filterRequest)
        {
            try
            {
                var sql = new StringBuilder(@"
                    SELECT 
                        up.ID as UgovorID,
                        up.Sifra as BrojUgovora,
                        k.Naziv as Komitent,
                        up.KomitentID,
                        a.Naziv as Artikal,
                        ups.ArtikalID,
                        ups.Kolicina as UgovorenaKolicina,
                        COALESCE(ups.JedinicnaCenaEur, 0) as JedinicnaCenaEur,
                        COALESCE((
                            SELECT SUM(rn.Kolicina) 
                            FROM RadniNalog rn 
                            WHERE rn.UgovorProdajaID = up.ID 
                            AND rn.DokumentStatus = 3  -- SAMO ZAVRŠENI RADNI NALOZI
                            AND rn.Aktivno = 1
                        ), 0) as Isporuceno,
                        (ups.Kolicina - COALESCE((
                            SELECT SUM(rn.Kolicina) 
                            FROM RadniNalog rn 
                            WHERE rn.UgovorProdajaID = up.ID 
                            AND rn.DokumentStatus = 3
                            AND rn.Aktivno = 1
                        ), 0)) as PreostalaKolicina,
                        up.DokumentStatus,
                        up.Datum as DatumUgovora
                    FROM UgovorProdaja up
                    INNER JOIN UgovorProdajaStavka ups ON up.ID = ups.UgovorProdajaID
                    INNER JOIN Komitent k ON up.KomitentID = k.ID
                    INNER JOIN Artikal a ON ups.ArtikalID = a.ID
                    WHERE up.DokumentStatus = 2  -- SAMO AKTIVNI UGOVORI
                      AND up.Aktivno = 1
                      AND (ups.Kolicina - COALESCE((
                            SELECT SUM(rn.Kolicina) 
                            FROM RadniNalog rn 
                            WHERE rn.UgovorProdajaID = up.ID 
                            AND rn.DokumentStatus = 3
                            AND rn.Aktivno = 1
                        ), 0)) > 0  -- SAMO NEPOTPUNO ISPORUČENI
                ");

                var parameters = new Dictionary<string, object>();

                // Filtriranje po komitentu
                if (!string.IsNullOrEmpty(filterRequest.Tip))
                {
                    sql.Append(" AND k.Naziv LIKE @Komitent");
                    parameters.Add("@Komitent", $"%{filterRequest.Tip}%");
                }

                // Filtriranje po artiklu
                if (!string.IsNullOrEmpty(filterRequest.Pakovanje))
                {
                    sql.Append(" AND a.Naziv LIKE @Artikal");
                    parameters.Add("@Artikal", $"%{filterRequest.Pakovanje}%");
                }

                // Filtriranje po broju ugovora
                /* if (!string.IsNullOrEmpty(filterRequest.SearchTerm))
                {
                    sql.Append(" AND up.Sifra LIKE @BrojUgovora");
                    parameters.Add("@BrojUgovora", $"%{filterRequest.SearchTerm}%");
                } */

                sql.Append(" ORDER BY up.Datum DESC");

                var rezultat = await _databaseService.QueryAsync<UgovorModel>(sql.ToString(), parameters);
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju ugovora sa filterima: {ex.Message}");
                return new List<UgovorModel>();
            }
        }

        public async Task<decimal> UcitajUkupnuVrednostAktivnihUgovora()
        {
            try
            {
                var sql = @"
                    SELECT COALESCE(SUM(ups.Kolicina * COALESCE(ups.JedinicnaCenaEur, 0)), 0) as UkupnaVrednost
                    FROM UgovorProdaja up
                    INNER JOIN UgovorProdajaStavka ups ON up.ID = ups.UgovorProdajaID
                    WHERE up.DokumentStatus = 2
                      AND up.Aktivno = 1
                      AND (ups.Kolicina - COALESCE((
                            SELECT SUM(rn.Kolicina) 
                            FROM RadniNalog rn 
                            WHERE rn.UgovorProdajaID = up.ID 
                            AND rn.DokumentStatus = 3
                            AND rn.Aktivno = 1
                        ), 0)) > 0
                ";

                var rezultat = await _databaseService.ExecuteScalarAsync<decimal>(sql);
                return rezultat;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju ukupne vrednosti ugovora: {ex.Message}");
                return 0;
            }
        }

        // Dodatna metoda za detalje radnih naloga po ugovoru
        public async Task<List<RadniNalogModel>> UcitajRadneNalogePoUgovoru(long ugovorId)
        {
            try
            {
                var sql = @"
                    SELECT 
                        rn.ID,
                        rn.Sifra as BrojNaloga,
                        rn.Datum as DatumPocetka,
                        rn.DokumentStatus,
                        rn.Kolicina,
                        a.Naziv as Artikal,
                        k.Naziv as Komitent
                    FROM RadniNalog rn
                    INNER JOIN Artikal a ON rn.ArtikalID = a.ID
                    INNER JOIN Komitent k ON rn.KomitentID = k.ID
                    WHERE rn.UgovorProdajaID = @UgovorId
                      AND rn.Aktivno = 1
                    ORDER BY rn.Datum DESC
                ";

                var rezultat = await _databaseService.QueryAsync<RadniNalogModel>(sql, new { UgovorId = ugovorId });
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju radnih naloga za ugovor: {ex.Message}");
                return new List<RadniNalogModel>();
            }
        }
    }
}