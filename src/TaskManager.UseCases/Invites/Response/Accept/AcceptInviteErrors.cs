using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Invites.Accept;

public static class AcceptInviteErrors
{
    public static readonly Error InviteNotFound = new("Invites.Accept.InviteNotFound",
        "Invite not found");

    public static readonly Error InviteAlreadyAccepted = new("Invites.Accept.InviteAlreadyAccepted",
        "The invite has been already accepted");

    public static readonly Error InviteAlreadyRejected = new("Invites.Accept.InviteAlreadyRejected",
        "The invite has been already rejected");

    public static readonly Error AccessDenied = new("Invites.Accept.AccessDenied",
        "You are not the user who was invited");

    public static readonly Error InvitedUserAlreadyAMember = new("Invites.Accept.InvitedUserAlreadyAMember",
        "You are already a project member");

    public static readonly Error ProjectNotFound = new("Invites.Accept.ProjectNotFound",
        "The project you were invited to join was not found");
}