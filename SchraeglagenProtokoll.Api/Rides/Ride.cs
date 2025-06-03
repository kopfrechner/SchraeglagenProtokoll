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

public record Ride(
    Guid Id,
    Guid RiderId,
    RideStatus Status,
    string StartLocation,
    string? Destination,
    Distance? Distance,
    SchraeglagenRating? Rating,
    List<string> TrackedLocations
)
{
    [JsonInclude]
    public int Version { get; private set; }

    public static Ride Create(RideStarted @event) =>
        new Ride(
            @event.RideId,
            @event.RiderId,
            RideStatus.Started,
            @event.StartLocation,
            null,
            null,
            null,
            [@event.StartLocation]
        );

    public Ride Apply(RideLocationTracked @event) =>
        this with
        {
            TrackedLocations = [.. TrackedLocations, @event.Location],
        };

    public Ride Apply(RideFinished @event) =>
        this with
        {
            Distance = @event.Distance,
            Status = RideStatus.Finished,
            Destination = @event.Destination,
            TrackedLocations = [.. TrackedLocations, @event.Destination],
        };

    public Ride Apply(RideRated @event) => this with { Rating = @event.Rating };
}
