using FruitSysWeb.Models;
using FruitSysWeb.Services.Interfaces;
using FruitSysWeb.Services.Models.Requests;
using System.Text;

namespace FruitSysWeb.Services.Implementations.IzvestajService
{
    public class UlazIzlazService : IUlazIzlazService
    {
        private readonly DatabaseService _databaseService;

        public UlazIzlazService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        #region Faktura metode

        public async Task<List<FakturaModel>> UcitajSveFakture(FilterRequest filterRequest)
        {
            try
            {
                var sql = new StringBuilder();
                sql.Append(@"
                    SELECT 
                        f.ID,
                        f.Sifra,
                        f.Datum,
                        f.Neto,
                        f.Bruto,
                        f.Porez,
                        f.NetoEur,
                        f.BrutoEur,
                        f.PorezEur,
                        f.Kreirano,
                        f.Azurirano,
                        f.Version,
                        f.KomitentID,
                        k.Naziv as Komitent,
                        f.OtpremnicaID,
                        f.UgovorID,
                        f.KursEur,
                        f.DokumentStatus,
                        f.Aktivno
                    FROM Faktura f
                    LEFT JOIN Komitent k ON f.KomitentID = k.ID
                    WHERE f.Aktivno = 1");

                if (filterRequest.OdDatum.HasValue)
                {
                    sql.Append(" AND f.Datum >= @odDatum");
                }

                if (filterRequest.DoDatum.HasValue)
                {
                    sql.Append(" AND f.Datum <= @doDatum");
                }

                if (filterRequest.KomitentId.HasValue)
                {
                    sql.Append(" AND f.KomitentID = @komitentId");
                }

                sql.Append(" ORDER BY f.Datum DESC");

                var parameters = new
                {
                    odDatum = filterRequest.OdDatum,
                    doDatum = filterRequest.DoDatum,
                    komitentId = filterRequest.KomitentId
                };

                return (await _databaseService.QueryAsync<FakturaModel>(sql.ToString(), parameters)).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajSveFakture: {ex.Message}");
                return new List<FakturaModel>();
            }
        }

        public async Task<FakturaModel?> UcitajFakturuPoId(long id)
        {
            try
            {
                var sql = @"
                    SELECT 
                        f.ID,
                        f.Sifra,
                        f.Datum,
                        f.Neto,
                        f.Bruto,
                        f.Porez,
                        f.NetoEur,
                        f.BrutoEur,
                        f.PorezEur,
                        f.Kreirano,
                        f.Azurirano,
                        f.Version,
                        f.KomitentID,
                        k.Naziv as Komitent,
                        f.OtpremnicaID,
                        f.UgovorID,
                        f.KursEur,
                        f.DokumentStatus,
                        f.Aktivno
                    FROM Faktura f
                    LEFT JOIN Komitent k ON f.KomitentID = k.ID
                    WHERE f.ID = @id";

                var parameters = new { id };
                return await _databaseService.QueryFirstOrDefaultAsync<FakturaModel>(sql, parameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajFakturuPoId: {ex.Message}");
                return null;
            }
        }

        public async Task<List<FakturaModel>> UcitajFakturePoKomitentu(long komitentId)
        {
            try
            {
                var sql = @"
                    SELECT 
                        f.ID,
                        f.Sifra,
                        f.Datum,
                        f.Neto,
                        f.Bruto,
                        f.Porez,
                        f.NetoEur,
                        f.BrutoEur,
                        f.PorezEur,
                        f.Kreirano,
                        f.Azurirano,
                        f.Version,
                        f.KomitentID,
                        k.Naziv as Komitent,
                        f.OtpremnicaID,
                        f.UgovorID,
                        f.KursEur,
                        f.DokumentStatus,
                        f.Aktivno
                    FROM Faktura f
                    LEFT JOIN Komitent k ON f.KomitentID = k.ID
                    WHERE f.KomitentID = @komitentId AND f.Aktivno = 1
                    ORDER BY f.Datum DESC";

                var parameters = new { komitentId };
                return (await _databaseService.QueryAsync<FakturaModel>(sql, parameters)).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajFakturePoKomitentu: {ex.Message}");
                return new List<FakturaModel>();
            }
        }

        public async Task<List<FakturaModel>> UcitajFakturePoDatumu(DateTime odDatum, DateTime doDatum)
        {
            try
            {
                var sql = @"
                    SELECT 
                        f.ID,
                        f.Sifra,
                        f.Datum,
                        f.Neto,
                        f.Bruto,
                        f.Porez,
                        f.NetoEur,
                        f.BrutoEur,
                        f.PorezEur,
                        f.Kreirano,
                        f.Azurirano,
                        f.Version,
                        f.KomitentID,
                        k.Naziv as Komitent,
                        f.OtpremnicaID,
                        f.UgovorID,
                        f.KursEur,
                        f.DokumentStatus,
                        f.Aktivno
                    FROM Faktura f
                    LEFT JOIN Komitent k ON f.KomitentID = k.ID
                    WHERE f.Datum BETWEEN @odDatum AND @doDatum AND f.Aktivno = 1
                    ORDER BY f.Datum DESC";

                var parameters = new { odDatum, doDatum };
                return (await _databaseService.QueryAsync<FakturaModel>(sql, parameters)).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajFakturePoDatumu: {ex.Message}");
                return new List<FakturaModel>();
            }
        }

        public async Task<List<FakturaModel>> UcitajFakturePoStatusu(int status)
        {
            try
            {
                var sql = @"
                    SELECT 
                        f.ID,
                        f.Sifra,
                        f.Datum,
                        f.Neto,
                        f.Bruto,
                        f.Porez,
                        f.NetoEur,
                        f.BrutoEur,
                        f.PorezEur,
                        f.Kreirano,
                        f.Azurirano,
                        f.Version,
                        f.KomitentID,
                        k.Naziv as Komitent,
                        f.OtpremnicaID,
                        f.UgovorID,
                        f.KursEur,
                        f.DokumentStatus,
                        f.Aktivno
                    FROM Faktura f
                    LEFT JOIN Komitent k ON f.KomitentID = k.ID
                    WHERE f.DokumentStatus = @status AND f.Aktivno = 1
                    ORDER BY f.Datum DESC";

                var parameters = new { status };
                return (await _databaseService.QueryAsync<FakturaModel>(sql, parameters)).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajFakturePoStatusu: {ex.Message}");
                return new List<FakturaModel>();
            }
        }

        public async Task<List<FakturaModel>> UcitajOtvoreneFakture()
        {
            return await UcitajFakturePoStatusu(2);
        }

        public async Task<List<FakturaModel>> UcitajZakljuceneFakture()
        {
            return await UcitajFakturePoStatusu(3);
        }

        public async Task<List<FakturaModel>> UcitajStornoFakture()
        {
            return await UcitajFakturePoStatusu(4);
        }

        #endregion

        #region OtkupniList metode

        public async Task<List<OtkupniListModel>> UcitajSveOtkupneListove(FilterRequest filterRequest)
        {
            try
            {
                var sql = new StringBuilder();
                sql.Append(@"
                    SELECT 
                        ol.ID,
                        ol.Sifra,
                        ol.Datum,
                        ol.KomitentID,
                        k.Naziv as Komitent,
                        ol.IznosOsnovice,
                        ol.StopaPDV,
                        ol.IznosPDV,
                        ol.IznosUkupno,
                        ol.DokumentStatus,
                        ol.Aktivno,
                        ol.Kreirano,
                        ol.Azurirano
                    FROM OtkupniList ol
                    LEFT JOIN Komitent k ON ol.KomitentID = k.ID
                    WHERE ol.Aktivno = 1
                ");

                var parameters = new Dictionary<string, object>();

                if (filterRequest.OdDatum.HasValue)
                {
                    sql.Append(" AND DATE(ol.Datum) >= @OdDatum");
                    parameters.Add("@OdDatum", filterRequest.OdDatum.Value.Date);
                }

                if (filterRequest.DoDatum.HasValue)
                {
                    sql.Append(" AND DATE(ol.Datum) <= @DoDatum");
                    parameters.Add("@DoDatum", filterRequest.DoDatum.Value.Date);
                }

                if (filterRequest.KomitentId.HasValue && filterRequest.KomitentId > 0)
                {
                    sql.Append(" AND ol.KomitentID = @KomitentId");
                    parameters.Add("@KomitentId", filterRequest.KomitentId.Value);
                }

                if (filterRequest.DokumentStatus.HasValue)
                {
                    sql.Append(" AND ol.DokumentStatus = @Status");
                    parameters.Add("@Status", filterRequest.DokumentStatus.Value);
                }

                sql.Append(" ORDER BY ol.Datum DESC, ol.Sifra");

                var rezultat = await _databaseService.QueryAsync<OtkupniListModel>(sql.ToString(), parameters);
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajSveOtkupneListove: {ex.Message}");
                return new List<OtkupniListModel>();
            }
        }

        public async Task<OtkupniListModel?> UcitajOtkupniListPoId(long id)
        {
            try
            {
                var sql = @"
                    SELECT 
                        ol.ID,
                        ol.Sifra,
                        ol.Datum,
                        ol.KomitentID,
                        k.Naziv as Komitent,
                        ol.IznosOsnovice,
                        ol.StopaPDV,
                        ol.IznosPDV,
                        ol.IznosUkupno,
                        ol.DokumentStatus,
                        ol.Aktivno
                    FROM OtkupniList ol
                    LEFT JOIN Komitent k ON ol.KomitentID = k.ID
                    WHERE ol.ID = @Id
                ";

                var rezultat = await _databaseService.QueryFirstOrDefaultAsync<OtkupniListModel>(sql, new { Id = id });
                return rezultat;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajOtkupniListPoId: {ex.Message}");
                return null;
            }
        }

        public async Task<List<OtkupniListModel>> UcitajOtkupneListovePoKomitentu(long komitentId)
        {
            try
            {
                var sql = @"
                    SELECT 
                        ol.ID,
                        ol.Sifra,
                        ol.Datum,
                        ol.KomitentID,
                        k.Naziv as Komitent,
                        ol.IznosOsnovice,
                        ol.StopaPDV,
                        ol.IznosPDV,
                        ol.IznosUkupno,
                        ol.DokumentStatus,
                        ol.Aktivno
                    FROM OtkupniList ol
                    LEFT JOIN Komitent k ON ol.KomitentID = k.ID
                    WHERE ol.KomitentID = @KomitentId AND ol.Aktivno = 1
                    ORDER BY ol.Datum DESC
                ";

                var rezultat = await _databaseService.QueryAsync<OtkupniListModel>(sql, new { KomitentId = komitentId });
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajOtkupneListovePoKomitentu: {ex.Message}");
                return new List<OtkupniListModel>();
            }
        }

        public async Task<List<OtkupniListModel>> UcitajOtkupneListovePoDatumu(DateTime odDatum, DateTime doDatum)
        {
            try
            {
                var sql = @"
                    SELECT 
                        ol.ID,
                        ol.Sifra,
                        ol.Datum,
                        ol.KomitentID,
                        k.Naziv as Komitent,
                        ol.IznosOsnovice,
                        ol.StopaPDV,
                        ol.IznosPDV,
                        ol.IznosUkupno,
                        ol.DokumentStatus,
                        ol.Aktivno
                    FROM OtkupniList ol
                    LEFT JOIN Komitent k ON ol.KomitentID = k.ID
                    WHERE ol.Datum BETWEEN @OdDatum AND @DoDatum AND ol.Aktivno = 1
                    ORDER BY ol.Datum DESC
                ";

                var rezultat = await _databaseService.QueryAsync<OtkupniListModel>(sql, new { OdDatum = odDatum, DoDatum = doDatum });
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajOtkupneListovePoDatumu: {ex.Message}");
                return new List<OtkupniListModel>();
            }
        }

        public async Task<List<OtkupniListModel>> UcitajOtkupneListovePoStatusu(int status)
        {
            try
            {
                var sql = @"
                    SELECT 
                        ol.ID,
                        ol.Sifra,
                        ol.Datum,
                        ol.KomitentID,
                        k.Naziv as Komitent,
                        ol.IznosOsnovice,
                        ol.StopaPDV,
                        ol.IznosPDV,
                        ol.IznosUkupno,
                        ol.DokumentStatus,
                        ol.Aktivno
                    FROM OtkupniList ol
                    LEFT JOIN Komitent k ON ol.KomitentID = k.ID
                    WHERE ol.DokumentStatus = @Status AND ol.Aktivno = 1
                    ORDER BY ol.Datum DESC
                ";

                var rezultat = await _databaseService.QueryAsync<OtkupniListModel>(sql, new { Status = status });
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajOtkupneListovePoStatusu: {ex.Message}");
                return new List<OtkupniListModel>();
            }
        }

        public async Task<List<OtkupniListModel>> UcitajOtkupneListovePoOtkupnomMestu(long otkupnoMestoId)
        {
            try
            {
                var sql = @"
                    SELECT 
                        ol.ID,
                        ol.Sifra,
                        ol.Datum,
                        ol.KomitentID,
                        k.Naziv as Komitent,
                        ol.IznosOsnovice,
                        ol.StopaPDV,
                        ol.IznosPDV,
                        ol.IznosUkupno,
                        ol.DokumentStatus,
                        ol.Aktivno
                    FROM OtkupniList ol
                    LEFT JOIN Komitent k ON ol.KomitentID = k.ID
                    WHERE ol.OtkupnoMestoID = @OtkupnoMestoId AND ol.Aktivno = 1
                    ORDER BY ol.Datum DESC
                ";

                var rezultat = await _databaseService.QueryAsync<OtkupniListModel>(sql, new { OtkupnoMestoId = otkupnoMestoId });
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajOtkupneListovePoOtkupnomMestu: {ex.Message}");
                return new List<OtkupniListModel>();
            }
        }

        public async Task<List<OtkupniListModel>> UcitajIsplaceneOtkupneListove()
        {
            try
            {
                var sql = @"
                    SELECT 
                        ol.ID,
                        ol.Sifra,
                        ol.Datum,
                        ol.KomitentID,
                        k.Naziv as Komitent,
                        ol.IznosOsnovice,
                        ol.StopaPDV,
                        ol.IznosPDV,
                        ol.IznosUkupno,
                        ol.DokumentStatus,
                        ol.Aktivno
                    FROM OtkupniList ol
                    LEFT JOIN Komitent k ON ol.KomitentID = k.ID
                    WHERE ol.Isplaceno = 1 AND ol.Aktivno = 1
                    ORDER BY ol.Datum DESC
                ";

                var rezultat = await _databaseService.QueryAsync<OtkupniListModel>(sql);
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajIsplaceneOtkupneListove: {ex.Message}");
                return new List<OtkupniListModel>();
            }
        }

        public async Task<List<OtkupniListModel>> UcitajNeisplaceneOtkupneListove()
        {
            try
            {
                var sql = @"
                    SELECT 
                        ol.ID,
                        ol.Sifra,
                        ol.Datum,
                        ol.KomitentID,
                        k.Naziv as Komitent,
                        ol.IznosOsnovice,
                        ol.StopaPDV,
                        ol.IznosPDV,
                        ol.IznosUkupno,
                        ol.DokumentStatus,
                        ol.Aktivno
                    FROM OtkupniList ol
                    LEFT JOIN Komitent k ON ol.KomitentID = k.ID
                    WHERE (ol.Isplaceno = 0 OR ol.Isplaceno IS NULL) AND ol.Aktivno = 1
                    ORDER BY ol.Datum DESC
                ";

                var rezultat = await _databaseService.QueryAsync<OtkupniListModel>(sql);
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajNeisplaceneOtkupneListove: {ex.Message}");
                return new List<OtkupniListModel>();
            }
        }

        #endregion

        #region Prijemnica metode

        public async Task<List<PrijemnicaModel>> UcitajSvePrijemnice(FilterRequest filterRequest)
        {
            try
            {
                var sql = new StringBuilder();
                sql.Append(@"
                    SELECT 
                        p.ID,
                        p.Sifra,
                        p.Datum,
                        p.KomitentID,
                        k.Naziv as Komitent,
                        p.MagacinID,
                        p.Kolicina,
                        p.DokumentStatus,
                        p.Aktivno
                    FROM Prijemnica p
                    LEFT JOIN Komitent k ON p.KomitentID = k.ID
                    WHERE p.Aktivno = 1
                ");

                var parameters = new Dictionary<string, object>();

                if (filterRequest.OdDatum.HasValue)
                {
                    sql.Append(" AND DATE(p.Datum) >= @OdDatum");
                    parameters.Add("@OdDatum", filterRequest.OdDatum.Value.Date);
                }

                if (filterRequest.DoDatum.HasValue)
                {
                    sql.Append(" AND DATE(p.Datum) <= @DoDatum");
                    parameters.Add("@DoDatum", filterRequest.DoDatum.Value.Date);
                }

                if (filterRequest.KomitentId.HasValue && filterRequest.KomitentId > 0)
                {
                    sql.Append(" AND p.KomitentID = @KomitentId");
                    parameters.Add("@KomitentId", filterRequest.KomitentId.Value);
                }

                if (filterRequest.DokumentStatus.HasValue)
                {
                    sql.Append(" AND p.DokumentStatus = @Status");
                    parameters.Add("@Status", filterRequest.DokumentStatus.Value);
                }

                sql.Append(" ORDER BY p.Datum DESC, p.Sifra");

                var rezultat = await _databaseService.QueryAsync<PrijemnicaModel>(sql.ToString(), parameters);
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajSvePrijemnice: {ex.Message}");
                return new List<PrijemnicaModel>();
            }
        }

        public async Task<PrijemnicaModel?> UcitajPrijemnicuPoId(long id)
        {
            try
            {
                var sql = @"
                    SELECT 
                        p.ID,
                        p.Sifra,
                        p.Datum,
                        p.KomitentID,
                        k.Naziv as Komitent,
                        p.MagacinID,
                        p.Kolicina,
                        p.DokumentStatus,
                        p.Aktivno
                    FROM Prijemnica p
                    LEFT JOIN Komitent k ON p.KomitentID = k.ID
                    WHERE p.ID = @Id
                ";

                var rezultat = await _databaseService.QueryFirstOrDefaultAsync<PrijemnicaModel>(sql, new { Id = id });
                return rezultat;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajPrijemnicuPoId: {ex.Message}");
                return null;
            }
        }

        public async Task<List<PrijemnicaModel>> UcitajPrijemnicePoKomitentu(long komitentId)
        {
            try
            {
                var sql = @"
                    SELECT 
                        p.ID,
                        p.Sifra,
                        p.Datum,
                        p.KomitentID,
                        k.Naziv as Komitent,
                        p.MagacinID,
                        p.Kolicina,
                        p.DokumentStatus,
                        p.Aktivno
                    FROM Prijemnica p
                    LEFT JOIN Komitent k ON p.KomitentID = k.ID
                    WHERE p.KomitentID = @KomitentId AND p.Aktivno = 1
                    ORDER BY p.Datum DESC
                ";

                var rezultat = await _databaseService.QueryAsync<PrijemnicaModel>(sql, new { KomitentId = komitentId });
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajPrijemnicePoKomitentu: {ex.Message}");
                return new List<PrijemnicaModel>();
            }
        }

        public async Task<List<PrijemnicaModel>> UcitajPrijemnicePoDatumu(DateTime odDatum, DateTime doDatum)
        {
            try
            {
                var sql = @"
                    SELECT 
                        p.ID,
                        p.Sifra,
                        p.Datum,
                        p.KomitentID,
                        k.Naziv as Komitent,
                        p.MagacinID,
                        p.Kolicina,
                        p.DokumentStatus,
                        p.Aktivno
                    FROM Prijemnica p
                    LEFT JOIN Komitent k ON p.KomitentID = k.ID
                    WHERE p.Datum BETWEEN @OdDatum AND @DoDatum AND p.Aktivno = 1
                    ORDER BY p.Datum DESC
                ";

                var rezultat = await _databaseService.QueryAsync<PrijemnicaModel>(sql, new { OdDatum = odDatum, DoDatum = doDatum });
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajPrijemnicePoDatumu: {ex.Message}");
                return new List<PrijemnicaModel>();
            }
        }

        public async Task<List<PrijemnicaModel>> UcitajPrijemnicePoStatusu(int status)
        {
            try
            {
                var sql = @"
                    SELECT 
                        p.ID,
                        p.Sifra,
                        p.Datum,
                        p.KomitentID,
                        k.Naziv as Komitent,
                        p.MagacinID,
                        p.Kolicina,
                        p.DokumentStatus,
                        p.Aktivno
                    FROM Prijemnica p
                    LEFT JOIN Komitent k ON p.KomitentID = k.ID
                    WHERE p.DokumentStatus = @Status AND p.Aktivno = 1
                    ORDER BY p.Datum DESC
                ";

                var rezultat = await _databaseService.QueryAsync<PrijemnicaModel>(sql, new { Status = status });
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajPrijemnicePoStatusu: {ex.Message}");
                return new List<PrijemnicaModel>();
            }
        }

        public async Task<List<PrijemnicaModel>> UcitajPrijemnicePoMagacinu(long magacinId)
        {
            try
            {
                var sql = @"
                    SELECT 
                        p.ID,
                        p.Sifra,
                        p.Datum,
                        p.KomitentID,
                        k.Naziv as Komitent,
                        p.MagacinID,
                        p.Kolicina,
                        p.DokumentStatus,
                        p.Aktivno
                    FROM Prijemnica p
                    LEFT JOIN Komitent k ON p.KomitentID = k.ID
                    WHERE p.MagacinID = @MagacinId AND p.Aktivno = 1
                    ORDER BY p.Datum DESC
                ";

                var rezultat = await _databaseService.QueryAsync<PrijemnicaModel>(sql, new { MagacinId = magacinId });
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajPrijemnicePoMagacinu: {ex.Message}");
                return new List<PrijemnicaModel>();
            }
        }

        public async Task<List<PrijemnicaModel>> UcitajPrijemnicePoTipuPrijema(int tipPrijema)
        {
            try
            {
                var sql = @"
                    SELECT 
                        p.ID,
                        p.Sifra,
                        p.Datum,
                        p.KomitentID,
                        k.Naziv as Komitent,
                        p.MagacinID,
                        p.Kolicina,
                        p.DokumentStatus,
                        p.Aktivno
                    FROM Prijemnica p
                    LEFT JOIN Komitent k ON p.KomitentID = k.ID
                    WHERE p.TipPrijema = @TipPrijema AND p.Aktivno = 1
                    ORDER BY p.Datum DESC
                ";

                var rezultat = await _databaseService.QueryAsync<PrijemnicaModel>(sql, new { TipPrijema = tipPrijema });
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajPrijemnicePoTipuPrijema: {ex.Message}");
                return new List<PrijemnicaModel>();
            }
        }

        public async Task<List<PrijemnicaModel>> UcitajPrijemniceZaKontrolu()
        {
            try
            {
                var sql = @"
                    SELECT 
                        p.ID,
                        p.Sifra,
                        p.Datum,
                        p.KomitentID,
                        k.Naziv as Komitent,
                        p.MagacinID,
                        p.Kolicina,
                        p.DokumentStatus,
                        p.Aktivno
                    FROM Prijemnica p
                    LEFT JOIN Komitent k ON p.KomitentID = k.ID
                    WHERE p.ZaKontrolu = 1 AND p.Aktivno = 1
                    ORDER BY p.Datum DESC
                ";

                var rezultat = await _databaseService.QueryAsync<PrijemnicaModel>(sql);
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajPrijemniceZaKontrolu: {ex.Message}");
                return new List<PrijemnicaModel>();
            }
        }

        public async Task<List<PrijemnicaModel>> UcitajReklamiranePrijemnice()
        {
            try
            {
                var sql = @"
                    SELECT 
                        p.ID,
                        p.Sifra,
                        p.Datum,
                        p.KomitentID,
                        k.Naziv as Komitent,
                        p.MagacinID,
                        p.Kolicina,
                        p.DokumentStatus,
                        p.Aktivno
                    FROM Prijemnica p
                    LEFT JOIN Komitent k ON p.KomitentID = k.ID
                    WHERE p.Reklamirano = 1 AND p.Aktivno = 1
                    ORDER BY p.Datum DESC
                ";

                var rezultat = await _databaseService.QueryAsync<PrijemnicaModel>(sql);
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajReklamiranePrijemnice: {ex.Message}");
                return new List<PrijemnicaModel>();
            }
        }

        #endregion

        #region Otpremnica metode

        public async Task<List<OtpremnicaModel>> UcitajSveOtpremnice(FilterRequest filterRequest)
        {
            try
            {
                var sql = new StringBuilder();
                sql.Append(@"
                    SELECT 
                        o.ID,
                        o.Sifra,
                        o.Datum,
                        o.KomitentID,
                        k.Naziv as Komitent,
                        o.MagacinID,
                        o.Kolicina,
                        o.DokumentStatus,
                        o.Aktivno
                    FROM Otpremnica o
                    LEFT JOIN Komitent k ON o.KomitentID = k.ID
                    WHERE o.Aktivno = 1
                ");

                var parameters = new Dictionary<string, object>();

                if (filterRequest.OdDatum.HasValue)
                {
                    sql.Append(" AND DATE(o.Datum) >= @OdDatum");
                    parameters.Add("@OdDatum", filterRequest.OdDatum.Value.Date);
                }

                if (filterRequest.DoDatum.HasValue)
                {
                    sql.Append(" AND DATE(o.Datum) <= @DoDatum");
                    parameters.Add("@DoDatum", filterRequest.DoDatum.Value.Date);
                }

                if (filterRequest.KomitentId.HasValue && filterRequest.KomitentId > 0)
                {
                    sql.Append(" AND o.KomitentID = @KomitentId");
                    parameters.Add("@KomitentId", filterRequest.KomitentId.Value);
                }

                if (filterRequest.DokumentStatus.HasValue)
                {
                    sql.Append(" AND o.DokumentStatus = @Status");
                    parameters.Add("@Status", filterRequest.DokumentStatus.Value);
                }

                sql.Append(" ORDER BY o.Datum DESC, o.Sifra");

                var rezultat = await _databaseService.QueryAsync<OtpremnicaModel>(sql.ToString(), parameters);
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajSveOtpremnice: {ex.Message}");
                return new List<OtpremnicaModel>();
            }
        }

        public async Task<OtpremnicaModel?> UcitajOtpremnicuPoId(long id)
        {
            try
            {
                var sql = @"
                    SELECT 
                        o.ID,
                        o.Sifra,
                        o.Datum,
                        o.KomitentID,
                        k.Naziv as Komitent,
                        o.MagacinID,
                        o.Kolicina,
                        o.DokumentStatus,
                        o.Aktivno
                    FROM Otpremnica o
                    LEFT JOIN Komitent k ON o.KomitentID = k.ID
                    WHERE o.ID = @Id
                ";

                var rezultat = await _databaseService.QueryFirstOrDefaultAsync<OtpremnicaModel>(sql, new { Id = id });
                return rezultat;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajOtpremnicuPoId: {ex.Message}");
                return null;
            }
        }

        public async Task<List<OtpremnicaModel>> UcitajOtpremnicePoKomitentu(long komitentId)
        {
            try
            {
                var sql = @"
                    SELECT 
                        o.ID,
                        o.Sifra,
                        o.Datum,
                        o.KomitentID,
                        k.Naziv as Komitent,
                        o.MagacinID,
                        o.Kolicina,
                        o.DokumentStatus,
                        o.Aktivno
                    FROM Otpremnica o
                    LEFT JOIN Komitent k ON o.KomitentID = k.ID
                    WHERE o.KomitentID = @KomitentId AND o.Aktivno = 1
                    ORDER BY o.Datum DESC
                ";

                var rezultat = await _databaseService.QueryAsync<OtpremnicaModel>(sql, new { KomitentId = komitentId });
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajOtpremnicePoKomitentu: {ex.Message}");
                return new List<OtpremnicaModel>();
            }
        }

        public async Task<List<OtpremnicaModel>> UcitajOtpremnicePoDatumu(DateTime odDatum, DateTime doDatum)
        {
            try
            {
                var sql = @"
                    SELECT 
                        o.ID,
                        o.Sifra,
                        o.Datum,
                        o.KomitentID,
                        k.Naziv as Komitent,
                        o.MagacinID,
                        o.Kolicina,
                        o.DokumentStatus,
                        o.Aktivno
                    FROM Otpremnica o
                    LEFT JOIN Komitent k ON o.KomitentID = k.ID
                    WHERE o.Datum BETWEEN @OdDatum AND @DoDatum AND o.Aktivno = 1
                    ORDER BY o.Datum DESC
                ";

                var rezultat = await _databaseService.QueryAsync<OtpremnicaModel>(sql, new { OdDatum = odDatum, DoDatum = doDatum });
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajOtpremnicePoDatumu: {ex.Message}");
                return new List<OtpremnicaModel>();
            }
        }

        public async Task<List<OtpremnicaModel>> UcitajOtpremnicePoStatusu(int status)
        {
            try
            {
                var sql = @"
                    SELECT 
                        o.ID,
                        o.Sifra,
                        o.Datum,
                        o.KomitentID,
                        k.Naziv as Komitent,
                        o.MagacinID,
                        o.Kolicina,
                        o.DokumentStatus,
                        o.Aktivno
                    FROM Otpremnica o
                    LEFT JOIN Komitent k ON o.KomitentID = k.ID
                    WHERE o.DokumentStatus = @Status AND o.Aktivno = 1
                    ORDER BY o.Datum DESC
                ";

                var rezultat = await _databaseService.QueryAsync<OtpremnicaModel>(sql, new { Status = status });
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajOtpremnicePoStatusu: {ex.Message}");
                return new List<OtpremnicaModel>();
            }
        }

        public async Task<List<OtpremnicaModel>> UcitajOtpremnicePoMagacinu(long magacinId)
        {
            try
            {
                var sql = @"
                    SELECT 
                        o.ID,
                        o.Sifra,
                        o.Datum,
                        o.KomitentID,
                        k.Naziv as Komitent,
                        o.MagacinID,
                        o.Kolicina,
                        o.DokumentStatus,
                        o.Aktivno
                    FROM Otpremnica o
                    LEFT JOIN Komitent k ON o.KomitentID = k.ID
                    WHERE o.MagacinID = @MagacinId AND o.Aktivno = 1
                    ORDER BY o.Datum DESC
                ";

                var rezultat = await _databaseService.QueryAsync<OtpremnicaModel>(sql, new { MagacinId = magacinId });
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajOtpremnicePoMagacinu: {ex.Message}");
                return new List<OtpremnicaModel>();
            }
        }

        public async Task<List<OtpremnicaModel>> UcitajOtpremnicePoTipu(int tipOtpreme)
        {
            try
            {
                var sql = @"
                    SELECT 
                        o.ID,
                        o.Sifra,
                        o.Datum,
                        o.KomitentID,
                        k.Naziv as Komitent,
                        o.MagacinID,
                        o.Kolicina,
                        o.DokumentStatus,
                        o.Aktivno
                    FROM Otpremnica o
                    LEFT JOIN Komitent k ON o.KomitentID = k.ID
                    WHERE o.TipOtpreme = @TipOtpreme AND o.Aktivno = 1
                    ORDER BY o.Datum DESC
                ";

                var rezultat = await _databaseService.QueryAsync<OtpremnicaModel>(sql, new { TipOtpreme = tipOtpreme });
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajOtpremnicePoTipu: {ex.Message}");
                return new List<OtpremnicaModel>();
            }
        }

        public async Task<List<OtpremnicaModel>> UcitajIzvozneOtpremnice()
        {
            try
            {
                var sql = @"
                    SELECT 
                        o.ID,
                        o.Sifra,
                        o.Datum,
                        o.KomitentID,
                        k.Naziv as Komitent,
                        o.MagacinID,
                        o.Kolicina,
                        o.DokumentStatus,
                        o.Aktivno
                    FROM Otpremnica o
                    LEFT JOIN Komitent k ON o.KomitentID = k.ID
                    WHERE o.Izvoz = 1 AND o.Aktivno = 1
                    ORDER BY o.Datum DESC
                ";

                var rezultat = await _databaseService.QueryAsync<OtpremnicaModel>(sql);
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajIzvozneOtpremnice: {ex.Message}");
                return new List<OtpremnicaModel>();
            }
        }

        public async Task<List<OtpremnicaModel>> UcitajTranzitneOtpremnice()
        {
            try
            {
                var sql = @"
                    SELECT 
                        o.ID,
                        o.Sifra,
                        o.Datum,
                        o.KomitentID,
                        k.Naziv as Komitent,
                        o.MagacinID,
                        o.Kolicina,
                        o.DokumentStatus,
                        o.Aktivno
                    FROM Otpremnica o
                    LEFT JOIN Komitent k ON o.KomitentID = k.ID
                    WHERE o.Tranzit = 1 AND o.Aktivno = 1
                    ORDER BY o.Datum DESC
                ";

                var rezultat = await _databaseService.QueryAsync<OtpremnicaModel>(sql);
                return rezultat.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajTranzitneOtpremnice: {ex.Message}");
                return new List<OtpremnicaModel>();
            }
        }

        #endregion

        #region Statistike metode

        public async Task<Dictionary<string, decimal>> UcitajStatistikuPoKomitentima(FilterRequest filterRequest)
        {
            try
            {
                var sql = @"
                    SELECT 
                        k.Naziv as Komitent,
                        SUM(f.Bruto) as UkupnaVrednost
                    FROM Faktura f
                    LEFT JOIN Komitent k ON f.KomitentID = k.ID
                    WHERE f.Aktivno = 1
                      AND f.DokumentStatus = 3
                    GROUP BY k.ID, k.Naziv
                    ORDER BY UkupnaVrednost DESC
                    LIMIT 10
                ";

                var rezultat = await _databaseService.QueryAsync<dynamic>(sql);

                return rezultat.ToDictionary(
                    x => (string)x.Komitent ?? "Nepoznato",
                    x => (decimal)x.UkupnaVrednost
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajStatistikuPoKomitentima: {ex.Message}");
                return new Dictionary<string, decimal>();
            }
        }

        public async Task<Dictionary<string, decimal>> UcitajStatistikuPoMagacinima(FilterRequest filterRequest)
        {
            try
            {
                var sql = @"
                    SELECT 
                        m.Naziv as Magacin,
                        SUM(p.Kolicina) as UkupnaKolicina
                    FROM Prijemnica p
                    LEFT JOIN Magacin m ON p.MagacinID = m.ID
                    WHERE p.Aktivno = 1
                      AND p.DokumentStatus = 3
                    GROUP BY m.ID, m.Naziv
                    ORDER BY UkupnaKolicina DESC
                ";

                var rezultat = await _databaseService.QueryAsync<dynamic>(sql);

                return rezultat.ToDictionary(
                    x => (string)x.Magacin ?? "Nepoznato",
                    x => (decimal)x.UkupnaKolicina
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajStatistikuPoMagacinima: {ex.Message}");
                return new Dictionary<string, decimal>();
            }
        }

        public async Task<Dictionary<string, decimal>> UcitajStatistikuPoStatusima(FilterRequest filterRequest)
        {
            try
            {
                var sql = @"
                    SELECT 
                        CASE 
                            WHEN DokumentStatus = 2 THEN 'Otvoren'
                            WHEN DokumentStatus = 3 THEN 'Zaključen'
                            WHEN DokumentStatus = 4 THEN 'Storno'
                            ELSE 'Nepoznato'
                        END as Status,
                        COUNT(*) as BrojDokumenata
                    FROM Faktura
                    WHERE Aktivno = 1
                    GROUP BY DokumentStatus
                ";

                var rezultat = await _databaseService.QueryAsync<dynamic>(sql);

                return rezultat.ToDictionary(
                    x => (string)x.Status,
                    x => (decimal)(int)x.BrojDokumenata
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajStatistikuPoStatusima: {ex.Message}");
                return new Dictionary<string, decimal>();
            }
        }

        public async Task<Dictionary<string, decimal>> UcitajStatistikuPoMesecima(FilterRequest filterRequest)
        {
            try
            {
                var sql = @"
                    SELECT 
                        DATE_FORMAT(Datum, '%Y-%m') as Mesec,
                        SUM(Bruto) as UkupnaVrednost
                    FROM Faktura
                    WHERE Aktivno = 1
                      AND DokumentStatus = 3
                      AND Datum >= DATE_SUB(NOW(), INTERVAL 12 MONTH)
                    GROUP BY DATE_FORMAT(Datum, '%Y-%m')
                    ORDER BY Mesec
                ";

                var rezultat = await _databaseService.QueryAsync<dynamic>(sql);

                return rezultat.ToDictionary(
                    x => (string)x.Mesec,
                    x => (decimal)x.UkupnaVrednost
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajStatistikuPoMesecima: {ex.Message}");
                return new Dictionary<string, decimal>();
            }
        }

        public async Task<decimal> UcitajUkupnuVrednostFaktura(FilterRequest filterRequest)
        {
            try
            {
                var sql = new StringBuilder();
                sql.Append(@"
                    SELECT COALESCE(SUM(f.Bruto), 0) as UkupnaVrednost
                    FROM Faktura f
                    WHERE f.Aktivno = 1
                      AND f.DokumentStatus = 3
                ");

                var parameters = new Dictionary<string, object>();

                if (filterRequest.OdDatum.HasValue)
                {
                    sql.Append(" AND DATE(f.Datum) >= @OdDatum");
                    parameters.Add("@OdDatum", filterRequest.OdDatum.Value.Date);
                }

                if (filterRequest.DoDatum.HasValue)
                {
                    sql.Append(" AND DATE(f.Datum) <= @DoDatum");
                    parameters.Add("@DoDatum", filterRequest.DoDatum.Value.Date);
                }

                var rezultat = await _databaseService.ExecuteScalarAsync<decimal>(sql.ToString(), parameters);
                return rezultat;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajUkupnuVrednostFaktura: {ex.Message}");
                return 0;
            }
        }

        public async Task<decimal> UcitajUkupnuVrednostOtkupnihListova(FilterRequest filterRequest)
        {
            try
            {
                var sql = new StringBuilder();
                sql.Append(@"
                    SELECT COALESCE(SUM(ol.IznosUkupno), 0) as UkupnaVrednost
                    FROM OtkupniList ol
                    WHERE ol.Aktivno = 1
                      AND ol.DokumentStatus = 3
                ");

                var parameters = new Dictionary<string, object>();

                if (filterRequest.OdDatum.HasValue)
                {
                    sql.Append(" AND DATE(ol.Datum) >= @OdDatum");
                    parameters.Add("@OdDatum", filterRequest.OdDatum.Value.Date);
                }

                if (filterRequest.DoDatum.HasValue)
                {
                    sql.Append(" AND DATE(ol.Datum) <= @DoDatum");
                    parameters.Add("@DoDatum", filterRequest.DoDatum.Value.Date);
                }

                var rezultat = await _databaseService.ExecuteScalarAsync<decimal>(sql.ToString(), parameters);
                return rezultat;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajUkupnuVrednostOtkupnihListova: {ex.Message}");
                return 0;
            }
        }

        public async Task<int> UcitajUkupanBrojDokumenata(FilterRequest filterRequest)
        {
            try
            {
                var sql = @"
                    SELECT COUNT(*) as BrojDokumenata
                    FROM (
                        SELECT ID FROM Faktura WHERE Aktivno = 1
                        UNION ALL
                        SELECT ID FROM OtkupniList WHERE Aktivno = 1
                        UNION ALL
                        SELECT ID FROM Prijemnica WHERE Aktivno = 1
                        UNION ALL
                        SELECT ID FROM Otpremnica WHERE Aktivno = 1
                    ) AS AllDokumenti
                ";

                var rezultat = await _databaseService.ExecuteScalarAsync<int>(sql);
                return rezultat;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajUkupanBrojDokumenata: {ex.Message}");
                return 0;
            }
        }

        public async Task<Dictionary<string, decimal>> UcitajTopKomitentePoVrednosti(FilterRequest filterRequest)
        {
            try
            {
                var sql = @"
                    SELECT 
                        k.Naziv as Komitent,
                        SUM(f.Bruto) as UkupnaVrednost
                    FROM Faktura f
                    LEFT JOIN Komitent k ON f.KomitentID = k.ID
                    WHERE f.Aktivno = 1
                      AND f.DokumentStatus = 3
                    GROUP BY k.ID, k.Naziv
                    ORDER BY UkupnaVrednost DESC
                    LIMIT 10
                ";

                var rezultat = await _databaseService.QueryAsync<dynamic>(sql);

                return rezultat.ToDictionary(
                    x => (string)x.Komitent ?? "Nepoznato",
                    x => (decimal)x.UkupnaVrednost
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajTopKomitentePoVrednosti: {ex.Message}");
                return new Dictionary<string, decimal>();
            }
        }

        public async Task<Dictionary<string, decimal>> UcitajTopMagacinePoKolicini(FilterRequest filterRequest)
        {
            try
            {
                var sql = @"
                    SELECT 
                        m.Naziv as Magacin,
                        SUM(ml.Kolicina) as UkupnaKolicina
                    FROM vwMagacinLager ml
                    LEFT JOIN Artikal a ON ml.ArtikalID = a.ID
                    LEFT JOIN Magacin m ON a.MagacinID = m.ID
                    WHERE ml.Kolicina > 0
                    GROUP BY m.ID, m.Naziv
                    ORDER BY UkupnaKolicina DESC
                    LIMIT 10
                ";

                var rezultat = await _databaseService.QueryAsync<dynamic>(sql);

                return rezultat.ToDictionary(
                    x => (string)x.Magacin ?? "Nepoznato",
                    x => (decimal)x.UkupnaKolicina
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajTopMagacinePoKolicini: {ex.Message}");
                return new Dictionary<string, decimal>();
            }
        }

        public async Task<decimal> UcitajUkupnuKolicinu(FilterRequest filterRequest)
        {
            try
            {
                var sql = @"
                    SELECT COALESCE(SUM(Kolicina), 0) as UkupnaKolicina
                    FROM vwMagacinLager
                    WHERE Kolicina > 0
                ";

                var rezultat = await _databaseService.ExecuteScalarAsync<decimal>(sql);
                return rezultat;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajUkupnuKolicinu: {ex.Message}");
                return 0;
            }
        }

        public async Task<decimal> UcitajUkupnuVrednost(FilterRequest filterRequest)
        {
            try
            {
                var sql = @"
                    SELECT COALESCE(SUM(f.Bruto + ol.IznosUkupno), 0) as UkupnaVrednost
                    FROM (
                        SELECT SUM(Bruto) as Bruto FROM Faktura WHERE Aktivno = 1 AND DokumentStatus = 3
                    ) f,
                    (
                        SELECT SUM(IznosUkupno) as IznosUkupno FROM OtkupniList WHERE Aktivno = 1 AND DokumentStatus = 3
                    ) ol
                ";

                var rezultat = await _databaseService.ExecuteScalarAsync<decimal>(sql);
                return rezultat;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajUkupnuVrednost: {ex.Message}");
                return 0;
            }
        }

        public async Task<int> UcitajBrojOtvorenihDokumenata()
        {
            try
            {
                var sql = @"
                    SELECT COUNT(*) as BrojDokumenata
                    FROM (
                        SELECT ID FROM Faktura WHERE Aktivno = 1 AND DokumentStatus = 2
                        UNION ALL
                        SELECT ID FROM OtkupniList WHERE Aktivno = 1 AND DokumentStatus = 2
                        UNION ALL
                        SELECT ID FROM Prijemnica WHERE Aktivno = 1 AND DokumentStatus = 2
                        UNION ALL
                        SELECT ID FROM Otpremnica WHERE Aktivno = 1 AND DokumentStatus = 2
                    ) AS OtvoreniDokumenti
                ";

                var rezultat = await _databaseService.ExecuteScalarAsync<int>(sql);
                return rezultat;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajBrojOtvorenihDokumenata: {ex.Message}");
                return 0;
            }
        }

        public async Task<int> UcitajBrojZakljucenihDokumenata()
        {
            try
            {
                var sql = @"
                    SELECT COUNT(*) as BrojDokumenata
                    FROM (
                        SELECT ID FROM Faktura WHERE Aktivno = 1 AND DokumentStatus = 3
                        UNION ALL
                        SELECT ID FROM OtkupniList WHERE Aktivno = 1 AND DokumentStatus = 3
                        UNION ALL
                        SELECT ID FROM Prijemnica WHERE Aktivno = 1 AND DokumentStatus = 3
                        UNION ALL
                        SELECT ID FROM Otpremnica WHERE Aktivno = 1 AND DokumentStatus = 3
                    ) AS ZakljuceniDokumenti
                ";

                var rezultat = await _databaseService.ExecuteScalarAsync<int>(sql);
                return rezultat;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajBrojZakljucenihDokumenata: {ex.Message}");
                return 0;
            }
        }

        public async Task<int> UcitajBrojStornoDokumenata()
        {
            try
            {
                var sql = @"
                    SELECT COUNT(*) as BrojDokumenata
                    FROM (
                        SELECT ID FROM Faktura WHERE Aktivno = 1 AND DokumentStatus = 4
                        UNION ALL
                        SELECT ID FROM OtkupniList WHERE Aktivno = 1 AND DokumentStatus = 4
                        UNION ALL
                        SELECT ID FROM Prijemnica WHERE Aktivno = 1 AND DokumentStatus = 4
                        UNION ALL
                        SELECT ID FROM Otpremnica WHERE Aktivno = 1 AND DokumentStatus = 4
                    ) AS StornoDokumenti
                ";

                var rezultat = await _databaseService.ExecuteScalarAsync<int>(sql);
                return rezultat;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u UcitajBrojStornoDokumenata: {ex.Message}");
                return 0;
            }
        }

        #endregion
    }
}
