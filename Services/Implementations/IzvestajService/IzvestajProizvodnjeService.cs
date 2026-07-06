using FruitSysWeb.Models;
using FruitSysWeb.Services.Core;
using FruitSysWeb.Services.Interfaces;

namespace FruitSysWeb.Services.Implementations.IzvestajService
{
    public class IzvestajProizvodnjeService : IIzvestajProizvodnjeService
    {
        private readonly DatabaseService _db;

        public IzvestajProizvodnjeService(DatabaseService db)
        {
            _db = db;
        }

        public async Task<IzvestajProizvodnjeModel> UcitajIzvestaj(DateTime datumOd, DateTime datumDo)
        {
            var tGotova     = UcitajGotovuRobu(datumOd, datumDo);
            var tSirovine   = UcitajSirovine(datumOd, datumDo);
            var tAmbalaza   = UcitajAmbalazu(datumOd, datumDo);
            var tDirektan   = UcitajTrosakDirektan(datumOd, datumDo);
            var tIndirektan = UcitajTrosakIndirektan(datumOd, datumDo);

            await Task.WhenAll(tGotova, tSirovine, tAmbalaza, tDirektan, tIndirektan);

            // Količina gotove robe po datumu (za TrosakPoKg po danu)
            var kgPoDatumu = tGotova.Result
                .GroupBy(r => r.Datum.Date)
                .ToDictionary(g => g.Key, g => g.Sum(r => r.Kolicina));

            decimal ukupnoKg = tGotova.Result.Sum(r => r.Kolicina);

            var direktan = tDirektan.Result;
            foreach (var red in direktan)
            {
                decimal kg = kgPoDatumu.TryGetValue(red.Datum.Date, out var v) ? v : 0;
                red.TrosakPoKg = kg > 0 ? red.Iznos / kg : 0;
            }

            var indirektan = tIndirektan.Result;
            foreach (var red in indirektan)
                red.TrosakPoKg = ukupnoKg > 0 ? red.Iznos / ukupnoKg : 0;

            return new IzvestajProizvodnjeModel
            {
                GotovaRoba       = tGotova.Result,
                UtroseneSirovine = tSirovine.Result,
                UtrosenaAmbalaza = tAmbalaza.Result,
                TrosakDirektan   = direktan,
                TrosakIndirektan = indirektan,
            };
        }

        // ── 1. Dobijena gotova roba (RpArtikalTip = 2, MagacinID = 6) ──────────
        private async Task<List<GotovaRobaRedModel>> UcitajGotovuRobu(DateTime od, DateTime do_)
        {
            const string sql = @"
                SELECT
                    DATE(er.Datum)      AS Datum,
                    a.Naziv             AS Artikal,
                    SUM(rpa.Kolicina)   AS Kolicina
                FROM EvidencijaRada er
                INNER JOIN RadniProcesArtikal rpa ON rpa.EvidencijaRadaID = er.ID
                INNER JOIN ArtikalInstanca    ai  ON ai.ID  = rpa.ArtikalInstancaID
                INNER JOIN Artikal            a   ON a.ID   = ai.ArtikalID
                WHERE CAST(er.Obrisan AS SIGNED) = 0
                  AND CAST(rpa.Storno AS SIGNED) = 0
                  AND rpa.RpArtikalTip = 2
                  AND a.MagacinID = 6
                  AND DATE(er.Datum) >= @Od
                  AND DATE(er.Datum) <= @Do
                GROUP BY DATE(er.Datum), a.Naziv
                ORDER BY DATE(er.Datum), a.Naziv";

            var rows = await _db.QueryAsync<GotovaRobaRedModel>(sql, new { Od = od.Date, Do = do_.Date });
            return rows.ToList();
        }

        // ── 2. Utrošene sirovine (RpArtikalTip = 1) ───────────────────────────
        private async Task<List<UtrosenaSirovinaRedModel>> UcitajSirovine(DateTime od, DateTime do_)
        {
            const string sql = @"
                SELECT
                    DATE(er.Datum)      AS Datum,
                    a.Naziv             AS Artikal,
                    SUM(rpa.Kolicina)   AS Kolicina
                FROM EvidencijaRada er
                INNER JOIN RadniProcesArtikal rpa ON rpa.EvidencijaRadaID = er.ID
                INNER JOIN ArtikalInstanca    ai  ON ai.ID  = rpa.ArtikalInstancaID
                INNER JOIN Artikal            a   ON a.ID   = ai.ArtikalID
                WHERE CAST(er.Obrisan AS SIGNED) = 0
                  AND CAST(rpa.Storno AS SIGNED) = 0
                  AND rpa.RpArtikalTip = 1
                  AND DATE(er.Datum) >= @Od
                  AND DATE(er.Datum) <= @Do
                GROUP BY DATE(er.Datum), a.Naziv
                ORDER BY DATE(er.Datum), a.Naziv";

            var rows = await _db.QueryAsync<UtrosenaSirovinaRedModel>(sql, new { Od = od.Date, Do = do_.Date });
            return rows.ToList();
        }

