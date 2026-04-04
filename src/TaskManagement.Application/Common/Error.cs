namespace TaskManagement.Application.Common;

public record Error(string Code, string Message)
{
    public static readonly Error None = new("", "");

    public static Error NotFound(string entity) =>
        new("not_found", $"{entity} not found");

    public static Error Conflict(string message) =>
        new("conflict", message);

    public static Error Validation(string message) =>
        new("validation_error", message);
}