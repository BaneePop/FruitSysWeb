using Dapper;
using FruitSysWeb.Models;
using FruitSysWeb.Services.Core;
using FruitSysWeb.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace FruitSysWeb.Services.Implementations.IzvestajService;

/// <summary>
/// Servis za finansijski pregled - kompletan izveštaj
/// </summary>
public class FinansijskiPregledService : IFinansijskiPregledService
{
    private readonly DatabaseService _db;
        private readonly ILogger<FinansijskiPregledService> _logger;

    public FinansijskiPregledService(DatabaseService db,
            ILogger<FinansijskiPregledService> logger)
    {
        _db = db;
            _logger = logger;
    }

    /// <summary>
    /// Učitava robu na zalihama grupisano po vrsti voća
    /// </summary>
    public async Task<List<RobaZalihaModel>> UcitajRobuNaZalihama(DateTime odDatum, DateTime doDatum)
    {
        try
        {
            // Lista vrsta voća
            var vrsteVoca = new List<string> { "Malina", "Kupina", "Šljiva", "Višnja", "Kajsija", "Jagoda", "Borovnica" };
            var rezultat = new List<RobaZalihaModel>();

            foreach (var vrsta in vrsteVoca)
            {
                var model = await UcitajPojedincanuVrstuVoca(vrsta, odDatum, doDatum);

                // Dodaj samo ako ima bilo kakve podatke
                if (model.NabavkaKolicina > 0 || model.ProdajaKolicina > 0 || model.LagerKolicina > 0)
                {
                    rezultat.Add(model);
                }
            }

            _logger.LogInformation($"Učitano {rezultat.Count} vrsta voća za roba na zalihama");

            return rezultat;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Greška u UcitajRobuNaZalihama");
            _logger.LogInformation($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    /// <summary>
    /// Učitava podatke za jednu vrstu voća
    /// </summary>
    private async Task<RobaZalihaModel> UcitajPojedincanuVrstuVoca(string vrstaVoca, DateTime odDatum, DateTime doDatum)
    {
        var model = new RobaZalihaModel
        {
            VrstaVoca = vrstaVoca
        };

        // WHERE uslovi za vrstu voća
        var whereUslov = GetWhereUslovZaVrstuVoca(vrstaVoca);

        // 1. NABAVKA (KL- dokumenti) - SAMO artikli sa cenom > 0
        var sqlNabavka = $@"
            SELECT 
                COALESCE(SUM(ABS(fm.Kolicina)), 0) as Kolicina,
                COALESCE(SUM(fm.Potrazuje), 0) as Vrednost,
                CASE 
                    WHEN SUM(ABS(fm.Kolicina)) > 0 
                    THEN AVG(fm.PCenaUkupno) 
                    ELSE 0 
                END as ProsecnaCena
            FROM vPrometFinansijev9 fm
            LEFT JOIN Artikal a ON fm.ArtikalID = a.ID
            WHERE fm.Dokument LIKE 'KL-%'
              AND fm.Datum >= @OdDatum
              AND fm.Datum <= @DoDatum
              AND fm.PCenaUkupno > 0
              AND a.Aktivno = 1
              AND a.MagacinID != 7
              AND ({whereUslov})";

        var parametersNabavka = new DynamicParameters();
        parametersNabavka.Add("@OdDatum", odDatum);
        parametersNabavka.Add("@DoDatum", doDatum);
        parametersNabavka.Add("@VrstaVoca", vrstaVoca);

        var nabavka = await _db.QueryFirstOrDefaultAsync<dynamic>(sqlNabavka, parametersNabavka);

        if (nabavka != null)
        {
            model.NabavkaKolicina = nabavka.Kolicina ?? 0;
            model.NabavkaVrednost = nabavka.Vrednost ?? 0;
            model.ProsecnaNabavnaCena = nabavka.ProsecnaCena ?? 0;
        }

        // 2. PRODAJA (FK- dokumenti) - SAMO artikli sa cenom > 0
        var sqlProdaja = $@"
            SELECT 
                COALESCE(SUM(ABS(fm.Kolicina)), 0) as Kolicina,
                COALESCE(SUM(fm.Duguje), 0) as Vrednost,
                CASE 
                    WHEN SUM(ABS(fm.Kolicina)) > 0 
                    THEN AVG(fm.PCenaUkupno) 
                    ELSE 0 
                END as ProsecnaCena
            FROM vPrometFinansijev9 fm
            LEFT JOIN Artikal a ON fm.ArtikalID = a.ID
            WHERE fm.Dokument LIKE 'FK-%'
              AND fm.Datum >= @OdDatum
              AND fm.Datum <= @DoDatum
              AND fm.PCenaUkupno > 0
              AND a.Aktivno = 1
              AND a.MagacinID != 7
              AND ({whereUslov})";

        var parametersProdaja = new DynamicParameters();
        parametersProdaja.Add("@OdDatum", odDatum);
        parametersProdaja.Add("@DoDatum", doDatum);
        parametersProdaja.Add("@VrstaVoca", vrstaVoca);

        var prodaja = await _db.QueryFirstOrDefaultAsync<dynamic>(sqlProdaja, parametersProdaja);

        if (prodaja != null)
        {
            model.ProdajaKolicina = prodaja.Kolicina ?? 0;
            model.ProdajaVrednost = prodaja.Vrednost ?? 0;
            model.ProsecnaProdajnaCena = prodaja.ProsecnaCena ?? 0;
        }

        // 3. LAGER (vwMagacinLager) - grupisano po vrsti
        var sqlLager = $@"
            SELECT 
                COALESCE(SUM(ml.Kolicina), 0) as LagerKolicina
            FROM vwMagacinLager ml
            LEFT JOIN Artikal a ON ml.ArtikalID = a.ID
            WHERE a.Aktivno = 1
              AND a.MagacinID != 7
              AND ml.Kolicina > 0
              AND ({whereUslov})";

        var parametersLager = new DynamicParameters();
        parametersLager.Add("@VrstaVoca", vrstaVoca);

        var lagerKolicina = await _db.ExecuteScalarAsync<decimal>(sqlLager, parametersLager);
        model.LagerKolicina = lagerKolicina;

        // 4. LAGER VREDNOST (iz KalkulacijaArtikalCena)
        if (model.LagerKolicina > 0)
        {
            model.LagerVrednost = await IzracunajLagerVrednostIzKalkulacije(vrstaVoca, model.LagerKolicina);
        }

        return model;
    }

    /// <summary>
    /// Vraća WHERE uslov za određenu vrstu voća
    /// </summary>
    private string GetWhereUslovZaVrstuVoca(string vrstaVoca)
    {
        return vrstaVoca switch
        {
            "Malina" => "a.Naziv LIKE '%Malina%'",
            "Kupina" => "a.Naziv LIKE '%Kupina%'",
            "Šljiva" => "(a.Naziv LIKE '%Šljiva%' OR a.Naziv LIKE '%Stenley%' OR a.Naziv LIKE '%Čačanska%' OR a.Naziv LIKE '%Požegača%')",
            "Višnja" => "(a.Naziv LIKE '%Višnja%' OR a.Naziv LIKE '%Visnja%')",
            "Borovnica" => "a.Naziv LIKE '%Borovnica%'",
            "Kajsija" => "a.Naziv LIKE '%Kajsija%'",
            "Jagoda" => "a.Naziv LIKE '%Jagoda%'",
            _ => "1=0"  // Nema match
        };
    }

    /// <summary>
    /// Računa lager vrednost iz KalkulacijaArtikalCena tabele
    /// </summary>
    private async Task<decimal> IzracunajLagerVrednostIzKalkulacije(string vrstaVoca, decimal kolicina)
    {
        if (kolicina <= 0) return 0;

        try
        {
            // Pronađi reprezentativni artikal za tu vrstu voća
            string artikalNazivPattern = vrstaVoca switch
            {
                "Malina" => GetArtikalPatternZaMalinu(),
                "Kupina" => GetArtikalPatternZaKupinu(),
                "Šljiva" => GetArtikalPatternZaSljivu(),
                "Višnja" => GetArtikalPatternZaVisnju(),
                "Borovnica" => GetArtikalPatternZaBorovnicu(),
                "Kajsija" => GetArtikalPatternZaKajsiju(),
                "Jagoda" => GetArtikalPatternZaJagodu(),
                _ => ""
            };

            if (string.IsNullOrEmpty(artikalNazivPattern)) return 0;

            // Pronađi najnoviju BrutoCenu iz KalkulacijaArtikalCena
            var sql = $@"
                SELECT kac.BrutoCena
                FROM KalkulacijaArtikalCena kac
                LEFT JOIN Artikal a ON kac.ArtikalID = a.ID
                WHERE {artikalNazivPattern}
                  AND kac.BrutoCena > 0
                  AND a.Aktivno = 1
                ORDER BY kac.Azurirano DESC, kac.ID DESC
                LIMIT 1";

            var brutoCena = await _db.ExecuteScalarAsync<decimal>(sql);

            return kolicina * brutoCena;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Greška u IzracunajLagerVrednostIzKalkulacije za {VrstaVoca}", vrstaVoca);
            return 0;
        }
    }

    private string GetArtikalPatternZaMalinu()
    {
        return @"(
            (a.Naziv LIKE '%D/Z Malina%' AND a.MagacinID != 6) OR
            (a.Naziv LIKE '%Malina Rollend%' AND a.MagacinID = 6)
        )";
    }

    private string GetArtikalPatternZaKupinu()
    {
        return @"(
            (a.Naziv LIKE '%D/Z Kupina%' AND a.MagacinID != 6) OR
            (a.Naziv LIKE '%Kupina Rollend%' AND a.MagacinID = 6)
        )";
    }

    private string GetArtikalPatternZaSljivu()
    {
        return @"(
            a.Naziv LIKE '%Šljiva%' OR 
            a.Naziv LIKE '%Stenley%' OR 
            a.Naziv LIKE '%Čačanska%' OR 
            a.Naziv LIKE '%Požegača%'
        )";
    }

