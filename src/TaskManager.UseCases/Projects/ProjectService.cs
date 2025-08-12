using Microsoft.Extensions.Logging;
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
    private readonly ILogger _logger;
    private readonly IProjectRepository _projectRepository;

    public ProjectService(IProjectRepository projectRepository, AppDbContext context,
        ICurrentUserService currentUserService, ILogger logger)
    {
        _projectRepository = projectRepository;
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<ProjectEntity>> CreateAsync(CreateProjectDto createProjectDto)
    {
        _logger.LogInformation("Creating project");

        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null)
        {
            _logger.LogWarning("Creating project failed - user unauthenticated");
            return Result<ProjectEntity>.Failure(UseCaseErrors.Unauthenticated);
        }

        var project = new ProjectEntity
        {
            CreatedAt = DateTime.UtcNow,
            Title = createProjectDto.Title,
            LeadUserId = currentUserId
        };

        _projectRepository.Create(project);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created Project: {ProjectId} successfully", project.Id);
        return Result<ProjectEntity>.Success(project);
    }

    public async Task<Result<IEnumerable<ProjectEntity>>> GetAllByUserAsync(RoleDto role)
    {
        _logger.LogInformation("Getting all projects by user");
        
        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null)
        {
            _logger.LogWarning("Getting projects failed - user unauthenticated");
            return Result<IEnumerable<ProjectEntity>>.Failure(UseCaseErrors.Unauthenticated);
        }

        var projectRole = role switch
        {
            RoleDto.Member => ProjectRole.Member,
            RoleDto.Manager => ProjectRole.Manager,
            _ => ProjectRole.Member
        };

        var projects = role switch
        {
            RoleDto.Any => await _projectRepository.GetAllByUserIdAsync(currentUserId),
            RoleDto.Lead => await _projectRepository.GetAllByUserIdWhereUserIsLead(currentUserId),
            RoleDto.Member or RoleDto.Manager => await _projectRepository.GetAllByUserIdWhereUserHasRoleAsync(
                currentUserId, projectRole),
            _ => await _projectRepository.GetAllByUserIdAsync(currentUserId)
        };

        _logger.LogInformation("Got projects successfully");
        return Result<IEnumerable<ProjectEntity>>.Success(projects);
    }

    public async Task<Result<ProjectEntity>> GetByIdAsync(long projectId)
    {
        _logger.LogInformation("Getting Project: {ProjectId}", projectId);
        
        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null) return Result<ProjectEntity>.Failure(UseCaseErrors.Unauthenticated);

        var project = await _projectRepository.FindByIdAsync(projectId);

        if (project is null)
        {
            _logger.LogWarning("Getting project failed - user unauthenticated");
            return Result<ProjectEntity>.Failure(GetProjectErrors.NotFound(projectId));
        }

        if (project.LeadUserId != currentUserId)
        {
            _logger.LogWarning("Getting project failed - access denied");
            return Result<ProjectEntity>.Failure(GetProjectErrors.AccessDenied);
        }

        _logger.LogInformation("Got project successfully");
        return Result<ProjectEntity>.Success(project);
    }

    public async Task<Result<ProjectEntity>> UpdateAsync(UpdateProjectDto updateProjectDto)
    {
        _logger.LogInformation("Updating Project: {ProjectId}", updateProjectDto.ProjectId);
        
        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null)
        {
            _logger.LogWarning("Updating project failed - user unauthenticated");
            return Result<ProjectEntity>.Failure(UseCaseErrors.Unauthenticated);
        }

        var project = await _projectRepository.FindByIdAsync(updateProjectDto.ProjectId);

        if (project is null)
        {
            _logger.LogWarning("Updating project failed - Project: {ProjectId} not found", updateProjectDto.ProjectId);
            return Result<ProjectEntity>.Failure(UpdateProjectErrors.NotFound(updateProjectDto.ProjectId));
        }

        if (project.LeadUserId != currentUserId)
        {
            _logger.LogWarning("Updating project failed - access denied");
            return Result<ProjectEntity>.Failure(UpdateProjectErrors.AccessDenied);
        }

        project.Title = updateProjectDto.ProjectTitle;

        _projectRepository.Update(project);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated project successfully");
        return Result<ProjectEntity>.Success(project);
    }

    public async Task<Result> DeleteAsync(long id)
    {
        _logger.LogInformation("Deleting Project: {ProjectId}", id);
        
        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null)
        {
            _logger.LogWarning("Deleting project failed - user unauthenticated");
            return Result.Failure(UseCaseErrors.Unauthenticated);
        }

        var project = await _projectRepository.FindByIdAsync(id);

        if (project is null)
        {
            _logger.LogWarning("Deleting project failed - Project: {ProjectId} not found", id);
            return Result.Failure(DeleteProjectErrors.NotFound(id));
        }

        if (project.LeadUserId != currentUserId)
        {
            _logger.LogWarning("Deleting project failed - access denied");
            return Result.Failure(DeleteProjectErrors.AccessDenied);
        }

        _projectRepository.Remove(project);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted project successfully");
        return Result.Success();
    }
}