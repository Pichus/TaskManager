namespace TaskManager.UseCases.ProjectMembers.Get;

public class GetProjectMembersDto
{
    public long ProjectId { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}