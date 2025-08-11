using Microsoft.EntityFrameworkCore;
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

    public async Task<IEnumerable<TaskEntity>> GetAllByProjectIdAsync(long projectId)
    {
        return await _context.Tasks.Where(task => task.ProjectId == projectId).ToListAsync();
    }

    public async Task<IEnumerable<TaskEntity>> GetAllByProjectIdAndStatusAsync(long projectId, Status taskStatus)
    {
        return await _context
            .Tasks
            .Where(task => task.Id == task.ProjectId
                           && task.Status == taskStatus)
            .ToListAsync();
    }

    public async Task<TaskEntity?> FindByIdAsync(long taskId)
    {
        return await _context.Tasks.FindAsync(taskId);
    }

    public void Update(TaskEntity task)
    {
        _context.Update(task);
    }

    public void Delete(TaskEntity task)
    {
        _context.Tasks.Remove(task);
    }
}