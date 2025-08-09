using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Profile.ProfileDetails.Get;

public static class GetCurrentUserProfileDetailsErrors
{
    public static readonly Error Unauthenticated = new("Profile.ProfileDetails.Get.Unauthenticated",
        "Unauthenticated");
}