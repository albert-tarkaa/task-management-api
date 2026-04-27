using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using TaskManagement.Application.Common;

namespace TaskManagement.Infrastructure.Services;

public class IdempotencyService : IIdempotencyService
{
    private static readonly ConcurrentDictionary<string, (object task, DateTime expiry)> Store = new();

    public async Task<T> ExecuteAsync<T>(string key, Func<Task<T>> action)
    {
        var now = DateTime.UtcNow;

        foreach (var kvp in Store)
        {
            if (kvp.Value.expiry < now)
            {
                Store.TryRemove(kvp.Key, out _);
            }
        }

        var entry = (Task<T>)Store.GetOrAdd(key, _ =>
        {
            var task = action();
            var expiry = now.AddHours(24);

            return ((object)task, expiry);
        }).task;

        try
        {
            var result = await entry;
            return result;
        }
        catch (Exception ex)
        {
            // critical: allow retry if failed
            Store.TryRemove(key, out _);
            throw;
        }
    }

    public async Task<object> ExecuteAsync(string key, Func<Task<object>> action)
    {
        var result = await ExecuteAsync<object>(key, action);
        return result;
    }
}