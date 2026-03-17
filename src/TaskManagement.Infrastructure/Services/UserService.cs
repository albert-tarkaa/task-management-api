using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Persistence;
using TaskManagement.Infrastructure.Security;

namespace TaskManagement.Infrastructure.Services;

public class UserService (ApplicationDbContext db,PasswordHasher hasher,JwtTokenGenerator jwt) : IUserService
{
    public async Task<User> RegisterAsync(string email, string password)
    {
        bool exists = await db.Users.AnyAsync(x => x.Email == email);
        if (exists)
            throw new Exception("Email already registered");

        var hash = hasher.Hash(password);

        var user = new User(email, hash);

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return user;
    }

    public async Task<string> LoginAsync(string email, string password)
    {
        var user = await db.Users.FirstOrDefaultAsync(x => x.Email == email);

        if (user == null)
            throw new Exception("Invalid credentials");

        bool valid = hasher.Verify(password, user.PasswordHash);

        if (valid) return jwt.Generate(user);
        throw new Exception("Invalid credentials");

    }

    public async Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId);

        if (user == null)
            throw new Exception("User not found");

        bool valid = hasher.Verify(currentPassword, user.PasswordHash);
        if (!valid)
            throw new Exception("Invalid credentials");

        var newHash = hasher.Hash(newPassword);

        user.ChangePassword(newHash);

        await db.SaveChangesAsync();
    }
}