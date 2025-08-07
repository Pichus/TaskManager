using TaskManager.Core.ProjectInviteAggregate;
using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Invites.Create;

public class CreateInviteResult : Result
{
    public ProjectInvite? Invite { get; set; }
}