using Dapper;
using MySqlConnector;
using FruitSysWeb.Models;
using Microsoft.Extensions.Configuration;

namespace FruitSysWeb.Services
{
    public class IzvestajService
    {
        private readonly string _connectionString;

        public IzvestajService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                             ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        public async Task<List<ProizvodnjaModel>> UcitajIzvestajProizvodnje(
            DateTime? odDatum, DateTime? doDatum, string? RadniNalog)
        {
            using var connection = new MySqlConnection(_connectionString);

            var query = @"
                SELECT 
                    vpp.RadniNalogID,
                    vpp.RadniNalog,
                    vpp.Artikal,
                    vpp.Komitent,
                    vpp.ArtikalPrvaKlasifikacija AS Klasifikacija,
                    vpp.Kolicina,
                    rn.DatumPocetka AS Datum,
                    vpp.RpArtikalTip AS TipArtikla
                FROM vPreradaPregled vpp
                LEFT JOIN RadniNalog rn ON vpp.RadniNalogID = rn.ID
                WHERE 1=1";

        var parameters = new DynamicParameters();

            if (odDatum.HasValue)
            {
                query += " AND rn.DatumIsporuke >= @OdDatum";
                parameters.Add("OdDatum", odDatum.Value);
            }

            if (doDatum.HasValue)
            {
                query += " AND rn.DatumIsporuke <= @DoDatum";
                parameters.Add("DoDatum", doDatum.Value);
            }

            if (!string.IsNullOrEmpty(RadniNalog))
            {
                query += " AND vpp.RadniNalog LIKE @RadniNalog";
                parameters.Add("RadniNalog", $"%{RadniNalog}%");
            }

            query += " ORDER BY rn.DatumIsporuke DESC, vpp.RadniNalog";

            try
            {
                var result = await connection.QueryAsync<ProizvodnjaModel>(query, parameters);
                return result.AsList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju podataka: {ex.Message}");
                return new List<ProizvodnjaModel>();
            }
        }

        // Ostale metode za proizvodnju - sada bez async jer ne koriste await
    public Task<List<ProizvodnjaModel>> UcitajIzvestajPoKomitentu(
            int komitentId, DateTime? odDatum, DateTime? doDatum)
        {
            // Implementacija ostaje ista
            return Task.FromResult(new List<ProizvodnjaModel>());
        }

        public Task<List<ProizvodnjaModel>> UcitajIzvestajPoArtiklu(
            int artikalId, DateTime? odDatum, DateTime? doDatum)
        {
            // Implementacija ostaje ista
            return Task.FromResult(new List<ProizvodnjaModel>());
        }
    




    public async Task<List<FinansijeModel>> UcitajFinansijskiIzvestaj(
    DateTime? odDatum, DateTime? doDatum, int? komitentId = null, int? artikalId = null)
        {
        using var connection = new MySqlConnection(_connectionString);


        var query = @"
                 SELECT 
                    fm.ID,
                    fm.KomitentID,
                    fm.Komitent,
                    fm.Otkupljivac,
                    fm.Proizvodjac,
                    fm.Dobavljac,
                    fm.Kupac,
                    fm.DokumentID,
                    fm.Datum,
                    fm.Dokument,
                    fm.DokumentTip,
                    fm.DokumentStatus,
                    fm.ArtikalID,
                    fm.Artikal,
                    fm.ArtikalPrvaKlasifikacijaID,
                    fm.Kolicina,
                    fm.Potrazuje,
                    fm.Duguje,
                    fm.Saldo,
                    fm.Roba,
                    fm.Provizija,
                    fm.Prevoz,
                    fm.Marza,
                    fm.PCenaPrijem,
                    fm.PCenaUkupno,
                    fm.Uplata,
                    fm.PorezIznos,
                    fm.PorezIznos2,
                    fm.UplataPdv,
                    fm.NetoIznosOtkup,
                    -- Dodaj podatke iz komitent tabele
                    k.FizickoLice,
                    -- Dodaj podatke iz artikal tabele
                    a.Tip AS ArtikalTip,
                    a.JedinicaMereID,
                    a.OtkupniArtikal
                FROM vPrometFinansijev9 fm
                LEFT JOIN Komitent k ON fm.KomitentID = k.ID
                LEFT JOIN Artikal a ON fm.ArtikalID = a.ID
                WHERE 1=1";

         var parameters = new DynamicParameters();

          if (odDatum.HasValue)
        {
        query += " AND fm.Datum >= @OdDatum";
        parameters.Add("OdDatum", odDatum.Value);
        }

        if (doDatum.HasValue)
        {
        query += " AND fm.Datum <= @DoDatum";
        parameters.Add("DoDatum", doDatum.Value);
        }

        if (komitentId.HasValue)
        {
        query += " AND fm.KomitentID = @KomitentId";
        parameters.Add("KomitentId", komitentId.Value);
        }

        if (artikalId.HasValue)
        {
        query += " AND fm.ArtikalID = @ArtikalId";
        parameters.Add("ArtikalId", artikalId.Value);
        }

        query += " ORDER BY fm.Datum DESC, fm.Dokument";

        try
        {
        var result = await connection.QueryAsync<FinansijeModel>(query, parameters);
        return result.AsList();
        }
        catch (Exception ex)
        {
        Console.WriteLine($"Greška pri učitavanju finansijskih podataka: {ex.Message}");
        return new List<FinansijeModel>();
         }
    }

    public async Task<decimal> UcitajUkupnoSaldo(DateTime? odDatum, DateTime? doDatum)
        {
        using var connection = new MySqlConnection(_connectionString);

        var query = @"
            SELECT SUM(fm.Saldo - fm.Uplata) AS UkupnoNetoSaldo
            FROM vPrometFinansijev9 fm
            WHERE 1=1";

        var parameters = new DynamicParameters();

        if (odDatum.HasValue)
        {
        query += " AND fm.Datum >= @OdDatum";
        parameters.Add("OdDatum", odDatum.Value);
        }

        if (doDatum.HasValue)
        {
        query += " AND fm.Datum <= @DoDatum";
        parameters.Add("DoDatum", doDatum.Value);
        }

        try
        {
        var result = await connection.ExecuteScalarAsync<decimal?>(query, parameters);
        return result ?? 0;
        }
        catch (Exception ex)
        {
        Console.WriteLine($"Greška pri učitavanju ukupnog salda: {ex.Message}");
        return 0;
        }
    }


                
    }
}