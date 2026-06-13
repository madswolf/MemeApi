using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using MemeApi.Models.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using Testcontainers.PostgreSql;
using Xunit;

[CollectionDefinition(nameof(DatabaseTestCollection))]
public class DatabaseTestCollection : ICollectionFixture<IntegrationTestFactory>
{
}

public class IntegrationTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .WithDatabase("db")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted("pg_isready"))
        .WithCleanUp(true)
        .Build();

    public MemeContext Db { get; private set; } = null!;
    private Respawner _respawner = null!;
    private DbConnection _connection = null!;

    public void ResetConnection()
    {
        if (Db != null)
        {
            var scope = Services.CreateScope();
            Db =  scope.ServiceProvider.GetRequiredService<MemeContext>();
        }
    }

    public async Task ResetDatabase()
    {
        await _respawner.ResetAsync(_connection);
        Db.ChangeTracker.Clear();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        var scope = Services.CreateScope();
        Db = scope.ServiceProvider.GetRequiredService<MemeContext>();
        _connection = Db.Database.GetDbConnection();
        await _connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = new[] { "public" },
            WithReseed = true
        });
    }

    public new async Task DisposeAsync()
    {
        await _connection.CloseAsync();
        await _container.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt_Secret"]                    = "test-jwt-secret-key-at-least-32-bytes-long!",
                ["Jwt_Issuer"]                    = "test-issuer",
                ["Jwt_Audience"]                  = "test-audience",
                ["Bot_Secret"]                    = "lol",
                ["Topic_Default_Topicname"]       = "Rotte-Grotte",
                ["Topic_MemeOfTheDay_Topicname"]  = "MemeOfTheDay",
                ["Media_Host"]                    = "test",
                ["Admin_Username"]                = "Admin",
                ["Admin_Password"]                = "wakabZfZ4VkUU4zBShZs",
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveDbContext<MemeContext>();
            services.AddDbContext<MemeContext>(options =>
            {
                options.UseNpgsql(_container.GetConnectionString());
            });
        });
    }
}

public static class ServiceCollectionExtensions
{
    public static void RemoveDbContext<T>(this IServiceCollection services) where T : DbContext
    {
        var descriptor = services.SingleOrDefault(x => x.ServiceType == typeof(DbContextOptions<T>));
        if (descriptor != null)
        {
            services.Remove(descriptor);
        }
    }

    public static void EnsureDbCreated<T>(this IServiceCollection services) where T : DbContext
    {
        using var scope = services.BuildServiceProvider().CreateScope();
        var serviceProvider = scope.ServiceProvider;
        var context = serviceProvider.GetRequiredService<T>();
        context.Database.EnsureCreated();
    }
}