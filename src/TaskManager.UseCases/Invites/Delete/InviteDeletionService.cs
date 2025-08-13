using Microsoft.Extensions.Logging;
using TaskManager.Core.ProjectInviteAggregate;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Identity.CurrentUser;
using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Invites.Delete;

public class InviteDeletionService : IInviteDeletionService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly AppDbContext _dbContext;
    private readonly ILogger _logger;
    private readonly IProjectInviteRepository _projectInviteRepository;

    public InviteDeletionService(ILogger logger, ICurrentUserService currentUserService,
        IProjectInviteRepository projectInviteRepository, AppDbContext dbContext)
    {
        _logger = logger;
        _currentUserService = currentUserService;
        _projectInviteRepository = projectInviteRepository;
        _dbContext = dbContext;
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

        if (canDeleteInvite)
        {
            _logger.LogWarning("Deleting invite failed - access denied");
            return Result.Failure(DeleteInviteErrors.AccessDenied);
        }

        _projectInviteRepository.Remove(invite);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Deleted invite successfully");
        return Result.Success();
    }
}