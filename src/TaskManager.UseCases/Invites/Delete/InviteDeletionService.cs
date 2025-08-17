using Microsoft.Extensions.Logging;
using TaskManager.Core.ProjectInviteAggregate;
using TaskManager.Infrastructure;
using TaskManager.Infrastructure.Identity.CurrentUser;
using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Invites.Delete;

public class InviteDeletionService : IInviteDeletionService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<InviteDeletionService> _logger;
    private readonly IProjectInviteRepository _projectInviteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public InviteDeletionService(ILogger<InviteDeletionService> logger, ICurrentUserService currentUserService,
        IProjectInviteRepository projectInviteRepository, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _currentUserService = currentUserService;
        _projectInviteRepository = projectInviteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> DeleteAsync(long inviteId)
    {
        _logger.LogInformation("Deleting Invite: {InvitedId}", inviteId);

        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null)
        {
            _logger.LogWarning("Deleting invite failed - user unauthenticated");
            return Result.Failure(UseCaseErrors.Unauthenticated);
        }

        var invite = await _projectInviteRepository.FindByIdAsync(inviteId);

        if (invite is null)
        {
            _logger.LogWarning("Deleting invite failed - invite not found");
            return Result.Failure(DeleteInviteErrors.InviteNotFound);
        }

        var canDeleteInvite = invite.InvitedByUserId == currentUserId;

        if (!canDeleteInvite)
        {
            _logger.LogWarning("Deleting invite failed - access denied");
            return Result.Failure(DeleteInviteErrors.AccessDenied);
        }

        _projectInviteRepository.Remove(invite);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Deleted invite successfully");
        return Result.Success();
    }
}