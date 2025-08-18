using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using TaskManager.Infrastructure.Data;
using Testcontainers.PostgreSql;

namespace TaskManager.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;
    private DbConnection _dbConnection;
    private Respawner _respawner;

    public CustomWebApplicationFactory()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:17.5-alpine3.22")
            .WithPortBinding(8080, true)
            .WithDatabase("test")
            .WithUsername("admin")
            .WithPassword("admin")
            .Build();
    }

    public HttpClient HttpClient { get; set; }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        _dbConnection = new NpgsqlConnection(_postgresContainer.GetConnectionString());
            
        HttpClient = CreateClient();

        await _dbConnection.OpenAsync();
        await InitializeRespawnerAsync();
        
        await RunMigrationsAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
        await _dbConnection.DisposeAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_dbConnection);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Postgres", _postgresContainer.GetConnectionString());
    }

    private async Task RunMigrationsAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            await context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Database migration failed during test setup", ex);
        }
    }

    private async Task InitializeRespawnerAsync()
    {
        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            SchemasToInclude = ["public"],
            DbAdapter = DbAdapter.Postgres
        });
    }
}