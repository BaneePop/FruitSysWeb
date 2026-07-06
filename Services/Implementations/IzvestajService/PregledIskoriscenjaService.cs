using FruitSysWeb.Models;
using FruitSysWeb.Services.Interfaces;
using Dapper;

namespace FruitSysWeb.Services.Implementations.IzvestajService
{
    public class PregledIskoriscenjaService : IPregledIskoriscenjaService
    {
        private readonly FruitSysWeb.Services.DatabaseService _db;
        private readonly ILogger<PregledIskoriscenjaService> _logger;

        public PregledIskoriscenjaService(FruitSysWeb.Services.DatabaseService db, ILogger<PregledIskoriscenjaService> logger)
        {
            _db = db;
            _logger = logger;
        }

        // ─────────────────────────────────────────────────────────────
        // TAB 1 — Sledljivost po šifri PL
        // ─────────────────────────────────────────────────────────────
        public async Task<PLSledljivostModel?> UcitajSledljivostPL(string sifra)
        {
            try
            {
                const string sqlPL = @"
                    SELECT
                        pl.ID AS PaletniListID,
                        pl.Sifra,
                        pl.Tezina,
                        pl.BrutoTezina,
                        pl.PaletniListTip,
                        pl.DatumKreiranja,
                        pl.LotDobavljaca,
                        a.Naziv AS Artikal,
                        k.Naziv AS Komitent
                    FROM PaletniList pl
                    JOIN Artikal a ON pl.ArtikalID = a.ID
                    LEFT JOIN Komitent k ON pl.KomitentID = k.ID
                    WHERE pl.Sifra = @Sifra
                    LIMIT 1";

                var pl = await _db.QueryFirstOrDefaultAsync<PLSledljivostModel>(sqlPL, new { Sifra = sifra });
                if (pl == null) return null;

                // EvidencijeRada u kojima je ovaj PL utrošen
                const string sqlUtroseni = @"
                    SELECT
                        er.ID AS EvidencijaRadaID,
                        er.Sifra AS EvidencijaSifra,
                        er.Datum AS EvidencijaDatum,
                        rn.Sifra AS RadniNalogSifra,
                        rn.ID AS RadniNalogID,
                        si.Broj AS SmenskiIzvestajBroj,
                        si.ID AS SmenskiIzvestajID,
                        si.Datum AS SmenskiDatum,
                        si.Smena,
                        r.ImePrezime AS PoslovodjaNaziv,
                        rp.Naziv AS RadniProcesNaziv
                    FROM EvidencijaRada_UtroseniPaletniListovi upl
                    JOIN EvidencijaRada er ON upl.EvidencijaRadaID = er.ID
                    JOIN RadniNalog rn ON er.RadniNalogID = rn.ID
                    JOIN SmenskiIzvestaj si ON er.SmenskiIzvestajID = si.ID
                    JOIN Radnik r ON si.PoslovodjaID = r.ID
                    JOIN RadniProces rp ON er.RadniProcesID = rp.ID
                    WHERE upl.PaletniListID = @ID AND er.Obrisan = 0";

                pl.UtroseniU = (await _db.QueryAsync<PLEvidencijaRadaModel>(sqlUtroseni, new { ID = pl.PaletniListID })).ToList();

                // Paletni listovi PROIZVODNJE koji su koristili ovaj PL kao sirovinu/ambalažu
                // PaletniListoviPracenje: TPaletniListID = traženi PL (sirovina/ambalaža)
                //                        PaletniListID  = PL proizvodnje koji je nastao
                const string sqlPovezani = @"
                    SELECT
                        plprod.ID AS PaletniListID,
                        plprod.Sifra,
                        a2.Naziv AS Artikal,
                        COALESCE(k2.Naziv, krn.Naziv) AS Komitent,
                        plp.Kolicina,
                        rn.Sifra AS RadniNalogSifra,
                        plprod.DatumKreiranja
                    FROM PaletniListoviPracenje plp
                    JOIN PaletniList plprod ON plp.PaletniListID = plprod.ID
                    JOIN Artikal a2 ON plprod.ArtikalID = a2.ID
                    LEFT JOIN Komitent k2 ON plprod.KomitentID = k2.ID
                    LEFT JOIN RadniNalog rn ON plprod.RadniNalogID = rn.ID
                    LEFT JOIN Komitent krn ON rn.KomitentID = krn.ID
                    WHERE plp.TPaletniListID = @ID
                    ORDER BY plprod.DatumKreiranja";

                pl.PovezaniProizvodni = (await _db.QueryAsync<PLPovezaniModel>(sqlPovezani, new { ID = pl.PaletniListID })).ToList();

                return pl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju sledljivosti PL {Sifra}", sifra);
                throw;
            }
        }

