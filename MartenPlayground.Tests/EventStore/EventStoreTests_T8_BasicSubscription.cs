using JasperFx.Events.Daemon;
using JasperFx.Events.Projections;
using Marten;
using Marten.Subscriptions;
using MartenPlayground.Tests.EventStore.Model;
using MartenPlayground.Tests.Setup;
using Shouldly;

namespace MartenPlayground.Tests.EventStore;

public class TransactionNotificationCollector
{
    public List<BankAccountEvent.Deposited> DepositedNotifications { get; } = new();
    public List<BankAccountEvent.Withdrawn> WithdrawnNotifications { get; } = new();

    public void Notify(BankAccountEvent.Deposited e) => DepositedNotifications.Add(e);

    public void Notify(BankAccountEvent.Withdrawn e) => WithdrawnNotifications.Add(e);
}

public class TransactionNotificationSubscription : SubscriptionBase
{
    private readonly TransactionNotificationCollector _collector;

    public TransactionNotificationSubscription(TransactionNotificationCollector collector)
    {
        _collector = collector;

        IncludeType<BankAccountEvent.Deposited>();
        IncludeType<BankAccountEvent.Withdrawn>();
    }

    public override async Task<IChangeListener> ProcessEventsAsync(
        EventRange page,
        ISubscriptionController controller,
        IDocumentOperations operations,
        CancellationToken cancellationToken
    )
    {
        foreach (var @event in page.Events)
        {
            switch (@event.Data)
            {
                case BankAccountEvent.Deposited deposited:
                    _collector.Notify(deposited);
                    break;
                case BankAccountEvent.Withdrawn withdrawn:
                    _collector.Notify(withdrawn);
                    break;
            }
        }
        return await Task.FromResult(NullChangeListener.Instance);
    }
}

public class EventStoreTests_T8_BasicSubscription : TestBase
{
    public static readonly string EST8BasicSubscription = nameof(EST8BasicSubscription);

    [Before(Class)]
    public static async Task CleanupSchema()
    {
        await ResetAllData(EST8BasicSubscription);
    }

    [Test]
    public async Task T1_subscription_receives_deposit_events()
    {
        var collector = new TransactionNotificationCollector();
        var subscription = new TransactionNotificationSubscription(collector);
        var store = Store(EST8BasicSubscription, options => options.Events.Subscribe(subscription));

        // Start daemon
        // Allow the subscription to process events
        using var daemon = await store.BuildProjectionDaemonAsync();
        await daemon.StartAllAsync();

        var accountId = Guid.NewGuid();
        await using (var session = store.LightweightSession())
        {
            session.Events.StartStream<BankAccount>(
                accountId,
                new BankAccountEvent.Opened(accountId, "Alice", Currency.USD),
                new BankAccountEvent.Deposited(Money.From(100, Currency.USD)),
                new BankAccountEvent.Deposited(Money.From(50, Currency.USD)),
                new BankAccountEvent.Withdrawn(Money.From(25, Currency.USD))
            );
            await session.SaveChangesAsync();
        }

        // Wait for daemon to process events
        await daemon.WaitForNonStaleData(10.Seconds());

        collector.DepositedNotifications.Count.ShouldBe(2);
        collector.DepositedNotifications[0].Amount.ShouldBe(Money.From(100, Currency.USD));
        collector.DepositedNotifications[1].Amount.ShouldBe(Money.From(50, Currency.USD));

        collector.WithdrawnNotifications.Count.ShouldBe(1);
        collector.WithdrawnNotifications[0].Amount.ShouldBe(Money.From(25, Currency.USD));
    }
}
