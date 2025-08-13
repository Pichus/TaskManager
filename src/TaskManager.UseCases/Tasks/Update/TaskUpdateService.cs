using Microsoft.Extensions.Logging;
using TaskManager.Core.ProjectAggregate;
using TaskManager.Core.TaskAggregate;
using TaskManager.Infrastructure;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Identity.CurrentUser;
using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Tasks.Update;

public class TaskUpdateService : ITaskUpdateService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;
    private readonly IProjectMemberRepository _projectMemberRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ITaskRepository _taskRepository;

    public TaskUpdateService(ICurrentUserService currentUserService, IUnitOfWork unitOfWork, ILogger logger,
        IProjectMemberRepository projectMemberRepository, IProjectRepository projectRepository,
        ITaskRepository taskRepository)
    {
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _projectMemberRepository = projectMemberRepository;
        _projectRepository = projectRepository;
        _taskRepository = taskRepository;
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
                                       await _projectMemberRepository.IsUserProjectManagerAsync(currentUserId,
                                           project.Id);

        if (!canCurrentUserUpdateTask)
        {
            _logger.LogWarning("Updating a task failed - access denied");
            return Result.Failure(UpdateTaskErrors.AccessDenied);
        }

        task.Description = updateTaskDto.Description;
        task.Title = updateTaskDto.Title;
        task.DueDate = updateTaskDto.DueDate;

        _taskRepository.Update(task);
        await _unitOfWork.SaveChangesAsync();

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
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Successfully updated Task: {TaskId} status", task.Id);
        return Result.Success();
    }
}