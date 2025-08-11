using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Tasks.Get;

public static class GetTaskErrors
{
    public static readonly Error ProjectNotFound = new("Tasks.Get.ProjectNotFound",
        "project not found");

    public static readonly Error AccessDenied = new("Tasks.Get.AccessDenied");
}