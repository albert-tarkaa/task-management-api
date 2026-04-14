namespace TaskManagement.Application.Common;

public interface IIdempotencyService
{
        Task<T> ExecuteAsync<T>(string key, Func<Task<T>> action);
}