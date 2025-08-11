using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Tasks.Create;

public static class CreateTaskErrors
{
    public static readonly Error ProjectNotFound = new("Tasks.Create.ProjectNotFound",
        "project not found");

    public static readonly Error AccessDenied = new("Tasks.Create.AccessDenied",
        "you have to be a project lead or a manager to create tasks");
}