using TaskManager.Core.ProjectInviteAggregate;
using TaskManager.Core.Shared;
using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Invites.Retrieve;

public interface IInviteRetrievalService
{
    Task<Result<PagedData<ProjectInvite>>> RetrievePendingInvitesForCurrentUserAsync(RetrievePendingInvitesDto dto);
    Task<Result<IEnumerable<ProjectInvite>>> RetrievePendingProjectInvitesAsync(long projectId);
}