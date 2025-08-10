using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.ProjectMembers.Delete;

public static class DeleteProjectMemberErrors
{
    public static readonly Error AccessDenied = new("Projects.Delete.AccessDenied",
        "You have to be a project lead to delete project members");

    public static readonly Error ProjectNotFound = new("Projects.Delete.ProjectNotFound",
        "Project not found");

    public static readonly Error ProjectMemberNotFound = new("Projects.Delete.ProjectMemberNotFound",
        "ProjectMemberNotFound");
}