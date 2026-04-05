using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TaskManagement.Application.Common;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Persistence;

namespace TaskManagement.Infrastructure.Services;

public class ProjectService(ApplicationDbContext db, ICacheService cache) : IProjectService
{
    public async Task<Result<Project>> CreateAsync(string name, Guid ownerId, string? description)
    {
        var project = new Project(name, ownerId, description);

        db.Projects.Add(project);
        await db.SaveChangesAsync();

        await cache.RemoveByPrefixAsync("projects:list");
        return Result<Project>.Success(project);
    }

    public async Task<Result<Project>> GetByIdAsync(Guid projectId)
    {
        var project = await db.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == projectId);

        if (project == null)
            return Result<Project>.Failure(Error.NotFound("Project"));

        return Result<Project>.Success(project);
    }

    public async Task<Result> UpdateAsync(Guid projectId, string name, string? description, byte[] rowVersion)
    {
        var project = await db.Projects.FirstOrDefaultAsync(x => x.Id == projectId);

        if (project == null) return Result<Project>.Failure(Error.NotFound("Project"));

        if (project.Name == name && project.Description == description)
            return Result.Failure(Error.Validation("No changes detected."));

        db.Entry(project).Property("RowVersion").OriginalValue = rowVersion;
        try
        {
            project.UpdateDetails(name, description);
            await db.SaveChangesAsync();

            await cache.RemoveByPrefixAsync("projects:list");
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

    public async Task<Result> ArchiveAsync(Guid projectId, byte[] rowVersion)
    {
        var project = await db.Projects.FirstOrDefaultAsync(x => x.Id == projectId);

        if (project == null) return Result<Project>.Failure(Error.NotFound("Project"));

        db.Entry(project).Property("RowVersion").OriginalValue = rowVersion;

        try
        {
            project.Archive();
            await db.SaveChangesAsync();

            await cache.RemoveByPrefixAsync("projects:list");
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

    public async Task<Result<(int total, List<Project> items)>> ListAsync(
        int page,
        int pageSize,
        string? sortBy,
        string? sortDir)
    {
        var cacheKey = $"projects:list:{page}:{pageSize}:{sortBy}:{sortDir}";
        var (found, cached) = await cache.GetAsync<(int total, List<Project> items)>(cacheKey);
        if (found)
        {
            return Result<(int total, List<Project> items)>.Success(cached);
        }
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

        var resultData = (total, items);

        await cache.SetAsync(cacheKey, resultData);
        return Result<(int total, List<Project> items)>.Success(resultData);
    }
}