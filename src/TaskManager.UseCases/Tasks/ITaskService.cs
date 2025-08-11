using TaskManager.Core.TaskAggregate;
using TaskManager.UseCases.Shared;
using TaskManager.UseCases.Tasks.Create;
using TaskManager.UseCases.Tasks.Get;
using TaskManager.UseCases.Tasks.Update;

namespace TaskManager.UseCases.Tasks;

public interface ITaskService
{
    Task<Result<IEnumerable<TaskEntity>>> GetAllByProjectIdAndStatusAsync(long projectId, StatusDto status);
    Task<Result<TaskEntity>> CreateAsync(CreateTaskDto createTaskDto);
    Task<Result> UpdateAsync(UpdateTaskDto updateTaskDto);
    Task<Result> UpdateStatusAsync(long taskId, Status status);
}