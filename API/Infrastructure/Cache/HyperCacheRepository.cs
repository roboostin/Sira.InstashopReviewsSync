using Microsoft.Extensions.Caching.Hybrid;
using Polly;
using Polly.CircuitBreaker;
using API.Infrastructure.Cache;
using Serilog;
using StackExchange.Redis;
using API.Domain.Entities;
using API.Helpers;

public class HybridCacheRepository : IHybridCacheRepository
{
    private readonly HybridCache _cache;
    private readonly ILogger<HybridCacheRepository> _logger;
    private readonly bool IsAllowedCaching = true;
    public HybridCacheRepository(HybridCache cache,ILogger<HybridCacheRepository> logger)
    {
        _cache = cache;
        _logger = logger;
        IsAllowedCaching = ConfigurationHelper.AllowRedis();
    }

    public async Task<T> GetAsync<T>(string Key) where T : RedisBaseModel, new()
    {
        if (!IsAllowedCaching)
        {
            _logger.LogWarning("❌ Caching is disabled - Skipping Redis get for {CacheKey}", Key);
            return default;
        }
        var cachedKey = $":{typeof(T).Name}:{Key}";

        try
        {
            return await _cache.GetOrCreateAsync(cachedKey, ct => ValueTask.FromResult<T>(default)).ConfigureAwait(false);

        }

        catch (Exception ex)
        {
            _logger.LogWarning(ex, "❌ Error retrieving {CacheKey}", Key);
            return default;
        }
    }
    public async Task SetAsync<T>(string Key, T value, TimeSpan duration, params string[] tags) where T : RedisBaseModel, new()
    {
        if (!IsAllowedCaching)
        {
            _logger.LogWarning("❌ Caching is disabled - Skipping Redis set for {CacheKey}", Key);
            return;
        }

        var cachedKey = $":{typeof(T).Name}:{Key}";

        try
        {
            await _cache.SetAsync(
                cachedKey,
                value,
                new HybridCacheEntryOptions
                {
                    Expiration = value.TTL.HasValue && value.TTL > DateTime.UtcNow
                         ? value.TTL.Value - DateTime.UtcNow
                         : duration,
                    //Expiration = duration,
                    LocalCacheExpiration = value.TTL.HasValue && value.TTL > DateTime.UtcNow
                         ? value.TTL.Value - DateTime.UtcNow
                         : duration,  //TimeSpan.FromMinutes(10)
                },
                tags
            ).ConfigureAwait(false);
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error caching {CacheKey}", cachedKey);
        }
    }

    public async Task RemoveAsync<T>(string Key) where T : RedisBaseModel, new()
    {
        if (!IsAllowedCaching)
        {
            _logger.LogWarning("❌ Caching is disabled - Skipping Redis remove for {CacheKey}", Key);
            return;
        }

        var cachedKey = $":{typeof(T).Name}:{Key}";

        try
        {
            _logger.LogWarning("Removing {CacheKey} from cache", cachedKey);
            await _cache.RemoveAsync(cachedKey).ConfigureAwait(false);
        }


        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error removing {CacheKey}", cachedKey);
        }
    }


}