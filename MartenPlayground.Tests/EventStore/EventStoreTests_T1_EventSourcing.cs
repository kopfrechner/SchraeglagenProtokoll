using System.Text.Json.Serialization;
using Marten;
using Marten.Events.Projections;
using MartenPlayground.Tests.Model;
using MartenPlayground.Tests.Setup;
using Shouldly;
using Weasel.Core;

namespace MartenPlayground.Tests.EventStore;

public class EventStoreTests_T1_EventSourcing : TestBase
{
    public static readonly string EST1EventSourcing = nameof(EST1EventSourcing);

    [Before(Class)]
    public static async Task CleanupSchema()
    {
        await ResetAllData(EST1EventSourcing);
    }

    [Test]
    public async Task T1_start_bank_account_stream()
    {
        // Create it manually for illustration purposes
        var store = DocumentStore.For(options =>
        {
            options.Connection(ConnectionString);
            options.DatabaseSchemaName = EST1EventSourcing;
            options.UseSystemTextJsonForSerialization(
                EnumStorage.AsString,
                Casing.Default,
                jsonOptions => jsonOptions.Converters.Add(new JsonStringEnumConverter())
            );
        });

        // Open a session
        await using var session = store.LightweightSession();

        // This will be our bankAccountId
        var bankAccountId = Guid.NewGuid();

        // Start a brand-new stream and commit the new events part of a transaction
        session.Events.StartStream<BankAccount>(
            bankAccountId,
            new BankAccountEvent.Opened("John Smith", Currency.USD),
            new BankAccountEvent.Deposited(Money.From(100, Currency.USD))
        );

        // Add some more events to the stream
        session.Events.Append(
            bankAccountId,
            new BankAccountEvent.Withdrawn(Money.From(50, Currency.USD))
        );

        // Save the pending changes to db
        await session.SaveChangesAsync();

        AddToBag("BankAccountId", bankAccountId);
    }

    [Test]
    [DependsOn(nameof(T1_start_bank_account_stream))]
    public async Task T2_load_bankaccount_aggregate()
    {
        // Arrange
        var bankAccountId = GetFromBag<Guid>(nameof(T1_start_bank_account_stream), "BankAccountId");

        // Act
        await using var session = Session(EST1EventSourcing);
        var bankAccount = await session.Events.AggregateStreamAsync<BankAccount>(bankAccountId);

        // Assert
        bankAccount
            .ShouldNotBeNull()
            .ShouldSatisfyAllConditions(
                b => b.Id.ShouldBe(bankAccountId),
                b => b.Owner.ShouldBe("John Smith"),
                b => b.Balance.ShouldBe(Money.From(50, Currency.USD))
            );
    }

    [Test]
    [DependsOn(nameof(T1_start_bank_account_stream))]
    public async Task T3_self_aggregate_projection_is_null_initially()
    {
        // Arrange
        var bankAccountId = GetFromBag<Guid>(nameof(T1_start_bank_account_stream), "BankAccountId");

        // Act
        await using var session = Session(
            EST1EventSourcing,
            options => options.Projections.Snapshot<BankAccount>(SnapshotLifecycle.Inline)
        );
        await session.SaveChangesAsync();

        // Assert
        var bankAccount = await session.LoadAsync<BankAccount>(bankAccountId);
        bankAccount.ShouldBeNull();
    }

    [Test]
    [DependsOn(nameof(T3_self_aggregate_projection_is_null_initially))]
    public async Task T4_rebuild_stream_then_projection_should_be_current_state()
    {
        // Arrange
        var bankAccountId = GetFromBag<Guid>(nameof(T1_start_bank_account_stream), "BankAccountId");

        // Act
        await using var session = Session(
            EST1EventSourcing,
            options => options.Projections.Snapshot<BankAccount>(SnapshotLifecycle.Inline)
        );

        // (Re)build the self-aggregate projection
        await session.DocumentStore.Advanced.RebuildSingleStreamAsync<BankAccount>(bankAccountId);

        // Assert
        var bankAccount = await session.LoadAsync<BankAccount>(bankAccountId);
        bankAccount
            .ShouldNotBeNull()
            .ShouldSatisfyAllConditions(
                b => b.Id.ShouldBe(bankAccountId),
                b => b.Owner.ShouldBe("John Smith"),
                b => b.Balance.ShouldBe(Money.From(50, Currency.USD))
            );
    }

    [Test]
    [DependsOn(nameof(T4_rebuild_stream_then_projection_should_be_current_state))]
    public async Task T5_append_event_on_self_aggregate()
    {
        // Arrange
        var bankAccountId = GetFromBag<Guid>(nameof(T1_start_bank_account_stream), "BankAccountId");

        await using var session = Session(
            EST1EventSourcing,
            options => options.Projections.Snapshot<BankAccount>(SnapshotLifecycle.Inline)
        );

        // Act
        await session.Events.WriteToAggregate<BankAccount>(
            bankAccountId,
            stream => stream.AppendOne(new BankAccountEvent.Withdrawn(Money.From(50, Currency.USD)))
        );
        await session.SaveChangesAsync();

        // Assert
        var bankAccount = await session.LoadAsync<BankAccount>(bankAccountId);
        bankAccount
            .ShouldNotBeNull()
            .ShouldSatisfyAllConditions(
                b => b.Id.ShouldBe(bankAccountId),
                b => b.Id.ShouldBe(bankAccountId),
                b => b.Owner.ShouldBe("John Smith"),
                b => b.Balance.ShouldBe(Money.From(0, Currency.USD))
            );
    }
}
