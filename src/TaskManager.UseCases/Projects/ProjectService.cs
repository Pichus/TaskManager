using TaskManager.Core.ProjectAggregate;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Identity.CurrentUser;

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
}