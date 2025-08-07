using Microsoft.AspNetCore.Mvc;
using TaskManager.Identity.Login;
using TaskManager.Identity.RefreshToken;
using TaskManager.Identity.Register;
using TaskManager.Infrastructure.Identity.User;
using TaskManager.UseCases.Identity.Login;
using TaskManager.UseCases.Identity.RefreshToken;
using TaskManager.UseCases.Identity.Register;
using TaskManager.UseCases.Identity.Shared;
using TaskManager.UseCases.Shared;

namespace TaskManager.Identity;

[Route("api/auth")]
[ApiController]
public class IdentityController : ControllerBase
{
    private readonly ILoginService _loginService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IRegisterService _registerService;

    public IdentityController(IRegisterService registerService, ILoginService loginService,
        IRefreshTokenService refreshTokenService)
    {
        _registerService = registerService;
        _loginService = loginService;
        _refreshTokenService = refreshTokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponse>> Register(RegisterRequest request)
    {
        var result = await _registerService.RegisterAsync(RegisterRequestToRegisterDto(request));

        if (result.IsFailure) return BadRequest(result.Error.Message);

        var response = UserToRegisterResponse(result.Value);

        return CreatedAtAction(nameof(Register), response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var result = await _loginService.LoginAsync(LoginRequestToLoginDto(request));

        if (result.IsFailure)
            if (result.Error.Code == LoginErrors.WrongEmailOrPassword.Code)
                return BadRequest(result.Error.Message);

        var response = AccessAndRefreshTokenToLoginResponse(result.Value);

        return Ok(response);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<RefreshTokenResponse>> RefreshToken(RefreshTokenRequest request)
    {
        var refreshTokenResult = await _refreshTokenService.RefreshTokenAsync(request.RefreshTokenString);

        if (refreshTokenResult.IsFailure)
        {
            var errorCode = refreshTokenResult.Error.Code;
            var errorMessage = refreshTokenResult.Error.Message;

            if (errorCode == RefreshTokenErrors.RefreshTokenExpired.Code ||
                errorCode == RefreshTokenErrors.RefreshTokenRevoked.Code)
                return BadRequest(errorMessage);

            if (errorCode == RefreshTokenErrors.RefreshTokenNotFound.Code ||
                errorCode == RefreshTokenErrors.RefreshTokenUserNotFound().Code)
                return NotFound(errorMessage);
        }

        var response = AccessAndRefreshTokenToRefreshTokenResponse(refreshTokenResult.Value);

        return Ok(response);
    }

    private LoginDto LoginRequestToLoginDto(LoginRequest request)
    {
        return new LoginDto
        {
            Email = request.Email,
            Password = request.Password
        };
    }

    private RegisterDto RegisterRequestToRegisterDto(RegisterRequest request)
    {
        return new RegisterDto
        {
            UserName = request.UserName,
            Email = request.Email,
            Password = request.Password
        };
    }

    private RegisterResponse UserToRegisterResponse(TaskManagerUser user)
    {
        return new RegisterResponse
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email
        };
    }
    
    private RefreshTokenResponse AccessAndRefreshTokenToRefreshTokenResponse(AccessAndRefreshTokenPair refreshTokenResult)
    {
        return new RefreshTokenResponse
        {
            AccessToken = refreshTokenResult.AccessToken,
            RefreshToken = refreshTokenResult.RefreshToken
        };
    }
    
    private LoginResponse AccessAndRefreshTokenToLoginResponse(AccessAndRefreshTokenPair refreshTokenResult)
    {
        return new LoginResponse()
        {
            AccessToken = refreshTokenResult.AccessToken,
            RefreshToken = refreshTokenResult.RefreshToken
        };
    }
}