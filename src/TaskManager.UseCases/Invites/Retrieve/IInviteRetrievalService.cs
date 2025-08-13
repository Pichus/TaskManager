using TaskManager.Core.ProjectInviteAggregate;
using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Invites.Retrieve;

public interface IInviteRetrievalService
{
    Task<Result<IEnumerable<ProjectInvite>>> RetrievePendingInvitesForCurrentUser();
    Task<Result<IEnumerable<ProjectInvite>>> RetrievePendingProjectInvitesAsync(long projectId);
}