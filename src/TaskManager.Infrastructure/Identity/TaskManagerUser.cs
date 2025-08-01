using Microsoft.AspNetCore.Identity;
using TaskManager.Core.ProjectAggregate;

namespace TaskManager.Infrastructure.Identity;

public class TaskManagerUser : IdentityUser
{
    public List<Project> Projects { get; set; }
}