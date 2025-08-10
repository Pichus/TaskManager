using TaskManager.Core.ProjectAggregate;
using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.ProjectMembers;

public interface IProjectMemberService
{
    Task<Result<IEnumerable<ProjectMemberWithUser>>> GetProjectMembersAsync(long projectId);
    Task<Result> UpdateProjectMemberAsync(long projectId, string userId, ProjectRole projectRole);
    Task<Result> DeleteAsync(long projectId, string memberId);
}