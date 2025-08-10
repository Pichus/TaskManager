using TaskManager.Core.ProjectAggregate;
using TaskManager.UseCases.Projects.Create;
using TaskManager.UseCases.Projects.Update;
using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Projects;

public interface IProjectService
{
    Task<Result<ProjectEntity>> CreateAsync(CreateProjectDto createProjectDto);
    Task<Result<IEnumerable<ProjectEntity>>> GetAllByUserAsync();
    Task<Result<ProjectEntity>> GetByIdAsync(long projectId);
    Task<Result<ProjectEntity>> UpdateAsync(UpdateProjectDto updateProjectDto);
    Task<Result> DeleteAsync(long id);
}