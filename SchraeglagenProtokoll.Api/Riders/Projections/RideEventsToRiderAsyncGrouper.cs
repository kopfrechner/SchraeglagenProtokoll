using JasperFx.Events;
using JasperFx.Events.Grouping;
using Marten;
using Marten.Events;
using Marten.Events.Aggregation;
using SchraeglagenProtokoll.Api.Rides;

namespace SchraeglagenProtokoll.Api.Riders.Projections;

public class RideEventsToRiderAsyncGrouper : IAggregateGrouper<Guid>
{
    /// <param name="session">Query session to enable additional loading of data when needed.</param>
    /// <param name="events">
    /// All the events that are handled in this scope.
    /// For synchronous projection, it will contain all events appended before saving changes.
    /// For asynchronous, it will contain a batch of processing events.
    /// Marten does processing in batches to improve performance.
    /// </param>
    /// <param name="grouping">Grouping to add transformed events, grouping them by specified stream id.</param>
    public async Task Group(
        IQuerySession session,
        IEnumerable<IEvent> events,
        IEventGrouping<Guid> grouping
    )
    {
        var archivedRiderEvents = events.OfType<IEvent<RiderDeletedAccount>>().ToList();

        var rideEvents = events.OfType<IEvent<IRideEvent>>().ToList();
        if (rideEvents.Count == 0 && archivedRiderEvents.Count == 0)
        {
            return;
        }

        var rideIds = rideEvents.Select(e => e.Data.RideId).Distinct().ToList();

        var rideStaredRawEvents = await session
            .Events.QueryAllRawEvents()
            .Where(x => x.EventTypesAre(typeof(RideStarted)))
            .Where(x => rideIds.Contains(x.StreamId))
            .ToListAsync();

        var rideStaredEvents = rideStaredRawEvents
            .Select(x => new { x.StreamId, ((RideStarted)x.Data).RiderId })
            .ToList();

        foreach (
            var group in rideStaredEvents.Select(g => new
            {
                g.RiderId,
                Events = rideEvents.Where(ev => ev.StreamId == g.StreamId).Cast<IEvent>(),
            })
        )
        {
            var riderArchivedEvent = archivedRiderEvents
                .Where(x => x.StreamId == group.RiderId)
                .ToList();
            grouping.AddEvents(group.RiderId, group.Events.Union(riderArchivedEvent));
        }
    }
}
