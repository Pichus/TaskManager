using Microsoft.AspNetCore.Identity;
using TaskManager.Core.UserAggregate;

namespace TaskManager.UseCases.Identity.Login;

public class LoginService : ILoginService
{
    private readonly SignInManager<TaskManagerUser> _signInManager;

    public LoginService(SignInManager<TaskManagerUser> signInManager)
    {
        _signInManager = signInManager;
    }

    public async Task<SignInResult> LoginAsync(LoginRequest request)
    {
        var loginResult = await _signInManager.PasswordSignInAsync(
            request.UserName, request.Password, false, false);

        return loginResult;
    }
}