namespace TaskManager.Core.ProjectAggregate;

public interface IProjectMemberRepository
{
    Task<ProjectMember?> GetByProjectIdAndMemberIdAsync(long projectId, string memberId);
    Task<bool> IsUserProjectMember(string userId, long projectId);
    Task<bool> IsUserProjectManager(string userId, long projectId);
    Task<IEnumerable<ProjectMemberWithUser>> GetProjectMembersWithUsersAsync(long projectId);
    void Update(ProjectMember projectMember);
    void Delete(ProjectMember projectMember);
}