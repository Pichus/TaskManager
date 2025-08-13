using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Identity.AccessToken;
using TaskManager.Infrastructure.Identity.RefreshToken;
using TaskManager.Infrastructure.Identity.User;
using TaskManager.UseCases.Identity.Shared;
using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Identity.Login;

public class LoginService : ILoginService
{
    private readonly IAccessTokenProvider _accessTokenProvider;
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _dbContext;
    private readonly ILogger _logger;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly UserManager<TaskManagerUser> _userManager;

    public LoginService(UserManager<TaskManagerUser> userManager, IRefreshTokenRepository refreshTokenRepository,
        IAccessTokenProvider accessTokenProvider, IRefreshTokenGenerator refreshTokenGenerator,
        IConfiguration configuration, AppDbContext dbContext, ILogger logger)
    {
        _userManager = userManager;
        _refreshTokenRepository = refreshTokenRepository;
        _accessTokenProvider = accessTokenProvider;
        _refreshTokenGenerator = refreshTokenGenerator;
        _configuration = configuration;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<AccessAndRefreshTokenPair>> LoginAsync(LoginDto dto)
    {
        _logger.LogInformation("Logging in User: {Email}", dto.Email);
        
        var user = await _userManager.FindByEmailAsync(dto.Email);

        if (user is null || await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            _logger.LogWarning("Logging in failed - wrong email or password");
            return Result<AccessAndRefreshTokenPair>.Failure(LoginErrors.WrongEmailOrPassword);
        }

        var jwtToken = _accessTokenProvider.CreateToken(user);

        var refreshTokenString = _refreshTokenGenerator.GenerateToken();

        await SaveRefreshTokenAsync(refreshTokenString, user.Id);

        var accessAndRefreshTokenPair = new AccessAndRefreshTokenPair(jwtToken, refreshTokenString);

        _logger.LogInformation("Logged in successfully");
        return Result<AccessAndRefreshTokenPair>.Success(accessAndRefreshTokenPair);
    }

    private async Task SaveRefreshTokenAsync(string refreshTokenString, string userId)
    {
        var refreshToken = new Infrastructure.Identity.RefreshToken.RefreshToken
        {
            CreatedAt = DateTime.UtcNow,
            Token = refreshTokenString,
            ExpiresAt = DateTime.UtcNow.AddDays(
                _configuration.GetValue<int>("JwtSettings:RefreshTokenExpirationInDays")),
            UserId = userId
        };

        _refreshTokenRepository.Create(refreshToken);
        await _dbContext.SaveChangesAsync();
    }
}