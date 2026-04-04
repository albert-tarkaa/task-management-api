using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.API.Common;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Enums;

namespace TaskManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController(ITaskService tasks) : ControllerBase
{

    public record TransitionRequest(byte[] RowVersion);

    [HttpPost]
    public async Task<IActionResult> Create(CreateTaskRequest request)
    {
        var result = await tasks.CreateAsync(
            request.Title,
            request.ProjectId,
            request.Priority,
            request.DueDate,
            request.Description);

        return result.ToActionResult();
    }

    [HttpGet("project/{projectId:guid}")]
    public async Task<IActionResult> ListByProject(
        Guid projectId,
        [FromQuery] WorkTaskStatus? status,
        [FromQuery] TaskPriority? priority,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortDir = "desc",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var (total, taskItems) =
            await tasks.ListByProjectAsync(projectId, page, pageSize, status, priority, sortBy, sortDir);

        return Ok(new
        {
            total,
            page,
            pageSize,
            items = taskItems.Select(x => new
            {
                x.Id,
                x.Title,
                x.Status,
                x.Priority,
                x.AssignedUserId,
                x.DueDate,
                x.CreatedAt
            })
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await tasks.GetByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpPost("{id:guid}/start")]
    public async Task<IActionResult> Start(Guid id, TransitionRequest request)
    {
        var result = await tasks.StartAsync(id, request.RowVersion);
        return result.ToActionResult();
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id, TransitionRequest request)
    {
        var result = await tasks.CompleteAsync(id, request.RowVersion);
        return result.ToActionResult();
    }
}