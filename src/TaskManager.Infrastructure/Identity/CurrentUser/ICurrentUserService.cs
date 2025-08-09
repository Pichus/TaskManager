namespace TaskManager.Infrastructure.Identity.CurrentUser;

public interface ICurrentUserService
{
    public bool IsAuthenticated { get; }
    string? UserId { get; }
    string? Email { get; }
}