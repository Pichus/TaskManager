namespace TaskManager.UseCases.Identity.Login;

public class LoginResult
{
    public bool Success { get; set; }
    public string? JwtToken { get; set; }
    public string? RefreshToken { get; set; }
    public string? ErrorMessage { get; set; }
}