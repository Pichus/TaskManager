using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Identity.Register;

public class RegisterResult : ResultBase
{
    public UserDto? CreatedUser { get; set; }
}