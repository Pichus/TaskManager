using Microsoft.AspNetCore.Identity;
using TaskManager.Infrastructure.Identity.AccessToken;
using TaskManager.Infrastructure.Identity.RefreshToken;
using TaskManager.Infrastructure.Identity.User;
using TaskManager.UseCases.Identity.Shared;
using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Identity.Login;

public class LoginService : ILoginService
{
    private readonly IAccessTokenProvider _accessTokenProvider;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly UserManager<TaskManagerUser> _userManager;

    public LoginService(UserManager<TaskManagerUser> userManager, IRefreshTokenRepository refreshTokenRepository,
        IAccessTokenProvider accessTokenProvider, IRefreshTokenGenerator refreshTokenGenerator)
    {
        _userManager = userManager;
        _refreshTokenRepository = refreshTokenRepository;
        _accessTokenProvider = accessTokenProvider;
        _refreshTokenGenerator = refreshTokenGenerator;
    }

    public async Task<Result<AccessAndRefreshTokenPair>> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);

        if (user is null || await _userManager.CheckPasswordAsync(user, dto.Password))
            return Result<AccessAndRefreshTokenPair>.Failure(LoginErrors.WrongEmailOrPassword);

        var jwtToken = _accessTokenProvider.CreateToken(user);

        var refreshTokenString = _refreshTokenGenerator.GenerateToken();

        await SaveRefreshToken(refreshTokenString, user.Id);

        var accessAndRefreshTokenPair = new AccessAndRefreshTokenPair(jwtToken, refreshTokenString);
        
        return Result<AccessAndRefreshTokenPair>.Success(accessAndRefreshTokenPair);
    }

    private async Task SaveRefreshToken(string refreshTokenString, string userId)
    {
        var refreshToken = new CreateRefreshTokenDto
        {
            TokenString = refreshTokenString,
            UserId = userId
        };

        await _refreshTokenRepository.CreateRefreshTokenAsync(refreshToken);
    }
}