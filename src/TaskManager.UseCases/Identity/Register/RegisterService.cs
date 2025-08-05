using Microsoft.AspNetCore.Identity;
using TaskManager.Infrastructure.Identity.User;
using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Identity.Register;

public class RegisterService : IRegisterService
{
    private readonly UserManager<TaskManagerUser> _userManager;

    public RegisterService(UserManager<TaskManagerUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<RegisterResult> RegisterAsync(RegisterDto dto)
    {
        var newUser = new TaskManagerUser
        {
            UserName = dto.UserName,
            Email = dto.Email
        };

        var createUserResult = await _userManager.CreateAsync(newUser);

        var registerResult = new RegisterResult();

        if (!createUserResult.Succeeded)
        {
            registerResult.Success = false;
            registerResult.Error = new Error(createUserResult.Errors.First().Code,
                createUserResult.Errors.First().Description);
            return registerResult;
        }

        registerResult.CreatedUser = new UserDto
        {
            Id = newUser.Id,
            UserName = newUser.UserName,
            Email = newUser.Email
        };

        return registerResult;
    }
}