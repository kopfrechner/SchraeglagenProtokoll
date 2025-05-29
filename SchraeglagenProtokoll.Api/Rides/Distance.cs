using System.Text.Json.Serialization;

namespace SchraeglagenProtokoll.Api.Rides;

[JsonConverter(typeof(JsonStringEnumConverter<DistanceUnit>))]
public enum DistanceUnit
{
    Miles,
    Kilometers,
}

public record Distance : IComparable<Distance>
{
    public double Value { get; init; }
    public DistanceUnit Unit { get; init; }

    public Distance(double value, DistanceUnit unit)
    {
        if (value < 0)
            throw new InvalidOperationException("Distance cannot be negative.");

        Value = value;
        Unit = unit;
    }

    public override string ToString() => $"{Value:N2} {Unit}";

    public static Distance operator +(Distance left, Distance right) => left.Add(right);

    public static Distance operator -(Distance left, Distance right) => left.Subtract(right);

    public static bool operator <(Distance left, Distance right) => left.CompareTo(right) < 0;

    public static bool operator >(Distance left, Distance right) => left.CompareTo(right) > 0;

    public static bool operator <=(Distance left, Distance right) => left.CompareTo(right) <= 0;

    public static bool operator >=(Distance left, Distance right) => left.CompareTo(right) >= 0;

    public int CompareTo(Distance? other)
    {
        if (other is null)
            return 1;

        var otherInThisUnit = other.ConvertTo(Unit);
        return Value.CompareTo(otherInThisUnit.Value);
    }

    private Distance Add(Distance other)
    {
        if (other.Value <= 0)
            throw new InvalidOperationException("Distance to add must be greater than zero.");

        var otherInMyUnit = other.ConvertTo(Unit);
        return new Distance(Value + otherInMyUnit.Value, Unit);
    }

    private Distance Subtract(Distance other)
    {
        if (other.Value <= 0)
            throw new InvalidOperationException("Distance to subtract must be greater than zero.");

        var otherInMyUnit = other.ConvertTo(Unit);

        if (Value < otherInMyUnit.Value)
            throw new InvalidOperationException("Resulting distance would be negative.");

        return new Distance(Value - otherInMyUnit.Value, Unit);
    }

    private Distance ConvertTo(DistanceUnit targetUnit)
    {
        if (Unit == targetUnit)
            return this;

        return (Unit, targetUnit) switch
        {
            (DistanceUnit.Miles, DistanceUnit.Kilometers) => new Distance(
                Value * 1.60934,
                DistanceUnit.Kilometers
            ),
            (DistanceUnit.Kilometers, DistanceUnit.Miles) => new Distance(
                Value / 1.60934,
                DistanceUnit.Miles
            ),
            _ => throw new InvalidOperationException("Unknown distance conversion."),
        };
    }
}