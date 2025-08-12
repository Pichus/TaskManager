using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Invites.GetPendingForProject;

public static class GetPendingInvitesForProjectErrors
{
    public static readonly Error ProjectNotFound = new("Invites.GetPendingForProject.ProjectNotFound",
        "Project not found");
    
    public static readonly Error AccessDenied = new("Invites.GetPendingForProject.AccessDenied",
        "Access denied");
}