        // ── 3. Utrošena ambalaza (RpArtikalTip = 4) ───────────────────────────
        private async Task<List<UtrosenaAmbalazeRedModel>> UcitajAmbalazu(DateTime od, DateTime do_)
        {
            const string sql = @"
                SELECT
                    DATE(er.Datum)                              AS Datum,
                    a.Naziv                                     AS Artikal,
                    CASE a.AmbalazaTip
                        WHEN 1 THEN 'Gajba'
                        WHEN 2 THEN 'Kesa'
                        WHEN 3 THEN 'Kutija'
                        WHEN 4 THEN 'Paleta'
                        ELSE 'Ostalo'
                    END                                         AS VrstaAmbalazeNaziv,
                    SUM(rpa.Kolicina)                           AS Kolicina
                FROM EvidencijaRada er
                INNER JOIN RadniProcesArtikal rpa ON rpa.EvidencijaRadaID = er.ID
                INNER JOIN ArtikalInstanca    ai  ON ai.ID  = rpa.ArtikalInstancaID
                INNER JOIN Artikal            a   ON a.ID   = ai.ArtikalID
                WHERE CAST(er.Obrisan AS SIGNED) = 0
                  AND CAST(rpa.Storno AS SIGNED) = 0
                  AND rpa.RpArtikalTip = 4
                  AND DATE(er.Datum) >= @Od
                  AND DATE(er.Datum) <= @Do
                GROUP BY DATE(er.Datum), a.Naziv, a.AmbalazaTip
                ORDER BY DATE(er.Datum), a.Naziv";

            var rows = await _db.QueryAsync<UtrosenaAmbalazeRedModel>(sql, new { Od = od.Date, Do = do_.Date });
            return rows.ToList();
        }

        // ── 4. Direktan trošak radne snage (DirektanRadObracunat = 1) ──────────
        private async Task<List<TrosakRadneSnageDirektanRedModel>> UcitajTrosakDirektan(DateTime od, DateTime do_)
        {
            const string sql = @"
                SELECT
                    DATE(er.Datum)                          AS Datum,
                    SUM(er.BrojRadnihSati)                  AS BrojRadnihSati,
                    SUM(er.CenaKostanjaDirektanRad)         AS Iznos
                FROM EvidencijaRada er
                WHERE CAST(er.Obrisan AS SIGNED) = 0
                  AND CAST(er.DirektanRadObracunat AS SIGNED) = 1
                  AND DATE(er.Datum) >= @Od
                  AND DATE(er.Datum) <= @Do
                GROUP BY DATE(er.Datum)
                ORDER BY DATE(er.Datum)";

            var rows = await _db.QueryAsync<dynamic>(sql, new { Od = od.Date, Do = do_.Date });
            return rows.Select(r => new TrosakRadneSnageDirektanRedModel
            {
                Datum          = (DateTime)r.Datum,
                BrojRadnihSati = Convert.ToDecimal(r.BrojRadnihSati),
                Iznos          = Convert.ToDecimal(r.Iznos),
                TrosakPoKg     = 0, // popunjava se nakon učitavanja gotove robe
            }).ToList();
        }

        // ── 5. Indirektan trošak radne snage (DirektanRadObracunat = 0) ────────
        private async Task<List<TrosakRadneSnageIndirektanRedModel>> UcitajTrosakIndirektan(DateTime od, DateTime do_)
        {
            const string sql = @"
                SELECT
                    DATE(er.Datum)                      AS Datum,
                    SUM(er.BrojRadnihSati)              AS BrojRadnihSati,
                    SUM(er.CenaKostanjaDirektanRad)     AS Iznos
                FROM EvidencijaRada er
                WHERE CAST(er.Obrisan AS SIGNED) = 0
                  AND CAST(er.DirektanRadObracunat AS SIGNED) = 0
                  AND DATE(er.Datum) >= @Od
                  AND DATE(er.Datum) <= @Do
                GROUP BY DATE(er.Datum)
                ORDER BY DATE(er.Datum)";

            var rows = await _db.QueryAsync<dynamic>(sql, new { Od = od.Date, Do = do_.Date });
            return rows.Select(r => new TrosakRadneSnageIndirektanRedModel
            {
                Datum          = (DateTime)r.Datum,
                BrojRadnihSati = Convert.ToDecimal(r.BrojRadnihSati),
                Iznos          = Convert.ToDecimal(r.Iznos),
                TrosakPoKg     = 0,
            }).ToList();
        }
    }
}
