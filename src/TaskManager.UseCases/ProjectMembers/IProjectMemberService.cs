using TaskManager.Core.ProjectAggregate;
using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.ProjectMembers;

public interface IProjectMemberService
{
    Task<Result<IEnumerable<ProjectMemberWithUser>>> GetProjectMembersAsync(long projectId);
}