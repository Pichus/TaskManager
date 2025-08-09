using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Invites.Get;

public class GetInviteErrors
{
    public static readonly Error Unauthenticated = new("Invites.Get.Unauthenticated",
        "Unauthenticated");
}