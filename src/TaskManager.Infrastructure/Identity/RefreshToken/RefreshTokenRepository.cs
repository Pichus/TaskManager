using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Shared;

namespace TaskManager.Infrastructure.Identity.RefreshToken;

public class RefreshTokenRepository : RepositoryBase<RefreshToken, long>, IRefreshTokenRepository
{
    public RefreshTokenRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<RefreshToken?> GetRefreshTokenByTokenStringAsync(string tokenString)
    {
        var result = await Context
            .RefreshTokens
            .FirstOrDefaultAsync(token => token.Token == tokenString);

        return result;
    }

    public void RevokeRefreshToken(RefreshToken token)
    {
        token.RevokedAt = DateTime.UtcNow;
    }
}