using Microsoft.AspNetCore.Mvc;
using FruitSysWeb.Services.Interfaces;
using FruitSysWeb.Services.Models.Requests;

namespace FruitSysWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UlazIzlazController : ControllerBase
    {
        private readonly IUlazIzlazService _ulazIzlazService;

        public UlazIzlazController(IUlazIzlazService ulazIzlazService)
        {
            _ulazIzlazService = ulazIzlazService;
        }

        #region Faktura endpoints

        [HttpGet("fakture")]
        public async Task<IActionResult> GetFakture([FromQuery] FilterRequest filterRequest)
        {
            try
            {
                var fakture = await _ulazIzlazService.UcitajSveFakture(filterRequest);
                return Ok(fakture);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju faktura: {ex.Message}");
            }
        }

        [HttpGet("fakture/{id}")]
        public async Task<IActionResult> GetFakturaById(long id)
        {
            try
            {
                var faktura = await _ulazIzlazService.UcitajFakturuPoId(id);
                if (faktura == null)
                    return NotFound();

                return Ok(faktura);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju fakture: {ex.Message}");
            }
        }

        [HttpGet("fakture/komitent/{komitentId}")]
        public async Task<IActionResult> GetFaktureByKomitent(long komitentId)
        {
            try
            {
                var fakture = await _ulazIzlazService.UcitajFakturePoKomitentu(komitentId);
                return Ok(fakture);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju faktura po komitentu: {ex.Message}");
            }
        }

        [HttpGet("fakture/datum")]
        public async Task<IActionResult> GetFaktureByDatum([FromQuery] DateTime odDatum, [FromQuery] DateTime doDatum)
        {
            try
            {
                var fakture = await _ulazIzlazService.UcitajFakturePoDatumu(odDatum, doDatum);
                return Ok(fakture);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju faktura po datumu: {ex.Message}");
            }
        }

        [HttpGet("fakture/status/{status}")]
        public async Task<IActionResult> GetFaktureByStatus(int status)
        {
            try
            {
                var fakture = await _ulazIzlazService.UcitajFakturePoStatusu(status);
                return Ok(fakture);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju faktura po statusu: {ex.Message}");
            }
        }

        [HttpGet("fakture/otvorene")]
        public async Task<IActionResult> GetOtvoreneFakture()
        {
            try
            {
                var fakture = await _ulazIzlazService.UcitajOtvoreneFakture();
                return Ok(fakture);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju otvorenih faktura: {ex.Message}");
            }
        }

        [HttpGet("fakture/zakljucene")]
        public async Task<IActionResult> GetZakljuceneFakture()
        {
            try
            {
                var fakture = await _ulazIzlazService.UcitajZakljuceneFakture();
                return Ok(fakture);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju zaključenih faktura: {ex.Message}");
            }
        }

        [HttpGet("fakture/storno")]
        public async Task<IActionResult> GetStornoFakture()
        {
            try
            {
                var fakture = await _ulazIzlazService.UcitajStornoFakture();
                return Ok(fakture);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju storno faktura: {ex.Message}");
            }
        }

        #endregion

        #region OtkupniList endpoints

        [HttpGet("otkupni-listovi")]
        public async Task<IActionResult> GetOtkupniListovi([FromQuery] FilterRequest filterRequest)
        {
            try
            {
                var listovi = await _ulazIzlazService.UcitajSveOtkupneListove(filterRequest);
                return Ok(listovi);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju otkupnih listova: {ex.Message}");
            }
        }

        [HttpGet("otkupni-listovi/{id}")]
        public async Task<IActionResult> GetOtkupniListById(long id)
        {
            try
            {
                var list = await _ulazIzlazService.UcitajOtkupniListPoId(id);
                if (list == null)
                    return NotFound();

                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju otkupnog lista: {ex.Message}");
            }
        }

        [HttpGet("otkupni-listovi/komitent/{komitentId}")]
        public async Task<IActionResult> GetOtkupniListoviByKomitent(long komitentId)
        {
            try
            {
                var listovi = await _ulazIzlazService.UcitajOtkupneListovePoKomitentu(komitentId);
                return Ok(listovi);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju otkupnih listova po komitentu: {ex.Message}");
            }
        }

        [HttpGet("otkupni-listovi/datum")]
        public async Task<IActionResult> GetOtkupniListoviByDatum([FromQuery] DateTime odDatum, [FromQuery] DateTime doDatum)
        {
            try
            {
                var listovi = await _ulazIzlazService.UcitajOtkupneListovePoDatumu(odDatum, doDatum);
                return Ok(listovi);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju otkupnih listova po datumu: {ex.Message}");
            }
        }

        [HttpGet("otkupni-listovi/status/{status}")]
        public async Task<IActionResult> GetOtkupniListoviByStatus(int status)
        {
            try
            {
                var listovi = await _ulazIzlazService.UcitajOtkupneListovePoStatusu(status);
                return Ok(listovi);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju otkupnih listova po statusu: {ex.Message}");
            }
        }

        [HttpGet("otkupni-listovi/otkupno-mesto/{otkupnoMestoId}")]
        public async Task<IActionResult> GetOtkupniListoviByOtkupnoMesto(long otkupnoMestoId)
        {
            try
            {
                var listovi = await _ulazIzlazService.UcitajOtkupneListovePoOtkupnomMestu(otkupnoMestoId);
                return Ok(listovi);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju otkupnih listova po otkupnom mestu: {ex.Message}");
            }
        }

        [HttpGet("otkupni-listovi/isplaceni")]
        public async Task<IActionResult> GetIsplaceniOtkupniListovi()
        {
            try
            {
                var listovi = await _ulazIzlazService.UcitajIsplaceneOtkupneListove();
                return Ok(listovi);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju isplaćenih otkupnih listova: {ex.Message}");
            }
        }

        [HttpGet("otkupni-listovi/neisplaceni")]
        public async Task<IActionResult> GetNeisplaceniOtkupniListovi()
        {
            try
            {
                var listovi = await _ulazIzlazService.UcitajNeisplaceneOtkupneListove();
                return Ok(listovi);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju neisplaćenih otkupnih listova: {ex.Message}");
            }
        }

        #endregion

        #region Prijemnica endpoints

        [HttpGet("prijemnice")]
        public async Task<IActionResult> GetPrijemnice([FromQuery] FilterRequest filterRequest)
        {
            try
            {
                var prijemnice = await _ulazIzlazService.UcitajSvePrijemnice(filterRequest);
                return Ok(prijemnice);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju prijemnica: {ex.Message}");
            }
        }

        [HttpGet("prijemnice/{id}")]
        public async Task<IActionResult> GetPrijemnicaById(long id)
        {
            try
            {
                var prijemnica = await _ulazIzlazService.UcitajPrijemnicuPoId(id);
                if (prijemnica == null)
                    return NotFound();

                return Ok(prijemnica);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju prijemnice: {ex.Message}");
            }
        }

        [HttpGet("prijemnice/komitent/{komitentId}")]
        public async Task<IActionResult> GetPrijemniceByKomitent(long komitentId)
        {
            try
            {
                var prijemnice = await _ulazIzlazService.UcitajPrijemnicePoKomitentu(komitentId);
                return Ok(prijemnice);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju prijemnica po komitentu: {ex.Message}");
            }
        }

        [HttpGet("prijemnice/datum")]
        public async Task<IActionResult> GetPrijemniceByDatum([FromQuery] DateTime odDatum, [FromQuery] DateTime doDatum)
        {
            try
            {
                var prijemnice = await _ulazIzlazService.UcitajPrijemnicePoDatumu(odDatum, doDatum);
                return Ok(prijemnice);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju prijemnica po datumu: {ex.Message}");
            }
        }

        [HttpGet("prijemnice/status/{status}")]
        public async Task<IActionResult> GetPrijemniceByStatus(int status)
        {
            try
            {
                var prijemnice = await _ulazIzlazService.UcitajPrijemnicePoStatusu(status);
                return Ok(prijemnice);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju prijemnica po statusu: {ex.Message}");
            }
        }

        [HttpGet("prijemnice/magacin/{magacinId}")]
        public async Task<IActionResult> GetPrijemniceByMagacin(long magacinId)
        {
            try
            {
                var prijemnice = await _ulazIzlazService.UcitajPrijemnicePoMagacinu(magacinId);
                return Ok(prijemnice);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju prijemnica po magacinu: {ex.Message}");
            }
        }

        [HttpGet("prijemnice/tip-prijema/{tipPrijema}")]
        public async Task<IActionResult> GetPrijemniceByTipPrijema(int tipPrijema)
        {
            try
            {
                var prijemnice = await _ulazIzlazService.UcitajPrijemnicePoTipuPrijema(tipPrijema);
                return Ok(prijemnice);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju prijemnica po tipu prijema: {ex.Message}");
            }
        }

        [HttpGet("prijemnice/kontrola")]
        public async Task<IActionResult> GetPrijemniceZaKontrolu()
        {
            try
            {
                var prijemnice = await _ulazIzlazService.UcitajPrijemniceZaKontrolu();
                return Ok(prijemnice);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju prijemnica za kontrolu: {ex.Message}");
            }
        }

        [HttpGet("prijemnice/reklamirane")]
        public async Task<IActionResult> GetReklamiranePrijemnice()
        {
            try
            {
                var prijemnice = await _ulazIzlazService.UcitajReklamiranePrijemnice();
                return Ok(prijemnice);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju reklamiranih prijemnica: {ex.Message}");
            }
        }

        #endregion

        #region Otpremnica endpoints

        [HttpGet("otpremnice")]
        public async Task<IActionResult> GetOtpremnice([FromQuery] FilterRequest filterRequest)
        {
            try
            {
                var otpremnice = await _ulazIzlazService.UcitajSveOtpremnice(filterRequest);
                return Ok(otpremnice);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju otpremnica: {ex.Message}");
            }
        }

        [HttpGet("otpremnice/{id}")]
        public async Task<IActionResult> GetOtpremnicaById(long id)
        {
            try
            {
                var otpremnica = await _ulazIzlazService.UcitajOtpremnicuPoId(id);
                if (otpremnica == null)
                    return NotFound();

                return Ok(otpremnica);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju otpremnice: {ex.Message}");
            }
        }

        [HttpGet("otpremnice/komitent/{komitentId}")]
        public async Task<IActionResult> GetOtpremniceByKomitent(long komitentId)
        {
            try
            {
                var otpremnice = await _ulazIzlazService.UcitajOtpremnicePoKomitentu(komitentId);
                return Ok(otpremnice);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju otpremnica po komitentu: {ex.Message}");
            }
        }

        [HttpGet("otpremnice/datum")]
        public async Task<IActionResult> GetOtpremniceByDatum([FromQuery] DateTime odDatum, [FromQuery] DateTime doDatum)
        {
            try
            {
                var otpremnice = await _ulazIzlazService.UcitajOtpremnicePoDatumu(odDatum, doDatum);
                return Ok(otpremnice);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju otpremnica po datumu: {ex.Message}");
            }
        }

        [HttpGet("otpremnice/status/{status}")]
        public async Task<IActionResult> GetOtpremniceByStatus(int status)
        {
            try
            {
                var otpremnice = await _ulazIzlazService.UcitajOtpremnicePoStatusu(status);
                return Ok(otpremnice);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju otpremnica po statusu: {ex.Message}");
            }
        }

        [HttpGet("otpremnice/magacin/{magacinId}")]
        public async Task<IActionResult> GetOtpremniceByMagacin(long magacinId)
        {
            try
            {
                var otpremnice = await _ulazIzlazService.UcitajOtpremnicePoMagacinu(magacinId);
                return Ok(otpremnice);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju otpremnica po magacinu: {ex.Message}");
            }
        }

        [HttpGet("otpremnice/tip/{tipOtpreme}")]
        public async Task<IActionResult> GetOtpremniceByTip(int tipOtpreme)
        {
            try
            {
                var otpremnice = await _ulazIzlazService.UcitajOtpremnicePoTipu(tipOtpreme);
                return Ok(otpremnice);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju otpremnica po tipu: {ex.Message}");
            }
        }

        [HttpGet("otpremnice/izvoz")]
        public async Task<IActionResult> GetIzvozneOtpremnice()
        {
            try
            {
                var otpremnice = await _ulazIzlazService.UcitajIzvozneOtpremnice();
                return Ok(otpremnice);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju izvoznih otpremnica: {ex.Message}");
            }
        }

        [HttpGet("otpremnice/tranzit")]
        public async Task<IActionResult> GetTranzitneOtpremnice()
        {
            try
            {
                var otpremnice = await _ulazIzlazService.UcitajTranzitneOtpremnice();
                return Ok(otpremnice);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju tranzitnih otpremnica: {ex.Message}");
            }
        }

        #endregion

        #region Statistika endpoints

        [HttpGet("statistika/komitenti")]
        public async Task<IActionResult> GetStatistikaPoKomitentima([FromQuery] FilterRequest filterRequest)
        {
            try
            {
                var statistika = await _ulazIzlazService.UcitajStatistikuPoKomitentima(filterRequest);
                return Ok(statistika);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju statistike po komitentima: {ex.Message}");
            }
        }

        [HttpGet("statistika/magacini")]
        public async Task<IActionResult> GetStatistikaPoMagacinima([FromQuery] FilterRequest filterRequest)
        {
            try
            {
                var statistika = await _ulazIzlazService.UcitajStatistikuPoMagacinima(filterRequest);
                return Ok(statistika);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju statistike po magacinima: {ex.Message}");
            }
        }

        [HttpGet("statistika/statusi")]
        public async Task<IActionResult> GetStatistikaPoStatusima([FromQuery] FilterRequest filterRequest)
        {
            try
            {
                var statistika = await _ulazIzlazService.UcitajStatistikuPoStatusima(filterRequest);
                return Ok(statistika);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju statistike po statusima: {ex.Message}");
            }
        }

        [HttpGet("statistika/meseci")]
        public async Task<IActionResult> GetStatistikaPoMesecima([FromQuery] FilterRequest filterRequest)
        {
            try
            {
                var statistika = await _ulazIzlazService.UcitajStatistikuPoMesecima(filterRequest);
                return Ok(statistika);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju statistike po mesecima: {ex.Message}");
            }
        }

        [HttpGet("statistika/ukupno")]
        public async Task<IActionResult> GetUkupnaStatistika([FromQuery] FilterRequest filterRequest)
        {
            try
            {
                var ukupnoDokumenata = await _ulazIzlazService.UcitajUkupanBrojDokumenata(filterRequest);
                var ukupnaVrednost = await _ulazIzlazService.UcitajUkupnuVrednost(filterRequest);
                var ukupnaKolicina = await _ulazIzlazService.UcitajUkupnuKolicinu(filterRequest);
                var otvoreni = await _ulazIzlazService.UcitajBrojOtvorenihDokumenata();
                var zakljuceni = await _ulazIzlazService.UcitajBrojZakljucenihDokumenata();
                var storno = await _ulazIzlazService.UcitajBrojStornoDokumenata();

                var statistika = new
                {
                    UkupnoDokumenata = ukupnoDokumenata,
                    UkupnaVrednost = ukupnaVrednost,
                    UkupnaKolicina = ukupnaKolicina,
                    Otvoreni = otvoreni,
                    Zakljuceni = zakljuceni,
                    Storno = storno
                };

                return Ok(statistika);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju ukupne statistike: {ex.Message}");
            }
        }

        #endregion
    }
}
