namespace TaskManager.Core.ProjectAggregate;

public interface IProjectMemberRepository
{
    Task<ProjectMember?> GetByProjectIdAndMemberIdAsync(long projectId, string memberId);
}