namespace TaskManager.Core.TaskAggregate;

public interface ITaskRepository
{
    void Create(TaskEntity task);
}