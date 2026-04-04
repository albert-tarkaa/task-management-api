using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.API.Common;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController(IProjectService projects)  : ControllerBase
{
    public record CreateProjectRequest(string Name, string? Description);
    public record UpdateProjectRequest(
        string Name,
        string? Description,
        byte[] RowVersion);
    public record ArchiveProjectRequest(byte[] RowVersion);

    [HttpPost]
    public async Task<IActionResult> Create(CreateProjectRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await projects.CreateAsync(
            request.Name,
            userId,
            request.Description);

        return result.ToActionResult();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await projects.GetByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateProjectRequest request)
    {
        var result = await projects.UpdateAsync(id,
            request.Name,
            request.Description,
            request.RowVersion);
        return result.ToActionResult();
    }

    [HttpPost("{id:guid}/archive")]
    public async Task<IActionResult> Archive(Guid id, ArchiveProjectRequest request)
    {
        var result = await projects.ArchiveAsync(id, request.RowVersion);
        return result.ToActionResult();
    }

    [HttpGet]
    public async Task<IActionResult> List(
        int page = 1,
        int pageSize = 20,
        string? sortBy = null,
        string? sortDir = "desc")
    {
        var result = await projects.ListAsync(page, pageSize, sortBy, sortDir);

        if (!result.IsSuccess)
            return result.ToActionResult();

        var (total, projectsItems) = result.Value;

        return Ok(new
        {
            total,
            page,
            pageSize,
            items = projectsItems.Select(x => new
            {
                x.Id,
                x.Name,
                x.Description,
                x.IsArchived,
                x.CreatedAt
            })
        });
    }
}