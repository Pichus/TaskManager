using System.Linq.Expressions;

namespace TaskManager.Core.ProjectInviteAggregate;

public interface IProjectInviteRepository
{
    void Create(ProjectInvite invite);

    Task<bool> AnyAsync(Expression<Func<ProjectInvite, bool>> predicate);
}