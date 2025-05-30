using System.Text.Json.Serialization;
using Marten.Events;

namespace SchraeglagenProtokoll.Api.Rides;

public record RideLogged(
    Guid Id,
    Guid RiderId,
    DateTimeOffset Date,
    string StartLocation,
    string Destination,
    Distance Distance
);

public record CommentAdded(Guid CommentedBy, string Text);

public class Ride
{
    [JsonInclude]
    public Guid Id { get; private set; }

    [JsonInclude]
    public int Version { get; private set; }

    [JsonInclude]
    public Guid RiderId { get; private set; }

    [JsonInclude]
    public DateTimeOffset Date { get; private set; }

    [JsonInclude]
    public string StartLocation { get; private set; } = null!;

    [JsonInclude]
    public string Destination { get; private set; } = null!;

    [JsonInclude]
    public Distance Distance { get; private set; }

    [JsonInclude]
    public List<Comment> Comments { get; private set; } = new();

    // Make serialization easy
    public Ride() { }

    public static Ride Create(RideLogged @event)
    {
        return new Ride
        {
            Id = @event.Id,
            RiderId = @event.RiderId,
            Date = @event.Date,
            StartLocation = @event.StartLocation,
            Destination = @event.Destination,
            Distance = @event.Distance,
        };
    }

    public void Apply(IEvent<CommentAdded> @event)
    {
        Comments.Add(new Comment(@event.Data.CommentedBy, @event.Data.Text, @event.Timestamp));
    }

    public record Comment(Guid CommentedBy, string Text, DateTimeOffset Timestamp);
}
