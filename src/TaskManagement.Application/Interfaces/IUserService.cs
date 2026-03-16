using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Interfaces;

public interface IUserService
{
    Task<User> RegisterAsync(string email, string password);

    Task<string> LoginAsync(string email, string password);

    Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
}