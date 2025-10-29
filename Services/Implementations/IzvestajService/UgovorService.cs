using FruitSysWeb.Models;
using FruitSysWeb.Services.Models.Requests;
using FruitSysWeb.Services.Interfaces;
using System.Text;
using Microsoft.Extensions.Logging;

namespace FruitSysWeb.Services.Implementations.IzvestajService
{
    public class UgovorService : IUgovorService
    {
        private readonly DatabaseService _databaseService;
        private readonly ILogger<UgovorService> _logger;

        public UgovorService(DatabaseService databaseService,
            ILogger<UgovorService> logger)
        {
            _databaseService = databaseService;
            _logger = logger;
        }

        public async Task<List<UgovorModel>> UcitajAktivneUgovore()
        {
            try
            {
                // OPTIMIZOVANO: Koristi Otpremnica i OtpremnicaStavka za isporuke
                // UgovorProdaja.Aktivno = 1 (aktivan ugovor)
                // Sabira isporuke iz otpremnica po UgovorID (ne razdvaja po artiklu)
                // NAPOMENA: Koristi BrojUgovora (broj kupca) umesto Sifra (naš interni broj)
                var sql = @"
                    SELECT
                        up.ID as UgovorID,
                        COALESCE(up.BrojUgovora, up.Sifra) as BrojUgovora,
                        k.Naziv as Komitent,
                        up.KomitentID,
                        a.Naziv as Artikal,
                        ups.ArtikalID,
                        ups.Kolicina as UgovorenaKolicina,
                        COALESCE(ups.JedinicnaCenaEur, 0) as JedinicnaCenaEur,
                        COALESCE(isporuke.Isporuceno, 0) as Isporuceno,
                        (ups.Kolicina - COALESCE(isporuke.Isporuceno, 0)) as PreostalaKolicina,
                        up.DokumentStatus,
                        up.Datum as DatumUgovora
                    FROM UgovorProdaja up
                    INNER JOIN UgovorProdajaStavka ups ON up.ID = ups.UgovorProdajaID
                    INNER JOIN Komitent k ON up.KomitentID = k.ID
                    INNER JOIN Artikal a ON ups.ArtikalID = a.ID
                    LEFT JOIN (
                        SELECT
                            o.UgovorID,
                            SUM(os.Kolicina) as Isporuceno
                        FROM Otpremnica o
                        INNER JOIN OtpremnicaStavka os ON o.ID = os.OtpremnicaID
                        WHERE o.DokumentStatus = 3
                        GROUP BY o.UgovorID
                    ) isporuke ON up.ID = isporuke.UgovorID
                    WHERE up.Aktivno = 1
                      AND up.DokumentStatus = 2
                      AND (ups.Kolicina - COALESCE(isporuke.Isporuceno, 0)) > 0
                    ORDER BY up.Datum DESC, COALESCE(up.BrojUgovora, up.Sifra), a.Naziv
                ";

                var rezultat = await _databaseService.QueryAsync<UgovorModel>(sql);
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju aktivnih ugovora");
                return new List<UgovorModel>();
            }
        }

