namespace MartenPlayground.Tests.EventStore;

public abstract record BankAccountEvent
{
    public record Opened(Guid Id, string Owner, Currency PreferredCurrency);

    public record Withdrawn(Money Amount);

    public record Deposited(Money Amount);
}

public record BankAccount(Guid Id, string Owner, Money Balance)
{
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
