using JasperFx.Events;
using JasperFx.Events.Grouping;
using Marten;
using Marten.Events;
using Marten.Events.Aggregation;
using SchraeglagenProtokoll.Api.Rides;

namespace SchraeglagenProtokoll.Api.Riders.Projections;

public class RideEventsToRiderInlineGrouper : IAggregateGrouper<Guid>
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

        // In case there are only RideStarted events, we can do a quick grouping
        if (events.All(x => x.EventTypesAre(typeof(IEvent<RideStarted>))))
        {
            foreach (var e in events)
            {
                var rideStarted = e.Data as RideStarted;
                if (rideStarted is not null)
                {
                    grouping.AddEvents(rideStarted.RiderId, [e]);
                }
            }
        }

        if (events.All(x => x.EventTypesAre(typeof(IEvent<Archived>))))
        {
            foreach (var e in events)
            {
                var riderArchivedEvent = e.Data as IEvent;
                if (riderArchivedEvent is not null)
                {
                    grouping.AddEvents(e.StreamId, [e]);
                }
            }
        }

        // Group events by StreamId
        var rideEventPerStream = rideEvents
            .GroupBy(x => x.StreamId)
            .Select(x => new { RideId = x.Key, Events = x.ToList() })
            .ToList();

        foreach (var streamEvents in rideEventPerStream)
        {
            // Try to find the RideStarted event in the events list.
            var rideStartedEvent = (IEvent<RideStarted>?)
                streamEvents.Events.FirstOrDefault(x => x.EventTypesAre(typeof(RideStarted)));
            if (rideStartedEvent is null)
            {
                // If not found, try to find it in the raw events.
                rideStartedEvent = (IEvent<RideStarted>?)
                    session
                        .Events.QueryAllRawEvents()
                        .Where(x => x.StreamId == streamEvents.RideId)
                        .FirstOrDefault(x => x.EventTypesAre(typeof(RideStarted)));
            }

            // If not found, skip this stream.
            if (rideStartedEvent is null)
            {
                continue;
            }

            // Add the events to the grouping.
            var riderArchivedEvent = archivedRiderEvents
                .Where(x => x.StreamId == rideStartedEvent.Data.RideId)
                .ToList();
            grouping.AddEvents(
                rideStartedEvent.Data.RiderId,
                streamEvents.Events.Cast<IEvent>().Union(riderArchivedEvent)
            );
        }

        await Task.CompletedTask;
    }
}
