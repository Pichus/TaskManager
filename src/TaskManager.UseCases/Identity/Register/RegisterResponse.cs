using Microsoft.AspNetCore.Identity;

namespace TaskManager.UseCases.Identity.Register;

public class RegisterResponse
{
    public IEnumerable<IdentityError> Errors { get; set; }
}