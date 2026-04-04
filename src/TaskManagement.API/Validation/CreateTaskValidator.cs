using FluentValidation;
using TaskManagement.Application.DTOs;

namespace TaskManagement.API.Validation;

public class CreateTaskValidator  : AbstractValidator<CreateTaskRequest>
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