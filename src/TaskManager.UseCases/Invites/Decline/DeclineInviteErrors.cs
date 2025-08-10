using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Invites.Decline;

public static class DeclineInviteErrors
{
    public static readonly Error InviteNotFound = new("Invites.Decline.InviteNotFound",
        "Invite not found");
    
    public static readonly Error ProjectNotFound = new("Invites.Decline.ProjectNotFound",
        "Project not found");
    
    public static readonly Error InviteAlreadyAccepted = new("Invites.Decline.InviteAlreadyAccepted",
        "The invite has been already accepted");

    public static readonly Error InviteAlreadyRejected = new("Invites.Decline.InviteAlreadyRejected",
        "The invite has been already rejected");

    public static readonly Error AccessDenied = new("Invites.Decline.AccessDenied",
        "You are not the user who was invited");
}