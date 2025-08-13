using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Invites.Delete;

public interface IInviteDeletionService
{
    Task<Result> DeleteAsync(long inviteId);
}