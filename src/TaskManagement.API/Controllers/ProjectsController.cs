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
}