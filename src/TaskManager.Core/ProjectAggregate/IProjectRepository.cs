namespace TaskManager.Core.ProjectAggregate;

public interface IProjectRepository
{
    void Create(ProjectEntity project);
    Task<ProjectEntity?> FindByIdAsync(long id);

    Task GetPendingInvitesByProjectId(long projectId);
}