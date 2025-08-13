using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Invites.Response;

public interface IInviteResponseService
{
    Task<Result> AcceptInviteAsync(long inviteId);
    Task<Result> DeclineInviteAsync(long inviteId);
}