using Microsoft.AspNetCore.Identity;

namespace TaskManager.UseCases.Identity.Login;

public interface ILoginService
{
    Task<SignInResult> LoginAsync(LoginRequest request);
}