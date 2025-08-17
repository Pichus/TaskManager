using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Tasks.Retrieve;

public static class RetrieveTaskErrors
{
    public static readonly Error ProjectNotFound = new("Tasks.Get.ProjectNotFound",
        "project not found");

    public static readonly Error TaskNotFound = new("Tasks.Get.TaskNotFound",
        "task not found");

    public static readonly Error AccessDenied = new("Tasks.Get.AccessDenied");

    public static readonly Error AssigneeUserNotFound = new("Tasks.Get.AssigneeUserNotFound");
}