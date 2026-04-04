using TaskManagement.Application.Common;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Interfaces;

public interface ITaskService
{
    Task<Result<WorkTask>> CreateAsync(
        string title,
        Guid projectId,
        TaskPriority priority,
        DateTime? dueDate,
        string? description);

    Task<Result<WorkTask>> GetByIdAsync(Guid id);

    Task <Result> AssignAsync(Guid taskId, Guid userId, byte[] rowVersion);

    Task<Result> StartAsync(Guid taskId, byte[] rowVersion);

    Task <Result> CompleteAsync(Guid taskId, byte[] rowVersion);

    Task <Result>UpdateAsync(
        Guid taskId,
        string title,
        string? description,
        DateTime? dueDate,
        TaskPriority priority,
        byte[] rowVersion);


    Task<(int total, List<WorkTask> items)> ListByProjectAsync(
        Guid projectId,
        int page,
        int pageSize,
        WorkTaskStatus? status,
        TaskPriority? priority,
        string? sortBy,
        string? sortDir);
}