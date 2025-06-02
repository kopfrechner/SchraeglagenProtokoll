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

    public void DocumentSession(Action<IDocumentSession> query)
    {
        using var scope = fixture.Host.Services.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
        using var session = store.LightweightSession();
        query(session);
    }

    public async Task<Guid> StartStream(params IList<object> events)
    {
        using var scope = fixture.Host.Services.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
        using var session = store.LightweightSession();

        if (events.Count == 0)
            throw new ArgumentException("At least one event is required");

        var first = events[0];

        var id = TryExtractEventIdFromFirstEvent(first);

        var stream = id.HasValue
            ? session.Events.StartStream(id.Value, events)
            : session.Events.StartStream(events);
        await session.SaveChangesAsync();

        return stream.Id;
    }

    public async Task<Guid> StartStreamWithTransactionPerEvent(params IList<object> events)
    {
        using var scope = fixture.Host.Services.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
        using var session = store.LightweightSession();

        if (events.Count == 0)
            throw new ArgumentException("At least one event is required");

        var first = events[0];
        var rest = events.Skip(1).ToArray();

        var id = TryExtractEventIdFromFirstEvent(first);

        var stream = id.HasValue
            ? session.Events.StartStream(id.Value, first)
            : session.Events.StartStream(first);
        await session.SaveChangesAsync();

        if (rest.Length > 0)
        {
            session.Events.Append(stream.Id, rest);
            await session.SaveChangesAsync();
        }

        return stream.Id;
    }

    private static Guid? TryExtractEventIdFromFirstEvent(object @event)
    {
        return @event.GetType().GetProperty("Id") is { PropertyType: Type t } p && t == typeof(Guid)
            ? p.GetValue(@event) as Guid?
            : null;
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
