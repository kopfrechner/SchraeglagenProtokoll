using Alba;
using Marten;
using SchraeglagenProtokoll.Api;
using Testcontainers.PostgreSql;
using TUnit.Core.Interfaces;

namespace SchraeglagenProtokoll.Tests;

internal enum DbSetup
{
    LocalDb,
    TestContainer,
}

public class WebAppFixture : IAsyncInitializer, IAsyncDisposable
{
    private static DbSetup UseTestContainer => DbSetup.LocalDb;

    private readonly PostgreSqlContainer? _container = UseTestContainer switch
    {
        DbSetup.TestContainer => new PostgreSqlBuilder()
            .WithImage("postgres:latest")
            .WithDatabase("marten-integration-tests")
            .WithUsername("marten")
            .WithPassword("in3gr4t10n")
            .WithPortBinding(61163, 5432)
            .WithAutoRemove(true)
            .Build(),
        _ => null,
    };

    public IAlbaHost Host { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        if (_container is not null)
        {
            await _container.StartAsync();
        }

        var connectionString =
            _container?.GetConnectionString()
            ?? "User ID=marten;Password=change-me-123#!;Host=localhost;Port=5680;Database=schraeglage";

        var configValues = new Dictionary<string, string?>
        {
            { "ConnectionStrings:Marten", connectionString },
        };

        Host = await AlbaHost.For<Program>(ConfigurationOverride.Create(configValues));
        await Host.CleanAllMartenDataAsync();
    }

    public async ValueTask DisposeAsync()
    {
        // Alba
        await Host.StopAsync();
        Host.Dispose();

        // Test container
        if (_container is not null)
        {
            await _container!.StopAsync();
            await _container!.DisposeAsync();
        }
    }
}
