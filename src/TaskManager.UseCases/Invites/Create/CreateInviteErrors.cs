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
    
    public static readonly Error AccessDenied = new("Invites.Create.AccessDenied",
        "You have to be a project leader to invite users");
    
    public static readonly Error InvitedUserAlreadyAMember = new("Invites.Create.InvitedUserAlreadyAMember",
        "Invited user is already a project member");
}