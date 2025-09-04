using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using FruitSysWeb.Models;

namespace FruitSysWeb.Services
{
    public class KomitentService
    {
        private readonly string _connectionString;

        public KomitentService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(configuration), "Connection string not found.");
        }

        // METODE ZA KOMITENTE
    public async Task<List<Komitent>> UcitajSveKomitente()
        {
            using var connection = new MySqlConnection(_connectionString);
    
            var query = @"
                SELECT 
                    ID,
                    Naziv,
                    FizickoLice,
                    JeDobavljac,
                    JeKupac,
                    JeOtkupljivac
                FROM Komitent
                ORDER BY Naziv";  // UKLONJEN WHERE Aktivan = 1

                try
                {
                var result = await connection.QueryAsync<Komitent>(query);
                return result.AsList();
                }
                catch (Exception ex)
                {
                Console.WriteLine($"Greška pri učitavanju komitenata: {ex.Message}");
                return new List<Komitent>();
                }
}
        public async Task<List<Komitent>> UcitajKomitentePoTipu(bool jeDobavljac = false, bool jeKupac = false, bool jeOtkupljivac = false)
            {
            using var connection = new MySqlConnection(_connectionString);

            var query = @"
                SELECT 
                    ID,
                    Naziv,
                    FizickoLice,
                    JeDobavljac,
                    JeKupac,
                    JeOtkupljivac
                FROM Komitent
                WHERE Aktivan 1= 1";

            var parameters = new DynamicParameters();

            if (jeDobavljac)
            {
                query += " AND JeDobavljac = TRUE";
            }

            if (jeKupac)
            {
                query += " AND JeKupac = TRUE";
            }

            if (jeOtkupljivac)
            {
                query += " AND JeOtkupljivac = TRUE";
            }

            query += " ORDER BY Naziv";

            try
            {
                var result = await connection.QueryAsync<Komitent>(query, parameters);
                return result.AsList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju komitenata: {ex.Message}");
                return new List<Komitent>();
            }
        }

        public async Task<Komitent?> UcitajKomitentaPoId(int id)
        {
            using var connection = new MySqlConnection(_connectionString);

            var query = @"
                SELECT 
                    ID,
                    Naziv,
                    FizickoLice,
                    JeDobavljac,
                    JeKupac,
                    JeOtkupljivac
                FROM Komitent
                WHERE ID = @Id AND Aktivan = 1";

            var parameters = new DynamicParameters();
            parameters.Add("Id", id);

            try
            {
                var result = await connection.QueryFirstOrDefaultAsync<Komitent>(query, parameters);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri učitavanju komitenta: {ex.Message}");
                return null;
            }
        }

        // POSTOJEĆA METODA (sa Dapper poboljšanjem)
        public async Task<List<Komitent>> GetKomitentListAsync()
        {
            using var connection = new MySqlConnection(_connectionString);

            var query = @"
                SELECT 
                    ID,
                    Naziv
                FROM Komitent
                WHERE Aktivan = 1
                ORDER BY Naziv";

            try
            {
                var result = await connection.QueryAsync<Komitent>(query);
                return result.AsList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri dohvatanju liste komitenata: {ex.Message}");
                return new List<Komitent>();
            }
        }


        public async Task<List<FinansijeModel>> UcitajFinansijskiIzvestaj(
        DateTime? odDatum, DateTime? doDatum, int? komitentId = null, int? artikalId = null)
        {
        using var connection = new MySqlConnection(_connectionString);
    
            var query = @"
                SELECT 
                    pf.Datum,
                    pf.Dokument,
                    pf.KomitentID,
                    k.Naziv AS KomitentNaziv,
                    k.JeDobavljac,
                    k.JeKupac,
                    k.JeOtkupljivac,
                    k.FizickoLice,
                    pf.ArtikalID,
                    a.Naziv AS ArtikalNaziv,
                    a.Tip AS ArtikalTip,
                    pf.Kolicina,
                    pf.Roba,
                    pf.PCenaPrijem,
                    pf.PCenaUkupno,
                    pf.Potrazuje,
                    pf.Duguje,
                    pf.Uplata
                FROM vPrometFinansijev9 pf
                LEFT JOIN Komitent k ON pf.KomitentID = k.ID
                LEFT JOIN Artikal a ON pf.ArtikalID = a.ID
                WHERE 1=1";

        var parameters = new DynamicParameters();

    if (odDatum.HasValue)
    {
        query += " AND pf.Datum >= @OdDatum";
        parameters.Add("OdDatum", odDatum.Value);
    }

    if (doDatum.HasValue)
    {
        query += " AND pf.Datum <= @DoDatum";
        parameters.Add("DoDatum", doDatum.Value);
    }

    if (komitentId.HasValue)
    {
        query += " AND pf.KomitentID = @KomitentId";
        parameters.Add("KomitentId", komitentId.Value);
    }

    if (artikalId.HasValue)
    {
        query += " AND pf.ArtikalID = @ArtikalId";
        parameters.Add("ArtikalId", artikalId.Value);
    }

    query += " ORDER BY pf.Datum DESC, pf.Dokument";

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
        SELECT SUM(Potrazuje - Duguje - Uplata) AS UkupnoSaldo
        FROM vPregledFinansijev9
        WHERE 1=1";

    var parameters = new DynamicParameters();

    if (odDatum.HasValue)
    {
        query += " AND Datum >= @OdDatum";
        parameters.Add("OdDatum", odDatum.Value);
    }

    if (doDatum.HasValue)
    {
        query += " AND Datum <= @DoDatum";
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