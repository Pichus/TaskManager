using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskManager.Core.ProjectAggregate;
using TaskManager.Core.ProjectInviteAggregate;
using TaskManager.Core.TaskAggregate;
using TaskManager.Infrastructure.Data.Configurations;
using TaskManager.Infrastructure.Identity.RefreshToken;
using TaskManager.Infrastructure.Identity.User;

namespace TaskManager.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<TaskManagerUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<TaskEntity> Tasks { get; set; }
    public DbSet<ProjectEntity> Projects { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<ProjectInvite> ProjectInvites { get; set; }
    public DbSet<ProjectMember> ProjectMembers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TaskEntityConfiguration).Assembly);
    }
}