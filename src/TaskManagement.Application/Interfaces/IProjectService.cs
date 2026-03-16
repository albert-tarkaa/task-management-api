using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Interfaces;

public interface IProjectService
{
    Task<Project> CreateAsync(string name, Guid ownerId, string? description);

    Task UpdateAsync(Guid projectId, string name, string? description, byte[] rowVersion);

    Task ArchiveAsync(Guid projectId, byte[] rowVersion);

    Task<Project?> GetByIdAsync(Guid projectId);
}