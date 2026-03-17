using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IUserService users)  : ControllerBase
{
    public record RegisterRequest(string Email, string Password);
    public record LoginRequest(string Email, string Password);

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var user = await users.RegisterAsync(request.Email, request.Password);

        return Ok(new
        {
            user.Id,
            user.Email,
            user.Role
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var token = await users.LoginAsync(request.Email, request.Password);
        return Ok(new { token });
    }

}