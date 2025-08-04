using TaskManager.Core.ProjectAggregate;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Identity.CurrentUser;
using TaskManager.UseCases.Projects.Create;
using TaskManager.UseCases.Projects.Get;

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
            ProjectId = project.Id,
            Title = project.Title,
            LeadUserId = project.LeadUserId
        };
    }

    public async Task<IEnumerable<GetProjectResult>> GetAllByUserAsync()
    {
        var projects = await _projectRepository.GetAllByUserIdAsync(_currentUserService.UserId);

        var projectDtos = projects.Select(project => new GetProjectResult
        {
            Title = project.Title,
            LeadUserId = project.LeadUserId
        });

        return projectDtos;
    }

    public async Task<GetProjectResult> GetByIdAsync(long projectId)
    {
        var project = await _projectRepository.FindByIdAsync(projectId);

        if (project is null)
            return new GetProjectResult
            {
                Success = false,
                ErrorMessage = $"Project with id {projectId} does not exist"
            };

        if (project.LeadUserId != _currentUserService.UserId)
            return new GetProjectResult
            {
                Success = false,
                ErrorMessage = "You are not the project lead"
            };

        return new GetProjectResult
        {
            Title = project.Title,
            LeadUserId = project.LeadUserId
        };
    }
}