using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Identity.RefreshToken;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _databaseContext;

    public RefreshTokenRepository(AppDbContext databaseContext, IConfiguration configuration)
    {
        _databaseContext = databaseContext;
        _configuration = configuration;
    }

    public async Task<RefreshToken?> GetRefreshTokenByTokenStringAsync(string tokenString)
    {
        var result = await _databaseContext
            .RefreshTokens
            .FirstOrDefaultAsync(token => token.Token == tokenString);

        return result;
    }

    public async Task<bool> RevokeRefreshTokenAsync(string tokenString)
    {
        var token = await GetRefreshTokenByTokenStringAsync(tokenString);

        if (token is null) return false;

        token.RevokedAt = DateTime.UtcNow;

        await _databaseContext.SaveChangesAsync();

        return true;
    }

    public async Task RevokeRefreshTokenAsync(RefreshToken refreshToken)
    {
        refreshToken.RevokedAt = DateTime.UtcNow;

        await _databaseContext.SaveChangesAsync();
    }

    public async Task CreateRefreshTokenAsync(CreateRefreshTokenDto createRefreshTokenDto)
    {
        var refreshToken = new RefreshToken
        {
            CreatedAt = DateTime.UtcNow,
            Token = createRefreshTokenDto.TokenString,
            ExpiresAt = DateTime.UtcNow.AddDays(
                _configuration.GetValue<int>("JwtSettings:RefreshTokenExpirationInDays")),
            UserId = createRefreshTokenDto.UserId
        };

        _databaseContext.RefreshTokens.Add(refreshToken);
        await _databaseContext.SaveChangesAsync();
    }
}