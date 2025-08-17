using Microsoft.Extensions.Logging;
using TaskManager.Core.ProjectAggregate;
using TaskManager.Core.ProjectInviteAggregate;
using TaskManager.Core.Shared;
using TaskManager.Infrastructure.Identity.CurrentUser;
using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Invites.Retrieve;

public class InviteRetrievalService : IInviteRetrievalService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<InviteRetrievalService> _logger;
    private readonly IProjectInviteRepository _projectInviteRepository;
    private readonly IProjectMemberRepository _projectMemberRepository;
    private readonly IProjectRepository _projectRepository;

    public InviteRetrievalService(ILogger<InviteRetrievalService> logger, ICurrentUserService currentUserService,
        IProjectInviteRepository projectInviteRepository, IProjectMemberRepository projectMemberRepository,
        IProjectRepository projectRepository)
    {
        _logger = logger;
        _currentUserService = currentUserService;
        _projectInviteRepository = projectInviteRepository;
        _projectMemberRepository = projectMemberRepository;
        _projectRepository = projectRepository;
    }

    public async Task<Result<PagedData<ProjectInvite>>> RetrievePendingInvitesForCurrentUserAsync(
        RetrievePendingInvitesDto dto)
    {
        _logger.LogInformation("Getting pending invites for current user");

        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null)
        {
            _logger.LogWarning("Getting pending invites for current user failed - user unauthenticated");
            return Result<PagedData<ProjectInvite>>.Failure(UseCaseErrors.Unauthenticated);
        }

        var pendingInvites = await _projectInviteRepository
            .GetPendingInvitesByInvitedUserIdAsync(currentUserId, dto.PageNumber, dto.PageSize);

        _logger.LogInformation("Got pending invites for current user successfully");
        return Result<PagedData<ProjectInvite>>.Success(pendingInvites);
    }

    public async Task<Result<IEnumerable<ProjectInvite>>> RetrievePendingProjectInvitesAsync(long projectId)
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
            return Result<IEnumerable<ProjectInvite>>.Failure(RetrieveInvitesErrors.ProjectNotFound);
        }

        var canCurrentUserGetPendingInvitesForProject =
            await _projectMemberRepository.IsUserProjectLeadAsync(currentUserId, projectId) ||
            await _projectMemberRepository.IsUserProjectManagerAsync(currentUserId, projectId);


        if (!canCurrentUserGetPendingInvitesForProject)
        {
            _logger.LogWarning("Getting pending invites for project failed - access denied");
            return Result<IEnumerable<ProjectInvite>>.Failure(RetrieveInvitesErrors.AccessDenied);
        }

        var invites = project.Invites;

        _logger.LogWarning("Got pending invites for project successfully");
        return Result<IEnumerable<ProjectInvite>>.Success(invites);
    }
}