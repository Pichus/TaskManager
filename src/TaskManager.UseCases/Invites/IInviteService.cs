using System.Collections;
using TaskManager.Core.ProjectInviteAggregate;
using TaskManager.UseCases.Invites.Create;
using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Invites;

public interface IInviteService
{
    Task<Result<ProjectInvite>> CreateAsync(CreateInviteDto createInviteDto);
    Task<Result> DeleteAsync(long inviteId);
    Task<Result<IEnumerable<ProjectInvite>>> GetPendingInvitesForCurrentUser();
    Task<Result> AcceptInviteAsync(long inviteId);
    Task<Result> DeclineInviteAsync(long inviteId);
    Task<Result<IEnumerable<ProjectInvite>>> GetPendingProjectInvitesAsync(long projectId);
}