using Marten.Events;
using Marten.Events.Projections;
using SchraeglagenProtokoll.Api.Riders.Features;
using SchraeglagenProtokoll.Api.Rides;

namespace SchraeglagenProtokoll.Api.Riders.Projections;

public class RiderHistory
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = default!;
    public string RoadName { get; set; } = default!;

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
        Identity<RideStarted>(_ => _.RiderId);
        Identity<IEvent<RiderRenamed>>(_ => _.StreamId);

        Identity<IEvent<RiderDeletedAccount>>(_ => _.StreamId);
        DeleteEvent<IEvent<RiderDeletedAccount>>(_ => true);
    }

    public void Apply(IEvent<RiderRegistered> e, RiderHistory details)
    {
        details.Id = e.Data.Id;
        details.FullName = e.Data.FullName;
        details.RoadName = e.Data.RoadName;

        details.AddHistoryEntry($"Rider registered with alias {details.RoadName}.", e.Timestamp);
    }

    public void Apply(IEvent<RideStarted> e, RiderHistory details)
    {
        details.AddHistoryEntry($"New ride started at {e.Data.StartLocation}.", e.Timestamp);
    }

    public void Apply(IEvent<RiderRenamed> e, RiderHistory details)
    {
        details.AddHistoryEntry(
            $"Rider renamed from {details.FullName} to {e.Data.FullName}.",
            e.Timestamp
        );
        details.FullName = e.Data.FullName;
    }

    public void Apply(IEvent<Delete.DeleteRiderCommand> e, RiderHistory details)
    {
        details.AddHistoryEntry($"Rider deleted: {e.Data.RiderFeedback}", e.Timestamp);
    }
}
