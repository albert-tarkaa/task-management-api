namespace TaskManagement.Domain.Entities;

public class Project
{
    public Guid Id { get; private set; }

    public string Name { get; private set; } = null!;

    public string? Description { get; private set; }

    public Guid OwnerId { get; private set; }

    public bool IsArchived { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public byte[] RowVersion { get; private set; } = null!;

    private Project() { }

    public Project(string name, Guid ownerId, string? description = null)
    {
        Id = Guid.NewGuid();
        Name = name;
        OwnerId = ownerId;
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string name, string? description)
    {
        Name = name;
        Description = description;
    }

    public void Archive()
    {
        IsArchived = true;
    }
}