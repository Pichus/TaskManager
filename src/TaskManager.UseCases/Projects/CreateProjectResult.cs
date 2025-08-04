using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Projects;

public class CreateProjectResult : ResultBase
{
    public long ProjectId { get; set; }
    public string Title { get; set; }
    public string LeadUserId { get; set; }
}