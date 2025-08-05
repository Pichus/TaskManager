using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Projects.Get;

public class GetProjectResult : Result
{
    public string Title { get; set; }
    public string LeadUserId { get; set; }
}