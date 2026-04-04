namespace TaskManagement.Application.Common;

public interface IValidationService
{
    Task ValidateAsync<T>(T model);
}