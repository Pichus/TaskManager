using System.Linq.Expressions;
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

    public async Task<bool> AnyAsync(Expression<Func<ProjectInvite, bool>> predicate)
    {
        return await Context.ProjectInvites
            .AnyAsync(predicate);
    }

    public IQueryable<ProjectInvite> GetPendingInvitesByInvitedUserIdAsync(string userId)
    {
        return Context.ProjectInvites.Where(invite =>
            invite.InvitedUserId == userId && invite.Status == InviteStatus.Pending);
    }
}