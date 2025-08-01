using Microsoft.AspNetCore.Identity;
using TaskManager.Core.ProjectAggregate;

namespace TaskManager.Core.UserAggregate;

public class TaskManagerUser : IdentityUser
{
    public List<ProjectEntity> Projects { get; set; }
}