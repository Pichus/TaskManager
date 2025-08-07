using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Invites.Create;

public static class CreateInviteErrors
{
    public static readonly Error InvitedUserNotFound = new("Invites.Create.InvitedUserNotFound",
        "Invited user not found");

    public static readonly Error UserAlreadyInvited = new("Invites.Create.UserAlreadyInvited",
        "Invited user has been invited to join this project already");

    public static readonly Error ProjectNotFound = new("Invites.Create.ProjectNotFound",
        "Project not found");
}