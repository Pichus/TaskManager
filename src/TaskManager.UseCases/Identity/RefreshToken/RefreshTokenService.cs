using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Identity.AccessToken;
using TaskManager.Infrastructure.Identity.RefreshToken;
using TaskManager.Infrastructure.Identity.User;
using TaskManager.UseCases.Identity.Shared;
using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Identity.RefreshToken;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly IAccessTokenProvider _accessTokenProvider;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly UserManager<TaskManagerUser> _userManager;
    private readonly AppDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public RefreshTokenService(IRefreshTokenRepository refreshTokenRepository,
        IRefreshTokenGenerator refreshTokenGenerator, IAccessTokenProvider accessTokenProvider,
        UserManager<TaskManagerUser> userManager, AppDbContext dbContext, IConfiguration configuration)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _refreshTokenGenerator = refreshTokenGenerator;
        _accessTokenProvider = accessTokenProvider;
        _userManager = userManager;
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public async Task<Result<AccessAndRefreshTokenPair>> RefreshTokenAsync(string refreshTokenString)
    {
        var oldRefreshToken = await _refreshTokenRepository.GetRefreshTokenByTokenStringAsync(refreshTokenString);

        if (oldRefreshToken is null)
            return Result<AccessAndRefreshTokenPair>.Failure(RefreshTokenErrors.RefreshTokenNotFound);

        if (oldRefreshToken.IsRevoked)
            return Result<AccessAndRefreshTokenPair>.Failure(RefreshTokenErrors.RefreshTokenRevoked);

        if (oldRefreshToken.IsExpired)
            return Result<AccessAndRefreshTokenPair>.Failure(RefreshTokenErrors.RefreshTokenExpired);

        var user = await _userManager.FindByIdAsync(oldRefreshToken.UserId);

        if (user is null)
            return Result<AccessAndRefreshTokenPair>.Failure(
                RefreshTokenErrors.RefreshTokenUserNotFound(oldRefreshToken.UserId));

        _refreshTokenRepository.RevokeRefreshToken(oldRefreshToken);
        await _dbContext.SaveChangesAsync();

        var newRefreshTokenString = await CreateAndSaveNewRefreshTokenAsync(user.Id);

        var accessToken = _accessTokenProvider.CreateToken(user);

        var accessAndRefreshTokenPair = new AccessAndRefreshTokenPair(accessToken, newRefreshTokenString);
        
        return Result<AccessAndRefreshTokenPair>.Success(accessAndRefreshTokenPair);
    }

    private async Task<string> CreateAndSaveNewRefreshTokenAsync(string userId)
    {
        var newRefreshTokenString = _refreshTokenGenerator.GenerateToken();

        var refreshToken = new Infrastructure.Identity.RefreshToken.RefreshToken
        {
            CreatedAt = DateTime.UtcNow,
            Token = newRefreshTokenString,
            ExpiresAt = DateTime.UtcNow.AddDays(
                _configuration.GetValue<int>("JwtSettings:RefreshTokenExpirationInDays")),
            UserId = userId
        };

        _refreshTokenRepository.CreateRefreshToken(refreshToken);
        await _dbContext.SaveChangesAsync();

        return newRefreshTokenString;
    }
}