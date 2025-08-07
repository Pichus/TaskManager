using TaskManager.Core.ProjectInviteAggregate;
using TaskManager.UseCases.Invites.Create;
using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Invites;

public interface IInviteService
{
    Task<Result<ProjectInvite>> CreateAsync(CreateInviteDto createInviteDto);
}