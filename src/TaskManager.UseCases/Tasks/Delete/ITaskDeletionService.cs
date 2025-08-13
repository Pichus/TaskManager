using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Tasks.Delete;

public interface ITaskDeletionService
{
    Task<Result> DeleteAsync(long projectId, long taskId);
}