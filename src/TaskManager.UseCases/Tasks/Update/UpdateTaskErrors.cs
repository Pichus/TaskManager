using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Tasks.Update;

public static class UpdateTaskErrors
{
    public static readonly Error ProjectNotFound = new("Tasks.Update.ProjectNotFound",
        "project not found");

    public static readonly Error TaskNotFound = new("Tasks.Update.TaskNotFound",
        "task not found");

    public static readonly Error AccessDenied = new("Tasks.Update.AccessDenied");
}