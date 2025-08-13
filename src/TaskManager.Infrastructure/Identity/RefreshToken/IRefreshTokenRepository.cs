using TaskManager.Core.Shared;

namespace TaskManager.Infrastructure.Identity.RefreshToken;

public interface IRefreshTokenRepository : IRepositoryBase<RefreshToken, long>
{
    Task<RefreshToken?> GetRefreshTokenByTokenStringAsync(string tokenString);
    void RevokeRefreshToken(RefreshToken token);
}