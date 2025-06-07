using System.Text.Json.Serialization;
using Marten;
using Marten.Events.Projections;
using MartenPlayground.Tests.Setup;
using Shouldly;
using Weasel.Core;

namespace MartenPlayground.Tests.EventStore;

public class EventStoreTests(PostgresContainerFixture fixture) : DocumentStoreTestBase(fixture)
{
    private const string EventStoreScenario01 = nameof(EventStoreScenario01);

    [Test]
    public async Task O1_create_eventstore_then_create_stream()
    {
        // Create it manually for illustration purposes
        var store = DocumentStore.For(options =>
        {
            options.Connection(ConnectionString);
            options.DatabaseSchemaName = EventStoreScenario01;
            options.UseSystemTextJsonForSerialization(
                EnumStorage.AsString,
                Casing.Default,
                jsonOptions => jsonOptions.Converters.Add(new JsonStringEnumConverter())
            );
        });
        await using var session = store.LightweightSession();

        // This will be our bankAccountId
        var bankAccountId = Guid.NewGuid();

        // Create events
        var opened = new BankAccountEvent.Opened(bankAccountId, "John Smith", Currency.USD);
        var deposited = new BankAccountEvent.Deposited(Money.From(100, Currency.USD));

        // Start a brand-new stream and commit the new events part of a transaction
        session.Events.StartStream<BankAccount>(bankAccountId, opened, deposited);

        // Add some more events to the stream
        var withdrawn = new BankAccountEvent.Withdrawn(Money.From(50, Currency.USD));
        session.Events.Append(bankAccountId, withdrawn);

        // Save the pending changes to db
        await session.SaveChangesAsync();

        AddToBag("BankAccountId", bankAccountId);
    }

    [Test]
    [DependsOn(nameof(O1_create_eventstore_then_create_stream))]
    public async Task O2_load_bankaccount_aggregate()
    {
        // Arrange
        var bankAccountId = GetFromBag<Guid>(
            nameof(O1_create_eventstore_then_create_stream),
            "BankAccountId"
        );

        // Act
        await using var session = Session(EventStoreScenario01);
        var bankAccount = await session.Events.AggregateStreamAsync<BankAccount>(bankAccountId);

        // Assert
        bankAccount
            .ShouldNotBeNull()
            .ShouldBe(new BankAccount(bankAccountId, "John Smith", Money.From(50, Currency.USD)));
    }

    [Test]
    [DependsOn(nameof(O1_create_eventstore_then_create_stream))]
    public async Task O3_when_create_self_aggregate_projection_then_projection_should_null()
    {
        // Arrange
        var bankAccountId = GetFromBag<Guid>(
            nameof(O1_create_eventstore_then_create_stream),
            "BankAccountId"
        );

        // Act
        await using var session = Session(
            EventStoreScenario01,
            options => options.Projections.Snapshot<BankAccount>(SnapshotLifecycle.Inline)
        );
        await session.SaveChangesAsync();

        // Assert
        var bankAccount = await session.LoadAsync<BankAccount>(bankAccountId);
        bankAccount.ShouldBeNull();
    }

    [Test]
    [DependsOn(nameof(O3_when_create_self_aggregate_projection_then_projection_should_null))]
    public async Task O4_rebuild_stream_then_projection_should_be_current_state()
    {
        // Arrange
        var bankAccountId = GetFromBag<Guid>(
            nameof(O1_create_eventstore_then_create_stream),
            "BankAccountId"
        );

        // Act
        await using var session = Session(
            EventStoreScenario01,
            options => options.Projections.Snapshot<BankAccount>(SnapshotLifecycle.Inline)
        );

        // (Re)build the self-aggregate projection
        await session.DocumentStore.Advanced.RebuildSingleStreamAsync<BankAccount>(bankAccountId);

        // Assert
        var bankAccount = await session.LoadAsync<BankAccount>(bankAccountId);
        bankAccount
            .ShouldNotBeNull()
            .ShouldBe(new BankAccount(bankAccountId, "John Smith", Money.From(50, Currency.USD)));
    }

    // TODO:
    // * Time Travel (Timestamp / Version)
    // * Append events starting at self-aggregate
    // * Async vs Inline projections
    // * Build a single-stream projection
    // * Build a multi-stream projection
    // * Archive a stream
    // * Mask events
    // * Build a notification handler
}
