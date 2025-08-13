using Microsoft.Extensions.Logging;
using TaskManager.Core.ProjectAggregate;
using TaskManager.Core.TaskAggregate;
using TaskManager.Infrastructure.Identity.CurrentUser;
using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Tasks.Retrieve;

public class TaskRetrievalService : ITaskRetrievalService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;
    private readonly IProjectMemberRepository _projectMemberRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ITaskRepository _taskRepository;

    public TaskRetrievalService(ILogger logger, ICurrentUserService currentUserService,
        IProjectRepository projectRepository, IProjectMemberRepository projectMemberRepository,
        ITaskRepository taskRepository)
    {
        _logger = logger;
        _currentUserService = currentUserService;
        _projectRepository = projectRepository;
        _projectMemberRepository = projectMemberRepository;
        _taskRepository = taskRepository;
    }

    public async Task<Result<IEnumerable<TaskEntity>>> RetrieveAllByProjectIdAndStatusAsync(long projectId,
        StatusDto status)
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
            return Result<IEnumerable<TaskEntity>>.Failure(RetrieveTaskErrors.ProjectNotFound);
        }

        var canViewTasks = await _projectMemberRepository.IsUserProjectParticipantAsync(currentUserId, projectId);

        if (!canViewTasks)
        {
            _logger.LogWarning("Getting all tasks failed - access denied");
            return Result<IEnumerable<TaskEntity>>.Failure(RetrieveTaskErrors.AccessDenied);
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

    public async Task<Result<TaskEntity>> RetrieveByProjectIdAndTaskIdAsync(long projectId, long taskId)
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
            return Result<TaskEntity>.Failure(RetrieveTaskErrors.ProjectNotFound);
        }

        var canCurrentUserViewTask =
            await _projectMemberRepository.IsUserProjectParticipantAsync(currentUserId, projectId);

        if (!canCurrentUserViewTask)
        {
            _logger.LogWarning("Getting task failed - access denied");
            return Result<TaskEntity>.Failure(RetrieveTaskErrors.AccessDenied);
        }

        var task = await _taskRepository.FindByIdAsync(taskId);

        if (task is null || task.ProjectId != projectId)
        {
            _logger.LogWarning("Getting task failed - Task: {TaskId} not found", taskId);
            return Result<TaskEntity>.Failure(RetrieveTaskErrors.TaskNotFound);
        }

        _logger.LogInformation("Successfully got Task: {TaskId}", task.Id);
        return Result<TaskEntity>.Success(task);
    }
}