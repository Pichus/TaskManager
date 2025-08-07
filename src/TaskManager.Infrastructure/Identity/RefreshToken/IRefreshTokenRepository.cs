namespace TaskManager.Infrastructure.Identity.RefreshToken;

public interface IRefreshTokenRepository
{
    void CreateRefreshToken(RefreshToken token);
    Task<RefreshToken?> GetRefreshTokenByTokenStringAsync(string tokenString);
    void RevokeRefreshToken(RefreshToken token);
}