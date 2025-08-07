using TaskManager.Core.ProjectAggregate;
using TaskManager.UseCases.Projects.Create;
using TaskManager.UseCases.Projects.Get;
using TaskManager.UseCases.Projects.Update;
using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Projects;

public interface IProjectService
{
    Task<CreateProjectResult> CreateAsync(CreateProjectDto createProjectDto);
    Task<IEnumerable<ProjectEntity>> GetAllByUserAsync();
    Task<GetProjectResult> GetByIdAsync(long projectId);
    Task<UpdateProjectResult> UpdateAsync(UpdateProjectDto updateProjectDto);
    Task<Result> DeleteAsync(long id);
}