using System.ComponentModel.DataAnnotations;

namespace TaskManager.UseCases.Identity.Register;

public class RegisterRequest
{
    public string UserName { get; set; }

    [EmailAddress] public string Email { get; set; }

    public string Password { get; set; }
}