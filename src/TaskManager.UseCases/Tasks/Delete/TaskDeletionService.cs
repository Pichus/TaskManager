using Microsoft.Extensions.Logging;
using TaskManager.Core.ProjectAggregate;
using TaskManager.Core.TaskAggregate;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Identity.CurrentUser;
using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Tasks.Delete;

public class TaskDeletionService : ITaskDeletionService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly AppDbContext _dbContext;
    private readonly ILogger _logger;
    private readonly IProjectMemberRepository _projectMemberRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ITaskRepository _taskRepository;

    public TaskDeletionService(ILogger logger, ICurrentUserService currentUserService,
        IProjectRepository projectRepository, ITaskRepository taskRepository,
        IProjectMemberRepository projectMemberRepository, AppDbContext dbContext)
    {
        _logger = logger;
        _currentUserService = currentUserService;
        _projectRepository = projectRepository;
        _taskRepository = taskRepository;
        _projectMemberRepository = projectMemberRepository;
        _dbContext = dbContext;
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

        var canCurrentUserDeleteTask =
            await _projectMemberRepository.IsUserProjectLeadAsync(currentUserId, projectId) ||
            await _projectMemberRepository.IsUserProjectManagerAsync(currentUserId, projectId);

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