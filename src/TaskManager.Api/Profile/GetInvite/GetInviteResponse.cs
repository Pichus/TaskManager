namespace TaskManager.Profile.GetInvite;

public class GetInviteResponse
{
    public long InviteId { get; set; }
    public long ProjectId { get; set; }
    public string InvitedUserId { get; set; }
    public string InvitedByUserId { get; set; }
    public string InviteStatus { get; set; }
}