using System.Text.Json.Serialization;

namespace MartenPlayground.Tests.EventStore;

public abstract record BankAccountEvent
{
    public record Opened(Guid Id, string Owner, Currency PreferredCurrency) : BankAccountEvent;

    public record Withdrawn(Money Amount) : BankAccountEvent;

    public record Deposited(Money Amount) : BankAccountEvent;
}

public record BankAccount(Guid Id, string Owner, Money Balance)
{
    [JsonInclude]
    public int Version { get; private set; }

    public static BankAccount Create(BankAccountEvent.Opened request) =>
        new BankAccount(request.Id, request.Owner, Money.From(0, request.PreferredCurrency));

    public BankAccount Apply(BankAccountEvent.Withdrawn @event) =>
        this with
        {
            Balance = Balance - @event.Amount,
        };

    public BankAccount Apply(BankAccountEvent.Deposited @event) =>
        this with
        {
            Balance = Balance + @event.Amount,
        };
}
