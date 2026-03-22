using TaskManagement.Domain.Enums;

namespace TaskManagement.Domain.Entities;

public class WorkTask
{
    public Guid Id { get; private set; }

    public string Title { get; private set; } = null!;

    public string? Description { get; private set; }

    public WorkTaskStatus Status { get; private set; }

    public TaskPriority Priority { get; private set; }

    public DateTime? DueDate { get; private set; }

    public Guid ProjectId { get; private set; }

    public Guid? AssignedUserId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public byte[] RowVersion { get; private set; } = null!;

    private WorkTask() { }

    public WorkTask(
        string title,
        Guid projectId,
        TaskPriority priority,
        DateTime? dueDate = null,
        string? description = null)
    {
        Id = Guid.NewGuid();
        Title = title;
        ProjectId = projectId;
        Priority = priority;
        DueDate = dueDate;
        Description = description;
        Status = WorkTaskStatus.Todo;
        CreatedAt = DateTime.UtcNow;
    }

    public void Start()
    {
        if (Status != WorkTaskStatus.Todo)
                throw new InvalidOperationException("Task already started or completed");

        Status = WorkTaskStatus.InProgress;
    }

    public void Complete()
    {
        Status = WorkTaskStatus.Done;
    }

    public void AssignUser(Guid userId)
    {
        AssignedUserId = userId;
    }

    public void UpdateDetails(string title, string? description, DateTime? dueDate, TaskPriority priority)
    {
        Title = title;
        Description = description;
        DueDate = dueDate;
        Priority = priority;
    }
}