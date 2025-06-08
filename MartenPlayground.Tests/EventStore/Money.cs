using System.Text.Json.Serialization;

namespace MartenPlayground.Tests.EventStore;

[JsonConverter(typeof(JsonStringEnumConverter<Currency>))]
public enum Currency
{
    EUR,
    USD,
    CHF,
}

public record Money(double Amount, Currency Currency) : IComparable<Money>
{
    public static Money From(double amount, Currency currency)
    {
        if (amount < 0)
            throw new InvalidOperationException("Money amount cannot be negative.");

        return new Money(amount, currency);
    }

    public static Money Zero(Currency currency = Currency.USD) => new(0, currency);

    public override string ToString() => $"{Amount:N2} {Currency}";

    public static Money operator +(Money left, Money right) => left.Add(right);

    public static Money operator -(Money left, Money right) => left.Subtract(right);

    public static bool operator <(Money left, Money right) => left.CompareTo(right) < 0;

    public static bool operator >(Money left, Money right) => left.CompareTo(right) > 0;

    public static bool operator <=(Money left, Money right) => left.CompareTo(right) <= 0;

    public static bool operator >=(Money left, Money right) => left.CompareTo(right) >= 0;

    public int CompareTo(Money? other)
    {
        if (other is null)
            return 1; // this is greater than null

        // Convert 'other' to this currency for comparison
        var otherInThisCurrency = other.ConvertTo(Currency);
        return Amount.CompareTo(otherInThisCurrency.Amount);
    }

    private Money Add(Money other)
    {
        if (other.Amount <= 0)
            throw new InvalidOperationException("Amount to add must be greater than zero.");

        var moneyInMyCurrency = other.ConvertTo(Currency);

        return new Money(Amount + moneyInMyCurrency.Amount, Currency);
    }

    private Money Subtract(Money other)
    {
        if (other.Amount <= 0)
            throw new InvalidOperationException("Amount to subtract must be greater than zero.");

        if (Amount < other.Amount)
            throw new InvalidOperationException("Insufficient funds.");

        var moneyInMyCurrency = other.ConvertTo(Currency);

        return new Money(Amount - moneyInMyCurrency.Amount, Currency);
    }

    private Money ConvertTo(Currency targetCurrency)
    {
        if (Currency == targetCurrency)
            return this;

        return (Currency, targetCurrency) switch
        {
            (Currency.EUR, Currency.USD) => new Money(Amount * 1.15, Currency.USD),
            (Currency.USD, Currency.EUR) => new Money(Amount * 0.85, Currency.EUR),

            (Currency.EUR, Currency.CHF) => new Money(Amount * 0.95, Currency.CHF),
            (Currency.CHF, Currency.EUR) => new Money(Amount * 1.05, Currency.EUR),

            (Currency.USD, Currency.CHF) => new Money(Amount * 0.83, Currency.CHF),
            (Currency.CHF, Currency.USD) => new Money(Amount * 1.20, Currency.USD),

            _ => throw new InvalidOperationException("Unknown currency conversion."),
        };
    }
}
