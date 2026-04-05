namespace TaskManagement.Application.Common;

public interface ICacheService
{
    Task<(bool found, T? value)> GetAsync<T>(string key);

    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);

    Task RemoveByPrefixAsync(string prefix);
}