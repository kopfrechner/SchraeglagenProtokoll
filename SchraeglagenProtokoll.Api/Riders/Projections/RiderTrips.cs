using Marten;
using Marten.Events;
using Marten.Events.Projections;
using SchraeglagenProtokoll.Api.Rides;

namespace SchraeglagenProtokoll.Api.Riders.Projections;

public class RiderTrips
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = default!;
    public string RoadName { get; set; } = default!;

    public List<RiderTrip> Trips { get; set; } = new();
}

public class RiderTrip
{
    public string StartLocation { get; set; } = default!;
    public string Destination { get; set; } = default!;
    public Distance Distance { get; set; } = default!;
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
}

public class RiderTripProjection : MultiStreamProjection<RiderTrips, Guid>
{
    public RiderTripProjection()
    {
        Identity<IEvent<IRiderEvent>>(_ => _.StreamId);
        CustomGrouping(new RideEventsToRiderAsyncGrouper());
    }

    public void Apply(IEvent<RiderRegistered> e, RiderTrips riderTrips)
    {
        riderTrips.Id = e.Data.RiderId;
        riderTrips.FullName = e.Data.FullName;
        riderTrips.RoadName = e.Data.RoadName;
    }

    public async Task Apply(IEvent<RideFinished> e, RiderTrips riderTrips, IQuerySession session)
    {
        var streamEvents = await session.Events.FetchStreamAsync(e.StreamId);
        var startedEvent =
            streamEvents.FirstOrDefault(x => x is IEvent<RideStarted>) as IEvent<RideStarted>;
        if (startedEvent is null)
        {
            return;
        }

        riderTrips.Trips.Add(
            new RiderTrip
            {
                StartLocation = startedEvent!.Data.StartLocation,
                Destination = e.Data.Destination,
                Distance = e.Data.Distance,
                StartTime = startedEvent.Timestamp,
                EndTime = e.Timestamp,
            }
        );
    }
}
