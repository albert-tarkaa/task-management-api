using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using TaskManagement.Application.Common;

namespace TaskManagement.Infrastructure.Services;

public class IdempotencyService : IIdempotencyService
{
    private static readonly ConcurrentDictionary<string, object> Store = new();

    public async Task<T> ExecuteAsync<T>(string key, Func<Task<T>> action)
    {
        var task = (Task<T>)Store.GetOrAdd(key, _ => action());

        try
        {
            var result = await task;
            return result;
        }
        catch (Exception ex)
        {
            // critical: allow retry if failed
            Store.TryRemove(key, out _);
            throw;
        }
    }
}