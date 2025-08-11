using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Tasks.Delete;

public static class DeleteTaskErrors
{
    public static readonly Error ProjectNotFound = new("Tasks.Delete.ProjectNotFound",
        "project not found");

    public static readonly Error TaskNotFound = new("Tasks.Delete.TaskNotFound",
        "task not found");

    public static readonly Error AccessDenied = new("Tasks.Delete.AccessDenied");
}