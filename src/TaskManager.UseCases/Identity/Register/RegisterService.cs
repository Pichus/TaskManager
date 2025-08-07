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

    public async Task<Result<TaskManagerUser>> RegisterAsync(RegisterDto dto)
    {
        var newUser = new TaskManagerUser
        {
            UserName = dto.UserName,
            Email = dto.Email
        };

        var createUserResult = await _userManager.CreateAsync(newUser);

        if (!createUserResult.Succeeded)
        {
            var error = new Error(createUserResult.Errors.First().Code,
                createUserResult.Errors.First().Description);
            return Result<TaskManagerUser>.Failure(error);
        }

        return Result<TaskManagerUser>.Success(newUser);
    }
}