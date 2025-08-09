using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Invites.Delete;

public static class DeleteInviteErrors
{
    public static readonly Error InviteNotFound = new("Invites.Delete.InviteNotFound",
        "Invite not found");

    public static readonly Error AccessDenied = new("Invites.Delete.AccessDenied",
        "You are not allowed to delete this invite");
    
    public static readonly Error Unauthenticated = new("Invites.Delete.Unauthenticated",
        "Unauthenticated");
}