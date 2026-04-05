using Microsoft.Extensions.Caching.Memory;
using TaskManagement.Application.Common;

namespace TaskManagement.Infrastructure.Services;

public class MemoryCacheService(IMemoryCache cache) : ICacheService
{
    private static readonly HashSet<string> Keys = [];

    public Task<(bool found, T? value)> GetAsync<T>(string key)
    {
        var found = cache.TryGetValue(key, out T? value);
        return Task.FromResult((found, value));
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        cache.Set(key, value, expiry ?? TimeSpan.FromMinutes(5));
        Keys.Add(key);
        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefix)
    {
        var keysToRemove = Keys.Where(k => k.StartsWith(prefix)).ToList();

        foreach (var key in keysToRemove)
        {
            cache.Remove(key);
            Keys.Remove(key);
        }

        return Task.CompletedTask;
    }
}