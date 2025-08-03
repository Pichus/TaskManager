using Microsoft.AspNetCore.Identity;
using TaskManager.Core.UserAggregate;

namespace TaskManager.UseCases.Identity.Register;

public class RegisterService : IRegisterService
{
    private readonly UserManager<TaskManagerUser> _userManager;

    public RegisterService(UserManager<TaskManagerUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IdentityResult> RegisterAsync(RegisterRequest request)
    {
        var newUser = new TaskManagerUser
        {
            UserName = request.UserName,
        };

        var createUserResult = await _userManager.CreateAsync(newUser);

        return createUserResult;
    }
}