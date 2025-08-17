using Microsoft.Extensions.Logging;
using TaskManager.Core.ProjectAggregate;
using TaskManager.UseCases.Shared;
using TaskManager.UseCases.Tasks.Retrieve;

namespace TaskManager.UseCases.Tasks.Authorization;

public class TaskAuthorizationService : ITaskAuthorizationService
{
    private readonly ILogger<TaskAuthorizationService> _logger;
    private readonly IProjectMemberRepository _projectMemberRepository;

    public TaskAuthorizationService(ILogger<TaskAuthorizationService> logger,
        IProjectMemberRepository projectMemberRepository)
    {
        _logger = logger;
        _projectMemberRepository = projectMemberRepository;
    }

    public async Task<Result> CanUserAccessProjectAsync(string userId, long projectId)
    {
        var canAccess = await _projectMemberRepository.IsUserProjectParticipantAsync(userId, projectId);

        if (!canAccess)
        {
            _logger.LogWarning("User {UserId} denied access to project {ProjectId}", userId, projectId);
            return Result.Failure(RetrieveTaskErrors.AccessDenied);
        }

        return Result.Success();
    }

    public async Task<Result> CanUserViewAssigneeTasks(string currentUserId, string assigneeUserId, long? projectId)
    {
        if (currentUserId == assigneeUserId) return Result.Success();

        if (projectId == null)
        {
            _logger.LogWarning("User {UserId} denied access to view tasks of user {AssigneeId} without project context",
                currentUserId, assigneeUserId);
            return Result.Failure(RetrieveTaskErrors.AccessDenied);
        }

        var isManager = await _projectMemberRepository.IsUserProjectManagerAsync(currentUserId, projectId.Value);
        var isLead = await _projectMemberRepository.IsUserProjectLeadAsync(currentUserId, projectId.Value);

        if (!isManager && !isLead)
        {
            _logger.LogWarning("User {UserId} denied access to view tasks of user {AssigneeId} in project {ProjectId}",
                currentUserId, assigneeUserId, projectId);
            return Result.Failure(RetrieveTaskErrors.AccessDenied);
        }

        return Result.Success();
    }
}