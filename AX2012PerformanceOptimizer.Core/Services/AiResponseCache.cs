using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AX2012PerformanceOptimizer.Core.Services;

/// <summary>
/// In-memory cache for AI responses to reduce costs
/// </summary>
public class AiResponseCache
{
    private readonly Dictionary<string, CachedResponse> _cache = new();
    private readonly TimeSpan _defaultTtl = TimeSpan.FromHours(24);
    private readonly int _maxCacheSize = 1000;

    public class CachedResponse
    {
        public string Response { get; set; } = string.Empty;
        public DateTime CachedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public int HitCount { get; set; }
    }

    /// <summary>
    /// Generate cache key from query text and parameters
    /// </summary>
    private string GenerateCacheKey(string queryText, string operation, Dictionary<string, string>? parameters = null)
    {
        var sb = new StringBuilder();
        sb.Append($"{operation}:");
        sb.Append(NormalizeQuery(queryText));

        if (parameters != null)
        {
            foreach (var (key, value) in parameters.OrderBy(p => p.Key))
            {
                sb.Append($"|{key}:{value}");
            }
        }

        // Create hash for compact key
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// Normalize query to improve cache hits
    /// </summary>
    private string NormalizeQuery(string query)
    {
        // Remove extra whitespace, normalize to uppercase
        return string.Join(" ",
            query.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
        ).ToUpperInvariant();
    }

    /// <summary>
    /// Try to get cached response
    /// </summary>
    public bool TryGet(string queryText, string operation, out string? response, Dictionary<string, string>? parameters = null)
    {
        response = null;
        var key = GenerateCacheKey(queryText, operation, parameters);

        if (!_cache.ContainsKey(key))
            return false;

        var cached = _cache[key];

        // Check if expired
        if (DateTime.UtcNow > cached.ExpiresAt)
        {
            _cache.Remove(key);
            return false;
        }

        // Update hit count
        cached.HitCount++;
        response = cached.Response;
        return true;
    }

    /// <summary>
    /// Store response in cache
    /// </summary>
    public void Set(string queryText, string operation, string response, TimeSpan? ttl = null, Dictionary<string, string>? parameters = null)
    {
        var key = GenerateCacheKey(queryText, operation, parameters);
        var expiry = ttl ?? _defaultTtl;

        _cache[key] = new CachedResponse
        {
            Response = response,
            CachedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(expiry),
            HitCount = 0
        };

        // Cleanup if cache too large
        if (_cache.Count > _maxCacheSize)
        {
            CleanupOldEntries();
        }
    }

    /// <summary>
    /// Remove old/rarely used entries
    /// </summary>
    private void CleanupOldEntries()
    {
        var toRemove = _cache
            .Where(kvp => DateTime.UtcNow > kvp.Value.ExpiresAt || kvp.Value.HitCount == 0)
            .OrderBy(kvp => kvp.Value.HitCount)
            .Take(_cache.Count / 4) // Remove 25%
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in toRemove)
        {
            _cache.Remove(key);
        }
    }

    /// <summary>
    /// Clear all cache
    /// </summary>
    public void Clear()
    {
        _cache.Clear();
    }

    /// <summary>
    /// Get cache statistics
    /// </summary>
    public CacheStatistics GetStatistics()
    {
        var now = DateTime.UtcNow;
        var validEntries = _cache.Values.Where(c => c.ExpiresAt > now).ToList();

        return new CacheStatistics
        {
            TotalEntries = _cache.Count,
            ValidEntries = validEntries.Count,
            TotalHits = validEntries.Sum(c => c.HitCount),
            AverageHitsPerEntry = validEntries.Any() ? validEntries.Average(c => c.HitCount) : 0,
            OldestEntry = validEntries.Any() ? validEntries.Min(c => c.CachedAt) : null,
            EstimatedSavings = CalculateEstimatedSavings(validEntries)
        };
    }

    private decimal CalculateEstimatedSavings(List<CachedResponse> entries)
    {
        // Average cost per AI request: $0.01
        const decimal avgCostPerRequest = 0.01m;
        var totalCacheHits = entries.Sum(c => c.HitCount);
        return totalCacheHits * avgCostPerRequest;
    }

    public class CacheStatistics
    {
        public int TotalEntries { get; set; }
        public int ValidEntries { get; set; }
        public int TotalHits { get; set; }
        public double AverageHitsPerEntry { get; set; }
        public DateTime? OldestEntry { get; set; }
        public decimal EstimatedSavings { get; set; }

        public string GetSummary()
        {
            return $@"Cache Statistics:
- Total Entries: {TotalEntries}
- Valid Entries: {ValidEntries}
- Total Cache Hits: {TotalHits}
- Avg Hits per Entry: {AverageHitsPerEntry:F1}
- Estimated Savings: ${EstimatedSavings:F2}
- Cache Hit Rate: {(TotalHits > 0 ? (TotalHits * 100.0 / (TotalHits + ValidEntries)) : 0):F1}%";
        }
    }
}
