using TaskManager.Core.Shared;

namespace TaskManager.Core.ProjectAggregate;

public interface IProjectRepository : IRepositoryBase<ProjectEntity, long>
{
    Task<ProjectEntity?> FindByIdWithProjectMembersIncludedAsync(long id);
    Task<ProjectEntity?> FindByIdWithInvitesIncludedAsync(long id);
    Task<IEnumerable<ProjectEntity>> GetAllByUserIdAsync(string userId);
    Task<IEnumerable<ProjectEntity>> GetAllByUserIdWhereUserIsLead(string userId);
    Task<IEnumerable<ProjectEntity>> GetAllByUserIdWhereUserHasRoleAsync(string userId, ProjectRole role);
}