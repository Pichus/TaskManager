using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using TaskManager.Infrastructure.Identity.CurrentUser;
using TaskManager.Infrastructure.Identity.User;
using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.ProfileDetails;

public class ProfileDetailsService : IProfileDetailsService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ProfileDetailsService> _logger;
    private readonly UserManager<TaskManagerUser> _userManager;

    public ProfileDetailsService(UserManager<TaskManagerUser> userManager, ICurrentUserService currentUserService,
        ILogger<ProfileDetailsService> logger)
    {
        _userManager = userManager;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<TaskManagerUser>> GetCurrentUserProfileDetailsAsync()
    {
        _logger.LogInformation("Getting current user profile details");

        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null)
        {
            _logger.LogWarning("Getting current user profile details failed - user unauthenticated");
            return Result<TaskManagerUser>.Failure(UseCaseErrors.Unauthenticated);
        }

        var user = await _userManager.FindByIdAsync(currentUserId);

        if (user is null)
        {
            _logger.LogWarning("Getting current user profile details failed - user unauthenticated");
            return Result<TaskManagerUser>.Failure(UseCaseErrors.Unauthenticated);
        }

        _logger.LogInformation("Got current user profile details successfully");
        return Result<TaskManagerUser>.Success(user);
    }
}