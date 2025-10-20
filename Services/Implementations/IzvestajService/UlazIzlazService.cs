using FruitSysWeb.Models;
using FruitSysWeb.Services.Interfaces;
using FruitSysWeb.Services.Models.Requests;
using FruitSysWeb.Services.Core;
using System.Text;
using Microsoft.Extensions.Logging;

namespace FruitSysWeb.Services.Implementations.IzvestajService
{
    public class UlazIzlazService : BaseService, IUlazIzlazService
    {
        private readonly DatabaseService _databaseService;
        private readonly ILogger<UlazIzlazService> _logger;

        public UlazIzlazService(DatabaseService databaseService,
            ILogger<UlazIzlazService> logger)
        {
            _databaseService = databaseService;
            _logger = logger;
        }

        #region Faktura metode

        public async Task<List<FakturaModel>> UcitajSveFakture(FilterRequest filterRequest)
        {
            try
            {
                var sql = CreateSqlBuilder();
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

                var parameters = CreateParameters();

                // Apply date and komitent filters using BaseService
                ApplyDateFilter(sql, parameters, filterRequest, "f.Datum");
                ApplyKomitentFilter(sql, parameters, filterRequest, "f.KomitentID");

                sql.Append(" ORDER BY f.Datum DESC");

                return (await _databaseService.QueryAsync<FakturaModel>(sql.ToString(), parameters)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajSveFakture");
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
                _logger.LogError(ex, "Greška u UcitajFakturuPoId");
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
                _logger.LogError(ex, "Greška u UcitajFakturePoKomitentu");
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
                _logger.LogError(ex, "Greška u UcitajFakturePoDatumu");
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
                _logger.LogError(ex, "Greška u UcitajFakturePoStatusu");
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
                var sql = CreateSqlBuilder();
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

                var parameters = CreateParameters();

                // Apply date filter using BaseService
                ApplyDateFilter(sql, parameters, filterRequest, "ol.Datum");

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
                _logger.LogError(ex, "Greška u UcitajSveOtkupneListove");
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
                _logger.LogError(ex, "Greška u UcitajOtkupniListPoId");
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
                _logger.LogError(ex, "Greška u UcitajOtkupneListovePoKomitentu");
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
                _logger.LogError(ex, "Greška u UcitajOtkupneListovePoDatumu");
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
                _logger.LogError(ex, "Greška u UcitajOtkupneListovePoStatusu");
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
                _logger.LogError(ex, "Greška u UcitajOtkupneListovePoOtkupnomMestu");
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
                _logger.LogError(ex, "Greška u UcitajIsplaceneOtkupneListove");
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
                _logger.LogError(ex, "Greška u UcitajNeisplaceneOtkupneListove");
                return new List<OtkupniListModel>();
            }
        }

        #endregion

        #region Prijemnica metode

        public async Task<List<PrijemnicaModel>> UcitajSvePrijemnice(FilterRequest filterRequest)
        {
            try
            {
                var sql = CreateSqlBuilder();
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

                var parameters = CreateParameters();

                // Apply date filter using BaseService
                ApplyDateFilter(sql, parameters, filterRequest, "p.Datum");

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
                _logger.LogError(ex, "Greška u UcitajSvePrijemnice");
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
                _logger.LogError(ex, "Greška u UcitajPrijemnicuPoId");
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
                _logger.LogError(ex, "Greška u UcitajPrijemnicePoKomitentu");
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
                _logger.LogError(ex, "Greška u UcitajPrijemnicePoDatumu");
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
                _logger.LogError(ex, "Greška u UcitajPrijemnicePoStatusu");
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
                _logger.LogError(ex, "Greška u UcitajPrijemnicePoMagacinu");
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
                _logger.LogError(ex, "Greška u UcitajPrijemnicePoTipuPrijema");
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
                _logger.LogError(ex, "Greška u UcitajPrijemniceZaKontrolu");
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
                _logger.LogError(ex, "Greška u UcitajReklamiranePrijemnice");
                return new List<PrijemnicaModel>();
            }
        }

        #endregion

        #region Otpremnica metode

        public async Task<List<OtpremnicaModel>> UcitajSveOtpremnice(FilterRequest filterRequest)
        {
            try
            {
                var sql = CreateSqlBuilder();
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

                var parameters = CreateParameters();

                // Apply date filter using BaseService
                ApplyDateFilter(sql, parameters, filterRequest, "o.Datum");

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
                _logger.LogError(ex, "Greška u UcitajSveOtpremnice");
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
                _logger.LogError(ex, "Greška u UcitajOtpremnicuPoId");
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
                _logger.LogError(ex, "Greška u UcitajOtpremnicePoKomitentu");
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
                _logger.LogError(ex, "Greška u UcitajOtpremnicePoDatumu");
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
                _logger.LogError(ex, "Greška u UcitajOtpremnicePoStatusu");
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
                _logger.LogError(ex, "Greška u UcitajOtpremnicePoMagacinu");
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
                _logger.LogError(ex, "Greška u UcitajOtpremnicePoTipu");
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
                _logger.LogError(ex, "Greška u UcitajIzvozneOtpremnice");
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
                _logger.LogError(ex, "Greška u UcitajTranzitneOtpremnice");
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
                _logger.LogError(ex, "Greška u UcitajStatistikuPoKomitentima");
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
                _logger.LogError(ex, "Greška u UcitajStatistikuPoMagacinima");
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
                _logger.LogError(ex, "Greška u UcitajStatistikuPoStatusima");
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
                _logger.LogError(ex, "Greška u UcitajStatistikuPoMesecima");
                return new Dictionary<string, decimal>();
            }
        }

        public async Task<decimal> UcitajUkupnuVrednostFaktura(FilterRequest filterRequest)
        {
            try
            {
                var sql = CreateSqlBuilder();
                sql.Append(@"
                    SELECT COALESCE(SUM(f.Bruto), 0) as UkupnaVrednost
                    FROM Faktura f
                    WHERE f.Aktivno = 1
                      AND f.DokumentStatus = 3
                ");

                var parameters = CreateParameters();

                // Apply date filter using BaseService
                ApplyDateFilter(sql, parameters, filterRequest, "f.Datum");

                var rezultat = await _databaseService.ExecuteScalarAsync<decimal>(sql.ToString(), parameters);
                return rezultat;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajUkupnuVrednostFaktura");
                return 0;
            }
        }

        public async Task<decimal> UcitajUkupnuVrednostOtkupnihListova(FilterRequest filterRequest)
        {
            try
            {
                var sql = CreateSqlBuilder();
                sql.Append(@"
                    SELECT COALESCE(SUM(ol.IznosUkupno), 0) as UkupnaVrednost
                    FROM OtkupniList ol
                    WHERE ol.Aktivno = 1
                      AND ol.DokumentStatus = 3
                ");

                var parameters = CreateParameters();

                // Apply date filter using BaseService
                ApplyDateFilter(sql, parameters, filterRequest, "ol.Datum");

                var rezultat = await _databaseService.ExecuteScalarAsync<decimal>(sql.ToString(), parameters);
                return rezultat;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajUkupnuVrednostOtkupnihListova");
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
                _logger.LogError(ex, "Greška u UcitajUkupanBrojDokumenata");
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
                _logger.LogError(ex, "Greška u UcitajTopKomitentePoVrednosti");
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
                _logger.LogError(ex, "Greška u UcitajTopMagacinePoKolicini");
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
                _logger.LogError(ex, "Greška u UcitajUkupnuKolicinu");
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
                _logger.LogError(ex, "Greška u UcitajUkupnuVrednost");
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
                _logger.LogError(ex, "Greška u UcitajBrojOtvorenihDokumenata");
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
                _logger.LogError(ex, "Greška u UcitajBrojZakljucenihDokumenata");
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
                _logger.LogError(ex, "Greška u UcitajBrojStornoDokumenata");
                return 0;
            }
        }

        #endregion
    }
}
