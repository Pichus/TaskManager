using Microsoft.EntityFrameworkCore;
using TaskManager.Core.ProjectInviteAggregate;
using TaskManager.Core.Shared;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Shared;

namespace TaskManager.Infrastructure.ProjectInvites;

public class ProjectInviteRepository : RepositoryBase<ProjectInvite, long>, IProjectInviteRepository
{
    public ProjectInviteRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<PagedData<ProjectInvite>> GetPendingInvitesByInvitedUserIdAsync(string userId, int pageNumber,
        int pageSize)
    {
        var invitesQuery = Context
            .ProjectInvites
            .Where(invite => invite.InvitedUserId == userId
                             && invite.Status == InviteStatus.Pending)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageNumber);

        var invites = await invitesQuery.ToListAsync();
        var totalRecords = await invitesQuery.CountAsync();

        return new PagedData<ProjectInvite>(invites, pageNumber, pageSize, totalRecords);
    }

    public async Task<bool> InviteExistsAsync(string invitedUserId, long projectId)
    {
        return await Context
            .ProjectInvites
            .AnyAsync(projectInvite => projectInvite.InvitedUserId == invitedUserId
                                       && projectInvite.ProjectId == projectId);
    }
}