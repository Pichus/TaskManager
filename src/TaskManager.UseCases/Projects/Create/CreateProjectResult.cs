using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Projects.Create;

public class CreateProjectResult : Result
{
    public long ProjectId { get; set; }
    public string Title { get; set; }
    public string LeadUserId { get; set; }
}