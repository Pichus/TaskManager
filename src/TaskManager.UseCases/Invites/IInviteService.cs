using TaskManager.Core.ProjectInviteAggregate;
using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Invites;

public interface IInviteService
{
    Task<Result<IEnumerable<ProjectInvite>>> GetPendingInvitesForCurrentUser();
    Task<Result<IEnumerable<ProjectInvite>>> GetPendingProjectInvitesAsync(long projectId);
}