using TaskManager.UseCases.Identity.Shared;
using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Identity.Login;

public interface ILoginService
{
    Task<Result<AccessAndRefreshTokenPair>> LoginAsync(LoginDto dto);
}