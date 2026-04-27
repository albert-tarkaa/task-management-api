using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TaskManagement.Application.Common;

namespace TaskManagement.API.Common;

public class IdempotencyFilter(IIdempotencyService idempotency, ILogger<IdempotencyFilter> logger) : IAsyncActionFilter

{
    public async Task OnActionExecutionAsync(ActionExecutingContext context,ActionExecutionDelegate next)
    {
        var key = context.HttpContext.Request.Headers["Idempotency-Key"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(key))
        {
            context.Result = new BadRequestObjectResult(
                Error.Validation("Idempotency-Key header is required"));
            return;
        }
        if (string.IsNullOrWhiteSpace(key))
        {
            await next();
            return;
        }

        logger.LogInformation("Idempotency filter triggered: Key={Key}", key);
        var result = await idempotency.ExecuteAsync(key, async () =>
        {
            var executedContext = await next();
            // Important: extract IActionResult
            if (executedContext.Result is ObjectResult obj)
                return obj;
            if (executedContext.Result is StatusCodeResult status)
                return status;
            return executedContext.Result!;
        });

        context.Result = (IActionResult)result;
    }
}