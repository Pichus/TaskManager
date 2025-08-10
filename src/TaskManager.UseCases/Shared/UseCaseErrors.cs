namespace TaskManager.UseCases.Shared;

public static class UseCaseErrors
{
    public static readonly Error Unauthenticated = new("Shared.Unauthenticated",
        "Unauthenticated");
}