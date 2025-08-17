using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TaskManager.Infrastructure;
using TaskManager.Infrastructure.Identity.AccessToken;
using TaskManager.Infrastructure.Identity.RefreshToken;
using TaskManager.Infrastructure.Identity.User;
using TaskManager.UseCases.Identity.Shared;
using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Identity.RefreshToken;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly IAccessTokenProvider _accessTokenProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RefreshTokenService> _logger;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<TaskManagerUser> _userManager;

    public RefreshTokenService(IRefreshTokenRepository refreshTokenRepository,
        IRefreshTokenGenerator refreshTokenGenerator, IAccessTokenProvider accessTokenProvider,
        UserManager<TaskManagerUser> userManager, IUnitOfWork unitOfWork, IConfiguration configuration,
        ILogger<RefreshTokenService> logger)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _refreshTokenGenerator = refreshTokenGenerator;
        _accessTokenProvider = accessTokenProvider;
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Result<AccessAndRefreshTokenPair>> RefreshTokenAsync(string refreshTokenString)
    {
        _logger.LogInformation("Refreshing Token");

        var oldRefreshToken = await _refreshTokenRepository.GetRefreshTokenByTokenStringAsync(refreshTokenString);

        if (oldRefreshToken is null)
        {
            _logger.LogWarning("Refreshing token failed - refresh token not found");
            return Result<AccessAndRefreshTokenPair>.Failure(RefreshTokenErrors.RefreshTokenNotFound);
        }

        if (oldRefreshToken.IsRevoked)
        {
            _logger.LogWarning("Refreshing token failed - refresh token not revoked");
            return Result<AccessAndRefreshTokenPair>.Failure(RefreshTokenErrors.RefreshTokenRevoked);
        }

        if (oldRefreshToken.IsExpired)
        {
            _logger.LogWarning("Refreshing token failed - refresh token expired");
            return Result<AccessAndRefreshTokenPair>.Failure(RefreshTokenErrors.RefreshTokenExpired);
        }

        var user = await _userManager.FindByIdAsync(oldRefreshToken.UserId);

        if (user is null)
        {
            _logger.LogWarning("Refreshing token failed - User: {UserId} not found", oldRefreshToken.UserId);
            return Result<AccessAndRefreshTokenPair>.Failure(
                RefreshTokenErrors.RefreshTokenUserNotFound(oldRefreshToken.UserId));
        }

        _refreshTokenRepository.RevokeRefreshToken(oldRefreshToken);
        await _unitOfWork.SaveChangesAsync();

        var newRefreshTokenString = await CreateAndSaveNewRefreshTokenAsync(user.Id);

        var accessToken = _accessTokenProvider.CreateToken(user);

        var accessAndRefreshTokenPair = new AccessAndRefreshTokenPair(accessToken, newRefreshTokenString);

        _logger.LogInformation("Refreshed token successfully");
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

        _refreshTokenRepository.Create(refreshToken);
        await _unitOfWork.SaveChangesAsync();

        return newRefreshTokenString;
    }
}