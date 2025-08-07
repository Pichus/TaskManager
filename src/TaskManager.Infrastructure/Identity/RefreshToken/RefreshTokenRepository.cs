using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Identity.RefreshToken;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _databaseContext;

    public RefreshTokenRepository(AppDbContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    public async Task<RefreshToken?> GetRefreshTokenByTokenStringAsync(string tokenString)
    {
        var result = await _databaseContext
            .RefreshTokens
            .FirstOrDefaultAsync(token => token.Token == tokenString);

        return result;
    }

    public void RevokeRefreshToken(RefreshToken token)
    {
        token.RevokedAt = DateTime.UtcNow;
    }

    public void CreateRefreshToken(RefreshToken token)
    {
        _databaseContext.RefreshTokens.Add(token);
    }
}