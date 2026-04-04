using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Infrastructure.Persistence;

namespace TaskManagement.Infrastructure.Services;

public class TaskService(ApplicationDbContext db) : ITaskService
{
    public async Task<Result<WorkTask>> CreateAsync(
        string title,
        Guid projectId,
        TaskPriority priority,
        DateTime? dueDate,
        string? description)
    {
        var projectExists = await db.Projects
            .AnyAsync(x => x.Id == projectId && !x.IsArchived);

        if (!projectExists)
            return Result<WorkTask>.Failure(Error.NotFound("Project"));

        var task = new WorkTask(
            title,
            projectId,
            priority,
            dueDate,
            description);

        db.Tasks.Add(task);
        await db.SaveChangesAsync();

        return Result<WorkTask>.Success(task);
    }

    public async Task<Result<WorkTask>> GetByIdAsync(Guid taskId)
    {
        var task = await db.Tasks.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == taskId);

        if (task != null) return Result<WorkTask>.Success(task);
        return Result<WorkTask>.Failure(Error.NotFound("Task"));

    }

    public async Task<Result> AssignAsync(Guid taskId, Guid userId, byte[] rowVersion)
    {
        var task = await db.Tasks.FirstOrDefaultAsync(x => x.Id == taskId);

        if (task == null)
            return Result.Failure(Error.NotFound("Task"));

        db.Entry(task).Property("RowVersion").OriginalValue = rowVersion;
        try
        {
            task.AssignUser(userId);
            await db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure(Error.Conflict("Concurrency conflict"));
        }
        catch (InvalidOperationException)
        {
            return Result.Failure(Error.Validation("Invalid task state"));
        }
        return Result.Success();
    }

    public async Task<Result> StartAsync(Guid taskId, byte[] rowVersion)
    {
        var task = await db.Tasks.FirstOrDefaultAsync(x => x.Id == taskId);

        if (task == null)
            return Result.Failure(Error.NotFound("Task"));

        db.Entry(task).Property("RowVersion").OriginalValue = rowVersion;

        try
        {
            task.Start();
            await db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure(Error.Conflict("Concurrency conflict"));
        }
        catch (InvalidOperationException)
        {
            return Result.Failure(Error.Validation("Invalid task state"));
        }

        return Result.Success();
    }

    public async Task <Result>  CompleteAsync(Guid taskId, byte[] rowVersion)
    {
        var task = await db.Tasks.FirstOrDefaultAsync(x => x.Id == taskId);

        if (task == null)
            return Result.Failure(Error.NotFound("Task"));

        db.Entry(task).Property("RowVersion").OriginalValue = rowVersion;

        try
        {
            task.Complete();
            await db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure(Error.Conflict("Concurrency conflict"));
        }

        return Result.Success();
    }

    public async Task<Result> UpdateAsync(
        Guid taskId,
        string title,
        string? description,
        DateTime? dueDate,
        TaskPriority priority,
        byte[] rowVersion)
    {
        var task = await db.Tasks.FirstOrDefaultAsync(x => x.Id == taskId);

        if (task == null)
            return Result.Failure(Error.NotFound("Task"));

        // Detect no changes (important)
        if (task.Title == title &&
            task.Description == description &&
            task.DueDate == dueDate &&
            task.Priority == priority)
        {
            return Result.Failure(Error.Validation("No changes detected"));
        }

        db.Entry(task).Property("RowVersion").OriginalValue = rowVersion;

        try
        {
            task.UpdateDetails(title, description, dueDate, priority);

            await db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure(Error.Conflict("Concurrency conflict"));
        }

        return Result.Success();
    }

    public async Task<(int total, List<WorkTask> items)> ListByProjectAsync(
        Guid projectId,
        int page,
        int pageSize,
        WorkTaskStatus? status,
        TaskPriority? priority,
        string? sortBy,
        string? sortDir)
    {
        var query = db.Tasks
            .AsNoTracking()
            .Where(x => x.ProjectId == projectId);

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        if (priority.HasValue)
            query = query.Where(x => x.Priority == priority.Value);

        var total = await query.CountAsync();

        var isAsc = string.Equals(sortDir, "asc", StringComparison.OrdinalIgnoreCase);

        query = (sortBy?.ToLowerInvariant()) switch
        {
            "duedate" => isAsc
                ? query.OrderBy(x => x.DueDate ?? DateTime.MaxValue)
                : query.OrderByDescending(x => x.DueDate ?? DateTime.MinValue),

            "priority" => isAsc
                ? query.OrderBy(x => x.Priority)
                : query.OrderByDescending(x => x.Priority),

            "status" => isAsc
                ? query.OrderBy(x => x.Status)
                : query.OrderByDescending(x => x.Status),

            _ => query.OrderByDescending(x => x.CreatedAt)
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (total, items);
    }
}