using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Identity.Register;

public class RegisterResult : Result
{
    public UserDto? CreatedUser { get; set; }
}