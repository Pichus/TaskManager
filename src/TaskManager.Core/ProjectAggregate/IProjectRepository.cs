namespace TaskManager.Core.ProjectAggregate;

public interface IProjectRepository
{
    void Create(ProjectEntity project);
    Task<ProjectEntity?> FindByIdAsync(long id);
    Task<IEnumerable<ProjectEntity>> GetAllByUserIdAsync(string userId);
    void Update(ProjectEntity project);
    void Remove(ProjectEntity project);
}