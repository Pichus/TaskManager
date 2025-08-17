using TaskManager.Core.Shared;

namespace TaskManager.Core.ProjectInviteAggregate;

public interface IProjectInviteRepository : IRepositoryBase<ProjectInvite, long>
{
    Task<PagedData<ProjectInvite>> GetPendingInvitesByInvitedUserIdAsync(string userId, int pageNumber, int pageSize);
    Task<PagedData<ProjectInvite>> GetPendingInvitesByProjectIdAsync(long projectId, int pageNumber, int pageSize);
    Task<bool> InviteExistsAsync(string invitedUserId, long projectId);
}