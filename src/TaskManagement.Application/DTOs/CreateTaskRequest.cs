using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.DTOs;

public record CreateTaskRequest(
    string Title,
    Guid ProjectId,
    TaskPriority Priority,
    DateTime? DueDate,
    string? Description);