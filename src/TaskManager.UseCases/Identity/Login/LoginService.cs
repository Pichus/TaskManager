using Microsoft.AspNetCore.Identity;
using TaskManager.Infrastructure.Identity.User;

namespace TaskManager.UseCases.Identity.Login;

public class LoginService : ILoginService
{
    private readonly UserManager<TaskManagerUser> _userManager;

    public LoginService(UserManager<TaskManagerUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<LoginResult> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null || await _userManager.CheckPasswordAsync(user, request.Password))
        {
            return new LoginResult
            {
                Success = false,
                ErrorMessage = "Wrong email or password"
            };
        }

        // todo jwt and refresh token stuff
        return new LoginResult
        {
            Success = true,
            JwtToken = "todo",
            RefreshToken = "todo"
        };
    }
}