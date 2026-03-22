using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Enums;

namespace TaskManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController(ITaskService tasks) : ControllerBase
{
    public record CreateTaskRequest(
        string Title,
        Guid ProjectId,
        TaskPriority Priority,
        DateTime? DueDate,
        string? Description);

    [HttpPost]
    public async Task<IActionResult> Create(CreateTaskRequest request)
    {
        try
        {
            var task = await tasks.CreateAsync(request.Title, request.ProjectId, request.Priority, request.DueDate,
                request.Description);
            return Ok(new {task.Id, task.Title, task.ProjectId, task.Priority, task.DueDate, task.Description});
        }
        catch (Exception ex)
        {
            return BadRequest(new { ex.Message });
        }
    }
}