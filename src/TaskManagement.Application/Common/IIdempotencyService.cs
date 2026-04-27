namespace TaskManagement.Application.Common;

public interface IIdempotencyService
{
        Task<T> ExecuteAsync<T>(string key, Func<Task<T>> action);

        Task<object> ExecuteAsync(string key, Func<Task<object>> action);
}

