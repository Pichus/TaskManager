using TaskManager.Core.ProjectAggregate;
using TaskManager.Core.Shared;

namespace TaskManager.Core.ProjectInviteAggregate;

public class ProjectInvite : EntityBase, IAggregateRoot
{
    public long ProjectId { get; set; }
    public string InvitedUserId { get; set; }
    public string InvitedByUserId { get; set; }
    public InviteStatus Status { get; set; }

    public ProjectEntity Project { get; set; }
}