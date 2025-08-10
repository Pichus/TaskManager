namespace TaskManager.Core.ProjectAggregate;

public class ProjectMember
{
    public long ProjectId { get; set; }
    public string MemberId { get; set; }
    public ProjectRole ProjectRole { get; set; }

    public ProjectEntity Project { get; set; }
}