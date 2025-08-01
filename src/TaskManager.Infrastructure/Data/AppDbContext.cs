using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskManager.Infrastructure.Identity;

namespace TaskManager.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<TaskManagerUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<Task> Tasks { get; set; }
    public DbSet<Task> Projects { get; set; }
}