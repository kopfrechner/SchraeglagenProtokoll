using JasperFx.Events;
using Marten.Events.Aggregation;

namespace SchraeglagenProtokoll.Api.Rides.Projections;

public record RideSummary(
    Guid Id,
    string StartLocation,
    DateTimeOffset StartedAt,
    string? Destination = null,
    DateTimeOffset? FinishedAt = null,
    TimeSpan? Duration = null,
    Distance? Distance = null
);

public class RideSummaryProjection : SingleStreamProjection<RideSummary, Guid>
{
    public RideSummaryProjection()
    {
        IncludeType<RideStarted>();
        IncludeType<RideFinished>();
    }

    public static RideSummary Create(IEvent<RideStarted> e) =>
        new(e.StreamId, e.Data.StartLocation, e.Timestamp);

    public RideSummary Apply(IEvent<RideFinished> e, RideSummary state) =>
        state with
        {
            Destination = e.Data.Destination,
            FinishedAt = e.Timestamp,
            Distance = e.Data.Distance,
            Duration = e.Timestamp - state.StartedAt,
        };
};
