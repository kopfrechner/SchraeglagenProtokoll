using JasperFx.Events;
using JasperFx.Events.Projections;
using Marten.Events;
using Marten.Events.Projections;
using MartenPlayground.Tests.Setup;
using Shouldly;

namespace MartenPlayground.Tests.EventStore;

public record OwnerAccountSummary(
    string Id,
    int AccountCount,
    Money TotalBalance,
    int TotalDeposits,
    int TotalWithdrawals
);

public class OwnerAccountSummaryProjection : MultiStreamProjection<OwnerAccountSummary, string>
{
    public OwnerAccountSummaryProjection()
    {
        // Group by Owner from Opened event
        Identity<BankAccountEvent.Opened>(e => e.Owner);

        CustomGrouping(
            async (session, events, grouping) =>
            {
                var bankAccountEvents = events
                    .Where(x =>
                        x.EventTypesAre(
                            typeof(BankAccountEvent.Deposited),
                            typeof(BankAccountEvent.Withdrawn),
                            typeof(BankAccountEvent.Deposited)
                        )
                    )
                    .ToList();
                if (!bankAccountEvents.Any())
                    return;

                var bankAccounts = await session.LoadManyAsync<BankAccount>(
                    bankAccountEvents.Select(x => x.StreamId)
                );
                foreach (var ownersBankAccounts in bankAccounts.GroupBy(x => x.Owner))
                {
                    var streams = ownersBankAccounts.Select(x => x.Id);
                    var eventsOfOwnersAccount = bankAccountEvents.Where(x =>
                        streams.Contains(x.StreamId)
                    );
                    grouping.AddEvents(ownersBankAccounts.Key, eventsOfOwnersAccount.ToList());
                }
            }
        );

        // Account opened: increment account count
        CreateEvent<BankAccountEvent.Opened>(e => new OwnerAccountSummary(
            e.Owner,
            1,
            Money.Zero(),
            0,
            0
        ));

        ProjectEvent<BankAccountEvent.Opened>(
            (summary, e) => summary with { AccountCount = summary.AccountCount + 1 }
        );

        // Deposited: increment deposit count and total balance
        ProjectEvent<BankAccountEvent.Deposited>(
            (summary, e) =>
                summary with
                {
                    TotalDeposits = summary.TotalDeposits + 1,
                    TotalBalance = summary.TotalBalance + e.Amount,
                }
        );

        // Withdrawn: increment withdrawal count and decrease total balance
        ProjectEvent<BankAccountEvent.Withdrawn>(
            (summary, e) =>
                summary with
                {
                    TotalWithdrawals = summary.TotalWithdrawals + 1,
                    TotalBalance = summary.TotalBalance - e.Amount,
                }
        );
    }
}

public class EventStoreTests_T5_AsyncMultiStreamProjection : TestBase
{
    public static string EST5AsyncMultiStream = nameof(EST5AsyncMultiStream);

    [Before(Class)]
    public static async Task CleanupSchema()
    {
        await ResetAllData(EST5AsyncMultiStream);
    }

    [Test]
    public async Task T1_owner_account_summary_is_eventually_consistent()
    {
        var owner = "Alice";
        var bankAccountId1 = Guid.NewGuid();
        var bankAccountId2 = Guid.NewGuid();

        var store = Store(
            EST5AsyncMultiStream,
            options =>
            {
                options.Projections.Snapshot<BankAccount>(SnapshotLifecycle.Inline);
                options.Projections.Add<OwnerAccountSummaryProjection>(ProjectionLifecycle.Async);
            }
        );

        // Start daemon
        using var daemon = await store.BuildProjectionDaemonAsync();
        await daemon.StartAllAsync();

        // Create two accounts for the same owner with deposits and withdrawals
        await using (var session = store.LightweightSession())
        {
            session.Events.StartStream<BankAccount>(
                bankAccountId1,
                new BankAccountEvent.Opened(bankAccountId1, owner, Currency.USD),
                new BankAccountEvent.Deposited(Money.From(100, Currency.USD)),
                new BankAccountEvent.Withdrawn(Money.From(30, Currency.USD))
            );
            session.Events.StartStream<BankAccount>(
                bankAccountId2,
                new BankAccountEvent.Opened(bankAccountId2, owner, Currency.USD),
                new BankAccountEvent.Deposited(Money.From(200, Currency.USD)),
                new BankAccountEvent.Deposited(Money.From(50, Currency.USD)),
                new BankAccountEvent.Withdrawn(Money.From(70, Currency.USD))
            );
            await session.SaveChangesAsync();
        }

        // Wait for daemon to process events
        await daemon.WaitForNonStaleData(10.Seconds());

        // Assert the summary is correct
        await using (var session = store.QuerySession())
        {
            var summary = await session.LoadAsync<OwnerAccountSummary>(owner);
            summary.ShouldNotBeNull();
            summary.Id.ShouldBe(owner);
            summary.AccountCount.ShouldBe(2);
            summary.TotalDeposits.ShouldBe(3);
            summary.TotalWithdrawals.ShouldBe(2);
            summary.TotalBalance.ShouldBe(Money.From(250, Currency.USD)); // 100 + 200 + 50 - 30 - 70
        }
    }
}
