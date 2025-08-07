using Microsoft.AspNetCore.Identity;
using TaskManager.Core.ProjectAggregate;
using TaskManager.Core.ProjectInviteAggregate;
using TaskManager.Core.TaskAggregate;

namespace TaskManager.Infrastructure.Identity.User;

public class TaskManagerUser : IdentityUser
{
    public ICollection<ProjectEntity> LedProjects { get; set; }
    public ICollection<TaskEntity> CreatedTasks { get; set; }
    public ICollection<TaskEntity> AssignedTasks { get; set; }
    public ICollection<ProjectInvite> Invites { get; set; }
}