namespace TaskManager.UseCases.Invites.Create;

public class CreateInviteDto
{
    public long ProjectId { get; set; }
    public string InvitedUserId { get; set; }
}