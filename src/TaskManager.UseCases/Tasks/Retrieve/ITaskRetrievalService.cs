using TaskManager.Core.TaskAggregate;
using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Tasks.Retrieve;

public interface ITaskRetrievalService
{
    Task<Result<IEnumerable<TaskEntity>>> RetrieveAllByProjectIdAndStatusAsync(long projectId, StatusDto status);
    Task<Result<TaskEntity>> RetrieveByProjectIdAndTaskIdAsync(long projectId, long taskId);
}