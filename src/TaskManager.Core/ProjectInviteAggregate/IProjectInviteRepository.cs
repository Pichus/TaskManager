using System.Linq.Expressions;
using TaskManager.Core.Shared;

namespace TaskManager.Core.ProjectInviteAggregate;

public interface IProjectInviteRepository : IRepositoryBase<ProjectInvite, long>
{
    Task<bool> AnyAsync(Expression<Func<ProjectInvite, bool>> predicate);
    IQueryable<ProjectInvite> GetPendingInvitesByInvitedUserIdAsync(string userId);
}