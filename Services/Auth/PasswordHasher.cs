using System.Security.Cryptography;

namespace FruitSysWeb.Services.Auth;

/// <summary>
/// PBKDF2-HMAC-SHA256 hashovanje lozinki (bez eksternih paketa).
/// So po korisniku, broj iteracija se čuva po redu — može se podići u budućnosti
/// bez migracije postojećih naloga (stari nastavljaju sa starom vrednošću dok se ne promeni lozinka).
/// </summary>
public static class PasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    public const int DefaultIterations = 210_000;

    public static (string Hash, string Salt, int Iterations) Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, DefaultIterations, HashAlgorithmName.SHA256, KeySize);
        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt), DefaultIterations);
    }

    public static bool Verify(string password, string hashBase64, string saltBase64, int iterations)
    {
        var salt = Convert.FromBase64String(saltBase64);
        var expected = Convert.FromBase64String(hashBase64);
        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
