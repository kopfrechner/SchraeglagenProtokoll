using System.Text.Json.Serialization;

namespace SchraeglagenProtokoll.Api.Rides;

public record RideStarted(Guid Id, Guid RiderId, string StartLocation);

public record RideLocationTracked(string Location);

public record RideFinished(string Destination, Distance Distance);

public record RideRated(SchraeglagenRating Rating);

// TODO Implement Pause and Resume

public enum SchraeglagenRating
{
    Bockgerade = 0,
    Pendlerstrecke = 1,
    Feierabendkurven = 2,
    Kurvenspa = 3,
    Kehrenparadies = 4,
}

public enum RideStatus
{
    Started,
    Finished,
}

public class Ride
{
    [JsonInclude]
    public Guid Id { get; private set; }

    [JsonInclude]
    public int Version { get; private set; }

    [JsonInclude]
    public Guid RiderId { get; private set; }

    [JsonInclude]
    public RideStatus Status { get; private set; }

    [JsonInclude]
    public string StartLocation { get; private set; } = null!;

    [JsonInclude]
    public string Destination { get; private set; } = null!;

    [JsonInclude]
    public Distance Distance { get; private set; }

    [JsonInclude]
    public SchraeglagenRating? Rating { get; private set; }

    [JsonInclude]
    public List<string> TrackedLocations { get; init; }

    public static Ride Create(RideStarted @event)
    {
        return new Ride
        {
            Id = @event.Id,
            RiderId = @event.RiderId,
            StartLocation = @event.StartLocation,
            Distance = Distance.Zero(),
            Status = RideStatus.Started,
            TrackedLocations = [@event.StartLocation],
        };
    }

    public void Apply(RideLocationTracked @event)
    {
        TrackedLocations.Add(@event.Location);
    }

    public void Apply(RideFinished @event)
    {
        Distance = @event.Distance;
        Status = RideStatus.Finished;
        Destination = @event.Destination;
    }

    public void Apply(RideRated @event)
    {
        Rating = @event.Rating;
    }
}
