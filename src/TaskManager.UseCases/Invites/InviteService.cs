using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using TaskManager.Core.ProjectAggregate;
using TaskManager.Core.ProjectInviteAggregate;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Identity.CurrentUser;
using TaskManager.Infrastructure.Identity.User;
using TaskManager.UseCases.Invites.GetPendingForProject;
using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Invites;

public class InviteService : IInviteService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly AppDbContext _dbContext;
    private readonly ILogger _logger;
    private readonly IProjectInviteRepository _projectInviteRepository;
    private readonly IProjectMemberRepository _projectMemberRepository;
    private readonly IProjectRepository _projectRepository;

    public InviteService(IProjectInviteRepository projectInviteRepository, IProjectRepository projectRepository,
        ICurrentUserService currentUserService, AppDbContext dbContext, UserManager<TaskManagerUser> userManager,
        IProjectMemberRepository projectMemberRepository, ILogger logger)
    {
        _projectInviteRepository = projectInviteRepository;
        _projectRepository = projectRepository;
        _currentUserService = currentUserService;
        _dbContext = dbContext;
        _projectMemberRepository = projectMemberRepository;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<ProjectInvite>>> GetPendingInvitesForCurrentUser()
    {
        _logger.LogInformation("Getting pending invites for current user");

        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null)
        {
            _logger.LogWarning("Getting pending invites for current user failed - user unauthenticated");
            return Result<IEnumerable<ProjectInvite>>.Failure(UseCaseErrors.Unauthenticated);
        }

        var pendingInvites = await _projectInviteRepository
            .GetPendingInvitesByInvitedUserIdAsync(currentUserId);

        _logger.LogInformation("Got pending invites for current user successfully");
        return Result<IEnumerable<ProjectInvite>>.Success(pendingInvites);
    }

    public async Task<Result<IEnumerable<ProjectInvite>>> GetPendingProjectInvitesAsync(long projectId)
    {
        _logger.LogInformation("Getting pending invites for Project: {ProjectId}", projectId);

        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null)
        {
            _logger.LogWarning("Getting pending invites for project failed - user unauthenticated");
            return Result<IEnumerable<ProjectInvite>>.Failure(UseCaseErrors.Unauthenticated);
        }

        var project = await _projectRepository.FindByIdWithInvitesIncludedAsync(projectId);

        if (project is null)
        {
            _logger.LogWarning("Getting pending invites for project failed - project not found");
            return Result<IEnumerable<ProjectInvite>>.Failure(GetPendingInvitesForProjectErrors.ProjectNotFound);
        }

        var canCurrentUserGetPendingInvitesForProject = await IsUserProjectLeadOrManagerAsync(currentUserId, project);

        if (!canCurrentUserGetPendingInvitesForProject)
        {
            _logger.LogWarning("Getting pending invites for project failed - access denied");
            return Result<IEnumerable<ProjectInvite>>.Failure(GetPendingInvitesForProjectErrors.AccessDenied);
        }

        var invites = project.Invites;

        _logger.LogWarning("Got pending invites for project successfully");
        return Result<IEnumerable<ProjectInvite>>.Success(invites);
    }

    private async Task<bool> IsUserProjectLeadOrManagerAsync(string userId, ProjectEntity project)
    {
        var isUserProjectManager = await _projectMemberRepository.IsUserProjectManagerAsync(userId, project.Id);

        return userId == project.LeadUserId || isUserProjectManager;
    }
}