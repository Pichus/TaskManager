using Microsoft.AspNetCore.Identity;

namespace TaskManager.UseCases.Identity.Register;

public interface IRegisterService
{
    Task<IdentityResult> RegisterAsync(RegisterRequest request);
}