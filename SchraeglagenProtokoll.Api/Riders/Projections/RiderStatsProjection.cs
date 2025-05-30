using System.Text.Json.Serialization;
using Marten.Events.Projections;
using SchraeglagenProtokoll.Api.Rides;

namespace SchraeglagenProtokoll.Api.Riders.Projections;

public class RiderStats
{
    // Helping us out to identify the document
    public static readonly Guid DocumentIdentifier = Guid.Parse(
        "0957ddc5-74ac-49e5-8ef7-be9e80f0b640"
    );

    // Required by Marten
    public Guid Id { get; private set; } = DocumentIdentifier;
    public List<RiderStat> RiderInfos { get; set; } = new();

    public class RiderStat
    {
        public Guid RiderId { get; init; }
        public string RoadName { get; init; }

        [JsonInclude]
        public int RidesCount { get; private set; } = 0;

        [JsonInclude]
        public Distance TotalDistance { get; private set; } = Distance.Zero();

        [JsonInclude]
        public Distance AverageDistance { get; private set; } = Distance.Zero();

        public void AddRide(RideLogged rideLogged)
        {
            RidesCount++;
            TotalDistance += rideLogged.Distance;
            AverageDistance = new Distance(TotalDistance.Value / RidesCount, TotalDistance.Unit);
        }
    }
}

public class RiderStatsProjection : MultiStreamProjection<RiderStats, Guid>
{
    public RiderStatsProjection()
    {
        Identity<RiderRegistered>(_ => RiderStats.DocumentIdentifier);
        Identity<RideLogged>(_ => RiderStats.DocumentIdentifier);
    }

    public void Apply(RiderRegistered e, RiderStats stats)
    {
        stats.RiderInfos.Add(new RiderStats.RiderStat { RiderId = e.Id, RoadName = e.RoadName });
    }

    public void Apply(RideLogged e, RiderStats stats)
    {
        var rider = stats.RiderInfos.Find(x => x.RiderId == e.RiderId);
        rider?.AddRide(e);
    }
}
