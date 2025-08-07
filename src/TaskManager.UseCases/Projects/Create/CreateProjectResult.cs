using TaskManager.Core.ProjectAggregate;
using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Projects.Create;

public class CreateProjectResult : Result
{
    public ProjectEntity Project { get; set; }
}