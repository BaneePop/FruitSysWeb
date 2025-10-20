using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace FruitSysWeb.Services.Core
{
    /// <summary>
    /// Wrapper around IMemoryCache with convenience methods and logging
    /// </summary>
    public class CacheService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<CacheService> _logger;

        // Default cache durations
        public static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(5);
        public static readonly TimeSpan ShortExpiration = TimeSpan.FromMinutes(1);
        public static readonly TimeSpan MediumExpiration = TimeSpan.FromMinutes(10);
        public static readonly TimeSpan LongExpiration = TimeSpan.FromHours(1);

        public CacheService(IMemoryCache cache, ILogger<CacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Gets a value from cache or executes the factory function and caches the result
        /// </summary>
        /// <typeparam name="T">Type of cached value</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="factory">Function to execute if cache miss</param>
        /// <param name="expiration">Cache duration (default: 5 minutes)</param>
        /// <returns>Cached or newly computed value</returns>
        public async Task<T> GetOrCreateAsync<T>(
            string key,
            Func<Task<T>> factory,
            TimeSpan? expiration = null)
        {
            // Try to get from cache
            if (_cache.TryGetValue(key, out T? cachedValue) && cachedValue != null)
            {
                _logger.LogDebug("Cache HIT for key: {CacheKey}", key);
                return cachedValue;
            }

            _logger.LogDebug("Cache MISS for key: {CacheKey}", key);

            // Cache miss - execute factory
            var value = await factory();

            // Cache the result
            var cacheExpiration = expiration ?? DefaultExpiration;
            _cache.Set(key, value, cacheExpiration);

            _logger.LogInformation("Cached value for key: {CacheKey}, Expiration: {Expiration}", key, cacheExpiration);

            return value;
        }

        /// <summary>
        /// Gets a value from cache or executes the factory function and caches the result (sync version)
        /// </summary>
        public T GetOrCreate<T>(
            string key,
            Func<T> factory,
            TimeSpan? expiration = null)
        {
            // Try to get from cache
            if (_cache.TryGetValue(key, out T? cachedValue) && cachedValue != null)
            {
                _logger.LogDebug("Cache HIT for key: {CacheKey}", key);
                return cachedValue;
            }

            _logger.LogDebug("Cache MISS for key: {CacheKey}", key);

            // Cache miss - execute factory
            var value = factory();

            // Cache the result
            var cacheExpiration = expiration ?? DefaultExpiration;
            _cache.Set(key, value, cacheExpiration);

            _logger.LogInformation("Cached value for key: {CacheKey}, Expiration: {Expiration}", key, cacheExpiration);

            return value;
        }

        /// <summary>
        /// Tries to get a value from cache
        /// </summary>
        public bool TryGetValue<T>(string key, out T? value)
        {
            var result = _cache.TryGetValue(key, out value);

            if (result)
            {
                _logger.LogDebug("Cache HIT for key: {CacheKey}", key);
            }
            else
            {
                _logger.LogDebug("Cache MISS for key: {CacheKey}", key);
            }

            return result;
        }

        /// <summary>
        /// Sets a value in cache
        /// </summary>
        public void Set<T>(string key, T value, TimeSpan? expiration = null)
        {
            var cacheExpiration = expiration ?? DefaultExpiration;
            _cache.Set(key, value, cacheExpiration);

            _logger.LogInformation("Cached value for key: {CacheKey}, Expiration: {Expiration}", key, cacheExpiration);
        }

        /// <summary>
        /// Removes a value from cache
        /// </summary>
        public void Remove(string key)
        {
            _cache.Remove(key);
            _logger.LogInformation("Removed cache key: {CacheKey}", key);
        }

        /// <summary>
        /// Removes all cache entries matching a pattern
        /// </summary>
        public void RemoveByPattern(string pattern)
        {
            // Note: IMemoryCache doesn't support pattern matching natively
            // This would require tracking keys separately or using a different cache provider
            _logger.LogWarning("RemoveByPattern not fully implemented for IMemoryCache. Pattern: {Pattern}", pattern);

            // For now, log a warning. To implement this properly, you'd need to:
            // 1. Track all cache keys in a separate collection
            // 2. Match keys against pattern
            // 3. Remove matched keys
        }

        /// <summary>
        /// Builds a cache key from multiple parts
        /// </summary>
        public static string BuildKey(params object[] parts)
        {
            return string.Join("_", parts.Select(p => p?.ToString() ?? "null"));
        }

        /// <summary>
        /// Builds a cache key with prefix
        /// </summary>
        public static string BuildKey(string prefix, params object[] parts)
        {
            var key = string.Join("_", parts.Select(p => p?.ToString() ?? "null"));
            return $"{prefix}:{key}";
        }
    }

    /// <summary>
    /// Cache key constants for common cache entries
    /// </summary>
    public static class CacheKeys
    {
        public const string DashboardStats = "Dashboard:Stats";
        public const string TopKupci = "Dashboard:TopKupci";
        public const string TopDobavljaci = "Dashboard:TopDobavljaci";
        public const string Artikli = "Artikli:All";
        public const string Komitenti = "Komitenti:All";
        public const string ArtikalKlasifikacije = "ArtikalKlasifikacije:All";

        // Prefix patterns
        public const string SledljivostPrefix = "Sledljivost";
        public const string FinansijePrefix = "Finansije";
        public const string ProizvodnjaPrefix = "Proizvodnja";
        public const string MagacinPrefix = "Magacin";

        /// <summary>
        /// Builds a cache key for sledljivost by sifra
        /// </summary>
        public static string Sledljivost(string sifra) => $"{SledljivostPrefix}:{sifra}";

        /// <summary>
        /// Builds a cache key for finansije by date range
        /// </summary>
        public static string Finansije(DateTime? odDatum, DateTime? doDatum) =>
            $"{FinansijePrefix}:{odDatum:yyyy-MM-dd}_{doDatum:yyyy-MM-dd}";
    }
}
