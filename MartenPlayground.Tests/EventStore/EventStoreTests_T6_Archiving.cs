using Marten;
using Marten.Events.Projections;
using MartenPlayground.Tests.EventStore.Model;
using MartenPlayground.Tests.Setup;
using Shouldly;

namespace MartenPlayground.Tests.EventStore;

public class EventStoreTests_T6_Archiving : TestBase
{
    public static readonly string EST6Archiving = nameof(EST6Archiving);

    [Before(Class)]
    public static async Task CleanupSchema()
    {
        await ResetAllData(EST6Archiving);
    }

    [Test]
    public async Task T1_archived_stream_is_not_returned_by_default_queries()
    {
        var bankAccountId = Guid.NewGuid();
        var store = Store(
            EST6Archiving,
            options => options.Projections.Snapshot<BankAccount>(SnapshotLifecycle.Inline)
        );

        // Create and populate stream
        await using (var session = store.LightweightSession())
        {
            session.Events.StartStream<BankAccount>(
                bankAccountId,
                new BankAccountEvent.Opened("Bob", Currency.USD),
                new BankAccountEvent.Deposited(Money.From(100, Currency.USD)),
                new BankAccountEvent.Withdrawn(Money.From(30, Currency.USD))
            );
            await session.SaveChangesAsync();
        }

        // Archive the stream
        await using (var session = store.LightweightSession())
        {
            session.Events.ArchiveStream(bankAccountId); // Two possible approaches
            // session.Events.Append(bankAccountId, new Archived("Because..."));
            await session.SaveChangesAsync();
        }

        // Normal query should return null
        await using (var session = store.QuerySession())
        {
            var account = await session.Events.AggregateStreamAsync<BankAccount>(bankAccountId);
            account.ShouldBeNull();

            // TODO: In BankAccount self-aggregate react to archived event:
            // public bool ShouldDelete(Archived archived) => true;

            // var selfAggregate = await session.LoadAsync<BankAccount>(bankAccountId);
            // selfAggregate.ShouldBeNull();
        }

        AddToBag("BankAccountId", bankAccountId);
    }

    [Test]
    [DependsOn(nameof(T1_archived_stream_is_not_returned_by_default_queries))]
    public async Task T2_archived_stream_data_is_preserved_and_queryable_with_special_parameter()
    {
        var bankAccountId = GetFromBag<Guid>(
            nameof(T1_archived_stream_is_not_returned_by_default_queries),
            "BankAccountId"
        );
        await using var session = Session(EST6Archiving);

        // Query archived events
        var archivedEvents = await session
            .Events.QueryAllRawEvents()
            .Where(x => x.IsArchived)
            .ToListAsync();
        archivedEvents.Count.ShouldBe(3);
        archivedEvents.Select(x => x.StreamId).ShouldAllBe(x => x == bankAccountId);
    }
}
