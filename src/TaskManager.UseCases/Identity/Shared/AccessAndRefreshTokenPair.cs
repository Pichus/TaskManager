namespace TaskManager.UseCases.Identity.Shared;

public class AccessAndRefreshTokenPair
{
    public AccessAndRefreshTokenPair(string accessToken, string refreshToken)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
    }

    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}