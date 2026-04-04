using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TaskManagement.Application.Common;

namespace TaskManagement.Infrastructure.Common;

public class ValidationService(IServiceProvider provider) : IValidationService
{
    public async Task ValidateAsync<T>(T model)
    {
        var validator = provider.GetService<IValidator<T>>();

        if (validator == null)
            return;

        var result = await validator.ValidateAsync(model);

        if (!result.IsValid)
        {
            var message = string.Join("; ",
                result.Errors.Select(e => e.ErrorMessage));

            throw new InvalidOperationException(message);
        }
    }
}