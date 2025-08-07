using TaskManager.UseCases.Identity.Shared;
using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Identity.RefreshToken;

public interface IRefreshTokenService
{
    Task<Result<AccessAndRefreshTokenPair>> RefreshTokenAsync(string refreshTokenString);
}