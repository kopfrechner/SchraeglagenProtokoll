using System.Text.Json.Serialization;

namespace SchraeglagenProtokoll.Api.Rides;

public interface IRideEvent
{
    Guid RideId { get; }
};

public record RideStarted(Guid RideId, Guid RiderId, string StartLocation) : IRideEvent;

public record RideLocationTracked(Guid RideId, string Location) : IRideEvent;

public record RideFinished(Guid RideId, string Destination, Distance Distance) : IRideEvent;

public record RideRated(Guid RideId, SchraeglagenRating Rating) : IRideEvent;

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
            Id = @event.RideId,
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
        TrackedLocations.Add(@event.Destination);
    }

    public void Apply(RideRated @event)
    {
        Rating = @event.Rating;
    }
}
