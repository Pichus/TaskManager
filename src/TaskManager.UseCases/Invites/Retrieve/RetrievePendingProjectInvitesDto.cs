namespace TaskManager.UseCases.Invites.Retrieve;

public class RetrievePendingProjectInvitesDto
{
    public long ProjectId { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}