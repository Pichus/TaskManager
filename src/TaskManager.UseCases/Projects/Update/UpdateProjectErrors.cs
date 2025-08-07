using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Projects.Update;

public static class UpdateProjectErrors
{
    public static readonly Error AccessDenied = new("Projects.Update.AccessDenied",
        "You do not have access to this project");

    public static Error NotFound(long projectId = 0)
    {
        return new Error("Projects.Update.NotFound", $"Project with id {projectId} not found");
    }
}