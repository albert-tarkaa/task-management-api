using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace TaskManagement.API.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogWarning(ex, "Concurrency conflict");

            await WriteResponse(context, HttpStatusCode.Conflict,
                "Resource was modified by another user.");
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Business rule violation");

            await WriteResponse(context, HttpStatusCode.BadRequest,
                ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");

            await WriteResponse(context, HttpStatusCode.InternalServerError,
                "An unexpected error occurred.");
        }
    }

    private static async Task WriteResponse(
        HttpContext context,
        HttpStatusCode status,
        string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)status;

        var payload = JsonSerializer.Serialize(new
        {
            error = message
        });

        await context.Response.WriteAsync(payload);
    }
}