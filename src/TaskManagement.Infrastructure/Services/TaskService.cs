using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Infrastructure.Persistence;

namespace TaskManagement.Infrastructure.Services;

public class TaskService(ApplicationDbContext db) : ITaskService
{

    public async Task<WorkTask> CreateAsync(
        string title,
        Guid projectId,
        TaskPriority priority,
        DateTime? dueDate,
        string? description)
    {
        var projectExists = await db.Projects
            .AnyAsync(x => x.Id == projectId && !x.IsArchived);

        if (!projectExists)
            throw new Exception("Project not found or archived");

        var task = new WorkTask(
            title,
            projectId,
            priority,
            dueDate,
            description);

        db.Tasks.Add(task);
        await db.SaveChangesAsync();

        return task;
    }

    public async Task<WorkTask?> GetByIdAsync(Guid taskId)
    {
        return await db.Tasks.AsNoTracking().FirstOrDefaultAsync(x => x.Id == taskId);
    }

    public async Task AssignAsync(Guid taskId, Guid userId, byte[] rowVersion)
    {
        var task = await db.Tasks.FirstOrDefaultAsync(x => x.Id == taskId);

        if (task == null)
            throw new Exception("Task not found");

        db.Entry(task).Property("RowVersion").OriginalValue = rowVersion;

        task.AssignUser(userId);

        await db.SaveChangesAsync();
    }

    public async Task StartAsync(Guid taskId, byte[] rowVersion)
    {
        var task = await db.Tasks.FirstOrDefaultAsync(x => x.Id == taskId);

        if (task == null)
            throw new Exception("Task not found");

        db.Entry(task).Property("RowVersion").OriginalValue = rowVersion;

        task.Start();

        await db.SaveChangesAsync();
    }

    public async Task CompleteAsync(Guid taskId, byte[] rowVersion)
    {
        var task = await db.Tasks.FirstOrDefaultAsync(x => x.Id == taskId);

        if (task == null)
            throw new Exception("Task not found");

        db.Entry(task).Property("RowVersion").OriginalValue = rowVersion;

        task.Complete();

        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(
        Guid taskId,
        string title,
        string? description,
        DateTime? dueDate,
        TaskPriority priority,
        byte[] rowVersion)
    {
        var task = await db.Tasks.FirstOrDefaultAsync(x => x.Id == taskId);

        if (task == null)
            throw new Exception("Task not found");

        db.Entry(task).Property("RowVersion").OriginalValue = rowVersion;

        task.UpdateDetails(title, description, dueDate, priority);

        await db.SaveChangesAsync();
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

