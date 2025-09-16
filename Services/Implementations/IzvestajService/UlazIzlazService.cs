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
                throw new Exception($"Greška pri učitavanju faktura: {ex.Message}", ex);
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
                throw new Exception($"Greška pri učitavanju fakture po ID: {ex.Message}", ex);
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
                throw new Exception($"Greška pri učitavanju faktura po komitentu: {ex.Message}", ex);
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
                throw new Exception($"Greška pri učitavanju faktura po datumu: {ex.Message}", ex);
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
                throw new Exception($"Greška pri učitavanju faktura po statusu: {ex.Message}", ex);
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

        #region Ostale metode - implementacija će biti dodana u sledećem koraku

        public async Task<List<OtkupniListModel>> UcitajSveOtkupneListove(FilterRequest filterRequest)
        {
            // Implementacija će biti dodana
            return new List<OtkupniListModel>();
        }

        public async Task<OtkupniListModel?> UcitajOtkupniListPoId(long id)
        {
            // Implementacija će biti dodana
            return null;
        }

        public async Task<List<OtkupniListModel>> UcitajOtkupneListovePoKomitentu(long komitentId)
        {
            // Implementacija će biti dodana
            return new List<OtkupniListModel>();
        }

        public async Task<List<OtkupniListModel>> UcitajOtkupneListovePoDatumu(DateTime odDatum, DateTime doDatum)
        {
            // Implementacija će biti dodana
            return new List<OtkupniListModel>();
        }

        public async Task<List<OtkupniListModel>> UcitajOtkupneListovePoStatusu(int status)
        {
            // Implementacija će biti dodana
            return new List<OtkupniListModel>();
        }

        public async Task<List<OtkupniListModel>> UcitajOtkupneListovePoOtkupnomMestu(long otkupnoMestoId)
        {
            // Implementacija će biti dodana
            return new List<OtkupniListModel>();
        }

        public async Task<List<OtkupniListModel>> UcitajIsplaceneOtkupneListove()
        {
            // Implementacija će biti dodana
            return new List<OtkupniListModel>();
        }

        public async Task<List<OtkupniListModel>> UcitajNeisplaceneOtkupneListove()
        {
            // Implementacija će biti dodana
            return new List<OtkupniListModel>();
        }

        public async Task<List<PrijemnicaModel>> UcitajSvePrijemnice(FilterRequest filterRequest)
        {
            // Implementacija će biti dodana
            return new List<PrijemnicaModel>();
        }

        public async Task<PrijemnicaModel?> UcitajPrijemnicuPoId(long id)
        {
            // Implementacija će biti dodana
            return null;
        }

        public async Task<List<PrijemnicaModel>> UcitajPrijemnicePoKomitentu(long komitentId)
        {
            // Implementacija će biti dodana
            return new List<PrijemnicaModel>();
        }

        public async Task<List<PrijemnicaModel>> UcitajPrijemnicePoDatumu(DateTime odDatum, DateTime doDatum)
        {
            // Implementacija će biti dodana
            return new List<PrijemnicaModel>();
        }

        public async Task<List<PrijemnicaModel>> UcitajPrijemnicePoStatusu(int status)
        {
            // Implementacija će biti dodana
            return new List<PrijemnicaModel>();
        }

        public async Task<List<PrijemnicaModel>> UcitajPrijemnicePoMagacinu(long magacinId)
        {
            // Implementacija će biti dodana
            return new List<PrijemnicaModel>();
        }

        public async Task<List<PrijemnicaModel>> UcitajPrijemnicePoTipuPrijema(int tipPrijema)
        {
            // Implementacija će biti dodana
            return new List<PrijemnicaModel>();
        }

        public async Task<List<PrijemnicaModel>> UcitajPrijemniceZaKontrolu()
        {
            // Implementacija će biti dodana
            return new List<PrijemnicaModel>();
        }

        public async Task<List<PrijemnicaModel>> UcitajReklamiranePrijemnice()
        {
            // Implementacija će biti dodana
            return new List<PrijemnicaModel>();
        }

        public async Task<List<OtpremnicaModel>> UcitajSveOtpremnice(FilterRequest filterRequest)
        {
            // Implementacija će biti dodana
            return new List<OtpremnicaModel>();
        }

        public async Task<OtpremnicaModel?> UcitajOtpremnicuPoId(long id)
        {
            // Implementacija će biti dodana
            return null;
        }

        public async Task<List<OtpremnicaModel>> UcitajOtpremnicePoKomitentu(long komitentId)
        {
            // Implementacija će biti dodana
            return new List<OtpremnicaModel>();
        }

        public async Task<List<OtpremnicaModel>> UcitajOtpremnicePoDatumu(DateTime odDatum, DateTime doDatum)
        {
            // Implementacija će biti dodana
            return new List<OtpremnicaModel>();
        }

        public async Task<List<OtpremnicaModel>> UcitajOtpremnicePoStatusu(int status)
        {
            // Implementacija će biti dodana
            return new List<OtpremnicaModel>();
        }

        public async Task<List<OtpremnicaModel>> UcitajOtpremnicePoMagacinu(long magacinId)
        {
            // Implementacija će biti dodana
            return new List<OtpremnicaModel>();
        }

        public async Task<List<OtpremnicaModel>> UcitajOtpremnicePoTipu(int tipOtpreme)
        {
            // Implementacija će biti dodana
            return new List<OtpremnicaModel>();
        }

        public async Task<List<OtpremnicaModel>> UcitajIzvozneOtpremnice()
        {
            // Implementacija će biti dodana
            return new List<OtpremnicaModel>();
        }

        public async Task<List<OtpremnicaModel>> UcitajTranzitneOtpremnice()
        {
            // Implementacija će biti dodana
            return new List<OtpremnicaModel>();
        }

        public async Task<Dictionary<string, decimal>> UcitajStatistikuPoKomitentima(FilterRequest filterRequest)
        {
            // Implementacija će biti dodana
            return new Dictionary<string, decimal>();
        }

        public async Task<Dictionary<string, decimal>> UcitajStatistikuPoMagacinima(FilterRequest filterRequest)
        {
            // Implementacija će biti dodana
            return new Dictionary<string, decimal>();
        }

        public async Task<Dictionary<string, decimal>> UcitajStatistikuPoStatusima(FilterRequest filterRequest)
        {
            // Implementacija će biti dodana
            return new Dictionary<string, decimal>();
        }

        public async Task<Dictionary<string, decimal>> UcitajStatistikuPoMesecima(FilterRequest filterRequest)
        {
            // Implementacija će biti dodana
            return new Dictionary<string, decimal>();
        }

        public async Task<decimal> UcitajUkupnuVrednostFaktura(FilterRequest filterRequest)
        {
            // Implementacija će biti dodana
            return 0;
        }

        public async Task<decimal> UcitajUkupnuVrednostOtkupnihListova(FilterRequest filterRequest)
        {
            // Implementacija će biti dodana
            return 0;
        }

        public async Task<int> UcitajUkupanBrojDokumenata(FilterRequest filterRequest)
        {
            // Implementacija će biti dodana
            return 0;
        }

        public async Task<Dictionary<string, decimal>> UcitajTopKomitentePoVrednosti(FilterRequest filterRequest)
        {
            // Implementacija će biti dodana
            return new Dictionary<string, decimal>();
        }

        public async Task<Dictionary<string, decimal>> UcitajTopMagacinePoKolicini(FilterRequest filterRequest)
        {
            // Implementacija će biti dodana
            return new Dictionary<string, decimal>();
        }

        public async Task<decimal> UcitajUkupnuKolicinu(FilterRequest filterRequest)
        {
            // Implementacija će biti dodana
            return 0;
        }

        public async Task<decimal> UcitajUkupnuVrednost(FilterRequest filterRequest)
        {
            // Implementacija će biti dodana
            return 0;
        }

        public async Task<int> UcitajBrojOtvorenihDokumenata()
        {
            // Implementacija će biti dodana
            return 0;
        }

        public async Task<int> UcitajBrojZakljucenihDokumenata()
        {
            // Implementacija će biti dodana
            return 0;
        }

        public async Task<int> UcitajBrojStornoDokumenata()
        {
            // Implementacija će biti dodana
            return 0;
        }

        #endregion
    }
}
