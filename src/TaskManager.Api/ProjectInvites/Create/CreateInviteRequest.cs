namespace TaskManager.ProjectInvites.Create;

public class CreateInviteRequest
{
    public long ProjectId { get; set; }
    public string InvitedUserId { get; set; }
}