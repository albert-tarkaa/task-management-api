using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.API.Controllers;

[ApiController]
[Route("api/projects")]
[Authorize]
public class ProjectsController(IProjectService projects)  : ControllerBase
{
    public record CreateProjectRequest(string Name, string? Description);
    public record UpdateProjectRequest(
        string Name,
        string? Description,
        byte[] RowVersion);

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
        catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
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
}