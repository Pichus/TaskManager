using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Identity.RefreshToken;

public static class RefreshTokenErrors
{
    public static readonly Error RefreshTokenNotFound =
        new("Identity.RefreshToken.RefreshTokenNotFound", "Refresh token not found");

    public static readonly Error RefreshTokenRevoked =
        new("Identity.RefreshToken.RefreshTokenRevoked",
            "The provided refresh token has already been revoked");

    public static readonly Error RefreshTokenExpired =
        new("Identity.RefreshToken.RefreshTokenExpired",
            "The provided refresh token has expired");

    public static Error RefreshTokenUserNotFound(string userId = "")
    {
        return new Error("Identity.RefreshToken.RefreshTokenUserNotFound",
            $"User with id {userId} not found");
    }
}