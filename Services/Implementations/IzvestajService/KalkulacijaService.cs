using FruitSysWeb.Models;
using FruitSysWeb.Services.Core;
using FruitSysWeb.Services.Interfaces;
using FormulaStavka = FruitSysWeb.Services.Core.FormulaIskoriscenjaStavka;

namespace FruitSysWeb.Services.Implementations.IzvestajService
{
    public class KalkulacijaService : IKalkulacijaService
    {
        private readonly DatabaseService _db;
        private readonly ILogger<KalkulacijaService> _logger;

        private static readonly Dictionary<int, string> VoceGrupe = new()
        {
            { 6,  "Malina"  },
            { 10, "Kupina"  },
            { 11, "Višnja"  },
            { 15, "Šljiva"  },
            { 28, "Kajsija" },
            { 34, "Jagoda"  },
        };

        private static readonly Dictionary<int, decimal> KaloPreradePct = new()
        {
            { 6,  2m },
            { 10, 3m },
            { 11, 5m },
            { 15, 7m },
            { 28, 8m },
            { 34, 2m },
        };

        private static readonly int[] PraceneKlasifikacije = PrenosZalihaHelper.PraceneKlasifikacije;

        private const string FilterUsluge = PrenosZalihaHelper.FilterIskljuciUslugu;
        private const string FilterSamoUsluge = PrenosZalihaHelper.FilterSamoUsluga;

        // 2=Sveža, 3=Sirovine, 5=Poluproizvodi, 6=Gotova roba
        private const string FilterMagacinKupljeno = "AND a.MagacinID IN (2, 3, 5, 6)";
        private const string FilterMagacinSirovine = "AND a.MagacinID IN (2, 3)";

        public KalkulacijaService(DatabaseService db, ILogger<KalkulacijaService> logger)
        {
            _db = db;
            _logger = logger;
        }

        // ─────────────────────────────────────────────────────────────
        // STANJE ROBE — Tabela 1
        // ─────────────────────────────────────────────────────────────

        public async Task<KalkulacijaStanjeRobeResult> UcitajStanjeRobe(KalkulacijaPeriod period, List<int> iskluceniArtikli)
        {
            try
            {
                var kupljeno             = await UcitajKupljeno(period, usluga: false);
                var prenosZaliha         = await UcitajPrenosaZaliha(period, usluga: false, samoSirovine: false, iskluceniArtikli);
                var prenosZalihaSirovine = await UcitajPrenosaZaliha(period, usluga: false, samoSirovine: true, iskluceniArtikli);
                var prodato              = await UcitajProdato(period, usluga: false);

                return SastembiTabelu(kupljeno, prenosZaliha, prenosZalihaSirovine, prodato);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajStanjeRobe");
                throw;
            }
        }

        // ─────────────────────────────────────────────────────────────
        // USLUGA — nova tabela ispod Stanja Robe
        // ─────────────────────────────────────────────────────────────

