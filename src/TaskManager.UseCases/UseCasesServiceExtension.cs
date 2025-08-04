using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.UseCases.Identity.Login;
using TaskManager.UseCases.Identity.RefreshToken;
using TaskManager.UseCases.Identity.Register;
using TaskManager.UseCases.Profile.Invites;
using TaskManager.UseCases.Profile.ProfileDetails;
using TaskManager.UseCases.Projects;

namespace TaskManager.UseCases;

public static class UseCasesServiceExtension
{
    public static IServiceCollection AddUseCasesServices(
        this IServiceCollection services,
        IConfiguration configuration,
        // ILogger logger,
        string environmentName)
    {
        if (environmentName == "Development")
            RegisterDevelopmentOnlyDependencies(services, configuration);
        else if (environmentName == "Testing")
            RegisterTestingOnlyDependencies(services);
        else
            RegisterProductionOnlyDependencies(services, configuration);

        return services;
    }

    private static void RegisterProductionOnlyDependencies(IServiceCollection services, IConfiguration configuration)
    {
        RegisterServices(services);
    }

    private static void RegisterTestingOnlyDependencies(IServiceCollection services)
    {
        RegisterServices(services);
    }

    private static void RegisterDevelopmentOnlyDependencies(IServiceCollection services, IConfiguration configuration)
    {
        RegisterServices(services);
    }

    private static void RegisterServices(IServiceCollection services)
    {
        services.AddScoped<IRegisterService, RegisterService>();
        services.AddScoped<ILoginService, LoginService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();

        services.AddScoped<IProjectService, ProjectService>();

        services.AddScoped<IInviteService, InviteService>();
        services.AddScoped<IProfileDetailsService, ProfileDetailsService>();
    }
}