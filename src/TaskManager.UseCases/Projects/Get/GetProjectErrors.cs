using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Projects.Get;

public static class GetProjectErrors
{
    public static readonly Error AccessDenied = new("Projects.Get.AccessDenied",
        "You do not have access to this project");

    public static Error NotFound(long projectId = 0)
    {
        return new Error("Projects.Get.NotFound", $"Project with id {projectId} not found");
    }
}