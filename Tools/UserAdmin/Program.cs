using System.Data;
using Dapper;
using FruitSysWeb.Services.Auth;
using Microsoft.Data.Sqlite;

var dataFolder = Path.Combine(Directory.GetCurrentDirectory(), "Data");
Directory.CreateDirectory(dataFolder);
var dbPath = Path.Combine(dataFolder, "app.db");
var connectionString = new SqliteConnectionStringBuilder { DataSource = dbPath, Cache = SqliteCacheMode.Shared }.ToString();

using var connection = new SqliteConnection(connectionString);
connection.Open();
using (var pragma = connection.CreateCommand())
{
    pragma.CommandText = "PRAGMA journal_mode=WAL;";
    pragma.ExecuteNonQuery();
}
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

if (args.Length == 0)
{
    PrintUsage();
    return 1;
}

var komanda = args[0].ToLowerInvariant();

switch (komanda)
{
    case "add":
    {
        if (args.Length < 2) { Console.WriteLine("Nedostaje korisničko ime: add <ime>"); return 1; }
        var ime = args[1];
        var lozinka = CitajLozinku("Nova lozinka: ");
        var potvrda = CitajLozinku("Potvrdi lozinku: ");
        if (lozinka != potvrda) { Console.WriteLine("Lozinke se ne poklapaju."); return 1; }

        var (hash, salt, iteracije) = PasswordHasher.Hash(lozinka);
        try
        {
            connection.Execute(
                "INSERT INTO korisnici (ime, lozinka_hash, lozinka_salt, iteracije) VALUES (@Ime, @Hash, @Salt, @Iteracije)",
                new { Ime = ime, Hash = hash, Salt = salt, Iteracije = iteracije });
            Console.WriteLine($"Korisnik '{ime}' kreiran.");
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            Console.WriteLine($"Korisnik '{ime}' već postoji.");
            return 1;
        }
        break;
    }
    case "passwd":
    {
        if (args.Length < 2) { Console.WriteLine("Nedostaje korisničko ime: passwd <ime>"); return 1; }
        var ime = args[1];
        var lozinka = CitajLozinku("Nova lozinka: ");
        var potvrda = CitajLozinku("Potvrdi lozinku: ");
        if (lozinka != potvrda) { Console.WriteLine("Lozinke se ne poklapaju."); return 1; }

        var (hash, salt, iteracije) = PasswordHasher.Hash(lozinka);
        var affected = connection.Execute(
            "UPDATE korisnici SET lozinka_hash=@Hash, lozinka_salt=@Salt, iteracije=@Iteracije, azurirano=datetime('now') WHERE ime=@Ime",
            new { Ime = ime, Hash = hash, Salt = salt, Iteracije = iteracije });
        Console.WriteLine(affected > 0 ? $"Lozinka za '{ime}' promenjena." : $"Korisnik '{ime}' ne postoji.");
        break;
    }
    case "disable":
    {
        if (args.Length < 2) { Console.WriteLine("Nedostaje korisničko ime: disable <ime>"); return 1; }
        var ime = args[1];
        var affected = connection.Execute(
            "UPDATE korisnici SET aktivan=0, azurirano=datetime('now') WHERE ime=@Ime", new { Ime = ime });
        Console.WriteLine(affected > 0 ? $"Korisnik '{ime}' deaktiviran." : $"Korisnik '{ime}' ne postoji.");
        break;
    }
    case "enable":
    {
        if (args.Length < 2) { Console.WriteLine("Nedostaje korisničko ime: enable <ime>"); return 1; }
        var ime = args[1];
        var affected = connection.Execute(
            "UPDATE korisnici SET aktivan=1, azurirano=datetime('now') WHERE ime=@Ime", new { Ime = ime });
        Console.WriteLine(affected > 0 ? $"Korisnik '{ime}' aktiviran." : $"Korisnik '{ime}' ne postoji.");
        break;
    }
    case "list":
    {
        var korisnici = connection.Query("SELECT ime, aktivan, kreirano FROM korisnici ORDER BY ime");
        Console.WriteLine($"{"Ime",-20} {"Status",-10} Kreirano");
        foreach (var k in korisnici)
        {
            string ime = k.ime;
            bool aktivan = (long)k.aktivan == 1;
            string kreirano = k.kreirano;
            Console.WriteLine($"{ime,-20} {(aktivan ? "aktivan" : "deaktiviran"),-10} {kreirano}");
        }
        break;
    }
    default:
        PrintUsage();
        return 1;
}

return 0;

static string CitajLozinku(string poruka)
{
    Console.Write(poruka);
    var lozinka = "";
    ConsoleKeyInfo tipka;
    while ((tipka = Console.ReadKey(true)).Key != ConsoleKey.Enter)
    {
        if (tipka.Key == ConsoleKey.Backspace && lozinka.Length > 0)
        {
            lozinka = lozinka[..^1];
            Console.Write("\b \b");
        }
        else if (!char.IsControl(tipka.KeyChar))
        {
            lozinka += tipka.KeyChar;
            Console.Write("*");
        }
    }
    Console.WriteLine();
    return lozinka;
}

static void PrintUsage()
{
    Console.WriteLine("Upotreba (pokrenuti iz korena repoa):");
    Console.WriteLine("  dotnet run --project Tools/UserAdmin -- add <ime>       # kreira novog korisnika");
    Console.WriteLine("  dotnet run --project Tools/UserAdmin -- passwd <ime>    # resetuje lozinku");
    Console.WriteLine("  dotnet run --project Tools/UserAdmin -- disable <ime>   # deaktivira nalog");
    Console.WriteLine("  dotnet run --project Tools/UserAdmin -- enable <ime>    # aktivira nalog");
    Console.WriteLine("  dotnet run --project Tools/UserAdmin -- list            # lista korisnika");
}
