namespace TaskManager.UseCases.Projects;

public interface IProjectService
{
    Task<CreateProjectResult> CreateAsync(CreateProjectDto createProjectDto);
}