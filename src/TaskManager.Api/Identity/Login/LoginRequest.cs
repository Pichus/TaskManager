using System.ComponentModel.DataAnnotations;

namespace TaskManager.Identity;

public class LoginRequest
{
    [EmailAddress] public string Email { get; set; }

    public string Password { get; set; }
}