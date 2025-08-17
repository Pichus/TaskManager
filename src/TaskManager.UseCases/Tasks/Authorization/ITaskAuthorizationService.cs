using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Tasks.Authorization;

public interface ITaskAuthorizationService
{
    Task<Result> CanUserAccessProjectAsync(string userId, long projectId);
    Task<Result> CanUserViewAssigneeTasks(string currentUserId, string assigneeUserId, long? projectId);
}