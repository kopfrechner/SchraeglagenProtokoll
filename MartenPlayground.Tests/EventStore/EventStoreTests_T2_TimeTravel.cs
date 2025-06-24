using MartenPlayground.Tests.Model;
using MartenPlayground.Tests.Setup;
using Shouldly;

namespace MartenPlayground.Tests.EventStore;

public class EventStoreTests_T2_TimeTravel : TestBase
{
    public static readonly string EST2TimeTravel = nameof(EST2TimeTravel);

    [Before(Class)]
    public static async Task CleanupSchema()
    {
        await ResetAllData(EST2TimeTravel);
    }

    [Test]
    public async Task T1_create_account_and_deposit_with_timestamps()
    {
        await using var session = Session(EST2TimeTravel);

        var bankAccountId = Guid.NewGuid();

        var opened = new BankAccountEvent.Opened("Alice", Currency.USD);
        var deposit1 = new BankAccountEvent.Deposited(Money.From(100, Currency.USD));
        var deposit2 = new BankAccountEvent.Deposited(Money.From(50, Currency.USD));
        var deposit3 = new BankAccountEvent.Deposited(Money.From(25, Currency.USD));

        // Start stream and append events with timestamps
        session.Events.StartStream<BankAccount>(bankAccountId, opened);
        await session.SaveChangesAsync();
        var t0 = DateTimeOffset.UtcNow;
        await Task.Delay(20); // Ensure different timestamps

        session.Events.Append(bankAccountId, deposit1);
        await session.SaveChangesAsync();
        var t1 = DateTimeOffset.UtcNow;
        await Task.Delay(20);

        session.Events.Append(bankAccountId, deposit2);
        await session.SaveChangesAsync();
        var t2 = DateTimeOffset.UtcNow;
        await Task.Delay(20);

        session.Events.Append(bankAccountId, deposit3);
        await session.SaveChangesAsync();
        var t3 = DateTimeOffset.UtcNow;

        AddToBag(
            "TimeTravel",
            new
            {
                bankAccountId,
                t0,
                t1,
                t2,
                t3,
            }
        );
    }

    [Test]
    [DependsOn(nameof(T1_create_account_and_deposit_with_timestamps))]
    public async Task T2_query_account_state_at_each_timestamp()
    {
        var bag = GetFromBag<dynamic>(
            nameof(T1_create_account_and_deposit_with_timestamps),
            "TimeTravel"
        );
        Guid bankAccountId = bag.bankAccountId;
        DateTimeOffset t0 = bag.t0;
        DateTimeOffset t1 = bag.t1;
        DateTimeOffset t2 = bag.t2;
        DateTimeOffset t3 = bag.t3;
        await using var session = Session(EST2TimeTravel);

        // At t0: Only account opened
        var acc0 = await session.Events.AggregateStreamAsync<BankAccount>(
            bankAccountId,
            timestamp: t0
        );
        acc0.ShouldNotBeNull();
        acc0.Balance.ShouldBe(Money.From(0, Currency.USD));

        // At t1: deposit1 applied
        var acc1 = await session.Events.AggregateStreamAsync<BankAccount>(
            bankAccountId,
            timestamp: t1
        );
        acc1.ShouldNotBeNull();
        acc1.Balance.ShouldBe(Money.From(100, Currency.USD));

        // At t2: deposit2 applied
        var acc2 = await session.Events.AggregateStreamAsync<BankAccount>(
            bankAccountId,
            timestamp: t2
        );
        acc2.ShouldNotBeNull();
        acc2.Balance.ShouldBe(Money.From(150, Currency.USD));

        // At t3: deposit3 applied
        var acc3 = await session.Events.AggregateStreamAsync<BankAccount>(
            bankAccountId,
            timestamp: t3
        );
        acc3.ShouldNotBeNull();
        acc3.Balance.ShouldBe(Money.From(175, Currency.USD));
    }

    [Test]
    [DependsOn(nameof(T1_create_account_and_deposit_with_timestamps))]
    public async Task T3_query_account_state_by_version()
    {
        var bag = GetFromBag<dynamic>(
            nameof(T1_create_account_and_deposit_with_timestamps),
            "TimeTravel"
        );
        Guid bankAccountId = bag.bankAccountId;
        await using var session = Session(EST2TimeTravel);

        // Version 1: Opened
        var accV1 = await session.Events.AggregateStreamAsync<BankAccount>(
            bankAccountId,
            version: 1
        );
        accV1.ShouldNotBeNull();
        accV1.Balance.ShouldBe(Money.From(0, Currency.USD));

        // Version 2: deposit1
        var accV2 = await session.Events.AggregateStreamAsync<BankAccount>(
            bankAccountId,
            version: 2
        );
        accV2.ShouldNotBeNull();
        accV2.Balance.ShouldBe(Money.From(100, Currency.USD));

        // Version 3: deposit2
        var accV3 = await session.Events.AggregateStreamAsync<BankAccount>(
            bankAccountId,
            version: 3
        );
        accV3.ShouldNotBeNull();
        accV3.Balance.ShouldBe(Money.From(150, Currency.USD));

        // Version 4: deposit3
        var accV4 = await session.Events.AggregateStreamAsync<BankAccount>(
            bankAccountId,
            version: 4
        );
        accV4.ShouldNotBeNull();
        accV4.Balance.ShouldBe(Money.From(175, Currency.USD));
    }
}
