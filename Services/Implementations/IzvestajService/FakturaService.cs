using FruitSysWeb.Models;
using FruitSysWeb.Services.Interfaces;
using FruitSysWeb.Services.Models.Requests;
using FruitSysWeb.Services.Core;
using System.Text;
using Microsoft.Extensions.Logging;

namespace FruitSysWeb.Services.Implementations.IzvestajService
{
    public class FakturaService : BaseService, IFakturaService
    {
        private readonly DatabaseService _databaseService;
        private readonly ILogger<FakturaService> _logger;

        public FakturaService(DatabaseService databaseService, ILogger<FakturaService> logger)
        {
            _databaseService = databaseService;
            _logger = logger;
        }

        public async Task<List<FakturaModel>> UcitajFakture(FilterRequest filterRequest, bool? samoIno = null)
        {
            try
            {
                var sql = CreateSqlBuilder(@"
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
                    WHERE f.Aktivno = 1
                      AND f.DokumentStatus != 4");

                var parameters = CreateParameters();

                ApplyDateFilter(sql, parameters, filterRequest, "f.Datum");
                ApplyKomitentFilter(sql, parameters, filterRequest, "f.KomitentID");

                if (filterRequest.DokumentStatus.HasValue && filterRequest.DokumentStatus > 0)
                {
                    sql.Append(" AND f.DokumentStatus = @DokumentStatus");
                    parameters.Add("@DokumentStatus", filterRequest.DokumentStatus.Value);
                }

                if (samoIno.HasValue)
                {
                    sql.Append(" AND k.Ino = @SamoIno");
                    parameters.Add("@SamoIno", samoIno.Value ? 1 : 0);
                }

                sql.Append(" ORDER BY f.Datum DESC");

                return (await _databaseService.QueryAsync<FakturaModel>(sql.ToString(), parameters)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajFakture");
                return new List<FakturaModel>();
            }
        }

        public async Task<FakturaDetaljiModel?> UcitajDetalje(long fakturaId)
        {
            try
            {
                // 1. Faktura header
                var sqlFaktura = @"
                    SELECT
                        f.ID, f.Sifra, f.Datum, f.Neto, f.Bruto, f.Porez,
                        f.NetoEur, f.BrutoEur, f.PorezEur,
                        f.Kreirano, f.Azurirano, f.Version,
                        f.KomitentID, k.Naziv as Komitent,
                        f.OtpremnicaID, f.UgovorID, f.KursEur,
                        f.DokumentStatus, f.Aktivno
                    FROM Faktura f
                    LEFT JOIN Komitent k ON f.KomitentID = k.ID
                    WHERE f.ID = @FakturaId";

                var faktura = await _databaseService.QueryFirstOrDefaultAsync<FakturaModel>(
                    sqlFaktura, new Dictionary<string, object> { { "@FakturaId", fakturaId } });

                if (faktura == null)
                    return null;

                var model = new FakturaDetaljiModel { Faktura = faktura };

                // 2. Kupac detalji
                if (faktura.KomitentID.HasValue)
                {
                    var sqlKupac = @"
                        SELECT k.ID, k.Naziv, k.Adresa, k.PostanskiBroj, k.Mesto, k.Drzava,
                               k.PoreskiBroj, k.MaticniBroj, k.BrojRacuna, k.Ino, k.Telefon
                        FROM Komitent k WHERE k.ID = @KomitentId";

                    model.Kupac = await _databaseService.QueryFirstOrDefaultAsync<KupacDetaljiModel>(
                        sqlKupac, new Dictionary<string, object> { { "@KomitentId", faktura.KomitentID.Value } });
                }

                // 3. Otpremnica info (sa podacima o radnom nalogu)
                if (faktura.OtpremnicaID.HasValue)
                {
                    var sqlOtp = @"
                        SELECT o.ID, o.Sifra, o.Datum, o.Vozilo, o.Vozac,
                               o.Kolicina AS BrutoTezina,
                               rn.Sifra AS RadniNalogSifra,
                               rn.LotNaloga,
                               rn.BrojPakovanja AS RadniNalogBrojPakovanja
                        FROM Otpremnica o
                        LEFT JOIN RadniNalog rn ON o.RadniNalogID = rn.ID
                        WHERE o.ID = @OtpremnicaId";

                    model.Otpremnica = await _databaseService.QueryFirstOrDefaultAsync<OtpremnicaInfoModel>(
                        sqlOtp, new Dictionary<string, object> { { "@OtpremnicaId", faktura.OtpremnicaID.Value } });
                }

                // 4. Ugovor info
                if (faktura.UgovorID.HasValue)
                {
                    var sqlUg = @"
                        SELECT u.ID, u.BrojUgovora, u.Datum, u.Paritet, u.Placanje, u.RokIsporuke
                        FROM UgovorProdaja u WHERE u.ID = @UgovorId";

                    model.Ugovor = await _databaseService.QueryFirstOrDefaultAsync<UgovorInfoModel>(
                        sqlUg, new Dictionary<string, object> { { "@UgovorId", faktura.UgovorID.Value } });
                }

                // 5. Stavke
                model.Stavke = await UcitajStavke(fakturaId);

                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajDetalje za FakturaId={Id}", fakturaId);
                return null;
            }
        }

        public async Task<List<FakturaStavkaModel>> UcitajStavke(long fakturaId)
        {
            try
            {
                var sql = @"
                    SELECT
                        fs.ID,
                        fs.FakturaID,
                        fs.ArtikalInstancaID,
                        a.Naziv        AS ArtikalNaziv,
                        ai.Lot,
                        p.Naziv        AS Pakovanje,
                        p.BrojJPuGP,
                        fs.Kolicina,
                        fs.BrojPakovanja,
                        fs.JedinicnaCena,
                        fs.JedinicnaCenaEur,
                        fs.NetoIznos,
                        fs.NetoIznosEur,
                        fs.PorezStopa,
                        fs.PorezIznos,
                        fs.BrutoIznos,
                        fs.BrutoIznosEur,
                        fs.KursEur
                    FROM FakturaStavka fs
                    JOIN ArtikalInstanca ai ON fs.ArtikalInstancaID = ai.ID
                    JOIN Artikal a          ON ai.ArtikalID = a.ID
                    JOIN Pakovanje p        ON ai.PakovanjeID = p.ID
                    WHERE fs.FakturaID = @FakturaId
                    ORDER BY fs.ID";

                return (await _databaseService.QueryAsync<FakturaStavkaModel>(
                    sql, new Dictionary<string, object> { { "@FakturaId", fakturaId } })).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajStavke za FakturaId={Id}", fakturaId);
                return new List<FakturaStavkaModel>();
            }
        }

        public async Task<List<KomitentDropdownModel>> UcitajKupce()
        {
            try
            {
                var sql = @"
                    SELECT k.ID, k.Naziv, k.Ino
                    FROM Komitent k
                    WHERE k.JeKupac = 1 AND k.Aktivno = 1
                    ORDER BY k.Naziv";

                return (await _databaseService.QueryAsync<KomitentDropdownModel>(sql, null)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška u UcitajKupce");
                return new List<KomitentDropdownModel>();
            }
        }
    }
}
