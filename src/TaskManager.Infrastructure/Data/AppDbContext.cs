using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskManager.Core.ProjectAggregate;
using TaskManager.Core.TaskAggregate;
using TaskManager.Core.UserAggregate;
using TaskManager.Infrastructure.Data.Configurations;

namespace TaskManager.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<TaskManagerUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<TaskEntity> Tasks { get; set; }
    public DbSet<ProjectEntity> Projects { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TaskEntityConfiguration).Assembly);
    }
}