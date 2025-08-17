using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using TaskManager.Core.ProjectAggregate;
using TaskManager.Infrastructure.Identity.User;
using TaskManager.UseCases.Shared;
using TaskManager.UseCases.Tasks.Retrieve;

namespace TaskManager.UseCases.Tasks.Validation;

public class TaskQueryValidatorService : ITaskQueryValidatorService
{
    private readonly ILogger<TaskQueryValidatorService> _logger;
    private readonly IProjectRepository _projectRepository;
    private readonly UserManager<TaskManagerUser> _userManager;

    public TaskQueryValidatorService(IProjectRepository projectRepository, ILogger<TaskQueryValidatorService> logger,
        UserManager<TaskManagerUser> userManager)
    {
        _projectRepository = projectRepository;
        _logger = logger;
        _userManager = userManager;
    }

    public async Task<Result<ProjectEntity?>> ValidateProjectAsync(long? projectId)
    {
        if (projectId is null)
            return Result<ProjectEntity?>.Success(null);

        var project = await _projectRepository.FindByIdAsync((long)projectId);

        if (project is null)
        {
            _logger.LogWarning("Project {ProjectId} not found", projectId);
            return Result<ProjectEntity?>.Failure(RetrieveTaskErrors.ProjectNotFound);
        }

        return Result<ProjectEntity?>.Success(project);
    }

    public async Task<Result<TaskManagerUser?>> ValidateAssigneeUser(string? assigneeUserId)
    {
        if (assigneeUserId == null) return Result<TaskManagerUser?>.Success(null);

        var user = await _userManager.FindByIdAsync(assigneeUserId);

        if (user == null)
        {
            _logger.LogWarning("Assignee user {UserId} not found", assigneeUserId);
            return Result<TaskManagerUser?>.Failure(RetrieveTaskErrors.AssigneeUserNotFound);
        }

        return Result<TaskManagerUser?>.Success(user);
    }
}