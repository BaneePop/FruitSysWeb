using Dapper;
using MySqlConnector;
using FruitSysWeb.Services.Interfaces;
using FruitSysWeb.Services.Models.Requests;
using FruitSysWeb.Models;
using Microsoft.Extensions.Configuration;

namespace FruitSysWeb.Services.Implementations.IzvestajService
{
    public class FinansijeService : IFinansijeService
    {
        private readonly string _connectionString;

        public FinansijeService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string not found.");
        }

        public async Task<List<FinansijeModel>> UcitajFinansijskiIzvestaj(FilterRequest filter)
        {
            using var connection = new MySqlConnection(_connectionString);

            var query = @"
                SELECT 
                    fm.ID, fm.KomitentID, fm.Komitent, fm.Otkupljivac, fm.Proizvodjac,
                    fm.Dobavljac, fm.Kupac, fm.DokumentID, fm.Datum, fm.Dokument,
                    fm.DokumentTip, fm.DokumentStatus, fm.ArtikalID, fm.Artikal,
                    fm.ArtikalPrvaKlasifikacijaID, fm.Kolicina, fm.Potrazuje,
                    fm.Duguje, fm.Saldo, fm.Roba, fm.Provizija, fm.Prevoz,
                    fm.Marza, fm.PCenaPrijem, fm.PCenaUkupno, fm.Uplata,
                    fm.PorezIznos, fm.PorezIznos2, fm.UplataPdv, fm.NetoIznosOtkup,
                    k.FizickoLice, a.Tip AS ArtikalTip, a.JedinicaMereID, a.OtkupniArtikal
                FROM vPrometFinansijev9 fm
                LEFT JOIN Komitent k ON fm.KomitentID = k.ID
                LEFT JOIN Artikal a ON fm.ArtikalID = a.ID
                WHERE 1=1";

            var parameters = new DynamicParameters();

            if (filter.OdDatum.HasValue)
            {
                query += " AND fm.Datum >= @OdDatum";
                parameters.Add("OdDatum", filter.OdDatum.Value);
            }

            if (filter.DoDatum.HasValue)
            {
                query += " AND fm.Datum <= @DoDatum";
                parameters.Add("DoDatum", filter.DoDatum.Value);
            }

            if (filter.KomitentId.HasValue)
            {
                query += " AND fm.KomitentID = @KomitentId";
                parameters.Add("KomitentId", filter.KomitentId.Value);
            }

            if (!string.IsNullOrEmpty(filter.KomitentTip))
            {
                switch (filter.KomitentTip)
                {
                    case "Kupac": query += " AND fm.Kupac = true"; break;
                    case "Dobavljac": query += " AND fm.Dobavljac = true"; break;
                    case "Proizvodjac": query += " AND fm.Proizvodjac = true"; break;
                    case "Otkupljivac": query += " AND fm.Otkupljivac = true"; break;
                }
            }

            if (filter.ArtikalId.HasValue)
            {
                query += " AND fm.ArtikalID = @ArtikalId";
                parameters.Add("ArtikalId", filter.ArtikalId.Value);
            }

            if (!string.IsNullOrEmpty(filter.ArtikalTip))
            {
                query += " AND a.Tip = @ArtikalTip";
                parameters.Add("ArtikalTip", filter.ArtikalTip);
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

        public async Task<decimal> UcitajUkupnoSaldo(FilterRequest filter)
        {
            using var connection = new MySqlConnection(_connectionString);

            var query = @"
                SELECT SUM(fm.Saldo - fm.Uplata) AS UkupnoNetoSaldo
                FROM vPrometFinansijev9 fm
                WHERE 1=1";

            var parameters = new DynamicParameters();

            if (filter.OdDatum.HasValue)
            {
                query += " AND fm.Datum >= @OdDatum";
                parameters.Add("OdDatum", filter.OdDatum.Value);
            }

            if (filter.DoDatum.HasValue)
            {
                query += " AND fm.Datum <= @DoDatum";
                parameters.Add("DoDatum", filter.DoDatum.Value);
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

        public async Task<Dictionary<string, decimal>> UcitajTopKupce(FilterRequest filter)
        {
            using var connection = new MySqlConnection(_connectionString);

            var query = @"
                SELECT Komitent, SUM(Saldo) as UkupnoSaldo
                FROM vPrometFinansijev9 
                WHERE Kupac = true
                AND (@OdDatum IS NULL OR Datum >= @OdDatum)
                AND (@DoDatum IS NULL OR Datum <= @DoDatum)
                GROUP BY Komitent 
                ORDER BY UkupnoSaldo DESC 
                LIMIT 10";

            var parameters = new
            {
                OdDatum = filter.OdDatum,
                DoDatum = filter.DoDatum
            };

            var result = await connection.QueryAsync<(string Komitent, decimal UkupnoSaldo)>(query, parameters);
            return result.ToDictionary(x => x.Komitent, x => x.UkupnoSaldo);
        }

        public async Task<Dictionary<string, decimal>> UcitajTopDobavljace(FilterRequest filter)
        {
            using var connection = new MySqlConnection(_connectionString);

            var query = @"
                SELECT Komitent, SUM(Saldo) as UkupnoSaldo
                FROM vPrometFinansijev9 
                WHERE Dobavljac = true
                AND (@OdDatum IS NULL OR Datum >= @OdDatum)
                AND (@DoDatum IS NULL OR Datum <= @DoDatum)
                GROUP BY Komitent 
                ORDER BY UkupnoSaldo DESC 
                LIMIT 10";

            var parameters = new
            {
                OdDatum = filter.OdDatum,
                DoDatum = filter.DoDatum
            };

            var result = await connection.QueryAsync<(string Komitent, decimal UkupnoSaldo)>(query, parameters);
            return result.ToDictionary(x => x.Komitent, x => x.UkupnoSaldo);
        }

        // Ostale metode iz IIzvestajService interfejsa
        public Task<List<ProizvodnjaModel>> UcitajIzvestajProizvodnje(FilterRequest filter)
        {
            // Ovo će biti implementirano u ProizvodnjaService
            return Task.FromResult(new List<ProizvodnjaModel>());
        }

        public Task<Dictionary<string, decimal>> UcitajRashodePoKategorijama(FilterRequest filter)
        {
            // Implementacija po potrebi
            return Task.FromResult(new Dictionary<string, decimal>());
        }
    }
}