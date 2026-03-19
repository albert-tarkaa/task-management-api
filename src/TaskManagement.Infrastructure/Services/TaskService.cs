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
}