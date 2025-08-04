using Microsoft.AspNetCore.Mvc;
using TaskManager.Identity.Register;
using TaskManager.UseCases.Identity.Login;
using TaskManager.UseCases.Identity.Register;

namespace TaskManager.Identity;

[Route("api/auth")]
[ApiController]
public class IdentityController : ControllerBase
{
    private readonly ILoginService _loginService;
    private readonly IRegisterService _registerService;

    public IdentityController(IRegisterService registerService, ILoginService loginService)
    {
        _registerService = registerService;
        _loginService = loginService;
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register(RegisterRequest request)
    {
        var result = await _registerService.RegisterAsync(RegisterRequestToRegisterDto(request));

        if (!result.Success) return BadRequest(result.ErrorMessage);

        return CreatedAtAction(nameof(Register), result.CreatedUser);
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var result = await _loginService.LoginAsync(LoginRequestToLoginDto(request));

        if (!result.Success) return BadRequest(result.ErrorMessage);

        var response = new LoginResponse
        {
            AccessToken = result.AccessToken!,
            RefreshToken = result.RefreshToken!
        };

        return Ok(response);
    }

    [HttpPost("refresh-token")]
    public async Task RefreshToken()
    {
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