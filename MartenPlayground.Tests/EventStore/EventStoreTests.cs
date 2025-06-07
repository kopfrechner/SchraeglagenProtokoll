using System.Text.Json.Serialization;
using Marten;
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
        var bankAccountId = GetFromBag<Guid>(
            nameof(O1_create_eventstore_then_create_stream),
            "BankAccountId"
        );

        await using var session = Session(EventStoreScenario01);
        var bankAccount = await session.Events.AggregateStreamAsync<BankAccount>(bankAccountId);

        bankAccount
            .ShouldNotBeNull()
            .ShouldBe(new BankAccount(bankAccountId, "John Smith", Money.From(50, Currency.USD)));
    }
}
