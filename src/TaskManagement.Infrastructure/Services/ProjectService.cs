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
}