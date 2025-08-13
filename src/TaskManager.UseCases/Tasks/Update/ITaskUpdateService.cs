using TaskManager.Core.TaskAggregate;
using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Tasks.Update;

public interface ITaskUpdateService
{
    Task<Result> UpdateAsync(UpdateTaskDto updateTaskDto);
    Task<Result> UpdateStatusAsync(long projectId, long taskId, Status status);
}