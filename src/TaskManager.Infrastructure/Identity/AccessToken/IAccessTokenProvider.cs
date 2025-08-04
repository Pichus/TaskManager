using TaskManager.Infrastructure.Identity.User;

namespace TaskManager.Infrastructure.Identity.AccessToken;

public interface IAccessTokenProvider
{
    string CreateToken(TaskManagerUser user);
}