        // ─────────────────────────────────────────────────────────────
        // TAB 2 — Lista iskorišćenosti (samo Nabavka, PaletniListTip=1)
        // ─────────────────────────────────────────────────────────────
        public async Task<List<PLIskoriscenjeListaModel>> UcitajListuIskoriscenja(PLIskoriscenjeFilter filter)
        {
            try
            {
                var sql = new System.Text.StringBuilder(@"
                    SELECT
                        pl.ID,
                        pl.Sifra,
                        a.Naziv AS Artikal,
                        a.ID AS ArtikalID,
                        k.Naziv AS Komitent,
                        pl.Tezina,
                        pl.BrutoTezina,
                        pl.DatumKreiranja,
                        pl.LotDobavljaca,
                        ak.Naziv AS VrstaArtikla,
                        (SELECT COUNT(*)
                         FROM EvidencijaRada_UtroseniPaletniListovi upl
                         JOIN EvidencijaRada er ON upl.EvidencijaRadaID = er.ID
                         WHERE upl.PaletniListID = pl.ID AND er.Obrisan = 0) AS BrojEvidencijaUtroseno
                    FROM PaletniList pl
                    JOIN Artikal a ON pl.ArtikalID = a.ID
                    LEFT JOIN ArtikalKlasifikacija ak ON a.PrvaKlasifikacijaID = ak.ID
                    LEFT JOIN Komitent k ON pl.KomitentID = k.ID
                    WHERE pl.PaletniListTip = 1");

                var p = new DynamicParameters();

                if (filter.OdDatum.HasValue)
                {
                    sql.Append(" AND pl.DatumKreiranja >= @OdDatum");
                    p.Add("OdDatum", filter.OdDatum.Value.Date);
                }
                if (filter.DoDatum.HasValue)
                {
                    sql.Append(" AND pl.DatumKreiranja < @DoDatum");
                    p.Add("DoDatum", filter.DoDatum.Value.Date.AddDays(1));
                }
                if (filter.PrvaKlasifikacijaID.HasValue)
                {
                    sql.Append(" AND a.PrvaKlasifikacijaID = @PrvaKlasifikacijaID");
                    p.Add("PrvaKlasifikacijaID", filter.PrvaKlasifikacijaID.Value);
                }
                if (filter.ArtikalID.HasValue)
                {
                    sql.Append(" AND pl.ArtikalID = @ArtikalID");
                    p.Add("ArtikalID", filter.ArtikalID.Value);
                }
                if (filter.KomitentID.HasValue)
                {
                    sql.Append(" AND pl.KomitentID = @KomitentID");
                    p.Add("KomitentID", filter.KomitentID.Value);
                }
                if (filter.SamoNeiskorisceni == true)
                {
                    sql.Append(@" AND NOT EXISTS (
                        SELECT 1 FROM EvidencijaRada_UtroseniPaletniListovi upl2
                        JOIN EvidencijaRada er2 ON upl2.EvidencijaRadaID = er2.ID
                        WHERE upl2.PaletniListID = pl.ID AND er2.Obrisan = 0)");
                }

                sql.Append(" ORDER BY pl.DatumKreiranja DESC LIMIT 1000");

                return (await _db.QueryAsync<PLIskoriscenjeListaModel>(sql.ToString(), p)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju liste iskorišćenja PL");
                throw;
            }
        }

        // ─────────────────────────────────────────────────────────────
        // Dropdowni
        // ─────────────────────────────────────────────────────────────
        public async Task<List<(long ID, string Naziv)>> UcitajArtikle(long? prvaKlasifikacijaID = null)
        {
            // Samo artikli koji se pojavljuju na PL nabavke
            var sql = @"
                SELECT DISTINCT a.ID, a.Naziv
                FROM Artikal a
                JOIN PaletniList pl ON pl.ArtikalID = a.ID
                WHERE pl.PaletniListTip = 1 AND a.Aktivan = 1";

            if (prvaKlasifikacijaID.HasValue)
                sql += " AND a.PrvaKlasifikacijaID = @PrvaKlasifikacijaID";

            sql += " ORDER BY a.Naziv";

            var result = await _db.QueryAsync<(long ID, string Naziv)>(sql,
                prvaKlasifikacijaID.HasValue ? new { PrvaKlasifikacijaID = prvaKlasifikacijaID.Value } : null);
            return result.ToList();
        }

        public async Task<List<(long ID, string Naziv)>> UcitajKomitente()
        {
            // Samo komitenti koji se pojavljuju na PL nabavke
            const string sql = @"
                SELECT DISTINCT k.ID, k.Naziv
                FROM Komitent k
                JOIN PaletniList pl ON pl.KomitentID = k.ID
                WHERE pl.PaletniListTip = 1 AND k.Aktivno = 1
                ORDER BY k.Naziv";
            var result = await _db.QueryAsync<(long ID, string Naziv)>(sql);
            return result.ToList();
        }

        // ─────────────────────────────────────────────────────────────
        // TAB 3 — PL Statistika: korišćeni u više naloga/evidencija
        // Sirovine (PrvaKlasifikacijaID != 25): >2 RN ili >2 ER
        // Ambalaža (PrvaKlasifikacijaID = 25): >4 RN ili >6 ER
        // ─────────────────────────────────────────────────────────────
        public async Task<List<PLStatistikaModel>> UcitajStatistikuPL(PLIskoriscenjeFilter filter)
        {
            try
            {
                var sql = new System.Text.StringBuilder(@"
                    SELECT
                        pl.ID,
                        pl.Sifra,
                        a.Naziv AS Artikal,
                        a.ID AS ArtikalID,
                        a.PrvaKlasifikacijaID AS VrstaArtiklaID,
                        ak.Naziv AS VrstaArtikla,
                        k.Naziv AS Komitent,
                        pl.Tezina,
                        pl.DatumKreiranja,
                        COUNT(DISTINCT er.RadniNalogID) AS BrojRadnihNaloga,
                        COUNT(DISTINCT upl.EvidencijaRadaID) AS BrojEvidencija
                    FROM PaletniList pl
                    JOIN Artikal a ON pl.ArtikalID = a.ID
                    LEFT JOIN ArtikalKlasifikacija ak ON a.PrvaKlasifikacijaID = ak.ID
                    LEFT JOIN Komitent k ON pl.KomitentID = k.ID
                    JOIN EvidencijaRada_UtroseniPaletniListovi upl ON upl.PaletniListID = pl.ID
                    JOIN EvidencijaRada er ON upl.EvidencijaRadaID = er.ID AND er.Obrisan = 0
                    WHERE pl.PaletniListTip = 1
                      AND a.PrvaKlasifikacijaID IN (6, 10, 11, 15, 25, 27, 28, 34, 39)");

                var p = new DynamicParameters();

                if (filter.OdDatum.HasValue)
                {
                    sql.Append(" AND pl.DatumKreiranja >= @OdDatum");
                    p.Add("OdDatum", filter.OdDatum.Value.Date);
                }
                if (filter.DoDatum.HasValue)
                {
                    sql.Append(" AND pl.DatumKreiranja < @DoDatum");
                    p.Add("DoDatum", filter.DoDatum.Value.Date.AddDays(1));
                }
                if (filter.PrvaKlasifikacijaID.HasValue)
                {
                    sql.Append(" AND a.PrvaKlasifikacijaID = @PrvaKlasifikacijaID");
                    p.Add("PrvaKlasifikacijaID", filter.PrvaKlasifikacijaID.Value);
                }
                if (filter.ArtikalID.HasValue)
                {
                    sql.Append(" AND pl.ArtikalID = @ArtikalID");
                    p.Add("ArtikalID", filter.ArtikalID.Value);
                }
                if (filter.KomitentID.HasValue)
                {
                    sql.Append(" AND pl.KomitentID = @KomitentID");
                    p.Add("KomitentID", filter.KomitentID.Value);
                }

                // HAVING: pragovi po vrsti
                // Ambalaža (ID=25): >4 RN ili >6 ER; Sirovine: >2 RN ili >2 ER
                sql.Append(@"
                    GROUP BY pl.ID, pl.Sifra, a.Naziv, a.ID, a.PrvaKlasifikacijaID,
                             ak.Naziv, k.Naziv, pl.Tezina, pl.DatumKreiranja
                    HAVING
                        (a.PrvaKlasifikacijaID = 25 AND (COUNT(DISTINCT er.RadniNalogID) > 4 OR COUNT(DISTINCT upl.EvidencijaRadaID) > 6))
                        OR
                        (a.PrvaKlasifikacijaID != 25 AND (COUNT(DISTINCT er.RadniNalogID) > 2 OR COUNT(DISTINCT upl.EvidencijaRadaID) > 2))
                    ORDER BY BrojRadnihNaloga DESC, BrojEvidencija DESC
                    LIMIT 500");

                return (await _db.QueryAsync<PLStatistikaModel>(sql.ToString(), p)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju statistike PL");
                throw;
            }
        }

        public async Task<List<(long ID, string Naziv)>> UcitajVrsteArtikala()
        {
            // Fiksirana lista vrsta prema PrvaKlasifikacijaID
            const string sql = @"
                SELECT ID, Naziv
                FROM ArtikalKlasifikacija
                WHERE ID IN (6, 10, 11, 15, 25, 27, 28, 34, 39)
                ORDER BY Naziv";
            var result = await _db.QueryAsync<(long ID, string Naziv)>(sql);
            return result.ToList();
        }
    }
}
