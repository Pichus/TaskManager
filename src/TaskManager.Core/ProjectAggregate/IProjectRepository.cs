namespace TaskManager.Core.ProjectAggregate;

public interface IProjectRepository
{
    void Create(ProjectEntity project);
    Task<ProjectEntity?> FindByIdAsync(long id);
    Task<IEnumerable<ProjectEntity>> GetAllByUserIdAsync(string userId);

    Task GetPendingInvitesByProjectId(long projectId);
}