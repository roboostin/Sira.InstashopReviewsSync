using API.Domain.Entities;

namespace API.Infrastructure.Cache;

public interface IHybridCacheRepository
{
    Task SetAsync<T>(string cacheKey, T value, TimeSpan duration, params string[] tags) where T : RedisBaseModel, new();
    Task<T> GetAsync<T>(string cacheKey) where T : RedisBaseModel, new();
    Task RemoveAsync<T>(string cacheKey) where T : RedisBaseModel, new();
}