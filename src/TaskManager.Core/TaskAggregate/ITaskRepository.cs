namespace TaskManager.Core.TaskAggregate;

public interface ITaskRepository
{
    void Create(TaskEntity task);
    Task<IEnumerable<TaskEntity>> GetAllByProjectIdAsync(long projectId);
    Task<IEnumerable<TaskEntity>> GetAllByProjectIdAndStatusAsync(long projectId, Status taskStatus);
}