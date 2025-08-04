namespace TaskManager.UseCases.Identity.Register;

public interface IRegisterService
{
    Task<RegisterResult> RegisterAsync(RegisterDto dto);
}