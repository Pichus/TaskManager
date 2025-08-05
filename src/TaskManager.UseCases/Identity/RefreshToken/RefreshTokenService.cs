using Microsoft.AspNetCore.Identity;
using TaskManager.Infrastructure.Identity.AccessToken;
using TaskManager.Infrastructure.Identity.RefreshToken;
using TaskManager.Infrastructure.Identity.User;

namespace TaskManager.UseCases.Identity.RefreshToken;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly IAccessTokenProvider _accessTokenProvider;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly UserManager<TaskManagerUser> _userManager;

    public RefreshTokenService(IRefreshTokenRepository refreshTokenRepository,
        IRefreshTokenGenerator refreshTokenGenerator, IAccessTokenProvider accessTokenProvider,
        UserManager<TaskManagerUser> userManager)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _refreshTokenGenerator = refreshTokenGenerator;
        _accessTokenProvider = accessTokenProvider;
        _userManager = userManager;
    }

    public async Task<RefreshTokenResult> RefreshTokenAsync(string refreshTokenString)
    {
        var oldRefreshToken = await _refreshTokenRepository.GetRefreshTokenByTokenStringAsync(refreshTokenString);

        if (oldRefreshToken is null)
            return new RefreshTokenResult
            {
                Success = false,
                Error = RefreshTokenErrors.RefreshTokenNotFound
            };

        if (oldRefreshToken.IsRevoked)
            return new RefreshTokenResult
            {
                Success = false,
                Error = RefreshTokenErrors.RefreshTokenRevoked
            };

        if (oldRefreshToken.IsExpired)
            return new RefreshTokenResult
            {
                Success = false,
                Error = RefreshTokenErrors.RefreshTokenExpired
            };

        var user = await _userManager.FindByIdAsync(oldRefreshToken.UserId);

        if (user is null)
            return new RefreshTokenResult
            {
                Success = false,
                Error = RefreshTokenErrors.RefreshTokenUserNotFound(oldRefreshToken.UserId)
            };

        await _refreshTokenRepository.RevokeRefreshTokenAsync(oldRefreshToken);

        var newRefreshTokenString = await CreateAndSaveNewRefreshTokenAsync(user.Id);

        var accessToken = _accessTokenProvider.CreateToken(user);

        return new RefreshTokenResult
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshTokenString
        };
    }

    private async Task<string> CreateAndSaveNewRefreshTokenAsync(string userId)
    {
        var newRefreshTokenString = _refreshTokenGenerator.GenerateToken();

        var newRefreshTokenDto = new CreateRefreshTokenDto
        {
            TokenString = newRefreshTokenString,
            UserId = userId
        };

        await _refreshTokenRepository.CreateRefreshTokenAsync(newRefreshTokenDto);

        return newRefreshTokenString;
    }
}