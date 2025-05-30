using Alba;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using SchraeglagenProtokoll.Tests.Faker;

namespace SchraeglagenProtokoll.Tests;

[ClassDataSource<WebAppFixture>(Shared = SharedType.PerTestSession)]
public abstract class WebAppTestBase(WebAppFixture fixture)
{
    protected IAlbaHost Host => fixture.Host;

    public EventFaker EventFaker { get; } = new();
    public CommandFaker CommandFaker { get; } = new();

    public Task<IScenarioResult> Scenario(Action<Scenario> configure)
    {
        return Host.Scenario(configure);
    }

    public async Task DocumentSessionAsync(Func<IDocumentSession, Task> query)
    {
        using var scope = fixture.Host.Services.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
        await using var session = store.LightweightSession();
        await query(session);
    }

    public void DocumentSession(Action<IDocumentSession> query)
    {
        using var scope = fixture.Host.Services.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
        using var session = store.LightweightSession();
        query(session);
    }
}
