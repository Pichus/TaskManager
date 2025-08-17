using TaskManager.Core.ProjectAggregate;
using TaskManager.Core.Shared;
using TaskManager.UseCases.ProjectMembers.Get;
using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.ProjectMembers;

public interface IProjectMemberService
{
    Task<Result<PagedData<ProjectMemberWithUser>>> GetProjectMembersAsync(GetProjectMembersDto dto);
    Task<Result> UpdateProjectMemberAsync(long projectId, string userId, ProjectRole projectRole);
    Task<Result> DeleteAsync(long projectId, string memberId);
}