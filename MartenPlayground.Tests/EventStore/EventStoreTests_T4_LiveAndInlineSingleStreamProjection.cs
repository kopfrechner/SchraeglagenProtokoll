using JasperFx.Events;
using JasperFx.Events.Projections;
using Marten.Events.Aggregation;
using MartenPlayground.Tests.Setup;
using Shouldly;

namespace MartenPlayground.Tests.EventStore;

// Projection model for counting deposits and withdrawals and tracking owner
public record AccountActivity(
    Guid Id,
    string Owner,
    int DepositCount = 0,
    int WithdrawalCount = 0,
    DateTimeOffset? LastActivity = null
);

// Inline projection for counting deposits and withdrawals and tracking owner
public class AccountActivityProjection : SingleStreamProjection<AccountActivity, Guid>
{
    public AccountActivityProjection()
    {
        CreateEvent<BankAccountEvent.Opened>(e => new AccountActivity(e.Id, e.Owner));
        ProjectEvent<IEvent<BankAccountEvent.Deposited>>(
            (a, e) => a with { DepositCount = a.DepositCount + 1, LastActivity = e.Timestamp }
        );
        ProjectEvent<IEvent<BankAccountEvent.Withdrawn>>(
            (a, e) => a with { WithdrawalCount = a.WithdrawalCount + 1, LastActivity = e.Timestamp }
        );
    }
}

[NotInParallel]
public class EventStoreTests_T4_LiveAndInlineSingleStreamProjection(PostgresContainerFixture fixture)
    : TestBase(fixture)
{
    private const string ESInlineProjection = nameof(ESInlineProjection);

    [Test]
    public async Task T1_projection_counts_deposits_and_withdrawals_and_owner_live()
    {
        // Arrange
        var bankAccountId = Guid.NewGuid();
        await using var session = Session(
            ESInlineProjection,
            options =>
                options.Projections.Add<AccountActivityProjection>(ProjectionLifecycle.Live)
        );

        session.Events.StartStream<BankAccount>(
            bankAccountId,
            new BankAccountEvent.Opened(bankAccountId, "Alice", Currency.USD),
            new BankAccountEvent.Deposited(Money.From(200, Currency.USD)),
            new BankAccountEvent.Deposited(Money.From(50, Currency.USD)),
            new BankAccountEvent.Withdrawn(Money.From(70, Currency.USD))
        );
        await session.SaveChangesAsync();

        // Act and Assert
        var activity = await session.Events.AggregateStreamAsync<AccountActivity>(bankAccountId);
        activity.ShouldNotBeNull();
        activity.Owner.ShouldBe("Alice");
        activity.DepositCount.ShouldBe(2);
        activity.WithdrawalCount.ShouldBe(1);
    }
    
    [Test]
    public async Task T1_projection_counts_deposits_and_withdrawals_and_owner_inline()
    {
        // Arrange
        var bankAccountId = Guid.NewGuid();
        await using var session = Session(
            ESInlineProjection,
            options =>
                options.Projections.Add<AccountActivityProjection>(ProjectionLifecycle.Inline)
        );

        session.Events.StartStream<BankAccount>(
            bankAccountId,
            new BankAccountEvent.Opened(bankAccountId, "Alice", Currency.USD),
            new BankAccountEvent.Deposited(Money.From(200, Currency.USD)),
            new BankAccountEvent.Deposited(Money.From(50, Currency.USD)),
            new BankAccountEvent.Withdrawn(Money.From(70, Currency.USD))
        );
        await session.SaveChangesAsync();

        // Act and Assert
        var activity = await session.LoadAsync<AccountActivity>(bankAccountId);
        activity.ShouldNotBeNull();
        activity.Owner.ShouldBe("Alice");
        activity.DepositCount.ShouldBe(2);
        activity.WithdrawalCount.ShouldBe(1);
    }
}
