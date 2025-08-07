using TaskManager.Core.ProjectInviteAggregate;

namespace TaskManager.ProjectInvites.Create;

public class CreateInviteResponse
{
    public long InviteId { get; set; }
    public string InvitedUserId { get; set; }
    public string InvitedByUserId { get; set; }
    public string Status { get; set; }
}