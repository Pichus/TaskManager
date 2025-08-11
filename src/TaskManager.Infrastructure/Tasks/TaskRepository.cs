using TaskManager.Core.TaskAggregate;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Tasks;

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _context;

    public TaskRepository(AppDbContext context)
    {
        _context = context;
    }

    public void Create(TaskEntity task)
    {
        _context.Tasks.Add(task);
    }
}