        public async Task<KalkulacijaUslugaResult> UcitajUslugu(KalkulacijaPeriod period)
        {
            try
            {
                var kupljeno     = await UcitajKupljeno(period, usluga: true);
                var prenosZaliha = await UcitajPrenosaZaliha(period, usluga: true, samoSirovine: false);
                var prodato      = await UcitajProdato(period, usluga: true);
                var naZalihama   = await UcitajNaZalihama(usluga: true);

                var result = new KalkulacijaUslugaResult();
                foreach (var (klasId, naziv) in VoceGrupe)
                {
                    decimal kupljenoKg    = kupljeno.TryGetValue(klasId, out var ku) ? ku : 0m;
                    decimal prenos      = prenosZaliha.TryGetValue(klasId, out var pr) ? pr : 0m;
                    decimal prodatoKg     = prodato.TryGetValue(klasId, out var po) ? po : 0m;
                    decimal naZalihamaKg  = naZalihama.TryGetValue(klasId, out var nz) ? nz : 0m;

                    if (kupljenoKg == 0 && prenos == 0 && prodatoKg == 0 && naZalihamaKg == 0)
                        continue;

                    result.Redovi.Add(new KalkulacijaUslugaRed
                    {
                        VrstaVoca    = naziv,
                        Kupljeno     = kupljenoKg,
                        PrenosaZaliha = prenos,
                        Prodato      = prodatoKg,
                        NaZalihama   = naZalihamaKg
                    });
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajUslugu");
                throw;
            }
        }

        // ─────────────────────────────────────────────────────────────
        // 1. KUPLJENO
        // ─────────────────────────────────────────────────────────────
        private async Task<Dictionary<int, decimal>> UcitajKupljeno(KalkulacijaPeriod period, bool usluga)
        {
            string filter = usluga ? FilterSamoUsluge : FilterUsluge;
            string sql = $@"
                SELECT
                    vp.ArtikalPrvaKlasifikacijaID AS KlasId,
                    SUM(ABS(vp.Kolicina)) AS Kolicina
                FROM vPrometFinansijev9 vp
                INNER JOIN Artikal a ON a.ID = vp.ArtikalID
                WHERE vp.Dokument LIKE 'KL-%'
                  AND vp.DokumentStatus NOT IN (2, 4)
                  {FilterMagacinKupljeno}
                  AND vp.ArtikalPrvaKlasifikacijaID IN (6, 10, 11, 15, 28, 34)
                  AND vp.Kolicina <> 0
                  AND COALESCE(vp.Potrazuje, 0) > 0
                  AND DATE(vp.Datum) >= @OdDatum
                  AND DATE(vp.Datum) <= @DoDatum
                  {filter}
                GROUP BY vp.ArtikalPrvaKlasifikacijaID";

            var rows = await _db.QueryAsync<dynamic>(sql, new
            {
                OdDatum = period.OdDatum.ToString("yyyy-MM-dd"),
                DoDatum = period.DoDatum.ToString("yyyy-MM-dd")
            });
            return rows.ToDictionary(r => (int)r.KlasId, r => (decimal)r.Kolicina);
        }

        // ─────────────────────────────────────────────────────────────
        // 2. PRENOS ZALIHA
        //    Sekcija 5: svi artikli (svi magacini + proizvodnja), bez filtera.
        //    Ukupno po vrsti = LagerHome algoritam; po redu = proporcionalna raspodela.
        //    Tabela 1: zbir redova minus označeni ArtikalID (Sekcija 5).
        // ─────────────────────────────────────────────────────────────
        private async Task<Dictionary<int, decimal>> UcitajPrenosaZaliha(
            KalkulacijaPeriod period, bool usluga, bool samoSirovine,
            List<int>? iskluceniArtikli = null)
        {
            bool? samoUsluga = usluga ? true : null;
            var redovi = await UcitajPrenosZalihaRedoveAsync(
                period, samoSirovine, samoUsluga, ukljuciProizvodnju: !samoSirovine);

            var zaSabiranje = redovi.Select(r => (r.KlasId, r.ArtikalID, r.Kolicina));
            return PrenosZalihaHelper.SaberiPrenosIzRedova(
                zaSabiranje, PraceneKlasifikacije, usluga ? null : iskluceniArtikli);
        }

        private async Task<Dictionary<int, decimal>> RekonstruisiStanjePoVrstiAsync(
            DateTime pocetakSezone, bool samoSirovine, bool? samoUsluga)
        {
            PrenosZalihaHelper.SqlStanjePoVrsti(
                PraceneKlasifikacije, samoSirovine, samoUsluga, out var sqlLager);
            PrenosZalihaHelper.SqlPrometPoVrsti(
                PraceneKlasifikacije, samoSirovine, samoUsluga, out var sqlPromet);

            var paramPromet = new
            {
                OdSezona = pocetakSezone.ToString("yyyy-MM-dd"),
                Danas = DateTime.Today.ToString("yyyy-MM-dd")
            };

            var lagerRows = await _db.QueryAsync<dynamic>(sqlLager);
            var prometRows = await _db.QueryAsync<dynamic>(sqlPromet, paramPromet);

            return PrenosZalihaHelper.RekonstruisiPoVrsti(
                lagerRows, prometRows, PraceneKlasifikacije);
        }

        /// <summary>
        /// Svi redovi za Sekciju 5 — količine na dan pre sezone, bez ikakvog filtera (osim usluga tabele).
        /// </summary>
        private async Task<List<KalkulacijaLagerPrenosStavka>> UcitajPrenosZalihaRedoveAsync(
            KalkulacijaPeriod period, bool samoSirovine, bool? samoUsluga, bool ukljuciProizvodnju)
        {
            static string NazivMagacina(int magacinId) => magacinId switch
            {
                2 => "Sveza Roba",
                3 => "Sirovine",
                5 => "Poluproizvodi",
                6 => "Gotova Roba",
                _ => $"Magacin {magacinId}"
            };

            var pocetakSezone = period.OdDatum.Date;
            string magacinFilter = samoSirovine ? "AND a.MagacinID IN (2, 3)" : "";

            string sqlMagacin = $@"
                SELECT
                    a.ID AS ArtikalID,
                    a.Naziv,
                    a.PrvaKlasifikacijaID AS KlasId,
                    a.MagacinID,
                    SUM(ml.Kolicina) AS Kolicina
                FROM MagacinLager ml
                JOIN ArtikalInstanca ai ON ml.ArtikalInstancaID = ai.ID
                JOIN Artikal a ON ai.ArtikalID = a.ID
                WHERE a.PrvaKlasifikacijaID IN (6, 10, 11, 15, 28, 34)
                  {magacinFilter}
                GROUP BY a.ID, a.Naziv, a.PrvaKlasifikacijaID, a.MagacinID
                HAVING SUM(ml.Kolicina) > 0";

            const string sqlProizvodnja = @"
                SELECT
                    a.ID AS ArtikalID,
                    a.Naziv,
                    a.PrvaKlasifikacijaID AS KlasId,
                    rn.KomitentID,
                    k.Naziv AS NazivKomitenta,
                    SUM(rnl.Kolicina) AS Kolicina
                FROM vwRadniNalogLager rnl
                INNER JOIN Artikal a ON a.ID = rnl.ArtikalID
                LEFT JOIN RadniNalog rn ON rnl.RadniNalogLager = rn.Sifra
                LEFT JOIN Komitent k ON k.ID = rn.KomitentID
                WHERE a.PrvaKlasifikacijaID IN (6, 10, 11, 15, 28, 34)
                  AND rnl.Kolicina > 0
                GROUP BY a.ID, a.Naziv, a.PrvaKlasifikacijaID, rn.KomitentID, k.Naziv
                HAVING SUM(rnl.Kolicina) > 0";

            var tMag = _db.QueryAsync<dynamic>(sqlMagacin);
            var tProd = ukljuciProizvodnju
                ? _db.QueryAsync<dynamic>(sqlProizvodnja)
                : Task.FromResult<IEnumerable<dynamic>>(Array.Empty<dynamic>());
            var tUkupno = RekonstruisiStanjePoVrstiAsync(pocetakSezone, samoSirovine, samoUsluga);
            await Task.WhenAll(tMag, tProd, tUkupno);

            var magacinRows = (await tMag).ToList();
            var prodRows = (await tProd).ToList();
            var ukupnoPoVrsti = await tUkupno;

            var trenutniZbirPoVrsti = new Dictionary<int, decimal>();
            foreach (var r in magacinRows)
            {
                int klasId = (int)r.KlasId;
                decimal kg = (decimal)r.Kolicina;
                trenutniZbirPoVrsti[klasId] = trenutniZbirPoVrsti.TryGetValue(klasId, out var ex) ? ex + kg : kg;
            }
            if (ukljuciProizvodnju)
            {
                foreach (var r in prodRows)
                {
                    int klasId = (int)r.KlasId;
                    decimal kg = (decimal)r.Kolicina;
                    trenutniZbirPoVrsti[klasId] = trenutniZbirPoVrsti.TryGetValue(klasId, out var ex) ? ex + kg : kg;
                }
            }

            var danPreSezone = pocetakSezone.AddDays(-1);
            var redovi = new List<KalkulacijaLagerPrenosStavka>();

            foreach (var r in magacinRows)
            {
                int artId = (int)r.ArtikalID;
                int klasId = (int)r.KlasId;
                decimal trenutno = (decimal)r.Kolicina;
                decimal zbir = trenutniZbirPoVrsti.TryGetValue(klasId, out var z) ? z : trenutno;
                decimal ukupno = ukupnoPoVrsti.TryGetValue(klasId, out var u) ? u : 0m;
                decimal naDan = PrenosZalihaHelper.RaspodeliRed(trenutno, ukupno, zbir);

                if (naDan <= 0m) continue;

                redovi.Add(new KalkulacijaLagerPrenosStavka
                {
                    ArtikalID = artId,
                    KlasId = klasId,
                    Naziv = (string)r.Naziv,
                    VrstaVoca = VoceGrupe.TryGetValue(klasId, out var vn) ? vn : klasId.ToString(),
                    NazivMagacina = NazivMagacina((int)r.MagacinID),
                    JeLagerProizvodnje = false,
                    Kolicina = naDan,
                    DatumPrenosa = danPreSezone
                });
            }

            if (ukljuciProizvodnju)
            {
                foreach (var r in prodRows)
                {
                    int artId = (int)r.ArtikalID;
                    int klasId = (int)r.KlasId;
                    decimal trenutno = (decimal)r.Kolicina;
                    decimal zbir = trenutniZbirPoVrsti.TryGetValue(klasId, out var z) ? z : trenutno;
                    decimal ukupno = ukupnoPoVrsti.TryGetValue(klasId, out var u) ? u : 0m;
                    decimal naDan = PrenosZalihaHelper.RaspodeliRed(trenutno, ukupno, zbir);

                    if (naDan <= 0m) continue;

                    redovi.Add(new KalkulacijaLagerPrenosStavka
                    {
                        ArtikalID = artId,
                        KlasId = klasId,
                        Naziv = (string)r.Naziv,
                        VrstaVoca = VoceGrupe.TryGetValue(klasId, out var vn) ? vn : klasId.ToString(),
                        NazivMagacina = "Lager Proizvodnje",
                        JeLagerProizvodnje = true,
                        KomitentID = r.KomitentID == null ? null : (int)r.KomitentID,
                        NazivKomitenta = r.NazivKomitenta?.ToString(),
                        Kolicina = naDan,
                        DatumPrenosa = danPreSezone
                    });
                }
            }

            _logger.LogInformation(
                "Prenos redovi na {Datum}: ukupno Malina={Malina:N2}, zbir redova Malina={ZbirRedova:N2}, redova={BrojRedova}",
                danPreSezone,
                ukupnoPoVrsti.TryGetValue(6, out var m) ? m : 0m,
                redovi.Where(x => x.KlasId == 6).Sum(x => x.Kolicina),
                redovi.Count);

            return redovi
                .OrderBy(a => a.VrstaVoca)
                .ThenBy(a => a.Naziv)
                .ThenBy(a => a.NazivMagacina)
                .ToList();
        }

        // ─────────────────────────────────────────────────────────────
        // 3. PRODATO
        // ─────────────────────────────────────────────────────────────
        private async Task<Dictionary<int, decimal>> UcitajProdato(KalkulacijaPeriod period, bool usluga)
        {
            string filter = usluga ? FilterSamoUsluge : FilterUsluge;
            string sql = $@"
                SELECT
                    vp.ArtikalPrvaKlasifikacijaID AS KlasId,
                    SUM(ABS(vp.Kolicina)) AS Kolicina
                FROM vPrometFinansijev9 vp
                INNER JOIN Artikal a ON a.ID = vp.ArtikalID
                WHERE vp.Dokument LIKE 'FK-%'
                  AND vp.DokumentStatus NOT IN (2, 4)
                  AND vp.ArtikalPrvaKlasifikacijaID IN (6, 10, 11, 15, 28, 34)
                  AND vp.Kolicina <> 0
                  AND DATE(vp.Datum) >= @OdDatum
                  AND DATE(vp.Datum) <= @DoDatum
                  {filter}
                GROUP BY vp.ArtikalPrvaKlasifikacijaID";

            var rows = await _db.QueryAsync<dynamic>(sql, new
            {
                OdDatum = period.OdDatum.ToString("yyyy-MM-dd"),
                DoDatum = period.DoDatum.ToString("yyyy-MM-dd")
            });
            return rows.ToDictionary(r => (int)r.KlasId, r => (decimal)r.Kolicina);
        }

        // ─────────────────────────────────────────────────────────────
        // 4. NA ZALIHAMA — direktno iz MagacinLager (svi magacini)
        // ─────────────────────────────────────────────────────────────
        private async Task<Dictionary<int, decimal>> UcitajNaZalihama(bool usluga)
        {
            string filter = usluga ? FilterSamoUsluge : FilterUsluge;
            string sql = $@"
                SELECT a.PrvaKlasifikacijaID AS KlasId, SUM(ml.Kolicina) AS Kolicina
                FROM MagacinLager ml
                JOIN ArtikalInstanca ai ON ml.ArtikalInstancaID = ai.ID
                JOIN Artikal a ON ai.ArtikalID = a.ID
                WHERE a.PrvaKlasifikacijaID IN (6, 10, 11, 15, 28, 34)
                  {filter}
                GROUP BY a.PrvaKlasifikacijaID";

            var rows = await _db.QueryAsync<dynamic>(sql);
            return rows.ToDictionary(r => (int)r.KlasId, r => (decimal)r.Kolicina);
        }

        // ─────────────────────────────────────────────────────────────
        // SASTAVLJANJE TABELE 1
        // ─────────────────────────────────────────────────────────────
        private KalkulacijaStanjeRobeResult SastembiTabelu(
            Dictionary<int, decimal> kupljeno,
            Dictionary<int, decimal> prenosZaliha,
            Dictionary<int, decimal> prenosZalihaSirovine,
            Dictionary<int, decimal> prodato)
        {
            var result = new KalkulacijaStanjeRobeResult();

            foreach (var (klasId, naziv) in VoceGrupe)
            {
                decimal kupljenoKg   = kupljeno.TryGetValue(klasId, out var ku) ? ku : 0m;
                decimal prenosaKg    = prenosZaliha.TryGetValue(klasId, out var pr) ? pr : 0m;
                decimal prenosaSirKg = prenosZalihaSirovine.TryGetValue(klasId, out var prs) ? prs : 0m;
                decimal prodatoKg    = prodato.TryGetValue(klasId, out var po) ? po : 0m;

                if (kupljenoKg == 0 && prenosaKg == 0 && prodatoKg == 0)
                    continue;

                decimal pct        = KaloPreradePct.TryGetValue(klasId, out var p) ? p : 0m;
                decimal kalo       = (kupljenoKg + prenosaSirKg) * pct / 100m;
                decimal naZalihama = kupljenoKg + prenosaKg - prodatoKg - kalo;

                result.Redovi.Add(new KalkulacijaStanjeRobeRed
                {
                    VrstaVoca     = naziv,
                    Kupljeno      = kupljenoKg,
                    PrenosaZaliha = prenosaKg,
                    Prodato       = prodatoKg,
                    Kalo          = kalo,
                    NaZalihama    = naZalihama
                });
            }

            return result;
        }

        // ─────────────────────────────────────────────────────────────
        // KONFIGURACIJA PRENOS ZALIHA — sve voće na lageru
        // ─────────────────────────────────────────────────────────────

        public async Task<List<KalkulacijaLagerPrenosStavka>> UcitajLagerZaPrenosKonfiguraciju(KalkulacijaPeriod period)
        {
            var redovi = await UcitajPrenosZalihaRedoveAsync(
                period, samoSirovine: false, samoUsluga: null, ukljuciProizvodnju: true);
            return redovi;
        }

        // ─────────────────────────────────────────────────────────────
        // OBRAČUN OTKUPA — Tabela 2
        // ─────────────────────────────────────────────────────────────

        public async Task<List<(int ID, string Naziv)>> UcitajDobavljace()
        {
            const string sql = @"
                SELECT DISTINCT vp.KomitentID AS ID, k.Naziv
                FROM vPrometFinansijev9 vp
                INNER JOIN Komitent k ON k.ID = vp.KomitentID
                WHERE vp.Dokument LIKE 'KL-%'
                  AND vp.DokumentStatus NOT IN (2, 4)
                ORDER BY k.Naziv";

            var rows = await _db.QueryAsync<dynamic>(sql);
            return rows.Select(r => ((int)r.ID, (string)r.Naziv)).ToList();
        }

        public async Task<KalkulacijaObracunOtkupaResult> UcitajObracunOtkupa(
            KalkulacijaPeriod period,
            List<int> izabraniDobavljaci,
            Dictionary<int, decimal> pocetnoStanjePoDobaveljacu)
        {
            if (izabraniDobavljaci.Count == 0)
                return new KalkulacijaObracunOtkupaResult();

            try
            {
                var param = new
                {
                    Dobavljaci = izabraniDobavljaci,
                    OdDatum    = period.OdDatum.ToString("yyyy-MM-dd"),
                    DoDatum    = period.DoDatum.ToString("yyyy-MM-dd")
                };

                const string sqlNazivi = @"
                    SELECT ID, Naziv FROM Komitent
                    WHERE ID IN @Dobavljaci
                    ORDER BY Naziv";

                // LEFT JOIN jer IS- dokumenti (isplate) nemaju ArtikalID — INNER JOIN bi ih izbacio
                const string sqlVrednosti = @"
                    SELECT
                        vp.KomitentID,
                        SUM(CASE WHEN vp.Dokument LIKE 'KL-%' THEN COALESCE(vp.Potrazuje, 0) ELSE 0 END) AS VrednostRobe,
                        SUM(CASE WHEN vp.Dokument LIKE 'IS-%' THEN COALESCE(vp.Duguje, 0) ELSE 0 END) AS Isplata
                    FROM vPrometFinansijev9 vp
                    LEFT JOIN Artikal a ON a.ID = vp.ArtikalID
                    WHERE vp.KomitentID IN @Dobavljaci
                      AND vp.DokumentStatus NOT IN (2, 4)
                      AND (vp.Dokument LIKE 'KL-%' OR vp.Dokument LIKE 'IS-%')
                      AND DATE(vp.Datum) >= @OdDatum
                      AND DATE(vp.Datum) <= @DoDatum
                    GROUP BY vp.KomitentID";

                var tNazivi    = _db.QueryAsync<dynamic>(sqlNazivi, new { Dobavljaci = izabraniDobavljaci });
                var tVrednosti = _db.QueryAsync<dynamic>(sqlVrednosti, param);
                await Task.WhenAll(tNazivi, tVrednosti);

                var naziviMapa    = tNazivi.Result.ToDictionary(r => (int)r.ID, r => (string)r.Naziv);
                var vrednostiMapa = tVrednosti.Result.ToDictionary(r => (int)r.KomitentID, r => r);

                var result = new KalkulacijaObracunOtkupaResult();

                foreach (var komitentId in izabraniDobavljaci)
                {
                    string naziv = naziviMapa.TryGetValue(komitentId, out var n) ? n : komitentId.ToString();

                    decimal vrednostRobe = 0m;
                    decimal isplata      = 0m;
                    if (vrednostiMapa.TryGetValue(komitentId, out var v))
                    {
                        vrednostRobe = (decimal)v.VrednostRobe;
                        isplata      = (decimal)v.Isplata;
                    }

                    decimal pocetnoStanje = pocetnoStanjePoDobaveljacu.TryGetValue(komitentId, out var ps) ? ps : 0m;

                    result.Redovi.Add(new KalkulacijaObracunOtkupaRed
                    {
                        KomitentID       = komitentId,
                        Dobavljac        = naziv,
                        VrednostSaMarzom = vrednostRobe,
                        PocetnoStanje    = pocetnoStanje,
                        UkupnaMarza      = 0m,
                        Isplata          = isplata
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajObracunOtkupa");
                throw;
            }
        }

        // ─────────────────────────────────────────────────────────────
        // OBRAČUN POSLOVANJA — Tabela 3
        // ─────────────────────────────────────────────────────────────

        public async Task<KalkulacijaObracunPosloResult> UcitajObracunPoslovanja(
            KalkulacijaPeriod period,
            Dictionary<string, decimal> ceneNabavke,
            Dictionary<string, decimal> cenePrerade,
            List<int>? iskluceniArtikli = null)
        {
            try
            {
                var param = new
                {
                    OdDatum  = period.OdDatum.ToString("yyyy-MM-dd"),
                    DoDatum  = period.DoDatum.ToString("yyyy-MM-dd"),
                    OdSezona = period.OdDatum.ToString("yyyy-MM-dd"),
                    Danas    = DateTime.Today.ToString("yyyy-MM-dd"),
                    OdCena   = period.OdDatum.AddYears(-1).ToString("yyyy-MM-dd")
                };

                // Red 1: Nabavna vrednost sezona — Potrazuje minus stvarni PDV (PorezIznos), bez ALTIVA/FRIKOS
                const string sqlNabSezona = @"
                    SELECT COALESCE(SUM(vp.Potrazuje - COALESCE(vp.PorezIznos, 0)), 0) AS Vrednost
                    FROM vPrometFinansijev9 vp
                    INNER JOIN Artikal a ON a.ID = vp.ArtikalID
                    WHERE vp.Dokument LIKE 'KL-%'
                      AND vp.DokumentStatus NOT IN (2, 4)
                      AND vp.Potrazuje > 0
                      AND a.Naziv NOT LIKE '%-ALTIVA%'
                      AND a.Naziv NOT LIKE '%FRIKOS%'
                      AND vp.ArtikalPrvaKlasifikacijaID IN (6, 10, 11, 15, 28, 34)
                      AND DATE(vp.Datum) >= @OdDatum
                      AND DATE(vp.Datum) <= @DoDatum";

                // Red 4: Prodaja — bez PDV, bez ALTIVA/FRIKOS
                const string sqlProdaja = @"
                    SELECT COALESCE(SUM(
                        CASE WHEN k.Ino = 1 THEN vp.Duguje
                             ELSE vp.Duguje * 10 / 11
                        END), 0) AS Vrednost
                    FROM vPrometFinansijev9 vp
                    INNER JOIN Artikal a ON a.ID = vp.ArtikalID
                    INNER JOIN Komitent k ON k.ID = vp.KomitentID
                    WHERE vp.Dokument LIKE 'FK-%'
                      AND vp.DokumentStatus NOT IN (2, 4)
                      AND vp.Duguje > 0
                      AND a.Naziv NOT LIKE '%-ALTIVA%'
                      AND a.Naziv NOT LIKE '%FRIKOS%'
                      AND vp.ArtikalPrvaKlasifikacijaID IN (6, 10, 11, 15, 28, 34)
                      AND DATE(vp.Datum) >= @OdDatum
                      AND DATE(vp.Datum) <= @DoDatum";

                // Prosečna nabavna cena po vrsti (prethodnih 12 meseci pre sezone), bez PDV
                const string sqlProsecnaCena = @"
                    SELECT
                        vp.ArtikalPrvaKlasifikacijaID AS KlasId,
                        SUM(vp.Potrazuje - COALESCE(vp.PorezIznos, 0)) / SUM(ABS(vp.Kolicina)) AS CenaPoKg
                    FROM vPrometFinansijev9 vp
                    INNER JOIN Artikal a ON a.ID = vp.ArtikalID
                    WHERE vp.Dokument LIKE 'KL-%'
                      AND vp.DokumentStatus NOT IN (2, 4)
                      AND vp.Potrazuje > 0
                      AND ABS(vp.Kolicina) > 0
                      AND a.MagacinID IN (2, 3)
                      AND a.Naziv NOT LIKE '%-ALTIVA%'
                      AND a.Naziv NOT LIKE '%FRIKOS%'
                      AND vp.ArtikalPrvaKlasifikacijaID IN (6, 10, 11, 15, 28, 34)
                      AND DATE(vp.Datum) >= @OdCena
                      AND DATE(vp.Datum) < @OdSezona
                    GROUP BY vp.ArtikalPrvaKlasifikacijaID";

                // Lager za Prenos Zaliha — za Preradu (po vrsti, samo sirovine, name filter)
                const string sqlLager = @"
                    SELECT a.PrvaKlasifikacijaID AS KlasId, SUM(ml.Kolicina) AS Kolicina
                    FROM MagacinLager ml
                    JOIN ArtikalInstanca ai ON ml.ArtikalInstancaID = ai.ID
                    JOIN Artikal a ON ai.ArtikalID = a.ID
                    WHERE a.PrvaKlasifikacijaID IN (6, 10, 11, 15, 28, 34)
                      AND a.MagacinID IN (2, 3)
                      AND a.Naziv NOT LIKE '%-ALTIVA%'
                      AND a.Naziv NOT LIKE '%FRIKOS%'
                    GROUP BY a.PrvaKlasifikacijaID";

                const string sqlPrometZaLager = @"
                    SELECT
                        fm.ArtikalPrvaKlasifikacijaID AS KlasId,
                        SUM(CASE WHEN LEFT(fm.Dokument, 2) = 'PR' THEN COALESCE(fm.Ulaz, 0) ELSE 0 END) AS Ulaz,
                        SUM(CASE WHEN LEFT(fm.Dokument, 2) = 'OT' THEN COALESCE(fm.Izlaz, 0) ELSE 0 END) AS Izlaz
                    FROM vPrometRobav6 fm
                    INNER JOIN Artikal a ON a.ID = fm.ArtikalID
                    WHERE fm.DokumentStatus = 3
                      AND fm.ArtikalPrvaKlasifikacijaID IN (6, 10, 11, 15, 28, 34)
                      AND a.MagacinID IN (2, 3)
                      AND a.Naziv NOT LIKE '%-ALTIVA%'
                      AND a.Naziv NOT LIKE '%FRIKOS%'
                      AND (fm.Ulaz > 0 OR fm.Izlaz > 0)
                      AND fm.Datum >= @OdSezona
                      AND fm.Datum <= @Danas
                    GROUP BY fm.ArtikalPrvaKlasifikacijaID";

                // Per-artikal lager i promet za NabavnaVrednostLager (ALTIVA/FRIKOS + exclusion lista)
                bool imaIskl = iskluceniArtikli?.Count > 0;
                string isklFilter = imaIskl ? "AND a.ID NOT IN @Iskljuceni" : "";

                string sqlLagerArtikl = $@"
                    SELECT a.ID AS ArtikalID, a.PrvaKlasifikacijaID AS KlasId, SUM(ml.Kolicina) AS Kolicina
                    FROM MagacinLager ml
                    JOIN ArtikalInstanca ai ON ml.ArtikalInstancaID = ai.ID
                    JOIN Artikal a ON ai.ArtikalID = a.ID
                    WHERE a.PrvaKlasifikacijaID IN (6, 10, 11, 15, 28, 34)
                      {FilterUsluge}
                      {isklFilter}
                    GROUP BY a.ID, a.PrvaKlasifikacijaID
                    HAVING SUM(ml.Kolicina) > 0";

                string sqlPrometArtikl = $@"
                    SELECT fm.ArtikalID, fm.ArtikalPrvaKlasifikacijaID AS KlasId,
                           SUM(CASE WHEN LEFT(fm.Dokument, 2) = 'PR' THEN COALESCE(fm.Ulaz, 0) ELSE 0 END) AS Ulaz,
                           SUM(CASE WHEN LEFT(fm.Dokument, 2) = 'OT' THEN COALESCE(fm.Izlaz, 0) ELSE 0 END) AS Izlaz
                    FROM vPrometRobav6 fm
                    INNER JOIN Artikal a ON a.ID = fm.ArtikalID
                    WHERE fm.DokumentStatus = 3
                      AND fm.ArtikalPrvaKlasifikacijaID IN (6, 10, 11, 15, 28, 34)
                      AND (fm.Ulaz > 0 OR fm.Izlaz > 0)
                      AND fm.Datum >= @OdSezona
                      AND fm.Datum <= @Danas
                      {FilterUsluge}
                      {isklFilter}
                    GROUP BY fm.ArtikalID, fm.ArtikalPrvaKlasifikacijaID";

                string sqlCeneArtikl = $@"
                    SELECT vp.ArtikalID,
                           SUM(vp.Potrazuje - COALESCE(vp.PorezIznos, 0)) / SUM(ABS(vp.Kolicina)) AS CenaPoKg
                    FROM vPrometFinansijev9 vp
                    INNER JOIN Artikal a ON a.ID = vp.ArtikalID
                    WHERE vp.Dokument LIKE 'KL-%'
                      AND vp.DokumentStatus NOT IN (2, 4)
                      AND vp.Potrazuje > 0
                      AND ABS(vp.Kolicina) > 0
                      AND a.PrvaKlasifikacijaID IN (6, 10, 11, 15, 28, 34)
                      {FilterUsluge}
                      {isklFilter}
                      AND DATE(vp.Datum) >= @OdCena
                      AND DATE(vp.Datum) < @OdSezona
                    GROUP BY vp.ArtikalID";

                // Kupljeno kg (sirovine) za Preradu — ista baza kao Kalo
                string sqlKupljenoKg = $@"
                    SELECT
                        vp.ArtikalPrvaKlasifikacijaID AS KlasId,
                        SUM(ABS(vp.Kolicina)) AS Kolicina
                    FROM vPrometFinansijev9 vp
                    INNER JOIN Artikal a ON a.ID = vp.ArtikalID
                    WHERE vp.Dokument LIKE 'KL-%'
                      AND vp.DokumentStatus NOT IN (2, 4)
                      {FilterMagacinSirovine}
                      {FilterUsluge}
                      AND vp.ArtikalPrvaKlasifikacijaID IN (6, 10, 11, 15, 28, 34)
                      AND vp.Kolicina <> 0
                      AND COALESCE(vp.Potrazuje, 0) > 0
                      AND DATE(vp.Datum) >= @OdDatum
                      AND DATE(vp.Datum) <= @DoDatum
                    GROUP BY vp.ArtikalPrvaKlasifikacijaID";

                var tNabSezona     = _db.QueryAsync<dynamic>(sqlNabSezona, param);
                var tProdaja       = _db.QueryAsync<dynamic>(sqlProdaja, param);
                var tProsecnaCena  = _db.QueryAsync<dynamic>(sqlProsecnaCena, param);
                var tLager         = _db.QueryAsync<dynamic>(sqlLager);
                var tPrometZaLager = _db.QueryAsync<dynamic>(sqlPrometZaLager, param);
                var tKupljenoKg    = _db.QueryAsync<dynamic>(sqlKupljenoKg, param);

                Task<IEnumerable<dynamic>> tLagerArt, tPrometArt, tCeneArt;
                if (imaIskl)
                {
                    var pArt  = new { OdSezona = param.OdSezona, Danas = param.Danas, OdCena = param.OdCena, Iskljuceni = iskluceniArtikli! };
                    var pArtL = new { Iskljuceni = iskluceniArtikli! };
                    tLagerArt  = _db.QueryAsync<dynamic>(sqlLagerArtikl,  pArtL);
                    tPrometArt = _db.QueryAsync<dynamic>(sqlPrometArtikl, pArt);
                    tCeneArt   = _db.QueryAsync<dynamic>(sqlCeneArtikl,   pArt);
                }
                else
                {
                    var pArt = new { OdSezona = param.OdSezona, Danas = param.Danas, OdCena = param.OdCena };
                    tLagerArt  = _db.QueryAsync<dynamic>(sqlLagerArtikl);
                    tPrometArt = _db.QueryAsync<dynamic>(sqlPrometArtikl, pArt);
                    tCeneArt   = _db.QueryAsync<dynamic>(sqlCeneArtikl,   pArt);
                }

                await Task.WhenAll(tNabSezona, tProdaja, tProsecnaCena, tLager, tPrometZaLager, tKupljenoKg,
                                   tLagerArt, tPrometArt, tCeneArt);

                decimal nabavnaVrednostSezona = (decimal)(tNabSezona.Result.FirstOrDefault()?.Vrednost ?? 0m);
                decimal prodaja = (decimal)(tProdaja.Result.FirstOrDefault()?.Vrednost ?? 0m);

                var prosecnaCenaMapa = tProsecnaCena.Result.ToDictionary(
                    r => (int)r.KlasId, r => (decimal)r.CenaPoKg);

                // Prenos zaliha sirovine (samo MagacinID 2,3)
                var trenutnoStanje = tLager.Result.ToDictionary(r => (int)r.KlasId, r => (decimal)r.Kolicina);
                var netoPromet = tPrometZaLager.Result.ToDictionary(
                    r => (int)r.KlasId,
                    r => (decimal)r.Ulaz - (decimal)r.Izlaz);

                var prenosaZalihaKg = new Dictionary<int, decimal>();
                foreach (var klasId in PraceneKlasifikacije)
                {
                    decimal trenutno = trenutnoStanje.TryGetValue(klasId, out var t) ? t : 0m;
                    decimal neto     = netoPromet.TryGetValue(klasId, out var np) ? np : 0m;
                    prenosaZalihaKg[klasId] = Math.Max(0, trenutno - neto);
                }

                var kupljenoKgMapa = tKupljenoKg.Result.ToDictionary(
                    r => (int)r.KlasId, r => (decimal)r.Kolicina);

                var result = new KalkulacijaObracunPosloResult
                {
                    NabavnaVrednostSezona = nabavnaVrednostSezona,
                    Prodaja = prodaja
                };

                // Red 2: Zalihe Vrednost — per-artikal, koristi config cene po ArtikalID
                var prometArtMapa = tPrometArt.Result.ToDictionary(
                    r => (int)r.ArtikalID,
                    r => (decimal)r.Ulaz - (decimal)r.Izlaz);
                var ceneArtMapa = tCeneArt.Result.ToDictionary(
                    r => (int)r.ArtikalID, r => (decimal)r.CenaPoKg);

                // Akumulatori po vrsti voća za LagerDetalji prikaz
                var lagerPoVrsti = new Dictionary<int, (decimal Kg, decimal Vrednost, bool HasRucna)>();
                decimal nabavnaVrednostLager = 0m;

                foreach (var r in tLagerArt.Result)
                {
                    int artId  = (int)r.ArtikalID;
                    int klasId = (int)r.KlasId;
                    decimal lagerKg  = (decimal)r.Kolicina;
                    decimal netoMov  = prometArtMapa.TryGetValue(artId, out var nm) ? nm : 0m;
                    decimal prenosaKg = Math.Max(0m, lagerKg - netoMov);
                    if (prenosaKg == 0m) continue;

                    bool isRucna  = false;
                    decimal cena  = 0m;
                    if (ceneNabavke.TryGetValue(artId.ToString(), out var cr) && cr > 0)
                    {
                        cena = cr; isRucna = true;
                    }
                    else if (ceneArtMapa.TryGetValue(artId, out var ca) && ca > 0)
                    {
                        cena = ca;
                    }

                    decimal vrednost = prenosaKg * cena;
                    nabavnaVrednostLager += vrednost;

                    if (lagerPoVrsti.TryGetValue(klasId, out var ex))
                        lagerPoVrsti[klasId] = (ex.Kg + prenosaKg, ex.Vrednost + vrednost, ex.HasRucna || isRucna);
                    else
                        lagerPoVrsti[klasId] = (prenosaKg, vrednost, isRucna);
                }

                foreach (var (klasId, d) in lagerPoVrsti)
                {
                    string vrsta   = VoceGrupe.TryGetValue(klasId, out var vn) ? vn : klasId.ToString();
                    decimal avgCena = d.Kg > 0 ? d.Vrednost / d.Kg : 0m;
                    result.LagerDetalji.Add(new KalkulacijaNabavnaVrednostLagerRed
                    {
                        VrstaVoca   = vrsta,
                        Kg          = d.Kg,
                        CenaPoKg    = avgCena,
                        CenaJeRucna = d.HasRucna
                    });
                }
                result.NabavnaVrednostLager = nabavnaVrednostLager;

                // Red 3: Prerada — ista baza kao Kalo: (Kupljeno_sirovine + PrenosaZaliha_sirovine) × cena
                decimal ukupnoPrerada = 0m;
                foreach (var klasId in PraceneKlasifikacije)
                {
                    decimal kgKupljeno = kupljenoKgMapa.TryGetValue(klasId, out var kkg) ? kkg : 0m;
                    decimal kgPrenosS  = prenosaZalihaKg.TryGetValue(klasId, out var pkz) ? pkz : 0m;
                    decimal kg = kgKupljeno + kgPrenosS;
                    if (kg == 0m) continue;

                    string vrsta = VoceGrupe.TryGetValue(klasId, out var pvn) ? pvn : klasId.ToString();
                    if (!cenePrerade.TryGetValue(vrsta, out var cenaP) || cenaP <= 0) continue;

                    decimal vrednost = kg * cenaP;
                    ukupnoPrerada += vrednost;
                    result.PreradeDetalji.Add(new KalkulacijaPreradeRed
                    {
                        VrstaVoca   = vrsta,
                        KgSirovine  = kg,
                        CenaPrerade = cenaP
                    });
                }
                result.Prerada = ukupnoPrerada;

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajObracunPoslovanja");
                throw;
            }
        }

        // ─────────────────────────────────────────────────────────────
        // KONFIGURACIJA — artikli na lageru sa prosečnom nabavnom cenom
        // ─────────────────────────────────────────────────────────────

        public async Task<List<KalkulacijaArtikalNaCeniRed>> UcitajArtikleZaCeneNabavke(
            KalkulacijaPeriod period, List<int> iskluceniArtikli)
        {
            bool imaIskljucenih = iskluceniArtikli.Count > 0;
            string iskljuceniFilter = imaIskljucenih ? "AND a.ID NOT IN @Iskljuceni" : "";

            string sqlLager = $@"
                SELECT
                    a.ID AS ArtikalID,
                    a.Naziv,
                    a.PrvaKlasifikacijaID AS KlasId,
                    a.MagacinID,
                    SUM(ml.Kolicina) AS KgNaLageru
                FROM MagacinLager ml
                JOIN ArtikalInstanca ai ON ml.ArtikalInstancaID = ai.ID
                JOIN Artikal a ON ai.ArtikalID = a.ID
                WHERE a.PrvaKlasifikacijaID IN (6, 10, 11, 15, 28, 34)
                  {iskljuceniFilter}
                GROUP BY a.ID, a.Naziv, a.PrvaKlasifikacijaID, a.MagacinID
                HAVING SUM(ml.Kolicina) > 0
                ORDER BY a.PrvaKlasifikacijaID, a.Naziv";

            string sqlPromet = $@"
                SELECT fm.ArtikalID,
                       SUM(CASE WHEN LEFT(fm.Dokument, 2) = 'PR' THEN COALESCE(fm.Ulaz, 0) ELSE 0 END) AS Ulaz,
                       SUM(CASE WHEN LEFT(fm.Dokument, 2) = 'OT' THEN COALESCE(fm.Izlaz, 0) ELSE 0 END) AS Izlaz
                FROM vPrometRobav6 fm
                INNER JOIN Artikal a ON a.ID = fm.ArtikalID
                WHERE fm.DokumentStatus = 3
                  AND fm.ArtikalPrvaKlasifikacijaID IN (6, 10, 11, 15, 28, 34)
                  AND (fm.Ulaz > 0 OR fm.Izlaz > 0)
                  AND fm.Datum >= @OdSezona
                  AND fm.Datum <= @Danas
                  {iskljuceniFilter}
                GROUP BY fm.ArtikalID";

            string sqlCene = $@"
                SELECT
                    vp.ArtikalID,
                    SUM(vp.Potrazuje - COALESCE(vp.PorezIznos, 0)) / SUM(ABS(vp.Kolicina)) AS CenaPoKg
                FROM vPrometFinansijev9 vp
                INNER JOIN Artikal a ON a.ID = vp.ArtikalID
                WHERE vp.Dokument LIKE 'KL-%'
                  AND vp.DokumentStatus NOT IN (2, 4)
                  AND vp.Potrazuje > 0
                  AND ABS(vp.Kolicina) > 0
                  AND a.PrvaKlasifikacijaID IN (6, 10, 11, 15, 28, 34)
                  {iskljuceniFilter}
                  AND DATE(vp.Datum) >= @OdCena
                  AND DATE(vp.Datum) < @OdSezona
                GROUP BY vp.ArtikalID";

            Task<IEnumerable<dynamic>> tLager, tPromet, tCene;
            if (imaIskljucenih)
            {
                var pBase = new { OdSezona = period.OdDatum.ToString("yyyy-MM-dd"), Danas = DateTime.Today.ToString("yyyy-MM-dd"), OdCena = period.OdDatum.AddYears(-1).ToString("yyyy-MM-dd"), Iskljuceni = iskluceniArtikli };
                tLager  = _db.QueryAsync<dynamic>(sqlLager,  new { Iskljuceni = iskluceniArtikli });
                tPromet = _db.QueryAsync<dynamic>(sqlPromet, pBase);
                tCene   = _db.QueryAsync<dynamic>(sqlCene,   pBase);
            }
            else
            {
                var pBase = new { OdSezona = period.OdDatum.ToString("yyyy-MM-dd"), Danas = DateTime.Today.ToString("yyyy-MM-dd"), OdCena = period.OdDatum.AddYears(-1).ToString("yyyy-MM-dd") };
                tLager  = _db.QueryAsync<dynamic>(sqlLager);
                tPromet = _db.QueryAsync<dynamic>(sqlPromet, pBase);
                tCene   = _db.QueryAsync<dynamic>(sqlCene,   pBase);
            }

            await Task.WhenAll(tLager, tPromet, tCene);

            var prometMapa = tPromet.Result.ToDictionary(
                r => (int)r.ArtikalID,
                r => (decimal)r.Ulaz - (decimal)r.Izlaz);
            var ceneMapa = tCene.Result.ToDictionary(r => (int)r.ArtikalID, r => (decimal)r.CenaPoKg);

            var rezultat = new List<KalkulacijaArtikalNaCeniRed>();
            foreach (var r in tLager.Result)
            {
                int artId   = (int)r.ArtikalID;
                decimal lagerKg  = (decimal)r.KgNaLageru;
                decimal netoMov  = prometMapa.TryGetValue(artId, out var nm) ? nm : 0m;
                decimal prenosaKg = Math.Max(0m, lagerKg - netoMov);
                if (prenosaKg == 0m) continue;

                int klasId = (int)r.KlasId;
                int magId  = (int)r.MagacinID;
                string magNaziv = magId switch
                {
                    2 => "Sveza Roba",
                    3 => "Sirovine",
                    5 => "Poluproizvodi",
                    6 => "Gotova Roba",
                    _ => $"Magacin {magId}"
                };
                rezultat.Add(new KalkulacijaArtikalNaCeniRed
                {
                    ArtikalID     = artId,
                    Naziv         = (string)r.Naziv,
                    VrstaVoca     = VoceGrupe.TryGetValue(klasId, out var vn) ? vn : klasId.ToString(),
                    NazivMagacina = magNaziv,
                    KgNaLageru    = prenosaKg,
                    CenaIzBaze    = ceneMapa.TryGetValue(artId, out var c) ? c : 0m,
                    CenaRucna     = 0m
                });
            }
            return rezultat;
        }

        // ─────────────────────────────────────────────────────────────
        // GOTOVA ROBA ZA DROPDOWN — Konfiguracija Faza 4
        // ─────────────────────────────────────────────────────────────

        public async Task<Dictionary<int, List<(int ID, string Naziv)>>> UcitajArtikleZaFormulu()
        {
            const string sql = @"
                SELECT a.ID, a.Naziv, a.PrvaKlasifikacijaID AS KlasId
                FROM Artikal a
                WHERE a.PrvaKlasifikacijaID IN (6, 10, 11, 15, 28, 34)
                  AND a.Naziv NOT LIKE '%-ALTIVA%'
                  AND a.Naziv NOT LIKE '%FRIKOS%'
                ORDER BY a.PrvaKlasifikacijaID, a.Naziv";

            var rows = await _db.QueryAsync<dynamic>(sql);
            return rows
                .GroupBy(r => (int)r.KlasId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(r => ((int)r.ID, (string)r.Naziv)).ToList());
        }

        // ─────────────────────────────────────────────────────────────
        // VREDNOST ROBE NA ZALIHAMA — Tabela 3, Red 5
        // ─────────────────────────────────────────────────────────────

        public async Task<KalkulacijaVrednostZalihaResult> UcitajVrednostZaliha(
            Dictionary<string, List<FormulaStavka>> formula,
            Dictionary<string, decimal> prodajneCene)
        {
            if (formula.Count == 0)
                return new KalkulacijaVrednostZalihaResult();

            try
            {
                // Sirovine na lageru (bez ALTIVA/FRIKOS)
                const string sqlSirovine = @"
                    SELECT a.PrvaKlasifikacijaID AS KlasId, SUM(ml.Kolicina) AS Kolicina
                    FROM MagacinLager ml
                    JOIN ArtikalInstanca ai ON ml.ArtikalInstancaID = ai.ID
                    JOIN Artikal a ON ai.ArtikalID = a.ID
                    WHERE a.MagacinID IN (2, 3)
                      AND a.PrvaKlasifikacijaID IN (6, 10, 11, 15, 28, 34)
                      AND a.Naziv NOT LIKE '%-ALTIVA%'
                      AND a.Naziv NOT LIKE '%FRIKOS%'
                      AND ml.Kolicina > 0
                    GROUP BY a.PrvaKlasifikacijaID";

                // Gotova roba na lageru (bez ALTIVA/FRIKOS)
                const string sqlGotovaRoba = @"
                    SELECT a.ID AS ArtikalID, a.Naziv, SUM(ml.Kolicina) AS Kolicina
                    FROM MagacinLager ml
                    JOIN ArtikalInstanca ai ON ml.ArtikalInstancaID = ai.ID
                    JOIN Artikal a ON ai.ArtikalID = a.ID
                    WHERE a.MagacinID = 6
                      AND a.Naziv NOT LIKE '%-ALTIVA%'
                      AND a.Naziv NOT LIKE '%FRIKOS%'
                      AND ml.Kolicina > 0
                    GROUP BY a.ID, a.Naziv";

                var tSirovine   = _db.QueryAsync<dynamic>(sqlSirovine);
                var tGotovaRoba = _db.QueryAsync<dynamic>(sqlGotovaRoba);
                await Task.WhenAll(tSirovine, tGotovaRoba);

                var sirovineLager = tSirovine.Result.ToDictionary(r => (int)r.KlasId, r => (decimal)r.Kolicina);
                var gotovaRobaLager = tGotovaRoba.Result.ToDictionary(r => (int)r.ArtikalID, r => (decimal)r.Kolicina);

                var rezultati = new Dictionary<int, KalkulacijaVrednostZalihaRed>();

                foreach (var (vrstaVoca, stavke) in formula)
                {
                    int klasId = VoceGrupe.FirstOrDefault(kv => kv.Value == vrstaVoca).Key;
                    if (klasId == 0) continue;

                    decimal sirovinaKg = sirovineLager.TryGetValue(klasId, out var sk) ? sk : 0m;
                    if (sirovinaKg == 0m) continue;

                    foreach (var stavka in stavke)
                    {
                        if (stavka.ArtikalID == 0 || stavka.Procenat <= 0) continue;

                        decimal kgIzSirovine = sirovinaKg * stavka.Procenat / 100m;

                        if (!rezultati.TryGetValue(stavka.ArtikalID, out var red))
                        {
                            red = new KalkulacijaVrednostZalihaRed
                            {
                                ArtikalID    = stavka.ArtikalID,
                                NazivGotovog = stavka.Naziv,
                                VrstaVoca    = vrstaVoca,
                                CenaPoKg     = prodajneCene.TryGetValue(stavka.ArtikalID.ToString(), out var cp) ? cp : 0m
                            };
                            rezultati[stavka.ArtikalID] = red;
                        }
                        red.KgIzSirovine += kgIzSirovine;
                    }
                }

                foreach (var (artikalId, kg) in gotovaRobaLager)
                {
                    if (rezultati.TryGetValue(artikalId, out var red))
                    {
                        red.KgDirektno += kg;
                    }
                    else if (prodajneCene.TryGetValue(artikalId.ToString(), out var cena) && cena > 0)
                    {
                        var naziv = tGotovaRoba.Result
                            .Where(r => (int)r.ArtikalID == artikalId)
                            .Select(r => (string)r.Naziv)
                            .FirstOrDefault() ?? artikalId.ToString();

                        rezultati[artikalId] = new KalkulacijaVrednostZalihaRed
                        {
                            ArtikalID    = artikalId,
                            NazivGotovog = naziv,
                            VrstaVoca    = string.Empty,
                            KgDirektno   = kg,
                            CenaPoKg     = cena
                        };
                    }
                }

                var result = new KalkulacijaVrednostZalihaResult();
                result.Redovi.AddRange(
                    rezultati.Values
                        .OrderBy(r => r.VrstaVoca)
                        .ThenBy(r => r.NazivGotovog));
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajVrednostZaliha");
                throw;
            }
        }
    }
}
