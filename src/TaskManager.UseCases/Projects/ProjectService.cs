using TaskManager.Core.ProjectAggregate;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Identity.CurrentUser;
using TaskManager.UseCases.Projects.Create;
using TaskManager.UseCases.Projects.Delete;
using TaskManager.UseCases.Projects.Get;
using TaskManager.UseCases.Projects.Update;
using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Projects;

public class ProjectService : IProjectService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IProjectRepository _projectRepository;

    public ProjectService(IProjectRepository projectRepository, AppDbContext context,
        ICurrentUserService currentUserService)
    {
        _projectRepository = projectRepository;
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<CreateProjectResult> CreateAsync(CreateProjectDto createProjectDto)
    {
        var project = new ProjectEntity
        {
            CreatedAt = DateTime.UtcNow,
            Title = createProjectDto.Title,
            LeadUserId = _currentUserService.UserId
        };

        _projectRepository.Create(project);
        await _context.SaveChangesAsync();

        return new CreateProjectResult
        {
            Project = project
        };
    }

    public async Task<IEnumerable<ProjectEntity>> GetAllByUserAsync()
    {
        var projects = await _projectRepository.GetAllByUserIdAsync(_currentUserService.UserId);

        return projects;
    }

    public async Task<GetProjectResult> GetByIdAsync(long projectId)
    {
        var project = await _projectRepository.FindByIdAsync(projectId);

        if (project is null)
            return new GetProjectResult
            {
                Success = false,
                Error = GetProjectErrors.NotFound(projectId)
            };

        if (project.LeadUserId != _currentUserService.UserId)
            return new GetProjectResult
            {
                Success = false,
                Error = GetProjectErrors.AccessDenied
            };

        return new GetProjectResult
        {
            Project = project
        };
    }

    public async Task<UpdateProjectResult> UpdateAsync(UpdateProjectDto updateProjectDto)
    {
        var project = await _projectRepository.FindByIdAsync(updateProjectDto.ProjectId);

        if (project is null)
        {
            return new UpdateProjectResult
            {
                Success = false,
                Error = UpdateProjectErrors.NotFound(updateProjectDto.ProjectId),
            };
        }

        var currentUserId = _currentUserService.UserId;

        if (project.LeadUserId != currentUserId)
        {
            return new UpdateProjectResult
            {
                Success = false,
                Error = UpdateProjectErrors.AccessDenied,
            };
        }

        project.Title = updateProjectDto.ProjectTitle;
        
        _projectRepository.Update(project);
        await _context.SaveChangesAsync();

        return new UpdateProjectResult
        {
            Project = project
        };
    }

    public async Task<Result> DeleteAsync(long id)
    {
        var project = await _projectRepository.FindByIdAsync(id);

        if (project is null)
        {
            return new Result
            {
                Success = false,
                Error = DeleteProjectErrors.NotFound(id)
            };
        }

        var currentUserId = _currentUserService.UserId;
        
        if (project.LeadUserId != currentUserId)
        {
            return new Result
            {
                Success = false,
                Error = DeleteProjectErrors.AccessDenied
            };
        }
        
        _projectRepository.Remove(project);
        await _context.SaveChangesAsync();

        return new Result();
    }
}