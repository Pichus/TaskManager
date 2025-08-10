using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Projects.Delete;

public static class DeleteProjectErrors
{
    public static readonly Error AccessDenied = new("Projects.Delete.AccessDenied",
        "You do not have access to this project");

    public static Error NotFound(long projectId = 0)
    {
        return new Error("Projects.Delete.NotFound", $"Project with id {projectId} not found");
    }
}