using Microsoft.AspNetCore.Identity;
using TaskManager.Core.ProjectAggregate;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Identity.CurrentUser;
using TaskManager.Infrastructure.Identity.User;
using TaskManager.UseCases.ProjectMembers.Get;
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
    private readonly IProjectMemberRepository _projectMemberRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly UserManager<TaskManagerUser> _userManager;

    public ProjectService(IProjectRepository projectRepository, AppDbContext context,
        ICurrentUserService currentUserService, UserManager<TaskManagerUser> userManager,
        IProjectMemberRepository projectMemberRepository)
    {
        _projectRepository = projectRepository;
        _context = context;
        _currentUserService = currentUserService;
        _userManager = userManager;
        _projectMemberRepository = projectMemberRepository;
    }

    public async Task<Result<ProjectEntity>> CreateAsync(CreateProjectDto createProjectDto)
    {
        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null) return Result<ProjectEntity>.Failure(UseCaseErrors.Unauthenticated);

        var project = new ProjectEntity
        {
            CreatedAt = DateTime.UtcNow,
            Title = createProjectDto.Title,
            LeadUserId = currentUserId
        };

        _projectRepository.Create(project);

        await _context.SaveChangesAsync();

        return Result<ProjectEntity>.Success(project);
    }

    public async Task<Result<IEnumerable<ProjectEntity>>> GetAllByUserAsync()
    {
        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null) return Result<IEnumerable<ProjectEntity>>.Failure(UseCaseErrors.Unauthenticated);

        var projects = await _projectRepository.GetAllByUserIdAsync(currentUserId);

        return Result<IEnumerable<ProjectEntity>>.Success(projects);
    }

    public async Task<Result<ProjectEntity>> GetByIdAsync(long projectId)
    {
        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null) return Result<ProjectEntity>.Failure(UseCaseErrors.Unauthenticated);

        var project = await _projectRepository.FindByIdAsync(projectId);

        if (project is null)
            return Result<ProjectEntity>.Failure(GetProjectErrors.NotFound(projectId));

        if (project.LeadUserId != currentUserId)
            return Result<ProjectEntity>.Failure(GetProjectErrors.AccessDenied);

        return Result<ProjectEntity>.Success(project);
    }

    public async Task<Result<ProjectEntity>> UpdateAsync(UpdateProjectDto updateProjectDto)
    {
        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null) return Result<ProjectEntity>.Failure(UseCaseErrors.Unauthenticated);

        var project = await _projectRepository.FindByIdAsync(updateProjectDto.ProjectId);

        if (project is null)
            return Result<ProjectEntity>.Failure(UpdateProjectErrors.NotFound(updateProjectDto.ProjectId));

        if (project.LeadUserId != currentUserId)
            return Result<ProjectEntity>.Failure(UpdateProjectErrors.AccessDenied);

        project.Title = updateProjectDto.ProjectTitle;

        _projectRepository.Update(project);
        await _context.SaveChangesAsync();

        return Result<ProjectEntity>.Success(project);
    }

    public async Task<Result> DeleteAsync(long id)
    {
        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null) return Result.Failure(UseCaseErrors.Unauthenticated);

        var project = await _projectRepository.FindByIdAsync(id);

        if (project is null)
            return Result.Failure(DeleteProjectErrors.NotFound(id));

        if (project.LeadUserId != currentUserId)
            return Result.Failure(DeleteProjectErrors.AccessDenied);

        _projectRepository.Remove(project);
        await _context.SaveChangesAsync();

        return Result.Success();
    }
}