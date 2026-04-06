using Microsoft.AspNetCore.Mvc;
using FruitSysWeb.Services.Interfaces;
using FruitSysWeb.Services.Models.Requests;

namespace FruitSysWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PreradaController : ControllerBase
    {
        private readonly IPreradaService _preradaService;

        public PreradaController(IPreradaService preradaService)
        {
            _preradaService = preradaService;
        }

        #region EvidencijaRada endpoints

        [HttpGet("evidencija-rada")]
        public async Task<IActionResult> GetEvidencijeRada()
        {
            try
            {
                var evidencije = await _preradaService.UcitajSveEvidencijeRada();
                return Ok(evidencije);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju evidencije rada: {ex.Message}");
            }
        }

        [HttpGet("evidencija-rada/{id}")]
        public async Task<IActionResult> GetEvidencijaRadaById(long id)
        {
            try
            {
                var evidencija = await _preradaService.UcitajEvidencijuRadaPoId(id);
                if (evidencija == null)
                    return NotFound();

                return Ok(evidencija);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju evidencije rada: {ex.Message}");
            }
        }

        [HttpGet("evidencija-rada/rezija/{rezijaId}")]
        public async Task<IActionResult> GetEvidencijeRadaByRezija(long rezijaId)
        {
            try
            {
                var evidencije = await _preradaService.UcitajEvidencijeRadaPoReziji(rezijaId);
                return Ok(evidencije);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju evidencije rada po reziji: {ex.Message}");
            }
        }

        [HttpGet("evidencija-rada/search")]
        public async Task<IActionResult> SearchEvidencijeRada([FromQuery] string naziv)
        {
            try
            {
                var evidencije = await _preradaService.UcitajEvidencijeRadaPoNazivu(naziv);
                return Ok(evidencije);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri pretraživanju evidencije rada: {ex.Message}");
            }
        }

        #endregion

        #region RadniProces endpoints

        [HttpGet("radni-procesi")]
        public async Task<IActionResult> GetRadniProcesi()
        {
            try
            {
                var procesi = await _preradaService.UcitajSveRadneProcese();
                return Ok(procesi);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju radnih procesa: {ex.Message}");
            }
        }

        [HttpGet("radni-procesi/{id}")]
        public async Task<IActionResult> GetRadniProcesById(long id)
        {
            try
            {
                var proces = await _preradaService.UcitajRadniProcesPoId(id);
                if (proces == null)
                    return NotFound();

                return Ok(proces);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju radnog procesa: {ex.Message}");
            }
        }

        [HttpGet("radni-procesi/search")]
        public async Task<IActionResult> SearchRadniProcesi([FromQuery] string naziv)
        {
            try
            {
                var procesi = await _preradaService.UcitajRadneProcesePoNazivu(naziv);
                return Ok(procesi);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri pretraživanju radnih procesa: {ex.Message}");
            }
        }

        [HttpGet("radni-procesi/novi")]
        public async Task<IActionResult> GetNoviRadniProcesi([FromQuery] int dana = 30)
        {
            try
            {
                var procesi = await _preradaService.UcitajNoveRadneProcese(dana);
                return Ok(procesi);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju novih radnih procesa: {ex.Message}");
            }
        }

        #endregion

        #region ProizvodniProces endpoints

        [HttpGet("proizvodni-procesi")]
        public async Task<IActionResult> GetProizvodniProcesi()
        {
            try
            {
                var procesi = await _preradaService.UcitajSveProizvodneProcese();
                return Ok(procesi);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju proizvodnih procesa: {ex.Message}");
            }
        }

        [HttpGet("proizvodni-procesi/{id}")]
        public async Task<IActionResult> GetProizvodniProcesById(long id)
        {
            try
            {
                var proces = await _preradaService.UcitajProizvodniProcesPoId(id);
                if (proces == null)
                    return NotFound();

                return Ok(proces);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju proizvodnog procesa: {ex.Message}");
            }
        }

        [HttpGet("proizvodni-procesi/kategorija/{kategorija}")]
        public async Task<IActionResult> GetProizvodniProcesiByKategorija(string kategorija)
        {
            try
            {
                var procesi = await _preradaService.UcitajProizvodneProcesePoKategoriji(kategorija);
                return Ok(procesi);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju proizvodnih procesa po kategoriji: {ex.Message}");
            }
        }

        [HttpGet("proizvodni-procesi/search")]
        public async Task<IActionResult> SearchProizvodniProcesi([FromQuery] string naziv)
        {
            try
            {
                var procesi = await _preradaService.UcitajProizvodneProcesePoNazivu(naziv);
                return Ok(procesi);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri pretraživanju proizvodnih procesa: {ex.Message}");
            }
        }

        #endregion

        #region SmenskiIzvestaj endpoints

        [HttpGet("smenski-izvestaji")]
        public async Task<IActionResult> GetSmenskiIzvestaji([FromQuery] FilterRequest filterRequest)
        {
            try
            {
                var izvestaji = await _preradaService.UcitajSveSmenskeIzvestaje(filterRequest);
                return Ok(izvestaji);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju smenskih izveštaja: {ex.Message}");
            }
        }

        [HttpGet("smenski-izvestaji/{id}")]
        public async Task<IActionResult> GetSmenskiIzvestajById(long id)
        {
            try
            {
                var izvestaj = await _preradaService.UcitajSmenskiIzvestajPoId(id);
                if (izvestaj == null)
                    return NotFound();

                return Ok(izvestaj);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju smenskog izveštaja: {ex.Message}");
            }
        }

        [HttpGet("smenski-izvestaji/datum")]
        public async Task<IActionResult> GetSmenskiIzvestajiByDatum([FromQuery] DateTime odDatum, [FromQuery] DateTime doDatum)
        {
            try
            {
                var izvestaji = await _preradaService.UcitajSmenskeIzvestajePoDatumu(odDatum, doDatum);
                return Ok(izvestaji);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju smenskih izveštaja po datumu: {ex.Message}");
            }
        }

        [HttpGet("smenski-izvestaji/smena/{smena}")]
        public async Task<IActionResult> GetSmenskiIzvestajiBySmena(int smena)
        {
            try
            {
                var izvestaji = await _preradaService.UcitajSmenskeIzvestajePoSmeni(smena);
                return Ok(izvestaji);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju smenskih izveštaja po smeni: {ex.Message}");
            }
        }

        [HttpGet("smenski-izvestaji/poslovodja/{poslovodjaId}")]
        public async Task<IActionResult> GetSmenskiIzvestajiByPoslovodja(long poslovodjaId)
        {
            try
            {
                var izvestaji = await _preradaService.UcitajSmenskeIzvestajePoPoslovodji(poslovodjaId);
                return Ok(izvestaji);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju smenskih izveštaja po poslovođi: {ex.Message}");
            }
        }

        [HttpGet("smenski-izvestaji/otvoreni")]
        public async Task<IActionResult> GetOtvoreniSmenskiIzvestaji()
        {
            try
            {
                var izvestaji = await _preradaService.UcitajOtvoreneSmenskeIzvestaje();
                return Ok(izvestaji);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju otvorenih smenskih izveštaja: {ex.Message}");
            }
        }

        [HttpGet("smenski-izvestaji/zakljuceni")]
        public async Task<IActionResult> GetZakljuceniSmenskiIzvestaji()
        {
            try
            {
                var izvestaji = await _preradaService.UcitajZakljuceneSmenskeIzvestaje();
                return Ok(izvestaji);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju zaključenih smenskih izveštaja: {ex.Message}");
            }
        }

        #endregion

        #region Statistika endpoints

        [HttpGet("statistika/smene")]
        public async Task<IActionResult> GetStatistikaPoSmenama([FromQuery] FilterRequest filterRequest)
        {
            try
            {
                var statistika = await _preradaService.UcitajStatistikuPoSmenama(filterRequest);
                return Ok(statistika);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju statistike po smenama: {ex.Message}");
            }
        }

        [HttpGet("statistika/poslovodje")]
        public async Task<IActionResult> GetStatistikaPoPoslovodjama([FromQuery] FilterRequest filterRequest)
        {
            try
            {
                var statistika = await _preradaService.UcitajStatistikuPoPoslovodjama(filterRequest);
                return Ok(statistika);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju statistike po poslovođama: {ex.Message}");
            }
        }

        [HttpGet("statistika/statusi")]
        public async Task<IActionResult> GetStatistikaPoStatusima([FromQuery] FilterRequest filterRequest)
        {
            try
            {
                var statistika = await _preradaService.UcitajStatistikuPoStatusima(filterRequest);
                return Ok(statistika);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri učitavanju statistike po statusima: {ex.Message}");
            }
        }

        [HttpGet("statistika/ukupno")]
        public async Task<IActionResult> GetUkupnaStatistika([FromQuery] FilterRequest filterRequest)
        {
            try
            {
                var ukupno = await _preradaService.UcitajUkupanBrojSmenskihIzvestaja(filterRequest);
                var otvoreni = await _preradaService.UcitajBrojOtvorenihSmenskihIzvestaja();
                var zakljuceni = await _preradaService.UcitajBrojZakljucenihSmenskihIzvestaja();

                var statistika = new
                {
                    Ukupno = ukupno,
                    Otvoreni = otvoreni,
                    Zakljuceni = zakljuceni
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
