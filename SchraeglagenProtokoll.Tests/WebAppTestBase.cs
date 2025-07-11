using System.Security.Cryptography;
using System.Text;
using Alba;
using Marten;
using Marten.Events;
using Microsoft.Extensions.DependencyInjection;
using SchraeglagenProtokoll.Tests.Faker;

namespace SchraeglagenProtokoll.Tests;

[ClassDataSource<WebAppFixture>(Shared = SharedType.PerTestSession)]
[NotInParallel]
public abstract class WebAppTestBase(WebAppFixture fixture)
{
    protected IAlbaHost Host => fixture.Host;

    public EventFaker FakeEvent { get; private set; } = null!;
    public CommandFaker FakeCommand { get; private set; } = null!;

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

    public async Task StartStream(Guid id, params IList<object> events)
    {
        using var scope = fixture.Host.Services.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
        using var session = store.LightweightSession();

        session.Events.StartStream(id, events);
        await session.SaveChangesAsync();
    }

    public async Task WaitForNonStaleProjectionDataAsync(TimeSpan timeout)
    {
        using var scope = fixture.Host.Services.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
        await store.WaitForNonStaleProjectionDataAsync(timeout);
    }

    [Before(Test)]
    public async Task CleanupDatabase(TestContext context)
    {
        await Host.CleanAllMartenDataAsync();

        var testClass = context.TestDetails.TestClass.Name;
        var testName = context.TestDetails.TestName;

        var stableSeed = GetStableIntFromString($"{testClass}.{testName}");
        FakeEvent = new EventFaker(stableSeed);
        FakeCommand = new CommandFaker(stableSeed);
    }

    public static int GetStableIntFromString(string input)
    {
        using var md5 = MD5.Create();
        byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));

        // Convert first 4 bytes of hash to int (little endian)
        return BitConverter.ToInt32(hashBytes, 0);
    }
}
