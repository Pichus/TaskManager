using TaskManager.Core.ProjectInviteAggregate;
using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Invites.Create;

public interface IInviteCreationService
{
    Task<Result<ProjectInvite>> CreateAsync(CreateInviteDto createInviteDto);
}