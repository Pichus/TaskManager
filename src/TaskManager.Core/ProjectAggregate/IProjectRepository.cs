namespace TaskManager.Core.ProjectAggregate;

public interface IProjectRepository
{
    void Create(ProjectEntity project);
    Task<ProjectEntity?> FindByIdAsync(long id);
    Task<ProjectEntity?> FindByIdWithProjectMembersIncludedAsync(long id);
    Task<IEnumerable<ProjectEntity>> GetAllByUserIdAsync(string userId);
    void Update(ProjectEntity project);
    void Remove(ProjectEntity project);
    Task<bool> IsUserProjectMemberAsync(string currentUserId, long projectId);
    void AddMember(ProjectEntity project, string memberId);
}