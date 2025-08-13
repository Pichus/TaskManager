using TaskManager.Core.Shared;

namespace TaskManager.Core.ProjectAggregate;

public interface IProjectMemberRepository : IRepositoryBase<ProjectMember, long>
{
    Task<ProjectMember?> GetByProjectIdAndMemberIdAsync(long projectId, string memberId);
    Task<bool> IsUserProjectMemberAsync(string userId, long projectId);
    Task<bool> IsUserProjectManagerAsync(string userId, long projectId);
    Task<bool> IsUserProjectLeadAsync(string userId, long projectId);
    Task<bool> IsUserProjectParticipantAsync(string userId, long projectId);
    Task<IEnumerable<ProjectMemberWithUser>> GetProjectMembersWithUsersAsync(long projectId);
}