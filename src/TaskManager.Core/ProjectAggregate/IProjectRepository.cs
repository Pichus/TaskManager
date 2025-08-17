using TaskManager.Core.Shared;

namespace TaskManager.Core.ProjectAggregate;

public interface IProjectRepository : IRepositoryBase<ProjectEntity, long>
{
    Task<ProjectEntity?> FindByIdWithProjectMembersIncludedAsync(long id);
    Task<ProjectEntity?> FindByIdWithInvitesIncludedAsync(long id);
    Task<PagedData<ProjectEntity>> GetAllByUserIdAsync(string userId, int pageNumber, int pageSize);
    Task<PagedData<ProjectEntity>> GetAllByUserIdWhereUserIsLead(string userId, int pageNumber, int pageSize);

    Task<PagedData<ProjectEntity>> GetAllByUserIdWhereUserHasRoleAsync(string userId, ProjectRole role, int pageNumber,
        int pageSize);
}