using System.ComponentModel.DataAnnotations;

namespace TaskManager.Identity.Login;

public class LoginRequest
{
    [EmailAddress] public string Email { get; set; }

    public string Password { get; set; }
}