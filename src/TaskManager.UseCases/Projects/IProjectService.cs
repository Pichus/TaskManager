using TaskManager.UseCases.Projects.Create;
using TaskManager.UseCases.Projects.Get;

namespace TaskManager.UseCases.Projects;

public interface IProjectService
{
    Task<CreateProjectResult> CreateAsync(CreateProjectDto createProjectDto);
    Task<IEnumerable<GetProjectResult>> GetAllByUserAsync();
    Task<GetProjectResult> GetByIdAsync(long projectId);
}