using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.Common;

namespace TaskManagement.API.Common;

public static class ResultExtensions
{
    public static IActionResult ToActionResult(this Result result)
    {
        if (result.IsSuccess)
            return new NoContentResult();

        return result.Error?.Code switch
        {
            "not_found" => new NotFoundObjectResult(result.Error),
            "conflict" => new ConflictObjectResult(result.Error),
            "validation_error" => new BadRequestObjectResult(result.Error),
            _ => new BadRequestObjectResult(result.Error)
        };
    }

    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        if (!result.IsSuccess)
        {
            return result.Error?.Code switch
            {
                "not_found" => new NotFoundObjectResult(result.Error),
                "conflict" => new ConflictObjectResult(result.Error),
                "validation_error" => new BadRequestObjectResult(result.Error),
                _ => new BadRequestObjectResult(result.Error)
            };
        }

        return new OkObjectResult(result.Value);
    }
}