using TaskManager.Core.ProjectAggregate;
using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Projects.Update;

public class UpdateProjectResult : Result
{
    public ProjectEntity? Project { get; set; }
}