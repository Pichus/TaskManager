using Microsoft.AspNetCore.Identity;
using TaskManager.Core.ProjectAggregate;
using TaskManager.Core.TaskAggregate;

namespace TaskManager.Infrastructure.Identity.User;

public class TaskManagerUser : IdentityUser
{
    public IEnumerable<ProjectEntity> LedProjects { get; set; }
    public IEnumerable<TaskEntity> CreatedTasks { get; set; }
    public IEnumerable<TaskEntity> AssignedTasks { get; set; }
    public IEnumerable<ProjectInvite> Invites { get; set; }
}