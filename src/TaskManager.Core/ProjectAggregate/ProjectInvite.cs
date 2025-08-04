namespace TaskManager.Core.ProjectAggregate;

public class ProjectInvite
{
    public long ProjectId { get; set; }
    public string InvitedUserId { get; set; }
    public string InvitedByUserId { get; set; }
    public DateTime InvitedAt { get; set; }
    public InviteStatus InviteStatus { get; set; }

    public ProjectEntity Project { get; set; }
}