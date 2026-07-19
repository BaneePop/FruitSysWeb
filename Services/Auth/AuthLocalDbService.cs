using Dapper;
using Microsoft.Data.Sqlite;

namespace FruitSysWeb.Services.Auth;

public class KorisnikRecord
{
    public long Id { get; set; }
    public string Ime { get; set; } = string.Empty;
    public string LozinkaHash { get; set; } = string.Empty;
    public string LozinkaSalt { get; set; } = string.Empty;
    public int Iteracije { get; set; }
    public bool Aktivan { get; set; }
}

/// <summary>
/// Lokalna SQLite baza za korisnike (Data/app.db) — odvojena od Data/solar.db
/// da polling na solar.db (svakih 60s) ne pravi lock-contention sa retkim auth upitima.
/// </summary>
public class AuthLocalDbService
{
    private readonly string _connectionString;
    private readonly ILogger<AuthLocalDbService> _logger;

    public AuthLocalDbService(IWebHostEnvironment env, ILogger<AuthLocalDbService> logger)
    {
        _logger = logger;

        var dataFolder = Path.Combine(env.ContentRootPath, "Data");
        Directory.CreateDirectory(dataFolder);

        var dbPath = Path.Combine(dataFolder, "app.db");
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = dbPath,
            Cache = SqliteCacheMode.Shared
        }.ToString();

        InitializeSchema();
    }

    private SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var pragma = connection.CreateCommand();
        pragma.CommandText = "PRAGMA journal_mode=WAL;";
        pragma.ExecuteNonQuery();
        return connection;
    }

    private void InitializeSchema()
    {
        using var connection = OpenConnection();
        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS korisnici (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                ime TEXT NOT NULL UNIQUE COLLATE NOCASE,
                lozinka_hash TEXT NOT NULL,
                lozinka_salt TEXT NOT NULL,
                iteracije INTEGER NOT NULL,
                aktivan INTEGER NOT NULL DEFAULT 1,
                kreirano TEXT NOT NULL DEFAULT (datetime('now')),
                azurirano TEXT NOT NULL DEFAULT (datetime('now'))
            );
        ");

        _logger.LogInformation("Auth SQLite šema inicijalizovana ({Path})", connection.DataSource);
    }

    public async Task<KorisnikRecord?> GetByImeAsync(string ime, CancellationToken ct = default)
    {
        using var connection = OpenConnection();
        var row = await connection.QueryFirstOrDefaultAsync<dynamic>(new CommandDefinition(@"
            SELECT id, ime, lozinka_hash, lozinka_salt, iteracije, aktivan
            FROM korisnici
            WHERE ime = @Ime
            LIMIT 1", new { Ime = ime }, cancellationToken: ct));

        return row == null ? null : MapRow(row);
    }

    public async Task<List<KorisnikRecord>> ListUsersAsync(CancellationToken ct = default)
    {
        using var connection = OpenConnection();
        var rows = await connection.QueryAsync<dynamic>(new CommandDefinition(@"
            SELECT id, ime, lozinka_hash, lozinka_salt, iteracije, aktivan
            FROM korisnici
            ORDER BY ime", cancellationToken: ct));

        return rows.Select(MapRow).ToList();
    }

    public async Task<bool> CreateUserAsync(string ime, string lozinka, CancellationToken ct = default)
    {
        var (hash, salt, iterations) = PasswordHasher.Hash(lozinka);

        using var connection = OpenConnection();
        try
        {
            await connection.ExecuteAsync(new CommandDefinition(@"
                INSERT INTO korisnici (ime, lozinka_hash, lozinka_salt, iteracije)
                VALUES (@Ime, @Hash, @Salt, @Iterations)",
                new { Ime = ime, Hash = hash, Salt = salt, Iterations = iterations }, cancellationToken: ct));
            return true;
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19) // UNIQUE constraint
        {
            return false;
        }
    }

    public async Task<bool> UpdatePasswordAsync(string ime, string novaLozinka, CancellationToken ct = default)
    {
        var (hash, salt, iterations) = PasswordHasher.Hash(novaLozinka);

        using var connection = OpenConnection();
        var affected = await connection.ExecuteAsync(new CommandDefinition(@"
            UPDATE korisnici
            SET lozinka_hash = @Hash, lozinka_salt = @Salt, iteracije = @Iterations, azurirano = datetime('now')
            WHERE ime = @Ime",
            new { Ime = ime, Hash = hash, Salt = salt, Iterations = iterations }, cancellationToken: ct));

        return affected > 0;
    }

    public async Task<bool> SetAktivanAsync(string ime, bool aktivan, CancellationToken ct = default)
    {
        using var connection = OpenConnection();
        var affected = await connection.ExecuteAsync(new CommandDefinition(@"
            UPDATE korisnici SET aktivan = @Aktivan, azurirano = datetime('now') WHERE ime = @Ime",
            new { Ime = ime, Aktivan = aktivan ? 1 : 0 }, cancellationToken: ct));

        return affected > 0;
    }

    private static KorisnikRecord MapRow(dynamic row) => new()
    {
        Id = (long)row.id,
        Ime = (string)row.ime,
        LozinkaHash = (string)row.lozinka_hash,
        LozinkaSalt = (string)row.lozinka_salt,
        Iteracije = (int)(long)row.iteracije,
        Aktivan = (long)row.aktivan == 1
    };
}
