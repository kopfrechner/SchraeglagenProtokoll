using JasperFx.Events;
using MartenPlayground.Tests.EventStore.Model;
using MartenPlayground.Tests.Setup;
using Shouldly;

namespace MartenPlayground.Tests.EventStore;

[NotInParallel]
public class EventStoreTests_T3_OptimisticConcurrency : TestBase
{
    public static readonly string EST3OptimisticConcurrency = nameof(EST3OptimisticConcurrency);

    [Before(Class)]
    public static async Task CleanupSchema()
    {
        await ResetAllData(EST3OptimisticConcurrency);
    }

    [Test]
    public async Task T1_when_ignoring_version_we_have_optimistic_concurrency_issues()
    {
        var bankAccountId = Guid.NewGuid();
        // Create initial account and deposit
        await using (var session = Session(EST3OptimisticConcurrency))
        {
            var opened = new BankAccountEvent.Opened(bankAccountId, "Bob", Currency.USD);
            var deposit = new BankAccountEvent.Deposited(Money.From(100, Currency.USD));
            session.Events.StartStream<BankAccount>(bankAccountId, opened, deposit);
            await session.SaveChangesAsync();
        }

        // Open two concurrent sessions
        await using var session1 = Session(EST3OptimisticConcurrency);
        await using var session2 = Session(EST3OptimisticConcurrency);

        // Both load the current aggregate
        var aggregate1 = await session1.Events.AggregateStreamAsync<BankAccount>(bankAccountId);
        var aggregate2 = await session2.Events.AggregateStreamAsync<BankAccount>(bankAccountId);

        // Session 1 appends a withdrawal and saves
        session1.Events.Append(
            bankAccountId,
            new BankAccountEvent.Withdrawn(Money.From(30, Currency.USD))
        );
        await session1.SaveChangesAsync();

        // Session 2 attempts to append another withdrawal, expecting the original version
        session2.Events.Append(
            bankAccountId,
            new BankAccountEvent.Withdrawn(Money.From(20, Currency.USD))
        );
        await session2.SaveChangesAsync();

        // Assert
        await using var assertSession = Session(EST3OptimisticConcurrency);
        var latest = await assertSession.Events.AggregateStreamAsync<BankAccount>(bankAccountId);
        latest.Balance.ShouldBe(Money.From(50, Currency.USD));
        latest.Version.ShouldBe(4);
    }

    [Test]
    public async Task T2_use_version_to_solve_optimistic_concurrency_issues()
    {
        var bankAccountId = Guid.NewGuid();
        // Create initial account and deposit
        await using (var session = Session(EST3OptimisticConcurrency))
        {
            var opened = new BankAccountEvent.Opened(bankAccountId, "Bob", Currency.USD);
            var deposit = new BankAccountEvent.Deposited(Money.From(100, Currency.USD));
            session.Events.StartStream<BankAccount>(bankAccountId, opened, deposit);
            await session.SaveChangesAsync();
        }

        // Open two concurrent sessions
        await using var session1 = Session(EST3OptimisticConcurrency);
        await using var session2 = Session(EST3OptimisticConcurrency);

        // Both load the current aggregate
        var aggregate1 = await session1.Events.AggregateStreamAsync<BankAccount>(bankAccountId);
        var aggregate2 = await session2.Events.AggregateStreamAsync<BankAccount>(bankAccountId);

        // Session 1 appends a withdrawal and saves
        session1.Events.Append(
            bankAccountId,
            aggregate1!.Version + 1,
            new BankAccountEvent.Withdrawn(Money.From(30, Currency.USD))
        );
        await session1.SaveChangesAsync();

        // Session 2 attempts to append another withdrawal, expecting the original version
        session2.Events.Append(
            bankAccountId,
            aggregate2!.Version + 1,
            new BankAccountEvent.Withdrawn(Money.From(20, Currency.USD))
        );
        var ex = async () => await session2.SaveChangesAsync();
        ex.ShouldThrow<EventStreamUnexpectedMaxEventIdException>();
    }

    [Test]
    public async Task T3_use_fetch_for_writing_to_solve_optimistic_concurrency_issues()
    {
        var bankAccountId = Guid.NewGuid();
        // Create initial account and deposit
        await using (var session = Session(EST3OptimisticConcurrency))
        {
            var opened = new BankAccountEvent.Opened(bankAccountId, "Bob", Currency.USD);
            var deposit = new BankAccountEvent.Deposited(Money.From(100, Currency.USD));
            session.Events.StartStream<BankAccount>(bankAccountId, opened, deposit);
            await session.SaveChangesAsync();
        }

        // Open two concurrent sessions
        await using var session1 = Session(EST3OptimisticConcurrency);
        await using var session2 = Session(EST3OptimisticConcurrency);

        // Both load the current aggregate
        var aggregate1 = await session1.Events.FetchForWriting<BankAccount>(bankAccountId);
        var aggregate2 = await session2.Events.FetchForWriting<BankAccount>(bankAccountId);

        // Session 1 appends a withdrawal and saves
        aggregate1.AppendOne(new BankAccountEvent.Withdrawn(Money.From(30, Currency.USD)));
        await session1.SaveChangesAsync();

        // Session 2 attempts to append another withdrawal, expecting the original version
        aggregate2.AppendOne(new BankAccountEvent.Withdrawn(Money.From(20, Currency.USD)));
        var ex = async () => await session2.SaveChangesAsync();
        ex.ShouldThrow<EventStreamUnexpectedMaxEventIdException>();
    }
}
