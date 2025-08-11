using TaskManager.Core.ProjectAggregate;
using TaskManager.Core.TaskAggregate;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Identity.CurrentUser;
using TaskManager.UseCases.Shared;
using TaskManager.UseCases.Tasks.Create;
using TaskManager.UseCases.Tasks.Get;
using TaskManager.UseCases.Tasks.Update;

namespace TaskManager.UseCases.Tasks;

public class TaskService : ITaskService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly AppDbContext _dbContext;
    private readonly IProjectMemberRepository _projectMemberRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ITaskRepository _taskRepository;

    public TaskService(ITaskRepository taskRepository, ICurrentUserService currentUserService,
        IProjectMemberRepository projectMemberRepository, IProjectRepository projectRepository, AppDbContext dbContext)
    {
        _taskRepository = taskRepository;
        _currentUserService = currentUserService;
        _projectMemberRepository = projectMemberRepository;
        _projectRepository = projectRepository;
        _dbContext = dbContext;
    }

    public async Task<Result<TaskEntity>> GetAllByProjectIdAndStatusAsync(long projectId, StatusDto status)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<TaskEntity>> CreateAsync(CreateTaskDto createTaskDto)
    {
        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null)
            return Result<TaskEntity>.Failure(UseCaseErrors.Unauthenticated);

        var project = await _projectRepository.FindByIdAsync(createTaskDto.ProjectId);

        if (project is null)
            return Result<TaskEntity>.Failure(CreateTaskErrors.ProjectNotFound);

        var currentUserCanCreateTask = await CanCreateTaskAsync(project, currentUserId);

        if (!currentUserCanCreateTask) return Result<TaskEntity>.Failure(CreateTaskErrors.AccessDenied);

        var task = new TaskEntity
        {
            CreatedAt = DateTime.UtcNow,
            Title = createTaskDto.Title,
            Description = createTaskDto.Description,
            Status = Status.ToDo,
            DueDate = createTaskDto.DueDate,
            CreatedByUserId = currentUserId,
            AssigneeUserId = createTaskDto.AssigneeUserId,
            ProjectId = project.Id
        };

        _taskRepository.Create(task);
        await _dbContext.SaveChangesAsync();

        return Result<TaskEntity>.Success(task);
    }

    public async Task<Result> UpdateAsync(UpdateTaskDto updateTaskDto)
    {
        throw new NotImplementedException();
    }

    public Task<Result> UpdateStatusAsync(long taskId, Status status)
    {
        throw new NotImplementedException();
    }

    private async Task<bool> CanCreateTaskAsync(ProjectEntity project, string userId)
    {
        return project.LeadUserId == userId ||
               await _projectMemberRepository.IsUserProjectManager(userId, project.Id);
    }
}