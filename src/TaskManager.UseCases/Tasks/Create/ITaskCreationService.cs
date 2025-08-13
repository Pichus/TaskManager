using TaskManager.Core.TaskAggregate;
using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Tasks.Create;

public interface ITaskCreationService
{
    Task<Result<TaskEntity>> CreateAsync(CreateTaskDto createTaskDto);
}