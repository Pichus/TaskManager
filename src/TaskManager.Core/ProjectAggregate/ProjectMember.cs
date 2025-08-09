namespace TaskManager.Core.ProjectAggregate;

public class ProjectMember
{
    public long ProjectId { get; set; }
    public string MemberId { get; set; }
    
    public ProjectEntity Project { get; set; }
    public MemberRole MemberRole { get; set; }
}