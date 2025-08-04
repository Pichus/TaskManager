namespace TaskManager.Infrastructure.Identity.RefreshToken;

public interface IRefreshTokenRepository
{
    Task CreateRefreshTokenAsync(CreateRefreshTokenDto createRefreshTokenDto);
    Task<RefreshToken?> GetRefreshTokenByTokenStringAsync(string tokenString);
    Task<bool> RevokeRefreshTokenAsync(string tokenString);
}