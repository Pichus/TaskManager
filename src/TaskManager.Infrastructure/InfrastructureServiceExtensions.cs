using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.Core.ProjectAggregate;
using TaskManager.Core.ProjectInviteAggregate;
using TaskManager.Core.TaskAggregate;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Identity.AccessToken;
using TaskManager.Infrastructure.Identity.CurrentUser;
using TaskManager.Infrastructure.Identity.RefreshToken;
using TaskManager.Infrastructure.Identity.User;
using TaskManager.Infrastructure.ProjectInvites;
using TaskManager.Infrastructure.ProjectMembers;
using TaskManager.Infrastructure.Projects;
using TaskManager.Infrastructure.Tasks;

namespace TaskManager.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration,
        string environmentName)
    {
        if (environmentName == "Development")
            RegisterDevelopmentOnlyDependencies(services, configuration);
        else if (environmentName == "Testing")
            RegisterTestingOnlyDependencies(services);
        else
            RegisterProductionOnlyDependencies(services, configuration);

        RegisterEFRepositories(services);
        RegisterServices(services);

        return services;
    }

    private static void AddDbContextWithPostgres(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres");
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddDefaultIdentity<TaskManagerUser>()
            .AddEntityFrameworkStores<AppDbContext>();
    }


    private static void RegisterDevelopmentOnlyDependencies(IServiceCollection services, IConfiguration configuration)
    {
        AddDbContextWithPostgres(services, configuration);
    }

    private static void RegisterTestingOnlyDependencies(IServiceCollection services)
    {
    }

    private static void RegisterProductionOnlyDependencies(IServiceCollection services, IConfiguration configuration)
    {
        AddDbContextWithPostgres(services, configuration);
    }

    private static void RegisterEFRepositories(IServiceCollection services)
    {
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IProjectInviteRepository, ProjectInviteRepository>();
        services.AddScoped<IProjectMemberRepository, ProjectMemberRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();
    }

    private static void RegisterServices(IServiceCollection services)
    {
        services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();
        services.AddScoped<IAccessTokenProvider, AccessTokenProvider>();

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }
}