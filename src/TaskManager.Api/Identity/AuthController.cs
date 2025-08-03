using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskManager.UseCases.Identity.Login;
using TaskManager.UseCases.Identity.Register;

namespace TaskManager.Identity;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IRegisterService _registerService;
    private readonly ILoginService _loginService;

    public AuthController(IRegisterService registerService, ILoginService loginService)
    {
        _registerService = registerService;
        _loginService = loginService;
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register(RegisterRequest request)
    {
        var result = await _registerService.RegisterAsync(request);

        return CreatedAtAction(nameof(Register), result);
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login(LoginRequest request)
    {
        var result = await _loginService.LoginAsync(request);

        if (!result.Success)
        {
            return BadRequest(result.ErrorMessage);
        }

        return Ok();
    }

    [HttpPost("refresh-token")]
    public async Task RefreshToken()
    {
    }
}