namespace TaskManager.Infrastructure.Identity.CurrentUser;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? Email { get; }
}