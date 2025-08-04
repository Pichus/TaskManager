using Microsoft.AspNetCore.Identity;
using TaskManager.Core.ProjectAggregate;
using TaskManager.Core.TaskAggregate;

namespace TaskManager.Infrastructure.Identity.User;

public class TaskManagerUser : IdentityUser
{
    public ICollection<ProjectEntity> LedProjects { get; set; }
    public ICollection<ProjectEntity>eatedTasks { get; set; }
    public ICollection<ProjectEntity>signedTasks { get; set; }
    public ICollection<ProjectEntity> Invites { get; set; }
}