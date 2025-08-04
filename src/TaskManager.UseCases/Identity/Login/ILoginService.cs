namespace TaskManager.UseCases.Identity.Login;

public interface ILoginService
{
    Task<LoginResult> LoginAsync(LoginDto dto);
}