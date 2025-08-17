using Microsoft.Extensions.Logging;
using TaskManager.Core.ProjectAggregate;
using TaskManager.Core.ProjectInviteAggregate;
using TaskManager.Infrastructure;
using TaskManager.Infrastructure.Identity.CurrentUser;
using TaskManager.UseCases.Invites.Response.Accept;
using TaskManager.UseCases.Invites.Response.Decline;
using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Invites.Response;

public class InviteResponseService : IInviteResponseService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<InviteResponseService> _logger;
    private readonly IProjectInviteRepository _projectInviteRepository;
    private readonly IProjectMemberRepository _projectMemberRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public InviteResponseService(ILogger<InviteResponseService> logger, ICurrentUserService currentUserService,
        IProjectInviteRepository projectInviteRepository, IProjectRepository projectRepository, IUnitOfWork unitOfWork,
        IProjectMemberRepository projectMemberRepository)
    {
        _logger = logger;
        _currentUserService = currentUserService;
        _projectInviteRepository = projectInviteRepository;
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
        _projectMemberRepository = projectMemberRepository;
    }

    public async Task<Result> AcceptInviteAsync(long inviteId)
    {
        _logger.LogInformation("Accepting Invite: {InviteId}", inviteId);

        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null)
        {
            _logger.LogWarning("Accepting invite failed - user unauthenticated");
            return Result.Failure(UseCaseErrors.Unauthenticated);
        }

        var invite = await _projectInviteRepository.FindByIdAsync(inviteId);

        if (invite is null)
        {
            _logger.LogWarning("Accepting invite failed - invite not found");
            return Result.Failure(AcceptInviteErrors.InviteNotFound);
        }

        if (invite.Status == InviteStatus.Accepted)
        {
            _logger.LogWarning("Accepting invite failed - invite already accepted");
            return Result.Failure(AcceptInviteErrors.InviteAlreadyAccepted);
        }

        if (invite.Status == InviteStatus.Rejected)
        {
            _logger.LogWarning("Accepting invite failed - invite already rejected");
            return Result.Failure(AcceptInviteErrors.InviteAlreadyRejected);
        }

        if (invite.InvitedUserId != currentUserId)
        {
            _logger.LogWarning("Accepting invite failed - access denied");
            return Result.Failure(AcceptInviteErrors.AccessDenied);
        }

        var isUserProjectParticipantAsync =
            await _projectMemberRepository.IsUserProjectParticipantAsync(currentUserId, invite.ProjectId);

        if (isUserProjectParticipantAsync)
        {
            _logger.LogWarning("Accepting invite failed - invited user is already a member");
            return Result.Failure(AcceptInviteErrors.InvitedUserAlreadyAMember);
        }

        var project = await _projectRepository.FindByIdAsync(invite.ProjectId);

        if (project is null)
        {
            _logger.LogWarning("Accepting invite failed - project not found");
            return Result.Failure(AcceptInviteErrors.ProjectNotFound);
        }

        var defaultProjectRole = ProjectRole.Member;

        var member = new ProjectMember
        {
            ProjectId = project.Id,
            MemberId = currentUserId,
            ProjectRole = defaultProjectRole
        };

        _projectMemberRepository.Create(member);

        invite.Status = InviteStatus.Accepted;
        _projectInviteRepository.Update(invite);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Accepted invite successfully");
        return Result.Success();
    }

    public async Task<Result> DeclineInviteAsync(long inviteId)
    {
        _logger.LogInformation("Declining Invite: {InviteId}", inviteId);

        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null)
        {
            _logger.LogWarning("Declining invite failed - user unauthenticated");
            return Result.Failure(UseCaseErrors.Unauthenticated);
        }

        var invite = await _projectInviteRepository.FindByIdAsync(inviteId);

        if (invite is null)
        {
            _logger.LogWarning("Declining invite failed - invite not found");
            return Result.Failure(DeclineInviteErrors.InviteNotFound);
        }

        if (invite.Status == InviteStatus.Accepted)
        {
            _logger.LogWarning("Declining invite failed - invite already accepted");
            return Result.Failure(DeclineInviteErrors.InviteAlreadyAccepted);
        }

        if (invite.Status == InviteStatus.Rejected)
        {
            _logger.LogWarning("Declining invite failed - invite already rejected");
            return Result.Failure(DeclineInviteErrors.InviteAlreadyRejected);
        }

        if (invite.InvitedUserId != currentUserId)
        {
            _logger.LogWarning("Declining invite failed - access denied");
            return Result.Failure(DeclineInviteErrors.AccessDenied);
        }

        var project = await _projectRepository.FindByIdAsync(invite.ProjectId);

        if (project is null)
        {
            _logger.LogWarning("Declining invite failed - Project: {ProjectId} not found", invite.ProjectId);
            return Result.Failure(DeclineInviteErrors.ProjectNotFound);
        }

        invite.Status = InviteStatus.Rejected;
        _projectInviteRepository.Update(invite);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Declined invite successfully");
        return Result.Success();
    }
}