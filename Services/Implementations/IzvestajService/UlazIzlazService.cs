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
            try
            {
                // Mock podaci za testiranje
                await Task.Delay(100); // Simuliranje async poziva
                
                return new List<OtkupniListModel>
                {
                    new OtkupniListModel
                    {
                        ID = 1,
                        Sifra = "OL-2025-001",
                        Datum = DateTime.Now.AddDays(-1),
                        KomitentID = 1,
                        IznosOsnovice = 15000.00m,
                        StopaPDV = 20.0m,
                        IznosPDV = 3000.00m,
                        IznosUkupno = 18000.00m,
                        DokumentStatus = 3
                    },
                    new OtkupniListModel
                    {
                        ID = 2,
                        Sifra = "OL-2025-002",
                        Datum = DateTime.Now.AddDays(-2),
                        KomitentID = 2,
                        IznosOsnovice = 25000.00m,
                        StopaPDV = 20.0m,
                        IznosPDV = 5000.00m,
                        IznosUkupno = 30000.00m,
                        DokumentStatus = 2
                    }
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Greška pri učitavanju otkupnih listova: {ex.Message}", ex);
            }
        }

        public async Task<OtkupniListModel?> UcitajOtkupniListPoId(long id)
        
        {
            await Task.Delay(100); // Simuliranje async poziva

            // Implementacija će biti dodana
            return null;
        }

        public async Task<List<OtkupniListModel>> UcitajOtkupneListovePoKomitentu(long komitentId)
        {

            await Task.Delay(100); // Simuliranje async poziva

            // Implementacija će biti dodana
            return new List<OtkupniListModel>();
        }

        public async Task<List<OtkupniListModel>> UcitajOtkupneListovePoDatumu(DateTime odDatum, DateTime doDatum)
        {
            await Task.Delay(100); // Simuliranje async poziva
           
            // Implementacija će biti dodana
            return new List<OtkupniListModel>();
        }

        public async Task<List<OtkupniListModel>> UcitajOtkupneListovePoStatusu(int status)
        {
            await Task.Delay(100); // Simuliranje async poziva

            // Implementacija će biti dodana
            return new List<OtkupniListModel>();
        }

        public async Task<List<OtkupniListModel>> UcitajOtkupneListovePoOtkupnomMestu(long otkupnoMestoId)
        {
            await Task.Delay(100); // Simuliranje async poziva

            // Implementacija će biti dodana
            return new List<OtkupniListModel>();
        }

        public async Task<List<OtkupniListModel>> UcitajIsplaceneOtkupneListove()
        {
            await Task.Delay(100); // Simuliranje async poziva

            // Implementacija će biti dodana
            return new List<OtkupniListModel>();
        }

        public async Task<List<OtkupniListModel>> UcitajNeisplaceneOtkupneListove()
        {
            await Task.Delay(100); // Simuliranje async poziva

            // Implementacija će biti dodana
            return new List<OtkupniListModel>();
        }

        public async Task<List<PrijemnicaModel>> UcitajSvePrijemnice(FilterRequest filterRequest)
        {
            await Task.Delay(100); // Simuliranje async poziva

            // Implementacija će biti dodana
            return new List<PrijemnicaModel>();
        }

        public async Task<PrijemnicaModel?> UcitajPrijemnicuPoId(long id)
        {
            await Task.Delay(100); // Simuliranje async poziva

            // Implementacija će biti dodana
            return null;
        }

        public async Task<List<PrijemnicaModel>> UcitajPrijemnicePoKomitentu(long komitentId)
        {
            await Task.Delay(100); // Simuliranje async poziva

            // Implementacija će biti dodana
            return new List<PrijemnicaModel>();
        }

        public async Task<List<PrijemnicaModel>> UcitajPrijemnicePoDatumu(DateTime odDatum, DateTime doDatum)
        {
            await Task.Delay(100); // Simuliranje async poziva

            // Implementacija će biti dodana
            return new List<PrijemnicaModel>();
        }

        public async Task<List<PrijemnicaModel>> UcitajPrijemnicePoStatusu(int status)
        {
            await Task.Delay(100); // Simuliranje async poziva

            // Implementacija će biti dodana
            return new List<PrijemnicaModel>();
        }

        public async Task<List<PrijemnicaModel>> UcitajPrijemnicePoMagacinu(long magacinId)
        {
            await Task.Delay(100); // Simuliranje async poziva

            // Implementacija će biti dodana
            return new List<PrijemnicaModel>();
        }

        public async Task<List<PrijemnicaModel>> UcitajPrijemnicePoTipuPrijema(int tipPrijema)
        {
            await Task.Delay(100); // Simuliranje async poziva
            // Implementacija će biti dodana
            return new List<PrijemnicaModel>();
        }

        public async Task<List<PrijemnicaModel>> UcitajPrijemniceZaKontrolu()
        {
            await Task.Delay(100); // Simuliranje async poziva
            // Implementacija će biti dodana
            return new List<PrijemnicaModel>();
        }

        public async Task<List<PrijemnicaModel>> UcitajReklamiranePrijemnice()
        {
            await Task.Delay(100); // Simuliranje async poziva
            // Implementacija će biti dodana
            return new List<PrijemnicaModel>();
        }

        public async Task<List<OtpremnicaModel>> UcitajSveOtpremnice(FilterRequest filterRequest)
        {
            await Task.Delay(100); // Simuliranje async poziva
            // Implementacija će biti dodana
            return new List<OtpremnicaModel>();
        }

        public async Task<OtpremnicaModel?> UcitajOtpremnicuPoId(long id)
        {
            await Task.Delay(100); // Simuliranje async poziva
            // Implementacija će biti dodana
            return null;
        }

        public async Task<List<OtpremnicaModel>> UcitajOtpremnicePoKomitentu(long komitentId)
        {
            await Task.Delay(100); // Simuliranje async poziva
            // Implementacija će biti dodana
            return new List<OtpremnicaModel>();
        }

        public async Task<List<OtpremnicaModel>> UcitajOtpremnicePoDatumu(DateTime odDatum, DateTime doDatum)
        {
            await Task.Delay(100); // Simuliranje async poziva
            // Implementacija će biti dodana
            return new List<OtpremnicaModel>();
        }

        public async Task<List<OtpremnicaModel>> UcitajOtpremnicePoStatusu(int status)
        {
            await Task.Delay(100); // Simuliranje async poziva
            // Implementacija će biti dodana
            return new List<OtpremnicaModel>();
        }

        public async Task<List<OtpremnicaModel>> UcitajOtpremnicePoMagacinu(long magacinId)
        {
            await Task.Delay(100); // Simuliranje async poziva
            // Implementacija će biti dodana
            return new List<OtpremnicaModel>();
        }

        public async Task<List<OtpremnicaModel>> UcitajOtpremnicePoTipu(int tipOtpreme)
        {
            await Task.Delay(100); // Simuliranje async poziva
            // Implementacija će biti dodana
            return new List<OtpremnicaModel>();
        }

        public async Task<List<OtpremnicaModel>> UcitajIzvozneOtpremnice()
        {
            await Task.Delay(100); // Simuliranje async poziva
            // Implementacija će biti dodana
            return new List<OtpremnicaModel>();
        }

        public async Task<List<OtpremnicaModel>> UcitajTranzitneOtpremnice()
        {
            await Task.Delay(100); // Simuliranje async poziva
            // Implementacija će biti dodana
            return new List<OtpremnicaModel>();
        }

        public async Task<Dictionary<string, decimal>> UcitajStatistikuPoKomitentima(FilterRequest filterRequest)
        {
            await Task.Delay(100); // Simuliranje async poziva
            // Implementacija će biti dodana
            return new Dictionary<string, decimal>();
        }

        public async Task<Dictionary<string, decimal>> UcitajStatistikuPoMagacinima(FilterRequest filterRequest)
        {
            await Task.Delay(100); // Simuliranje async poziva
            // Implementacija će biti dodana
            return new Dictionary<string, decimal>();
        }

        public async Task<Dictionary<string, decimal>> UcitajStatistikuPoStatusima(FilterRequest filterRequest)
        {
            await Task.Delay(100); // Simuliranje async poziva
            // Implementacija će biti dodana
            return new Dictionary<string, decimal>();
        }

        public async Task<Dictionary<string, decimal>> UcitajStatistikuPoMesecima(FilterRequest filterRequest)
        {
            await Task.Delay(100); // Simuliranje async poziva
            // Implementacija će biti dodana
            return new Dictionary<string, decimal>();
        }

        public async Task<decimal> UcitajUkupnuVrednostFaktura(FilterRequest filterRequest)
        {
            await Task.Delay(100); // Simuliranje async poziva
            // Implementacija će biti dodana
            return 0;
        }

        public async Task<decimal> UcitajUkupnuVrednostOtkupnihListova(FilterRequest filterRequest)
        {
            await Task.Delay(100); // Simuliranje async poziva
            // Implementacija će biti dodana
            return 0;
        }

        public async Task<int> UcitajUkupanBrojDokumenata(FilterRequest filterRequest)
        {
            await Task.Delay(100); // Simuliranje async poziva
            // Implementacija će biti dodana
            return 0;
        }

        public async Task<Dictionary<string, decimal>> UcitajTopKomitentePoVrednosti(FilterRequest filterRequest)
        {
            await Task.Delay(100); // Simuliranje async poziva
            // Implementacija će biti dodana
            return new Dictionary<string, decimal>();
        }

        public async Task<Dictionary<string, decimal>> UcitajTopMagacinePoKolicini(FilterRequest filterRequest)
        {
            await Task.Delay(100); // Simuliranje async poziva
            // Implementacija će biti dodana
            return new Dictionary<string, decimal>();
        }

        public async Task<decimal> UcitajUkupnuKolicinu(FilterRequest filterRequest)
        {
            await Task.Delay(100); // Simuliranje async poziva
            // Implementacija će biti dodana
            return 0;
        }

        public async Task<decimal> UcitajUkupnuVrednost(FilterRequest filterRequest)
        {
            await Task.Delay(100); // Simuliranje async poziva
            // Implementacija će biti dodana
            return 0;
        }

        public async Task<int> UcitajBrojOtvorenihDokumenata()
        {
            await Task.Delay(100); // Simuliranje async poziva
            // Implementacija će biti dodana
            return 0;
        }

        public async Task<int> UcitajBrojZakljucenihDokumenata()
        {
            await Task.Delay(100); // Simuliranje async poziva
            // Implementacija će biti dodana
            return 0;
        }

        public async Task<int> UcitajBrojStornoDokumenata()
        {
            await Task.Delay(100); // Simuliranje async poziva
            // Implementacija će biti dodana
            return 0;
        }

        #endregion
    }
}
