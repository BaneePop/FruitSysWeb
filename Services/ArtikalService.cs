using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using FruitSysWeb.Models;

    public class ArtikalService
    {
    private readonly string _connectionString;

    public ArtikalService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException(nameof(configuration), "Connection string not found.");
    }


    public class ProizvodnjaPodatak
    {
        public DateTime Datum { get; set; }
        public string? Komitent { get; set; }
        public string? BrojRadnogNaloga { get; set; }
        public string? VrstaProizvoda { get; set; }
        public decimal Kolicina { get; set; }
    }

    // METODE ZA ARTIKLE
    public async Task<List<Artikal>> UcitajSveArtikle()
    {
        using var connection = new MySqlConnection(_connectionString);

        var query = @"
                SELECT 
                    ID,
                    Naziv,
                    Tip,
                    PrvaKlasifikacijaID,
                    JedinicaMereID,
                    OtkupniArtikal
                FROM Artikal
                ORDER BY Naziv";

        try
        {
            var result = await connection.QueryAsync<Artikal>(query);
            return result.AsList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Greška pri učitavanju artikala: {ex.Message}");
            return new List<Artikal>();
        }
    }

}