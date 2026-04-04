using TaskManagement.Application.Common;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Interfaces;

public interface IProjectService
{
    Task <Result<Project>> CreateAsync(string name, Guid ownerId, string? description);

    Task <Result> UpdateAsync (Guid projectId, string name, string? description, byte[] rowVersion);

    Task <Result> ArchiveAsync(Guid projectId, byte[] rowVersion);

    Task <Result<Project>> GetByIdAsync(Guid projectId);

    Task <Result<(int total, List<Project> items)>> ListAsync(
        int page,
        int pageSize,
        string? sortBy,
        string? sortDir);
}