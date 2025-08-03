using System.ComponentModel.DataAnnotations;

namespace TaskManager.UseCases.Identity.Login;

public class LoginRequest
{
    [EmailAddress] public string Email { get; set; }

    public string Password { get; set; }
}