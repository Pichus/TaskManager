namespace TaskManager.Core.ProjectAggregate;

public class ProjectMemberWithUser
{
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public Role Role { get; set; }
}