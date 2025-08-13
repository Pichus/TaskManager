using TaskManager.Core.Shared;

namespace TaskManager.Core.ProjectInviteAggregate;

public interface IProjectInviteRepository : IRepositoryBase<ProjectInvite, long>
{
    Task<IEnumerable<ProjectInvite>> GetPendingInvitesByInvitedUserIdAsync(string userId);
    Task<bool> InviteExistsAsync(string invitedUserId, long projectId);
}