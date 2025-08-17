using Microsoft.EntityFrameworkCore;
using TaskManager.Core.Shared;
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
            .Where(task => task.ProjectId == projectId
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

    public async Task<PagedData<TaskEntity>> GetAll(string sortOrder, int pageNumber = 1, int pageSize = 25,
        Status? status = null, long? projectId = null, string? assigneeUserId = null)
    {
        var query = _context
            .Tasks
            .Where(entity => true);

        if (status is not null)
            query = query.Where(task => task.Status == status);

        if (projectId is not null)
            query = query.Where(task => task.ProjectId == projectId);

        if (assigneeUserId is not null)
            query = query.Where(task => task.AssigneeUserId == assigneeUserId);

        var totalRecords = await query.CountAsync();
        
        query = sortOrder switch
        {
            "DueDate" => query
                .OrderBy(task => task.DueDate)
                .ThenBy(task => task.Id),
            "DueDate_Desc" => query
                .OrderByDescending(task => task.DueDate)
                .ThenByDescending(task => task.Id),
            "Title" => query
                .OrderBy(task => task.Title)
                .ThenBy(task => task.Id),
            "Title_Desc" => query
                .OrderByDescending(task => task.Title)
                .ThenByDescending(task => task.Id),
            "Id_Desc" => query
                .OrderByDescending(task => task.Id),
            _ => query.OrderBy(task => task.Id)
        };

        query = query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);

        var tasks =  await query.ToListAsync();
        
        var paginatedData = new PagedData<TaskEntity>(tasks, pageNumber, pageSize, totalRecords);

        return paginatedData;
    }
}