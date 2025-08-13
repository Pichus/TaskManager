using Microsoft.Extensions.Logging;
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
    private readonly ILogger _logger;
    private readonly IProjectMemberRepository _projectMemberRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ITaskRepository _taskRepository;

    public TaskService(ITaskRepository taskRepository, ICurrentUserService currentUserService,
        IProjectMemberRepository projectMemberRepository, IProjectRepository projectRepository, AppDbContext dbContext,
        ILogger logger)
    {
        _taskRepository = taskRepository;
        _currentUserService = currentUserService;
        _projectMemberRepository = projectMemberRepository;
        _projectRepository = projectRepository;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<TaskEntity>>> GetAllByProjectIdAndStatusAsync(long projectId, StatusDto status)
    {
        _logger.LogInformation("Getting all tasks for ProjectId: {ProjectId} with Status: {Status}", projectId,
            status);
        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null)
        {
            _logger.LogWarning("Getting all tasks failed - user not authenticated for ProjectId: {ProjectId}",
                projectId);
            return Result<IEnumerable<TaskEntity>>.Failure(UseCaseErrors.Unauthenticated);
        }

        var project = await _projectRepository.FindByIdAsync(projectId);

        if (project is null)
        {
            _logger.LogWarning("Getting all tasks failed - Project: {ProjectId} not found",
                projectId);
            return Result<IEnumerable<TaskEntity>>.Failure(GetTaskErrors.ProjectNotFound);
        }

        var canViewTasks = await _projectMemberRepository.IsUserProjectParticipantAsync(currentUserId, projectId);

        if (!canViewTasks)
        {
            _logger.LogWarning("Getting all tasks failed - access denied");
            return Result<IEnumerable<TaskEntity>>.Failure(GetTaskErrors.AccessDenied);
        }

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

        _logger.LogInformation("Successfully got all tasks for ProjectId: {ProjectId} with Status: {Status}", projectId,
            status);
        return Result<IEnumerable<TaskEntity>>.Success(tasks);
    }

    public async Task<Result<TaskEntity>> CreateAsync(CreateTaskDto createTaskDto)
    {
        _logger.LogInformation("Creating a task for Project: {ProjectId}", createTaskDto.ProjectId);

        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null)
        {
            _logger.LogWarning("Creating a task failed - user not authenticated for ProjectId: {ProjectId}",
                createTaskDto.ProjectId);
            return Result<TaskEntity>.Failure(UseCaseErrors.Unauthenticated);
        }

        var project = await _projectRepository.FindByIdAsync(createTaskDto.ProjectId);

        if (project is null)
        {
            _logger.LogWarning("Creating a task failed - Project: {ProjectId} not found", createTaskDto.ProjectId);
            return Result<TaskEntity>.Failure(CreateTaskErrors.ProjectNotFound);
        }

        var canCurrentUserCanCreateTask = project.LeadUserId == currentUserId ||
                                          await _projectMemberRepository.IsUserProjectManagerAsync(currentUserId,
                                              project.Id);

        if (!canCurrentUserCanCreateTask)
        {
            _logger.LogWarning("Creating a task failed - access denied");
            return Result<TaskEntity>.Failure(CreateTaskErrors.AccessDenied);
        }

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

        _logger.LogInformation("Successfully created a task for Project: {ProjectId}", createTaskDto.ProjectId);
        return Result<TaskEntity>.Success(task);
    }

    public async Task<Result> UpdateAsync(UpdateTaskDto updateTaskDto)
    {
        _logger.LogInformation("Updating a Task: {TaskId}", updateTaskDto.TaskId);
        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null)
        {
            _logger.LogWarning("Updating a task failed - user not authenticated for Project: {ProjectId}",
                updateTaskDto.ProjectId);
            return Result<TaskEntity>.Failure(UseCaseErrors.Unauthenticated);
        }

        var project = await _projectRepository.FindByIdAsync(updateTaskDto.ProjectId);

        if (project is null)
        {
            _logger.LogWarning("Updating a task failed - Project: {ProjectId} not found",
                updateTaskDto.ProjectId);
            return Result.Failure(UpdateTaskErrors.ProjectNotFound);
        }

        var task = await _taskRepository.FindByIdAsync(updateTaskDto.TaskId);

        if (task is null || task.ProjectId != project.Id)
        {
            _logger.LogWarning("Updating a task failed - Task: {ProjectId} not found",
                updateTaskDto.ProjectId);
            return Result.Failure(UpdateTaskErrors.TaskNotFound);
        }

        var canCurrentUserUpdateTask = project.LeadUserId == currentUserId ||
                                       await _projectMemberRepository.IsUserProjectManagerAsync(currentUserId, project.Id);

        if (!canCurrentUserUpdateTask)
        {
            _logger.LogWarning("Updating a task failed - access denied");
            return Result.Failure(UpdateTaskErrors.AccessDenied);
        }

        task.Description = updateTaskDto.Description;
        task.Title = updateTaskDto.Title;
        task.DueDate = updateTaskDto.DueDate;

        _taskRepository.Update(task);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Successfully updated a Task: {TaskId}", task.Id);
        return Result.Success();
    }

    public async Task<Result> UpdateStatusAsync(long projectId, long taskId, Status status)
    {
        _logger.LogInformation("Updating Task: {TaskId} status", taskId);
        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null)
        {
            _logger.LogWarning("Updating task status failed - user not authenticated for Project: {ProjectId}",
                projectId);
            return Result<TaskEntity>.Failure(UseCaseErrors.Unauthenticated);
        }


        var project = await _projectRepository.FindByIdAsync(projectId);

        if (project is null)
        {
            _logger.LogWarning("Updating task status failed - Project: {ProjectId} not found", projectId);
            return Result.Failure(UpdateTaskErrors.ProjectNotFound);
        }


        var task = await _taskRepository.FindByIdAsync(taskId);

        if (task is null || task.ProjectId != projectId)
        {
            _logger.LogWarning("Updating task status failed - Task: {TaskId} not found", taskId);
            return Result.Failure(UpdateTaskErrors.TaskNotFound);
        }

        var canCurrentUserUpdateTaskStatus =
            currentUserId == project.LeadUserId || task.AssigneeUserId == currentUserId;

        if (!canCurrentUserUpdateTaskStatus)
        {
            _logger.LogWarning("Updating task status failed - access denied");
            return Result.Failure(UpdateTaskErrors.AccessDenied);
        }

        if (task.Status == status)
        {
            _logger.LogWarning("Updating task status failed - status already set");
            return Result.Failure(UpdateTaskErrors.StatusAlreadySet);
        }

        task.Status = status;
        _taskRepository.Update(task);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Successfully updated Task: {TaskId} status", task.Id);
        return Result.Success();
    }

    public async Task<Result<TaskEntity>> GetByProjectIdAndTaskIdAsync(long projectId, long taskId)
    {
        _logger.LogInformation("Getting Task: {TaskId}", taskId);       
        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null)
        {
            _logger.LogWarning("Getting task failed - user not authenticated for Project: {ProjectId}",
                projectId);
            return Result<TaskEntity>.Failure(UseCaseErrors.Unauthenticated);
        }

        var project = await _projectRepository.FindByIdAsync(projectId);

        if (project is null)
        {
            _logger.LogWarning("Getting task failed - Project: {ProjectId} not found", projectId);
            return Result<TaskEntity>.Failure(GetTaskErrors.ProjectNotFound);
        }

        var canCurrentUserViewTask = await _projectMemberRepository.IsUserProjectParticipantAsync(currentUserId, projectId);

        if (!canCurrentUserViewTask)
        {
            _logger.LogWarning("Getting task failed - access denied");
            return Result<TaskEntity>.Failure(GetTaskErrors.AccessDenied);
        }

        var task = await _taskRepository.FindByIdAsync(taskId);

        if (task is null || task.ProjectId != projectId)
        {
            _logger.LogWarning("Getting task failed - Task: {TaskId} not found", taskId);
            return Result<TaskEntity>.Failure(GetTaskErrors.TaskNotFound);
        }

        _logger.LogInformation("Successfully got Task: {TaskId}", task.Id);
        return Result<TaskEntity>.Success(task);
    }

    public async Task<Result> DeleteAsync(long projectId, long taskId)
    {
        _logger.LogInformation("Deleting Task: {TaskId}", taskId);
        
        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null)
        {
            _logger.LogWarning("Deleting task failed - user not authenticated for Project: {ProjectId}",
                projectId);
            return Result<TaskEntity>.Failure(UseCaseErrors.Unauthenticated);
        }

        var project = await _projectRepository.FindByIdAsync(projectId);

        if (project is null)
        {
            _logger.LogWarning("Deleting task failed - Project: {ProjectId} not found", projectId);
            return Result.Failure(DeleteTaskErrors.ProjectNotFound);
        }

        var task = await _taskRepository.FindByIdAsync(taskId);

        if (task == null || task.ProjectId != projectId)
        {
            _logger.LogWarning("Deleting task failed - Task: {TaskId} not found", taskId);
            return Result.Failure(DeleteTaskErrors.TaskNotFound);
        }

        var canCurrentUserDeleteTask = project.LeadUserId == currentUserId
                                       || await _projectMemberRepository.IsUserProjectManagerAsync(currentUserId, projectId);

        if (!canCurrentUserDeleteTask)
        {
            _logger.LogWarning("Deleting task failed - access denied");
            return Result.Failure(DeleteTaskErrors.AccessDenied);
        }

        _taskRepository.Delete(task);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Successfully deleted Task: {TaskId}", task.Id);
        return Result.Success();
    }
}