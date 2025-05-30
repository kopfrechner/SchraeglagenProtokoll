using Alba;
using Oakton;
using SchraeglagenProtokoll.Api;
using Testcontainers.PostgreSql;
using TUnit.Core.Interfaces;

namespace SchraeglagenProtokoll.Tests;

public class WebAppFixture : IAsyncInitializer, IAsyncDisposable
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithDatabase("schraeglage")
        .WithUsername("marten")
        .WithPassword("in3gr4t10n")
        .WithPortBinding(61165, 5432)
        .WithAutoRemove(true)
        .Build();

    public IAlbaHost Host { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        OaktonEnvironment.AutoStartHost = true;

        var configValues = new Dictionary<string, string?>
        {
            { "ConnectionStrings:Marten", _container.GetConnectionString() },
        };

        Host = await AlbaHost.For<Program>(ConfigurationOverride.Create(configValues));
    }

    public async ValueTask DisposeAsync()
    {
        // Alba
        await Host.StopAsync();
        Host.Dispose();

        // Test container
        await _container.StopAsync();
        await _container.DisposeAsync();
    }
}
