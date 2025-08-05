using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Identity.Login;

public class LoginResult : Result
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
}