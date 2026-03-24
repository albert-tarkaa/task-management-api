using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Persistence;

namespace TaskManagement.Infrastructure.Services;

public class ProjectService(ApplicationDbContext db) : IProjectService
{
    public async Task<Project> CreateAsync(string name, Guid ownerId, string? description)
    {
        var project = new Project(name, ownerId, description);

        db.Projects.Add(project);
        await db.SaveChangesAsync();

        return project;
    }

    public async Task<Project?> GetByIdAsync(Guid projectId)
    {
        return await db.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == projectId);
    }

    public async Task UpdateAsync(Guid projectId, string name, string? description, byte[] rowVersion)
    {
        var project = await db.Projects.FirstOrDefaultAsync(x => x.Id == projectId);

        if (project == null) throw new Exception("Project not found");

        if (project.Name == name && project.Description == description)
            throw new InvalidOperationException("No changes detected.");

        db.Entry(project).Property("RowVersion").OriginalValue = rowVersion;

        project.UpdateDetails(name, description);

        await db.SaveChangesAsync();
    }

    public async Task ArchiveAsync(Guid projectId, byte[] rowVersion)
    {
        var project = await db.Projects.FirstOrDefaultAsync(x => x.Id == projectId);

        if (project == null) throw new Exception("Project not found");

        db.Entry(project).Property("RowVersion").OriginalValue = rowVersion;

        project.Archive();

        await db.SaveChangesAsync();
    }

    public async Task<(int total, List<Project> items)> ListAsync(
        int page,
        int pageSize,
        string? sortBy,
        string? sortDir)
    {
        var query = db.Projects.AsNoTracking();

        var total = await query.CountAsync();

        var isAsc = string.Equals(sortDir, "asc", StringComparison.OrdinalIgnoreCase);

        query = (sortBy?.ToLowerInvariant()) switch
        {
            "name" => isAsc
                ? query.OrderBy(x => x.Name)
                : query.OrderByDescending(x => x.Name),

            _ => query.OrderByDescending(x => x.CreatedAt)
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (total, items);
    }
}