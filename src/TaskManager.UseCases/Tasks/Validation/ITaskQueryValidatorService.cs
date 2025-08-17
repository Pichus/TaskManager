using TaskManager.Core.ProjectAggregate;
using TaskManager.Infrastructure.Identity.User;
using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Tasks.Validation;

public interface ITaskQueryValidatorService
{
    Task<Result<ProjectEntity?>> ValidateProjectAsync(long? projectId);
    Task<Result<TaskManagerUser?>> ValidateAssigneeUser(string? assigneeUserId);
}