using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Identity.Login;

public static class LoginErrors
{
    public static readonly Error WrongEmailOrPassword =
        new("Identity.Login.WrongEmailOrPassword", "Wrong email or password");
}