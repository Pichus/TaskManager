namespace TaskManager.UseCases.Identity.RefreshToken;

public interface IRefreshTokenService
{
    Task<RefreshTokenResult> RefreshTokenAsync(string refreshTokenString);
}