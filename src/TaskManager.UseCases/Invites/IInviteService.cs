using TaskManager.UseCases.Invites.Create;

namespace TaskManager.UseCases.Invites;

public interface IInviteService
{
    Task<CreateInviteResult> CreateAsync(CreateInviteDto createInviteDto);
}