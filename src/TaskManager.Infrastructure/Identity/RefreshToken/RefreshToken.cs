using TaskManager.Core;
using TaskManager.Infrastructure.Identity.User;

namespace TaskManager.Infrastructure.Identity.RefreshToken;

public class RefreshToken : EntityBase
{
    public string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string UserId { get; set; }

    public TaskManagerUser User { get; set; }
    
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt.HasValue;
    public bool IsActive => !IsExpired && !IsRevoked;
}