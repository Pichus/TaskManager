namespace TaskManager.Core.ProjectAggregate;

public class MemberRole
{
    public string UserId { get; set; }
    public long ProjectId { get; set; }
    public Role Role { get; set; }
}