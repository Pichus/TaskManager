using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using TaskManager.Infrastructure.Identity.User;
using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Identity.Register;

public class RegisterService : IRegisterService
{
    private readonly ILogger<RegisterService> _logger;
    private readonly UserManager<TaskManagerUser> _userManager;

    public RegisterService(UserManager<TaskManagerUser> userManager, ILogger<RegisterService> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Result<TaskManagerUser>> RegisterAsync(RegisterDto dto)
    {
        _logger.LogInformation("Registering User: {Email}", dto.Email);

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
            _logger.LogWarning("Registration failed - {Message}", error.Message);
            return Result<TaskManagerUser>.Failure(error);
        }

        _logger.LogInformation("Registered successfully");
        return Result<TaskManagerUser>.Success(newUser);
    }
}