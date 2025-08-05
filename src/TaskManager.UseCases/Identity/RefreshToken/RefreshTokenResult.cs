using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Identity.RefreshToken;

public class RefreshTokenResult : Result
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}