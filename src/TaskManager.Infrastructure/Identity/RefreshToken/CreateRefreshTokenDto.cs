namespace TaskManager.Infrastructure.Identity.RefreshToken;

public class CreateRefreshTokenDto
{
    public string TokenString { get; set; }
    public string UserId { get; set; }
}