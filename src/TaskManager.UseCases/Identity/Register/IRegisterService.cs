using TaskManager.Infrastructure.Identity.User;
using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Identity.Register;

public interface IRegisterService
{
    Task<Result<TaskManagerUser>> RegisterAsync(RegisterDto dto);
}