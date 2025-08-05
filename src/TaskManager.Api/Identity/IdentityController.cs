using Microsoft.AspNetCore.Mvc;
using TaskManager.Identity.Login;
using TaskManager.Identity.RefreshToken;
using TaskManager.Identity.Register;
using TaskManager.UseCases.Identity.Login;
using TaskManager.UseCases.Identity.RefreshToken;
using TaskManager.UseCases.Identity.Register;

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

        if (!result.Success) return BadRequest(result.Error.Message);

        return CreatedAtAction(nameof(Register), result.CreatedUser);
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var result = await _loginService.LoginAsync(LoginRequestToLoginDto(request));

        if (!result.Success)
            if (result.Error.Code == LoginErrors.WrongEmailOrPassword.Code)
                return BadRequest(result.Error.Message);

        var response = new LoginResponse
        {
            AccessToken = result.AccessToken!,
            RefreshToken = result.RefreshToken!
        };

        return Ok(response);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<RefreshTokenResponse>> RefreshToken(RefreshTokenRequest request)
    {
        var refreshTokenResult = await _refreshTokenService.RefreshTokenAsync(request.RefreshTokenString);

        if (!refreshTokenResult.Success)
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

        var response = new RefreshTokenResponse
        {
            AccessToken = refreshTokenResult.AccessToken,
            RefreshToken = refreshTokenResult.RefreshToken
        };

        return response;
    }

    private LoginDto LoginRequestToLoginDto(LoginRequest request)
    {
        var dto = new LoginDto
        {
            Email = request.Email,
            Password = request.Password
        };

        return dto;
    }

    private RegisterDto RegisterRequestToRegisterDto(RegisterRequest request)
    {
        var dto = new RegisterDto
        {
            UserName = request.UserName,
            Email = request.Email,
            Password = request.Password
        };

        return dto;
    }
}