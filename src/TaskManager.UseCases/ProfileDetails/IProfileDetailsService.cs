using TaskManager.Infrastructure.Identity.User;
using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.ProfileDetails;

public interface IProfileDetailsService
{
    Task<Result<TaskManagerUser>> GetCurrentUserProfileDetailsAsync();
}