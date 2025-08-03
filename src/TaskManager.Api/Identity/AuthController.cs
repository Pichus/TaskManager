using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Core.UserAggregate;
using TaskManager.UseCases.Identity.Login;
using TaskManager.UseCases.Identity.Register;

namespace TaskManager.Identity;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IRegisterService _registerService;
    private readonly ILoginService _loginService;

    public AuthController(IRegisterService registerService)
    {
        _registerService = registerService;
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register(RegisterRequest request)
    {
        var result = await _registerService.RegisterAsync(request);

        return CreatedAtAction(nameof(Register), result);
    }

    [HttpPost("login")]
    public async Task Login()
    {
    }

    [HttpPost("refresh-token")]
    public async Task RefreshToken()
    {
    }
}