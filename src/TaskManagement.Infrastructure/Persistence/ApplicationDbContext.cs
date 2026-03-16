using Microsoft.EntityFrameworkCore;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext (options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<WorkTask> Tasks => Set<WorkTask>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        ConfigureUser(builder);
        ConfigureProject(builder);
        ConfigureTask(builder);
    }

    private static void ConfigureUser(ModelBuilder builder)
    {
        builder.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);

            e.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(256);

            e.Property(x => x.PasswordHash)
                .IsRequired();

            e.Property(x => x.Role)
                .IsRequired()
                .HasMaxLength(50);
        });
    }

    private static void ConfigureProject(ModelBuilder builder)
    {
        builder.Entity<Project>(e =>
        {
            e.HasKey(x => x.Id);

            e.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            e.Property(x => x.RowVersion)
                .IsRowVersion();

            e.HasMany(x => x.Tasks)
                .WithOne()
                .HasForeignKey(x => x.ProjectId);
        });
    }

    private static void ConfigureTask(ModelBuilder builder)
    {
        builder.Entity<WorkTask>(e =>
        {
            e.HasKey(x => x.Id);

            e.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

            e.Property(x => x.Status)
                .HasConversion<int>();

            e.Property(x => x.Priority)
                .HasConversion<int>();

            e.Property(x => x.RowVersion)
                .IsRowVersion();
        });
    }
}