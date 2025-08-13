using Microsoft.EntityFrameworkCore;
using TaskManager.Core.ProjectInviteAggregate;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Shared;

namespace TaskManager.Infrastructure.ProjectInvites;

public class ProjectInviteRepository : RepositoryBase<ProjectInvite, long>, IProjectInviteRepository
{
    public ProjectInviteRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ProjectInvite>> GetPendingInvitesByInvitedUserIdAsync(string userId)
    {
        return await Context
            .ProjectInvites
            .Where(invite => invite.InvitedUserId == userId
                             && invite.Status == InviteStatus.Pending)
            .ToListAsync();
    }

    public async Task<bool> InviteExistsAsync(string invitedUserId, long projectId)
    {
        return await Context
            .ProjectInvites
            .AnyAsync(projectInvite => projectInvite.InvitedUserId == invitedUserId
                                       && projectInvite.ProjectId == projectId);
    }
}