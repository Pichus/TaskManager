using System.ComponentModel.DataAnnotations;

namespace TaskManager.UseCases.Identity.Register;

public class RegisterRequest
{
    public string UserName { get; set; }
    public string Password { get; set; }
}