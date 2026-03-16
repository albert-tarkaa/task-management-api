namespace TaskManagement.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }

    public string Email { get; private set; } = null!;

    public string PasswordHash { get; private set; } = null!;

    public string Role { get; private set; } = "User";

    public DateTime CreatedAt { get; private set; }

    private User() { } // EF

    public User(string email, string passwordHash)
    {
        Id = Guid.NewGuid();
        Email = email;
        PasswordHash = passwordHash;
        CreatedAt = DateTime.UtcNow;
    }

    public void ChangeRole(string role)
    {
        Role = role;
    }

    public void ChangePassword(string newHash)
    {
        PasswordHash = newHash;
    }
}