using TaskManager.Core.Shared;

namespace TaskManager.Core.TaskAggregate;

public interface ITaskRepository
{
    void Create(TaskEntity task);

    Task<PagedData<TaskEntity>> GetAll(string sortOrder, int pageNumber = 1, int pageSize = 25,
        Status? status = null, long? projectId = null, string? assigneeUserId = null);

    Task<IEnumerable<TaskEntity>> GetAllByProjectIdAsync(long projectId);
    Task<IEnumerable<TaskEntity>> GetAllByProjectIdAndStatusAsync(long projectId, Status taskStatus);
    Task<TaskEntity?> FindByIdAsync(long taskId);
    void Update(TaskEntity task);
    void Delete(TaskEntity task);
}