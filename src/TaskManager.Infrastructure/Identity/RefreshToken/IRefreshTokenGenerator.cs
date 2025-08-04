namespace TaskManager.Infrastructure.Identity.RefreshToken;

public interface IRefreshTokenGenerator
{
    string GenerateToken();
}