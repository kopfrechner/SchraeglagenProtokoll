using System.Text.Json.Serialization;
using JasperFx.Core;
using Marten.Events;

namespace SchraeglagenProtokoll.Api.Rides;

public record RideStarted(Guid Id, Guid RiderId, string StartLocation);

public record RideLocationTracked(Guid Id, string Location);

public record RideFinished(Guid Id, string Destination, Distance Distance);

public record RideRated(Guid Id, SchraeglagenRating Rating);

// TODO Implement Pause and Resume

public enum SchraeglagenRating
{
    Bockgerade = 0,
    Pendlerstrecke = 1,
    Feierabendkurven = 2,
    Kurvenspa = 3,
    Kehrenparadies = 4,
}

public record RideLogged(
    Guid Id,
    Guid RiderId,
    DateTimeOffset Date,
    string StartLocation,
    string Destination,
    Distance Distance
);

public record CommentAdded(Guid CommentedBy, string Text);

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
    public List<Comment> Comments { get; private set; } = new();

    [JsonInclude]
    public List<string> TrackedLocations { get; init; }

    // Make serialization easy
    public Ride() { }

    public static Ride Create(RideLogged @event)
    {
        return new Ride
        {
            Id = @event.Id,
            RiderId = @event.RiderId,
            StartLocation = @event.StartLocation,
            Destination = @event.Destination,
            Distance = @event.Distance,
            Status = RideStatus.Finished,
            TrackedLocations = [@event.StartLocation],
        };
    }

    public void Apply(IEvent<CommentAdded> @event)
    {
        Comments.Add(new Comment(@event.Data.CommentedBy, @event.Data.Text, @event.Timestamp));
    }

    public record Comment(Guid CommentedBy, string Text, DateTimeOffset Timestamp);
}
