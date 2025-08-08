using TaskManager.Infrastructure.Identity.User;
using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Profile.ProfileDetails;

public interface IProfileDetailsService
{
    Task<Result<TaskManagerUser>> GetCurrentUserProfileDetailsAsync();
}