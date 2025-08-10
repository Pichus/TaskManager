namespace TaskManager.Core.ProjectAggregate;

public class MemberRole : EntityBase
{
    public long ProjectMemberId { get; set; }
    public Role Role { get; set; }
    
    public ProjectMember ProjectMember { get; set; }
}