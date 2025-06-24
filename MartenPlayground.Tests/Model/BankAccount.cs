using System.Text.Json.Serialization;

namespace MartenPlayground.Tests.Model;

public abstract record BankAccountEvent
{
    public record Opened(string Owner, Currency PreferredCurrency) : BankAccountEvent;

    public record Deposited(Money Amount) : BankAccountEvent;

    public record Withdrawn(Money Amount) : BankAccountEvent;
}

public record BankAccount(string Owner, Money Balance)
{
    [JsonInclude]
    public Guid Id { get; private set; }

    [JsonInclude]
    public int Version { get; private set; }

    public static BankAccount Create(BankAccountEvent.Opened request) =>
        new(request.Owner, Money.From(0, request.PreferredCurrency));

    public BankAccount Apply(BankAccountEvent.Deposited @event) =>
        this with
        {
            Balance = Balance + @event.Amount,
        };

    public BankAccount Apply(BankAccountEvent.Withdrawn @event) =>
        this with
        {
            Balance = Balance - @event.Amount,
        };
}
