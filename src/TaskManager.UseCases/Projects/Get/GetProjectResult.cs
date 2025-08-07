using TaskManager.Core.ProjectAggregate;
using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Projects.Get;

public class GetProjectResult : Result
{
    public ProjectEntity Project { get; set; }
}