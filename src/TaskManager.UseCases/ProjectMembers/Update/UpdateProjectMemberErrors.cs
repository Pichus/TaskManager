using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.ProjectMembers.Update;

public static class UpdateProjectMemberErrors
{
    public static readonly Error MemberNotFound = new("ProjectMembers.Update.MemberNotFound",
        "Member not found");

    public static readonly Error ProjectNotFound = new("ProjectMembers.Update.ProjectNotFound",
        "Project not found");

    public static readonly Error UserIsNotAProjectMember = new("ProjectMembers.Update.UserIsNotAProjectMember",
        "You want to update a user who's not a project member");

    public static readonly Error AccessDenied = new("ProjectMembers.Update.AccessDenied",
        "You have to be a project lead assign roles");

    public static readonly Error MemberAlreadyHasThisRole = new("ProjectMembers.Update.MemberAlreadyHasThisRole",
        "MemberAlreadyHasThisRole");
}