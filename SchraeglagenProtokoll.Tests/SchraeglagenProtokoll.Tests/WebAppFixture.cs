using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using SchraeglagenProtokoll.Api;
using Testcontainers.PostgreSql;
using TUnit.Core.Interfaces;

namespace SchraeglagenProtokoll.Tests;

//[Collection("run-sequentially-due-to-shared-test-container")]
public class WebAppFixture: IAsyncInitializer, IAsyncDisposable
{
    private WebApplicationFactory<Program> _app = null!;
    
    private readonly PostgreSqlContainer _container =
        new PostgreSqlBuilder()
            .WithImage("postgres:latest")
            .WithDatabase("marten-integration-tests")
            .WithUsername("marten")
            .WithPassword("in3gr4t10n")
            .WithPortBinding(61163, 5432)
            .WithAutoRemove(true)
            .Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        
        var configValues = new Dictionary<string, string?>
        {
            { "ConnectionStrings:Marten", _container.GetConnectionString() },
        };
        
        _app = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(configValues);
                });
            });
    }
    
    public HttpClient CreateClient() => _app.CreateClient();

    public async ValueTask DisposeAsync()
    {
        await _app.DisposeAsync();
        await _container.StopAsync();
        await _container.DisposeAsync();
    }
}