        public async Task<List<UgovorModel>> UcitajUgovoreSaFilterima(FilterRequest filterRequest)
        {
            try
            {
                // OPTIMIZOVANO: Koristi Otpremnica i OtpremnicaStavka za isporuke
                // NAPOMENA: Koristi BrojUgovora (broj kupca) umesto Sifra (naš interni broj)
                var sql = new StringBuilder(@"
                    SELECT
                        up.ID as UgovorID,
                        COALESCE(up.BrojUgovora, up.Sifra) as BrojUgovora,
                        k.Naziv as Komitent,
                        up.KomitentID,
                        a.Naziv as Artikal,
                        ups.ArtikalID,
                        ups.Kolicina as UgovorenaKolicina,
                        COALESCE(ups.JedinicnaCenaEur, 0) as JedinicnaCenaEur,
                        COALESCE(isporuke.Isporuceno, 0) as Isporuceno,
                        (ups.Kolicina - COALESCE(isporuke.Isporuceno, 0)) as PreostalaKolicina,
                        up.DokumentStatus,
                        up.Datum as DatumUgovora
                    FROM UgovorProdaja up
                    INNER JOIN UgovorProdajaStavka ups ON up.ID = ups.UgovorProdajaID
                    INNER JOIN Komitent k ON up.KomitentID = k.ID
                    INNER JOIN Artikal a ON ups.ArtikalID = a.ID
                    LEFT JOIN (
                        SELECT
                            o.UgovorID,
                            SUM(os.Kolicina) as Isporuceno
                        FROM Otpremnica o
                        INNER JOIN OtpremnicaStavka os ON o.ID = os.OtpremnicaID
                        WHERE o.DokumentStatus = 3
                        GROUP BY o.UgovorID
                    ) isporuke ON up.ID = isporuke.UgovorID
                    WHERE up.Aktivno = 1
                      AND up.DokumentStatus = 2
                      AND (ups.Kolicina - COALESCE(isporuke.Isporuceno, 0)) > 0
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

                sql.Append(" ORDER BY up.Datum DESC, COALESCE(up.BrojUgovora, up.Sifra), a.Naziv");

                var rezultat = await _databaseService.QueryAsync<UgovorModel>(sql.ToString(), parameters);
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju ugovora sa filterima");
                return new List<UgovorModel>();
            }
        }

        public async Task<decimal> UcitajUkupnuVrednostAktivnihUgovora()
        {
            try
            {
                // OPTIMIZOVANO: Koristi Otpremnica i OtpremnicaStavka za isporuke
                var sql = @"
                    SELECT COALESCE(SUM((ups.Kolicina - COALESCE(isporuke.Isporuceno, 0)) * COALESCE(ups.JedinicnaCenaEur, 0)), 0) as UkupnaVrednost
                    FROM UgovorProdaja up
                    INNER JOIN UgovorProdajaStavka ups ON up.ID = ups.UgovorProdajaID
                    LEFT JOIN (
                        SELECT
                            o.UgovorID,
                            SUM(os.Kolicina) as Isporuceno
                        FROM Otpremnica o
                        INNER JOIN OtpremnicaStavka os ON o.ID = os.OtpremnicaID
                        WHERE o.DokumentStatus = 3
                        GROUP BY o.UgovorID
                    ) isporuke ON up.ID = isporuke.UgovorID
                    WHERE up.Aktivno = 1
                      AND up.DokumentStatus = 2
                      AND (ups.Kolicina - COALESCE(isporuke.Isporuceno, 0)) > 0
                ";

                var rezultat = await _databaseService.ExecuteScalarAsync<decimal>(sql);
                return rezultat;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju ukupne vrednosti ugovora");
                return 0;
            }
        }

        // Dodatna metoda za detalje otpremnica po ugovoru
        public async Task<List<OtpremnicaDetaljiModel>> UcitajOtpremnicePoUgovoru(long ugovorId)
        {
            try
            {
                var sql = @"
                    SELECT
                        o.ID,
                        o.Sifra as BrojOtpremnice,
                        o.Datum,
                        o.DokumentStatus,
                        os.Kolicina,
                        a.Naziv as Artikal,
                        k.Naziv as Komitent
                    FROM Otpremnica o
                    INNER JOIN OtpremnicaStavka os ON o.ID = os.OtpremnicaID
                    INNER JOIN Artikal a ON os.ArtikalID = a.ID
                    INNER JOIN Komitent k ON o.KomitentID = k.ID
                    WHERE o.UgovorID = @UgovorId
                      AND o.DokumentStatus = 3
                    ORDER BY o.Datum DESC
                ";

                var rezultat = await _databaseService.QueryAsync<OtpremnicaDetaljiModel>(sql, new { UgovorId = ugovorId });
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju otpremnica za ugovor");
                return new List<OtpremnicaDetaljiModel>();
            }
        }
    }
}