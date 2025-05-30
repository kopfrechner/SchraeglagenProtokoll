using Marten.Events;
using Marten.Events.Projections;
using SchraeglagenProtokoll.Api.Riders.Features;
using SchraeglagenProtokoll.Api.Rides;

namespace SchraeglagenProtokoll.Api.Riders.Projections;

public class RiderHistory
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = default!;
    public string NerdAlias { get; set; } = default!;

    public List<RiderHistoryEntry> History { get; set; } = new();

    public void AddHistoryEntry(string action, DateTimeOffset timestamp) =>
        History.Add(new RiderHistoryEntry { Action = action, Timestamp = timestamp.UtcDateTime });
}

public class RiderHistoryEntry
{
    public string Action { get; set; } = default!;
    public DateTimeOffset Timestamp { get; set; }
}

public class RiderHistoryProjection : MultiStreamProjection<RiderHistory, Guid>
{
    public RiderHistoryProjection()
    {
        Identity<RiderRegistered>(_ => _.Id);
        Identity<RideLogged>(_ => _.RiderId);
        Identity<IEvent<RiderRenamed>>(_ => _.StreamId);
        Identity<CommentAdded>(_ => _.CommentedBy);

        Identity<IEvent<RiderDeletedAccount>>(_ => _.StreamId);
        DeleteEvent<IEvent<RiderDeletedAccount>>(_ => true);
    }

    public void Apply(IEvent<RiderRegistered> e, RiderHistory details)
    {
        details.Id = e.Data.Id;
        details.FullName = e.Data.FullName;
        details.NerdAlias = e.Data.NerdAlias;

        details.AddHistoryEntry($"Rider registered with alias {details.NerdAlias}.", e.Timestamp);
    }

    public void Apply(IEvent<RideLogged> e, RiderHistory details)
    {
        details.AddHistoryEntry(
            $"Ride from {e.Data.StartLocation} to {e.Data.Destination} ({e.Data.Distance}) logged.",
            e.Timestamp
        );
    }

    public void Apply(IEvent<RiderRenamed> e, RiderHistory details)
    {
        details.AddHistoryEntry(
            $"Rider renamed from {details.FullName} to {e.Data.FullName}.",
            e.Timestamp
        );
        details.FullName = e.Data.FullName;
    }

    public void Apply(IEvent<CommentAdded> e, RiderHistory details)
    {
        details.AddHistoryEntry($"Rider added comment to a ride: {e.Data.Text}", e.Timestamp);
    }

    public void Apply(IEvent<Delete.DeleteRiderCommand> e, RiderHistory details)
    {
        details.AddHistoryEntry($"Rider deleted: {e.Data.RiderFeedback}", e.Timestamp);
    }
}
