using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

    public record TransitionRequest(byte[] RowVersion);

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateTaskRequest request,
        IValidator<CreateTaskRequest> validator)
    {
        var result = await validator.ValidateAsync(request);

        if (!result.IsValid)
            return BadRequest(result.Errors);

        var task = await tasks.CreateAsync(
            request.Title,
            request.ProjectId,
            request.Priority,
            request.DueDate,
            request.Description);

        return Ok(task);
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
        var task = await tasks.GetByIdAsync(id);

        if (task == null)
            return NotFound();

        return Ok(new
        {
            task.Id,
            task.Title,
            task.Description,
            task.Status,
            task.Priority,
            task.ProjectId,
            task.AssignedUserId,
            task.DueDate,
            task.RowVersion,
            task.CreatedAt
        });
    }

    [HttpPost("{id:guid}/start")]
    public async Task<IActionResult> Start(Guid id, TransitionRequest request)
    {
        try
        {
            await tasks.StartAsync(id, request.RowVersion);
            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new
            {
                message = "Task was modified by another user. Refresh and try again."
            });
        }
        catch (Exception ex)
        {
            return NotFound(new { ex.Message });
        }
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id, TransitionRequest request)
    {
        try
        {
            await tasks.CompleteAsync(id, request.RowVersion);
            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new
            {
                message = "Task was modified by another user. Refresh and try again."
            });
        }
        catch (Exception ex)
        {
            return NotFound(new { ex.Message });
        }
    }
}