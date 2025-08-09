using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Invites.GetPendingForProject;

public static class GetPendingInvitesForProjectErrors
{
    public static readonly Error Unauthenticated = new("Invites.GetPendingForProject.Unauthenticated",
        "Unauthenticated");
    
    public static readonly Error ProjectNotFound = new("Invites.GetPendingForProject.ProjectNotFound",
        "Project not found");
}