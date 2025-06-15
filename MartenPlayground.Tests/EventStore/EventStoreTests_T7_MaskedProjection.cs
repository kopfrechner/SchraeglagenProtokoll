using JasperFx.Events;
using MartenPlayground.Tests.EventStore.Model;
using MartenPlayground.Tests.Setup;
using Shouldly;

namespace MartenPlayground.Tests.EventStore;

public class EventStoreTests_T7_MaskedProjection : TestBase
{
    public static string EST7Masked = nameof(EST7Masked);

    [Before(Class)]
    public static async Task CleanupSchema()
    {
        await ResetAllData(EST7Masked);
    }

    [Test]
    public async Task T1_when_mask_events_protected_data_is_erased()
    {
        var owner = "Alice";
        var bankAccountId = Guid.NewGuid();
        var session = Session(
            EST7Masked,
            options =>
                options.Events.AddMaskingRuleForProtectedInformation<BankAccountEvent.Opened>(x =>
                    x with
                    {
                        Owner = "****",
                    }
                )
        );

        // Store events with unmasked owner
        session.Events.StartStream<BankAccount>(
            bankAccountId,
            new BankAccountEvent.Opened(bankAccountId, owner, Currency.USD),
            new BankAccountEvent.Deposited(Money.From(100, Currency.USD)),
            new BankAccountEvent.Withdrawn(Money.From(30, Currency.USD))
        );
        await session.SaveChangesAsync();

        // Mask events
        await session.DocumentStore.Advanced.ApplyEventDataMasking(x =>
            x.IncludeStream(bankAccountId)
        );
        await session.SaveChangesAsync();

        // Projection should show masked owner
        var events = await session.Events.FetchStreamAsync(bankAccountId);
        var opened = events.OfType<IEvent<BankAccountEvent.Opened>>().Single();
        opened.Data.Owner.ShouldBe("****");
    }
}
