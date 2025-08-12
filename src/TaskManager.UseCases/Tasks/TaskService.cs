using TaskManager.Core.ProjectAggregate;
using TaskManager.Core.TaskAggregate;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Identity.CurrentUser;
using TaskManager.UseCases.Shared;
using TaskManager.UseCases.Tasks.Create;
using TaskManager.UseCases.Tasks.Delete;
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

    public async Task<Result<IEnumerable<TaskEntity>>> GetAllByProjectIdAndStatusAsync(long projectId, StatusDto status)
    {
        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null)
            return Result<IEnumerable<TaskEntity>>.Failure(UseCaseErrors.Unauthenticated);

        var project = await _projectRepository.FindByIdAsync(projectId);

        if (project is null)
            return Result<IEnumerable<TaskEntity>>.Failure(GetTaskErrors.ProjectNotFound);

        var canViewTasks = await _projectMemberRepository.IsUserProjectMember(currentUserId, projectId);

        if (!canViewTasks)
            return Result<IEnumerable<TaskEntity>>.Failure(GetTaskErrors.AccessDenied);

        var taskStatus = status switch
        {
            StatusDto.Any => Status.ToDo,
            StatusDto.ToDo => Status.ToDo,
            StatusDto.InProgress => Status.InProgress,
            StatusDto.Complete => Status.Complete,
            _ => Status.ToDo
        };

        var tasks = status switch
        {
            StatusDto.Any => await _taskRepository.GetAllByProjectIdAsync(projectId),
            StatusDto.ToDo or StatusDto.InProgress or StatusDto.Complete => await _taskRepository
                .GetAllByProjectIdAndStatusAsync(projectId, taskStatus),
            _ => await _taskRepository.GetAllByProjectIdAsync(projectId)
        };

        return Result<IEnumerable<TaskEntity>>.Success(tasks);
    }

    public async Task<Result<TaskEntity>> CreateAsync(CreateTaskDto createTaskDto)
    {
        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null)
            return Result<TaskEntity>.Failure(UseCaseErrors.Unauthenticated);

        var project = await _projectRepository.FindByIdAsync(createTaskDto.ProjectId);

        if (project is null)
            return Result<TaskEntity>.Failure(CreateTaskErrors.ProjectNotFound);

        var canCurrentUserCanCreateTask = project.LeadUserId == currentUserId ||
                                          await _projectMemberRepository.IsUserProjectManager(currentUserId,
                                              project.Id);

        if (!canCurrentUserCanCreateTask) return Result<TaskEntity>.Failure(CreateTaskErrors.AccessDenied);

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
        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null)
            return Result<TaskEntity>.Failure(UseCaseErrors.Unauthenticated);

        var project = await _projectRepository.FindByIdAsync(updateTaskDto.ProjectId);

        if (project is null)
            return Result.Failure(UpdateTaskErrors.ProjectNotFound);

        var task = await _taskRepository.FindByIdAsync(updateTaskDto.TaskId);

        if (task is null || task.ProjectId != project.Id)
            return Result.Failure(UpdateTaskErrors.TaskNotFound);

        var canCurrentUserUpdateTask = project.LeadUserId == currentUserId ||
                                       await _projectMemberRepository.IsUserProjectManager(currentUserId, project.Id);

        if (!canCurrentUserUpdateTask)
            return Result.Failure(UpdateTaskErrors.AccessDenied);

        task.Description = updateTaskDto.Description;
        task.Title = updateTaskDto.Title;
        task.DueDate = updateTaskDto.DueDate;

        _taskRepository.Update(task);
        await _dbContext.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> UpdateStatusAsync(long projectId, long taskId, Status status)
    {
        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null)
            return Result<TaskEntity>.Failure(UseCaseErrors.Unauthenticated);

        var project = await _projectRepository.FindByIdAsync(projectId);

        if (project is null)
            return Result.Failure(UpdateTaskErrors.ProjectNotFound);

        var task = await _taskRepository.FindByIdAsync(taskId);

        if (task is null || task.ProjectId != projectId)
            return Result.Failure(UpdateTaskErrors.TaskNotFound);

        var canCurrentUserUpdateTaskStatus = currentUserId == project.LeadUserId || task.AssigneeUserId == currentUserId;

        if (!canCurrentUserUpdateTaskStatus)
            return Result.Failure(UpdateTaskErrors.AccessDenied);

        if (task.Status == status)
            return Result.Failure(UpdateTaskErrors.StatusAlreadySet);

        task.Status = status;
        _taskRepository.Update(task);
        await _dbContext.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<TaskEntity>> GetByProjectIdAndTaskIdAsync(long projectId, long taskId)
    {
        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null)
            return Result<TaskEntity>.Failure(UseCaseErrors.Unauthenticated);

        var project = await _projectRepository.FindByIdAsync(projectId);

        if (project is null)
            return Result<TaskEntity>.Failure(GetTaskErrors.ProjectNotFound);

        var canCurrentUserViewTask = project.LeadUserId == currentUserId
                                     || await _projectMemberRepository.IsUserProjectMember(currentUserId, projectId);

        if (!canCurrentUserViewTask)
            return Result<TaskEntity>.Failure(GetTaskErrors.AccessDenied);

        var task = await _taskRepository.FindByIdAsync(taskId);

        if (task is null || task.ProjectId != projectId)
            return Result<TaskEntity>.Failure(GetTaskErrors.TaskNotFound);

        return Result<TaskEntity>.Success(task);
    }

    public async Task<Result> DeleteAsync(long projectId, long taskId)
    {
        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null)
            return Result<TaskEntity>.Failure(UseCaseErrors.Unauthenticated);

        var project = await _projectRepository.FindByIdAsync(projectId);

        if (project is null)
            return Result.Failure(DeleteTaskErrors.ProjectNotFound);

        var task = await _taskRepository.FindByIdAsync(taskId);
        
        if (task == null || task.ProjectId != projectId)
            return Result.Failure(DeleteTaskErrors.TaskNotFound);

        var canCurrentUserDeleteTask = project.LeadUserId == currentUserId
                                       || await _projectMemberRepository.IsUserProjectManager(currentUserId, projectId);
        
        if (!canCurrentUserDeleteTask)
            return Result.Failure(DeleteTaskErrors.AccessDenied);

        _taskRepository.Delete(task);
        await _dbContext.SaveChangesAsync();
        
        return Result.Success();
    }
}