    private string GetArtikalPatternZaVisnju()
    {
        return @"(
            (a.Naziv LIKE '%D/Z Višnja%' AND a.MagacinID != 6) OR
            (a.Naziv LIKE '%Višnja S/K%' AND a.MagacinID = 6) OR
            (a.Naziv LIKE '%D/Z Visnja%' AND a.MagacinID != 6) OR
            (a.Naziv LIKE '%Visnja S/K%' AND a.MagacinID = 6)
        )";
    }

    private string GetArtikalPatternZaBorovnicu()
    {
        return "a.Naziv LIKE '%Borovnica%'";
    }

    private string GetArtikalPatternZaKajsiju()
    {
        return "a.Naziv LIKE '%Kajsija%'";
    }

    private string GetArtikalPatternZaJagodu()
    {
        return "a.Naziv LIKE '%Jagoda%'";
    }

    public async Task<List<ObracunOtkupaModel>> UcitajObracunOtkupa(DateTime odDatum, DateTime doDatum)
    {
        try
        {
            var sql = @"
                SELECT 
                    k.ID as KomitentID,
                    k.Naziv as Dobavljac,
                    COALESCE(SUM(CASE 
                        WHEN fm.Dokument LIKE 'KL-%'
                            AND fm.Datum >= @OdDatum 
                            AND fm.Datum <= @DoDatum 
                            AND fm.PCenaUkupno > 0
                        THEN fm.Potrazuje 
                        ELSE 0 
                    END), 0) as UkupnoZaduzenje,
                    COALESCE(SUM(CASE 
                        WHEN (fm.Dokument LIKE 'IS-%' OR fm.Dokument LIKE 'UP-%')
                            AND fm.Datum >= @OdDatum 
                            AND fm.Datum <= @DoDatum 
                        THEN fm.Uplata 
                        ELSE 0 
                    END), 0) as UkupnoIsplata
                FROM Komitent k
                LEFT JOIN vPrometFinansijev9 fm ON k.ID = fm.KomitentID
                LEFT JOIN Artikal a ON fm.ArtikalID = a.ID
                WHERE k.Aktivan = 1
                  AND a.MagacinID IN (2, 3)
                GROUP BY k.ID, k.Naziv
                HAVING UkupnoZaduzenje > 0
                ORDER BY UkupnoZaduzenje DESC
                LIMIT 50";

            var parameters = new DynamicParameters();
            parameters.Add("@OdDatum", odDatum);
            parameters.Add("@DoDatum", doDatum);

            var rezultat = await _db.QueryAsync<ObracunOtkupaModel>(sql, parameters);

            var lista = rezultat?.ToList() ?? new List<ObracunOtkupaModel>();
            foreach (var item in lista)
            {
                item.Stanje = item.UkupnoZaduzenje - item.UkupnoIsplata;
                item.UkupnoZaduzenjeEur = item.UkupnoZaduzenje / 117.5m;
                item.UkupnoIsplataEur = item.UkupnoIsplata / 117.5m;
                item.StanjeEur = item.Stanje / 117.5m;
            }

            return lista;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Greška u UcitajObracunOtkupa");
            throw;
        }
    }

    public async Task<ObracunSvaRobaModel> UcitajObracunSvaRoba(DateTime odDatum, DateTime doDatum)
    {
        try
        {
            var model = new ObracunSvaRobaModel();

            var sqlVoce = @"
                SELECT COALESCE(SUM(fm.Potrazuje), 0) as Vrednost
                FROM vPrometFinansijev9 fm
                LEFT JOIN Artikal a ON fm.ArtikalID = a.ID
                WHERE fm.Dokument LIKE 'KL-%'
                  AND fm.Datum >= @OdDatum
                  AND fm.Datum <= @DoDatum
                  AND fm.PCenaUkupno > 0
                  AND a.MagacinID = 3
                  AND a.Aktivno = 1";

            var parametersVoce = new DynamicParameters();
            parametersVoce.Add("@OdDatum", odDatum);
            parametersVoce.Add("@DoDatum", doDatum);
            model.NabavnaVrednostVoca = await _db.ExecuteScalarAsync<decimal>(sqlVoce, parametersVoce);

            var sqlAmbalaza = @"
                SELECT COALESCE(SUM(fm.Potrazuje), 0) as Vrednost
                FROM vPrometFinansijev9 fm
                LEFT JOIN Artikal a ON fm.ArtikalID = a.ID
                WHERE fm.Dokument LIKE 'KL-%'
                  AND fm.Datum >= @OdDatum
                  AND fm.Datum <= @DoDatum
                  AND fm.PCenaUkupno > 0
                  AND a.MagacinID = 4
                  AND a.Aktivno = 1";

            var parametersAmbalaza = new DynamicParameters();
            parametersAmbalaza.Add("@OdDatum", odDatum);
            parametersAmbalaza.Add("@DoDatum", doDatum);
            model.NabavnaVrednostAmbalaze = await _db.ExecuteScalarAsync<decimal>(sqlAmbalaza, parametersAmbalaza);

            var sqlPrerada = @"
                SELECT COALESCE(SUM(er.TrosakRada), 0) as Trosak
                FROM EvidencijaRada er
                LEFT JOIN RadniNalog rn ON er.RadniNalogID = rn.ID
                WHERE er.Datum >= @OdDatum
                  AND er.Datum <= @DoDatum
                  AND rn.Status != 4";

            var parametersPrerada = new DynamicParameters();
            parametersPrerada.Add("@OdDatum", odDatum);
            parametersPrerada.Add("@DoDatum", doDatum);
            model.UkupanTrosakPrerade = await _db.ExecuteScalarAsync<decimal>(sqlPrerada, parametersPrerada);

            var sqlProdaja = @"
                SELECT COALESCE(SUM(fm.Duguje), 0) as Vrednost
                FROM vPrometFinansijev9 fm
                LEFT JOIN Artikal a ON fm.ArtikalID = a.ID
                WHERE fm.Dokument LIKE 'FK-%'
                  AND fm.Datum >= @OdDatum
                  AND fm.Datum <= @DoDatum
                  AND fm.PCenaUkupno > 0
                  AND a.MagacinID = 6
                  AND a.Aktivno = 1";

            var parametersProdaja = new DynamicParameters();
            parametersProdaja.Add("@OdDatum", odDatum);
            parametersProdaja.Add("@DoDatum", doDatum);
            model.UkupnaProdaja = await _db.ExecuteScalarAsync<decimal>(sqlProdaja, parametersProdaja);

            var robaZalihe = await UcitajRobuNaZalihama(odDatum, doDatum);
            model.VrednostRobeNaZalihama = robaZalihe.Sum(x => x.LagerVrednost);

            var sqlKgTotal = @"
                SELECT COALESCE(SUM(ABS(fm.Kolicina)), 1) as UkupnoKg
                FROM vPrometFinansijev9 fm
                LEFT JOIN Artikal a ON fm.ArtikalID = a.ID
                WHERE fm.Dokument LIKE 'FK-%'
                  AND fm.Datum >= @OdDatum
                  AND fm.Datum <= @DoDatum
                  AND fm.PCenaUkupno > 0
                  AND a.MagacinID = 6";

            var parametersKg = new DynamicParameters();
            parametersKg.Add("@OdDatum", odDatum);
            parametersKg.Add("@DoDatum", doDatum);

            var ukupnoKg = await _db.ExecuteScalarAsync<decimal>(sqlKgTotal, parametersKg);
            model.ProfitPoKg = ukupnoKg > 0 ? model.Profit / ukupnoKg : 0;

            return model;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Greška u UcitajObracunSvaRoba");
            throw;
        }
    }

    public async Task<decimal> IzracunajProsecnuCenu(int artikalId)
    {
        try
        {
            var sql = @"
                SELECT kac.BrutoCena
                FROM KalkulacijaArtikalCena kac
                WHERE kac.ArtikalID = @ArtikalID
                  AND kac.BrutoCena > 0
                ORDER BY kac.Azurirano DESC, kac.ID DESC
                LIMIT 1";

            var parameters = new DynamicParameters();
            parameters.Add("@ArtikalID", artikalId);

            return await _db.ExecuteScalarAsync<decimal>(sql, parameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Greška u IzracunajProsecnuCenu");
            return 0;
        }
    }
}
