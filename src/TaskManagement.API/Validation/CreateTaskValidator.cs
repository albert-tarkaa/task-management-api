using FluentValidation;
using TaskManagement.API.Controllers;

namespace TaskManagement.API.Validation;

public class CreateTaskValidator  : AbstractValidator<TasksController.CreateTaskRequest>
{
    public CreateTaskValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.ProjectId)
            .NotEmpty();

        RuleFor(x => x.Priority)
            .IsInEnum();

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow)
            .When(x => x.DueDate.HasValue);
    }
}