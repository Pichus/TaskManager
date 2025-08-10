using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Projects.GetMembers;

public static class GetProjectMembersErrors
{
    public static readonly Error AccessDenied = new("Projects.GetMembers.AccessDenied",
        "You do not have access to this project");

    public static readonly Error ProjectNotFound = new("Projects.GetMembers.ProjectNotFound",
        "Project not found");
}