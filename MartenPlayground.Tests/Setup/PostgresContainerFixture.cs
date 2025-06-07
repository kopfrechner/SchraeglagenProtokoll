using Testcontainers.PostgreSql;
using TUnit.Core.Interfaces;

namespace MartenPlayground.Tests.Setup;

public class PostgresContainerFixture : IAsyncInitializer, IAsyncDisposable
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithDatabase("playground")
        .WithUsername("marten")
        .WithPassword("in3gr4t10n")
        .WithPortBinding(61166, 5432)
        .WithAutoRemove(true)
        .Build();

    public PostgreSqlContainer Container => _container;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        // Test container
        await _container.StopAsync();
        await _container.DisposeAsync();
    }
}
