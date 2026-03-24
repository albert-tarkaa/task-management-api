using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        var project = await projects.CreateAsync(
            request.Name,
            userId,
            request.Description);

        return Ok(new
        {
            project.Id,
            project.Name,
            project.Description,
            project.CreatedAt
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var project = await projects.GetByIdAsync(id);

        if (project == null) return NotFound();

        return Ok(new
        {
            project.Id,
            project.Name,
            project.Description,
            project.IsArchived,
            project.CreatedAt,
            project.RowVersion
        });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateProjectRequest request)
    {
        try
        {
            await projects.UpdateAsync(
                id,
                request.Name,
                request.Description,
                request.RowVersion);

            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new
            {
                message = "Project was modified by another user. Refresh and try again."
            });
        }
        catch (Exception ex)
        {
            return NotFound(new { ex.Message });
        }
    }

    [HttpPost("{id:guid}/archive")]
    public async Task<IActionResult> Archive(Guid id, ArchiveProjectRequest request)
    {
        try
        {
            await projects.ArchiveAsync(id, request.RowVersion);
            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new
            {
                message = "Project was modified by another user. Refresh and try again."
            });
        }
        catch (Exception ex)
        {
            return NotFound(new { ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDir = "desc")
    {
        var (total, projectsItems) =
            await projects.ListAsync(page, pageSize, sortBy, sortDir);

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