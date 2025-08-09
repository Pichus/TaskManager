using Microsoft.AspNetCore.Identity;
using TaskManager.Infrastructure.Identity.CurrentUser;
using TaskManager.Infrastructure.Identity.User;
using TaskManager.UseCases.Profile.ProfileDetails.Get;
using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Profile.ProfileDetails;

public class ProfileDetailsService : IProfileDetailsService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly UserManager<TaskManagerUser> _userManager;

    public ProfileDetailsService(UserManager<TaskManagerUser> userManager, ICurrentUserService currentUserService)
    {
        _userManager = userManager;
        _currentUserService = currentUserService;
    }

    public async Task<Result<TaskManagerUser>> GetCurrentUserProfileDetailsAsync()
    {
        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null)
        {
            return Result<TaskManagerUser>.Failure(GetCurrentUserProfileDetailsErrors.Unauthenticated);
        }

        var user = await _userManager.FindByIdAsync(currentUserId);

        if (user is null)
        {
            return Result<TaskManagerUser>.Failure(GetCurrentUserProfileDetailsErrors.Unauthenticated);
        }

        return Result<TaskManagerUser>.Success(user);
    }
}